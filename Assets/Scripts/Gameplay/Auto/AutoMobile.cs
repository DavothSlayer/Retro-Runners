using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Burst;
using UnityEngine;
using V3CTOR;
using Random = UnityEngine.Random;
using SkrilStudio;

namespace RetroCode
{
    public class AutoMobile : MonoBehaviour
    {
        #region Visible References
        [Header("Main References")]
        public InputManager input;
        public GameManager gameManager;
        public MainGameHUD hud;

        [Header("Physics")]
        public Rigidbody rb;

        [Header("Car Data")]
        public AutoData data;
        public AutoAbility ability;

        [HideInInspector]
        public AbilityState abilityState = AbilityState.Ready;
        [HideInInspector]
        public float abilityTimer;
        [HideInInspector]
        public float abilityCooldownTimer;

        [Header("VFX")]
        [SerializeField]
        private ParticleSystem fixFX;
        [SerializeField]
        private ParticleSystem tokenFX;
        [SerializeField]
        private VariableDmgFX[] damageFX;
        [SerializeField]
        private ParticleSystem[] tireSmokeFX;
        [SerializeField]
        private ParticleSystem[] sparkFX;
        [SerializeField]
        private LayerMask sparkLayerMask;
        [SerializeField]
        private ParticleSystem exhaustFX;
        [SerializeField]
        private ParticleSystem boostFX;
        [SerializeField]
        private ParticleSystem[] abilityFX;
        [SerializeField]
        private GameObject deathFX;

        [Header("Objects")]
        [SerializeField]
        private GameObject[] carModels;
        [SerializeField]
        private GameObject deadCarModel;
        [SerializeField]
        private Transform[] wheelSets;
        [SerializeField]
        private float wheelDiameter;

        [Header("SFX")]
        public RealisticEngineSound engineSFX;
        [SerializeField]
        private AudioClip abilityClip;
        #endregion

        #region Hidden References
        // HIDDEN //
        [HideInInspector]
        public int currentHealth;
        [HideInInspector]
        public int engineLevel;
        [HideInInspector]
        public int gearboxLevel;
        [HideInInspector]
        public int tiresLevel;
        [HideInInspector]
        public int armorLevel;
        [HideInInspector]
        public int powerLevel;
        // HIDDEN //
        #endregion

        public delegate void AutoDamaged();
        public event AutoDamaged Damaged;

        [BurstCompile]
        private void Update()
        {
            AutoMath();

            HandleSFX();
            HandleVFX();

            AbilityHandler();
        }

        [BurstCompile]
        private void FixedUpdate() => AutoMovement();

        [BurstCompile]
        public void OnCollisionEnter(Collision col)
        {
            //if (health == 0) { return; }

            Damageable damageable = col.collider.GetComponentInParent<Damageable>();
            if (damageable == null) { col.collider.GetComponent<Damageable>(); }
            if (damageable == null) { return; }

            print($"Dot Product: {Vector3.Dot(col.GetContact(0).otherCollider.transform.forward, transform.forward)}");

            // .75f FOR SAME DIRECTION CRASH, -.75f FOR HEAD-ON COLLISION //
            if (Vector3.Dot(col.GetContact(0).otherCollider.transform.forward, transform.forward) >= 0.75f)
            {
                if (Vector3.Dot(col.GetContact(0).normal, transform.forward) >= -0.8f) return;

                gameManager.ShakeTheCam(0.4f);

                rb.AddForce(Mathf.Abs(col.relativeVelocity.magnitude) * transform.forward, ForceMode.VelocityChange);
                
                damageable.Damage(1);
                HandleDamage(damageable.DamageToPlayer());

                //evaluation -= rb.linearVelocity.magnitude / data.autoLevelData[engineLevel].TopSpeed * 0.3f;
            }
            else if (Vector3.Dot(col.GetContact(0).otherCollider.transform.forward, transform.forward) < -0.75f)
            {
                if (Vector3.Dot(col.GetContact(0).normal, transform.forward) >= -0.8f) return;

                gameManager.ShakeTheCam(0.4f);

                rb.AddForce(Mathf.Abs(col.relativeVelocity.magnitude) * transform.forward, ForceMode.VelocityChange);

                damageable.Damage(damageable.Health());
                HandleDamage(damageable.DamageToPlayer() * 2);

                //evaluation -= rb.linearVelocity.magnitude / data.autoLevelData[engineLevel].TopSpeed * 0.3f;
            }
        }

