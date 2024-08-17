using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Burst;
using UnityEngine;
using V3CTOR;
using Random = UnityEngine.Random;

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
        public UnityEngine.Rigidbody rb;

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

        [Header("SFX")]
        public AudioSource engineSFX;
        [SerializeField]
        private AudioClip abilityClip;
        #endregion

        #region Hidden References
        // HIDDEN //
        [HideInInspector]
        public int health;
        [HideInInspector]
        public int engineLevel;
        [HideInInspector]
        public int gearboxLevel;
        [HideInInspector]
        public int tiresLevel;
        [HideInInspector]
        public int armorLevel;

        private float menuTorque;
        // HIDDEN //
        #endregion

        public delegate void AutoDamaged();
        public event AutoDamaged Damaged;

        private void OnEnable()
        {
            if(ability.abilityName == "Harvester")
                gameManager.DestroyedCOPEvent += HarvesterMethod;
        }

        private void OnDestroy()
        {
            if (ability.abilityName == "Harvester")
                gameManager.DestroyedCOPEvent -= HarvesterMethod;
        }

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
            if (health == 0) { return; }

            Damageable damageable = col.collider.GetComponentInParent<Damageable>();
            if (damageable == null) { col.collider.GetComponent<Damageable>(); }
            if (damageable == null) { return; }

            if (Vector3.Dot(col.GetContact(0).otherCollider.transform.forward, transform.forward) >= 0.75f)
            {
                if (Vector3.Dot(col.GetContact(0).normal, transform.forward) >= -0.8f) return;

                gameManager.ShakeTheCam(0.4f);

                HandleDamage(damageable.DamageToPlayer());
                damageable.Damage(1);

                evaluation -= rb.linearVelocity.magnitude / data.autoLevelData[engineLevel].TopSpeed * 0.3f;
                rb.AddForce(-col.relativeVelocity * 0.3f * massDragMltplr, ForceMode.Impulse);
            }
            else if (Vector3.Dot(col.GetContact(0).otherCollider.transform.forward, transform.forward) <= -0.75f)
            {
                if (Vector3.Dot(col.GetContact(0).normal, transform.forward) >= -0.8f) return;

                gameManager.ShakeTheCam(0.4f);

                HandleDamage(damageable.DamageToPlayer() * 2);
                damageable.Damage(damageable.Health());

                evaluation -= rb.linearVelocity.magnitude / data.autoLevelData[engineLevel].TopSpeed * 0.3f;
                rb.AddForce(-col.relativeVelocity * 0.3f * massDragMltplr, ForceMode.Impulse);
            }
        }

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

        private bool ValidNearMiss(Collider col)
        {
            NearMissable missable = col.GetComponentInParent<NearMissable>();

            // FILTER OUT GUARDRAILS AND THE ROAD & WALL COLLIDERS //
            if (!col.GetComponent<GuardRail>() && col.gameObject.layer != 7 && col.gameObject.layer != 8 && missable != null)
            {
                return true;
            }

            return false;
        }

        #region Driving
        private float engineTorque;
        private float evaluation;
        [HideInInspector]
        public float targetTorque;
        [HideInInspector]
        public bool boost;
        private float boostMltplr;
        private float massDragMltplr;

        private void AutoMath()
        {
            if (GameManager.gameState == GameState.InGame)
            {
                evaluation += Time.deltaTime / data.autoLevelData[gearboxLevel].Acceleration;
                evaluation = Mathf.Clamp01(evaluation);
            }

            if (GameManager.gameState == GameState.InMenu)
            {
                evaluation = menuTorque / data.autoLevelData[engineLevel].TopSpeed;
            }

            if (GameManager.gameState == GameState.GameOver)
            {
                evaluation = 0f;
            }

            massDragMltplr = rb.linearDamping * rb.mass;

            if (boost) { boostMltplr = Mathf.Lerp(boostMltplr, 2f, 6f * Time.deltaTime); }
            else { boostMltplr = Mathf.Lerp(boostMltplr, 1f, 12f * Time.deltaTime); }

            engineTorque = data.enginePitchCurve.Evaluate(evaluation) * boostMltplr;

            targetTorque = engineTorque * data.autoLevelData[engineLevel].TopSpeed * massDragMltplr;
        }

        private void AutoMovement()
        {
            rb.linearVelocity = Vector3.ClampMagnitude(rb.linearVelocity, data.autoLevelData[engineLevel].TopSpeed * 2f);

            if (GameManager.gameState == GameState.GameOver) { return; }

            if (GameManager.gameState != GameState.InGame)
            {
                rb.AddForce(Vector3.forward * menuTorque * massDragMltplr, ForceMode.Force);
                rb.rotation = Quaternion.Euler(0f, 0f, 0f);
                return;
            }

            rb.AddForce(Vector3.forward * targetTorque, ForceMode.Force);

            if (GameManager.gameState == GameState.InMenu) { return; }

            if (input.xTouch != 0f)
            {
                rb.AddForce(Vector3.right * input.xTouch * data.autoLevelData[tiresLevel].TurnSpeed * massDragMltplr, ForceMode.Force);
            }
            if (input.xTouch == 0f)
            {
                rb.AddForce(-Vector3.right * rb.linearVelocity.x * 3f * massDragMltplr, ForceMode.Force);
            }

            rb.rotation = Quaternion.Euler(0f, 6f * input.xTouchLerp, 0f);
        }

        public void HandleActivateAbility()
        {
            if (abilityState != AbilityState.Ready || GameManager.gameState != GameState.InGame) return;

            abilityState = AbilityState.Active;
            ability.ActivateAbility(this);

            foreach (ParticleSystem ps in abilityFX)
                ps.PlaySystem();

            engineSFX.PlayOneShot(abilityClip);

            gameManager.ShakeTheCam(1f);
        }

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
        public void HandleDamage(int dmg)
        {
            if (health <= 0) return;

            Damaged?.Invoke();

            health -= dmg;

            // PLAY THE DAMAGE FX //
            if (health >= data.autoLevelData[armorLevel].MaxHealth)
            {
                foreach (VariableDmgFX variableDmgFX in damageFX)
                    foreach (ParticleSystem fx in variableDmgFX.dmgFX)
                        fx.StopSystem();
            }
            else
            {
                foreach (VariableDmgFX variableDmgFX in damageFX)
                    if (health <= variableDmgFX.healthThreshold)
                        PlayDMGFX(variableDmgFX.dmgFX);
            }

            if (health <= 0 && GameManager.gameState != GameState.GameOver) { HandleGameOver(); }
        }

        private void HandleGameOver()
        {
            gameManager.GameOver();

            engineSFX.enabled = false;

            carModels[0].SetActive(false);

            rb.mass = 2.5f;
            rb.linearDamping = 0.25f;
            rb.constraints = RigidbodyConstraints.None;

            Vector3 explosionTorqueVector =
                transform.right * Random.Range(-15f, 15f) +
                transform.up * Random.Range(-5f, 5f) +
                transform.forward * Random.Range(-25f, 25f);

            rb.AddForce(Vector3.up * .2f, ForceMode.VelocityChange);
            rb.AddTorque(explosionTorqueVector, ForceMode.VelocityChange);

            deadCarModel.SetActive(true);
            deathFX.SetActive(true);
        }

        public void FixAuto()
        {
            boost = false;
            health = data.autoLevelData[armorLevel].MaxHealth;
            menuTorque = data.autoLevelData[engineLevel].TopSpeed / 2f;

            rb.linearVelocity = Vector3.zero;
            rb.AddForce(transform.forward * menuTorque, ForceMode.VelocityChange);

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
                    if(ability.abilityName == "Specter")
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
                    }

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

            NearMissPitch = Mathf.Lerp(NearMissPitch, 0f, Time.deltaTime * 1.25f);

            if (GameManager.gameState == GameState.InMenu)
            {
                engineSFX.pitch = data.IdlePitch;
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

                engineSFX.pitch = Mathf.Lerp(engineSFX.pitch,
                    data.IdlePitch + data.enginePitchCurve.Evaluate(rb.linearVelocity.z / data.autoLevelData[engineLevel].TopSpeed) / 2f + BoostPitch + NearMissPitch, 8f * Time.deltaTime);
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
            health = data.autoLevelData[armorLevel].MaxHealth;

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

            if (health >= data.autoLevelData[armorLevel].MaxHealth) return;

            health++;
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
            if (health >= data.autoLevelData[armorLevel].MaxHealth) return;

            health = data.autoLevelData[armorLevel].MaxHealth;
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