        [BurstCompile]        
        public void OnTriggerEnter(Collider col)
        {
            if (GameManager.gameState != GameState.InGame) return;

            // NEAR MISS //
            if (ValidNearMiss(col))
            {
                gameManager.NearMiss();

                // ADD 25% SPEED BOOST FOR SUCCESSFUL NEAR MISS //
                gameManager.ShakeTheCam(0.4f);
                rb.AddForce(Vector3.forward * rb.linearVelocity.magnitude * 0.25f * massDragMltplr, ForceMode.Impulse);
                NearMissPitch += 0.15f;
            }

            // PICKUPS //
            if (col.TryGetComponent<PickUp>(out PickUp pickup))
            {
                if (pickup.Hoverable().activeInHierarchy)
                {
                    gameManager.HandlePickup(pickup.GetPickupType()); 
                    pickup.PickupInit();
                }
            }
        }

        private Collider lastTrigger;
        private bool ValidNearMiss(Collider col)
        {
            NearMissable missable = col.GetComponent<NearMissable>();

            if (missable == null) return false;

            // FILTER OUT GUARDRAILS, THE ROAD & WALL COLLIDERS //
            if (!col.GetComponent<GuardRail>() && col.gameObject.layer != 7 && col.gameObject.layer != 8)
            {
                if (lastTrigger == col) return false;

                lastTrigger = col;

                if (transform.position.z + data.nearMissProxy > col.transform.position.z - missable.NearMissProxy()) return false;
                else return true;
            }

            return false;
        }

        #region Driving
        private float currentRPM;
        private int currentGear = 1;
        private float shiftUpThreshold;
        private float shiftDownThreshold;
        private float targetTorque;

        private bool pressingGas = true;

        [HideInInspector]
        public bool boost;
        private float boostMltplr, massDragMltplr;
        
        private float maxRPM, gearRatio, highestGear, maxTorque, shiftPercent;
        private float turnRatio;

        [BurstCompile]        
        private void AutoMath()
        {
            if (GameManager.gameState == GameState.GameOver) return;

            turnRatio = Mathf.Lerp(turnRatio, rb.linearVelocity.z / data.autoLevelData[engineLevel].TopSpeed, Time.deltaTime);

            engineSFX.gasPedalPressing = pressingGas;             

            massDragMltplr = rb.linearDamping * rb.mass;

            highestGear = data.autoLevelData[gearboxLevel].MaxGear;
            maxRPM = data.autoLevelData[engineLevel].MaxRPM;
            maxTorque = data.autoLevelData[gearboxLevel].Torque;
            gearRatio = (float)currentGear / highestGear;
            
            shiftDownThreshold = 0.3f + 0.2f * gearRatio;
            shiftUpThreshold = 0.6f + 0.2f * gearRatio;

            shiftPercent = rb.linearVelocity.z / (data.autoLevelData[engineLevel].TopSpeed * gearRatio);

            if (currentGear < highestGear && shiftPercent > shiftUpThreshold && pressingGas) ShiftGear(1);

            if (currentGear > 1 && shiftPercent < shiftDownThreshold && pressingGas) ShiftGear(-1);

            currentRPM = Mathf.Clamp(currentRPM, data.IdleRPM(gearboxLevel), maxRPM);

            if (pressingGas) 
            {
                if (GameManager.gameState == GameState.InGame)
                {
                    //currentRPM = Mathf.Lerp(currentRPM, maxRPM, Time.deltaTime / (data.Acceleration(gearboxLevel) * Mathf.Pow(gearRatio, 2f)));
                    currentRPM = Mathf.Lerp(currentRPM, maxRPM, Time.deltaTime / data.Acceleration(gearboxLevel));
                    targetTorque = data.Acceleration(gearboxLevel) * (currentRPM / maxRPM) * massDragMltplr;
                }
                else
                {
                    currentRPM = maxRPM * 0.5f;
                    targetTorque = data.autoLevelData[engineLevel].TopSpeed * 0.3f * massDragMltplr;
                }
            } 
            else currentRPM = Mathf.Lerp(currentRPM, data.IdleRPM(gearboxLevel), Time.deltaTime);

            // DEBUGGING //
            print($"Pressing Gas: {pressingGas} | GearRatio: {gearRatio} | ShiftPercent: {shiftPercent}% | Gear: {currentGear} | RPM: {currentRPM} | VelZ: {rb.linearVelocity.z}" );
        }

        private async void ShiftGear(int shift)
        {
            pressingGas = false;

            engineSFX.isShifting = true;

            currentGear += shift;

            await Task.Delay(450 - 50 * gearboxLevel);

            engineSFX.isShifting = false;

            pressingGas = true;
        }

        [BurstCompile]
        private void AutoMovement()
        {
            if (GameManager.gameState == GameState.GameOver) return;

            rb.linearVelocity = Vector3.ClampMagnitude(rb.linearVelocity, data.autoLevelData[engineLevel].TopSpeed);

            if (GameManager.gameState == GameState.InGame)
            {
                if (input.xTouch != 0f)
                {
                    rb.AddForce(Vector3.right * input.xTouch * data.SteerSpeed(tiresLevel) * massDragMltplr, ForceMode.Force);
                }
               
                if (input.xTouch == 0f)
                {
                    rb.AddForce(-Vector3.right * rb.linearVelocity.x * 1f * massDragMltplr, ForceMode.Force);
                }

                rb.rotation = Quaternion.Euler(0f, data.RotateAmount(tiresLevel) * input.xTouchLerp * (1f - turnRatio), 0f);
            }            

            if (GameManager.gameState == GameState.InMenu) rb.rotation = Quaternion.Euler(0f, 0f, 0f);

            rb.AddForce(Vector3.forward * targetTorque, ForceMode.Force);
        }

        [BurstCompile]
        public void HandleActivateAbility()
        {
            if (abilityState != AbilityState.Ready || GameManager.gameState != GameState.InGame) return;

            abilityState = AbilityState.Active;
            ability.ActivateAbility(this);

            foreach (ParticleSystem ps in abilityFX)
                ps.PlaySystem();

            //engineSFX.PlayOneShot(abilityClip);

            gameManager.ShakeTheCam(1f);
        }

        [BurstCompile]
        private void AbilityHandler()
        {
            switch (abilityState)
            {
                case AbilityState.Ready:
                    abilityTimer = 0f;
                    break;
                case AbilityState.Active:
                    if (abilityTimer < ability.duration)
                    {
                        abilityTimer += Time.deltaTime;
                    }
                    else
                    {
                        abilityCooldownTimer = 0f;
                        abilityState = AbilityState.Cooldown;
                    }
                    break;
                case AbilityState.Cooldown:
                    if (abilityCooldownTimer < ability.cooldownTime)
                        abilityCooldownTimer += Time.deltaTime;
                    else
                    {
                        gameManager.HandleAbilityReady();
                        abilityState = AbilityState.Ready;
                    }
                    break;
            }
        }
        #endregion

        #region Damage & Game Over
        [BurstCompile]
        public void HandleDamage(int dmg)
        {
            if (currentHealth <= 0) return;

            Damaged?.Invoke();

            currentHealth -= dmg;

            // PLAY THE DAMAGE FX //
            if (currentHealth >= data.autoLevelData[armorLevel].MaxHealth)
            {
                foreach (VariableDmgFX variableDmgFX in damageFX)
                    foreach (ParticleSystem fx in variableDmgFX.dmgFX)
                        fx.StopSystem();
            }
            else
            {
                foreach (VariableDmgFX variableDmgFX in damageFX)
                    if (currentHealth <= variableDmgFX.healthThreshold)
                        PlayDMGFX(variableDmgFX.dmgFX);
            }

            if (currentHealth <= 0 && GameManager.gameState != GameState.GameOver) { HandleGameOver(); }
        }

        [BurstCompile]
        private void HandleGameOver()
        {
            gameManager.GameOver();

            engineSFX.enabled = false;

            carModels[0].SetActive(false);

            rb.mass = 5f;
            rb.linearDamping = 0.25f;
            rb.constraints = RigidbodyConstraints.None;

            Vector3 explosionTorqueVector =
                transform.right * Random.Range(-5f, 5f) +
                transform.up * Random.Range(-5f, 5f) +
                transform.forward * Random.Range(-5f, 5f);

            rb.AddForce(Vector3.up, ForceMode.VelocityChange);
            rb.AddTorque(explosionTorqueVector, ForceMode.VelocityChange);

            deadCarModel.SetActive(true);
            deathFX.SetActive(true);
        }

        [BurstCompile]
        public void FixAuto()
        {
            boost = false;
            
            currentHealth = data.autoLevelData[armorLevel].MaxHealth;
            engineSFX.maxRPMLimit = data.autoLevelData[powerLevel].MaxRPM;

            rb.linearVelocity = Vector3.zero;
            rb.linearVelocity = Vector3.forward * targetTorque;

            exhaustFX.StopSystem();
            boostFX.StopSystem();

            foreach (VariableDmgFX dFX in damageFX)
                foreach (ParticleSystem fx in dFX.dmgFX)
                    fx.StopSystem();

            carModels[0].SetActive(true);

            rb.mass = 1f;
            rb.linearDamping = 0.5f;
            rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionY;

            engineSFX.enabled = true;
            deathFX.SetActive(false);
            deadCarModel.SetActive(false);
        }
        #endregion

        #region FX
        private void HandleVFX()
        {
            foreach(Transform wheels in wheelSets)
            {
                wheels.Rotate(Vector3.right, (rb.linearVelocity.magnitude * Time.deltaTime / wheelDiameter * 3.14f) * 360f, Space.Self);
            }

            switch (GameManager.gameState)
            {
                case GameState.GameOver:
                    for (int i = 0; i < tireSmokeFX.Length; i++)
                    {
                        ParticleSystem.ForceOverLifetimeModule forceModule = tireSmokeFX[i].forceOverLifetime;
                        tireSmokeFX[i].StopSystem();
                        forceModule.z = 0f;
                    }

                    exhaustFX.StopSystem();
                    boostFX.StopSystem();

                    foreach (ParticleSystem fx in sparkFX) fx.StopSystem();

                break;
                case GameState.InGame:
                case GameState.InMenu:
                    foreach (ParticleSystem tireSmoke in tireSmokeFX)
                    {
                        ParticleSystem.VelocityOverLifetimeModule velModule = tireSmoke.velocityOverLifetime;

                        tireSmoke.PlaySystem();
                        velModule.z = rb.linearVelocity.z * 0.85f / tireSmoke.main.simulationSpeed;
                        velModule.x = rb.linearVelocity.x * 0.85f / tireSmoke.main.simulationSpeed;
                    }

                    if (boost)
                    {
                        exhaustFX.StopSystem();
                        boostFX.PlaySystem();
                    }
                    else
                    {
                        exhaustFX.PlaySystem();
                        boostFX.StopSystem();
                    }

                    // SPECTER SHOULD IGNORE SPARKS //
                    /*if(ability.abilityName == "Specter")
                    {
                        if (abilityState != AbilityState.Active)
                        {
                            if (Physics.Raycast(sparkFX[0].transform.position, transform.right, data.scrapeRayRange, sparkLayerMask))
                            {
                                sparkFX[0].PlaySystem();
                            }
                            else { sparkFX[0].StopSystem(); }

                            if (Physics.Raycast(sparkFX[1].transform.position, -transform.right, data.scrapeRayRange, sparkLayerMask))
                            {
                                sparkFX[1].PlaySystem();
                            }
                            else { sparkFX[1].StopSystem(); }
                        }
                        else
                        {
                            foreach (ParticleSystem ps in sparkFX)
                                ps.StopSystem();
                        }
                    }
                    else
                    {
                        if (Physics.Raycast(sparkFX[0].transform.position, transform.right, data.scrapeRayRange, sparkLayerMask))
                        {
                            sparkFX[0].PlaySystem();
                        }
                        else { sparkFX[0].StopSystem(); }

                        if (Physics.Raycast(sparkFX[1].transform.position, -transform.right, data.scrapeRayRange, sparkLayerMask))
                        {
                            sparkFX[1].PlaySystem();
                        }
                        else { sparkFX[1].StopSystem(); }
                    }*/

                    break;
            }

            foreach (VariableDmgFX dFX in damageFX)
            {
                foreach (ParticleSystem fx in dFX.dmgFX)
                {
                    ParticleSystem.VelocityOverLifetimeModule velModule = fx.velocityOverLifetime;
                    velModule.z = rb.linearVelocity.z * 0.85f / fx.main.simulationSpeed;
                    velModule.x = rb.linearVelocity.x * 0.65f / fx.main.simulationSpeed;
                }
            }
        }

        private float BoostPitch, NearMissPitch;
        private void HandleSFX()
        {
            if (GameManager.gameState == GameState.GameOver) { return; }

            engineSFX.engineCurrentRPM = currentRPM;
            engineSFX.maxRPMLimit = data.autoLevelData[gearboxLevel].MaxRPM;
            engineSFX.carCurrentSpeed = rb.linearVelocity.magnitude * 2.2f;
            engineSFX.carMaxSpeed = data.autoLevelData[engineLevel].TopSpeed * 2.2f;

            NearMissPitch = Mathf.Lerp(NearMissPitch, 0f, Time.deltaTime * 1.25f);            

            if (GameManager.gameState == GameState.InMenu)
            {
                //engineSFX.pitch = data.IdlePitch;
            }
            else
            {
                if (boost)
                {
                    BoostPitch = Mathf.Lerp(BoostPitch, .3f, 4f * Time.deltaTime);
                }
                else
                {
                    BoostPitch = Mathf.Lerp(BoostPitch, 0f, 4f * Time.deltaTime);
                }

                //engineSFX.pitch = Mathf.Lerp(engineSFX.pitch,
                    //data.IdlePitch + data.enginePitchCurve.Evaluate(rb.linearVelocity.z / data.autoLevelData[engineLevel].TopSpeed) / 2f + BoostPitch + NearMissPitch, 8f * Time.deltaTime);
            }
        }

        private void PlayDMGFX(ParticleSystem[] fx)
        {
            foreach (VariableDmgFX dmg in damageFX)
                foreach (ParticleSystem particle in dmg.dmgFX)
                    particle.StopSystem();

            foreach (ParticleSystem particle2 in fx)
                particle2.PlaySystem();
        }
        #endregion

        #region Pickups
        public void HandleFixer()
        {
            currentHealth = data.autoLevelData[armorLevel].MaxHealth;

            foreach (VariableDmgFX variableDmgFX in damageFX)
                foreach (ParticleSystem fx in variableDmgFX.dmgFX)
                    fx.StopSystem();

            fixFX.PlaySystem();
        }

        public void HandlePickupBonus()
        {
            tokenFX.PlaySystem();
        }
        #endregion

        #region Abilities
        // BANE
        public void HarvesterMethod()
        {
            if (abilityState != AbilityState.Active) return;

            gameManager.AddExternalScore(500f);

            if (currentHealth >= data.autoLevelData[armorLevel].MaxHealth) return;

            currentHealth++;
            fixFX.PlaySystem();

            HandleDamage(0);
        }

        // IROCK //
        public void RockstarMethod()
        {
            RockstarRoutine();
        }
        private async void RockstarRoutine()
        {
            gameManager.AddMultiplier("Rockstar", 3f);

            while (abilityState == AbilityState.Active && GameManager.gameState == GameState.InGame)
            {
                await Task.Yield();
            }

            gameManager.RemoveMultiplier("Rockstar");
        }

        // KRONOS //
        public void TimelessMethod()
        {
            float oldX = transform.position.x;
            Vector3 oldVel = rb.linearVelocity;

            rb.linearVelocity = Vector3.zero;
            transform.SetPositionAndRotation(new Vector3(oldX, 0.02f, 600f), Quaternion.identity);
            rb.linearVelocity = oldVel;

            gameManager.HandleTimeless();
        }

        // FORZA //
        public void ForzaGrandeMethod()
        {
            ForzaGrandeRoutine();
        }
        private async void ForzaGrandeRoutine()
        {
            while (abilityState == AbilityState.Active && GameManager.gameState == GameState.InGame)
            {
                rb.mass = 50f;
                await Task.Yield();
            }

            rb.mass = 1f;
        }

        // ELEGANZA //
        public void SoluzioneEleganteMethod()
        {
            gameManager.heatLevelScoreOffset = gameManager.currentRunScore;
        }

        // RATTLE //
        public void MechanicMethod()
        {
            if (currentHealth >= data.autoLevelData[armorLevel].MaxHealth) return;

            currentHealth = data.autoLevelData[armorLevel].MaxHealth;
            fixFX.PlaySystem();
            HandleDamage(0);
        }

        // PHANTOM //
        public void SpecterMethod()
        {
            SpecterRoutine();
        }
        private async void SpecterRoutine()
        {
            Transform colliderParent = transform.GetChild(0);
            MeshCollider mainCollider = colliderParent.Find("phantom1").GetComponent<MeshCollider>();
            MeshCollider specterCollider = colliderParent.Find("SpecterCollider").GetComponent<MeshCollider>();

            List<Material> matList = new List<Material> { gameManager.scoreTable.specterMaterial, gameManager.scoreTable.godrayMaterial };
            colliderParent.Find("phantom1").GetComponent<MeshRenderer>().SetMaterials(matList);

            mainCollider.enabled = false;
            specterCollider.enabled = true;

            while (abilityState == AbilityState.Active && GameManager.gameState == GameState.InGame)
                await Task.Yield();

            matList = new List<Material> { gameManager.scoreTable.universalMaterial, gameManager.scoreTable.godrayMaterial };
            colliderParent.Find("phantom1").GetComponent<MeshRenderer>().SetMaterials(matList);

            mainCollider.enabled = true;
            specterCollider.enabled = false;
        }
        #endregion
    }

    [Serializable]
    public class VariableDmgFX
    {
        public int healthThreshold;
        public ParticleSystem[] dmgFX;
    }
}