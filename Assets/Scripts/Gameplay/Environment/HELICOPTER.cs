using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using V3CTOR;

namespace RetroCode
{
    public class HELICOPTER : MonoBehaviour
    {
        #region Visible References
        [Header("Main")]
        [SerializeField]
        private GameManager gameManager;
        [SerializeField]
        private SpawnManager spawnManager;
        public HeliState heliState;

        [Header("Positions")]
        public Vector3 offsetFromPlayer;
        public Vector3 hidePosition;

        #region Values
        [Header("Values")]
        [SerializeField]
        private float moveSpeed;
        [SerializeField]
        private float rotateSpeed;
        [SerializeField]
        private float mainRotorRPS;
        [SerializeField]
        private float tailRotorRPS;
        [SerializeField]
        private float maxSwayAmount;
        [SerializeField]
        private float swaySpeed;
        [SerializeField]
        private float fireRate = 2f;
        [SerializeField]
        private float fireSpread;
        [SerializeField]
        private float minBurstDelay = 2f;
        [SerializeField]
        private float maxBurstDelay = 5f;
        [SerializeField]
        private int minShotsPerBurst = 3;
        [SerializeField]
        private int maxShotsPerBurst = 6;
        #endregion

        #region Turret
        [Header("Turret")]
        [SerializeField]
        private Transform turretX;
        [SerializeField]
        private Transform turretY;
        [SerializeField]
        private LayerMask layerMask;
        #endregion

        [Header("Rotors")]
        [SerializeField]
        private Transform mainRotor;
        [SerializeField]
        private Transform tailRotor;

        [Header("VFX")]
        [SerializeField]
        private List<GameObject> tracerPool = new List<GameObject>();
        [SerializeField]
        private List<GameObject> hitEffectPool = new List<GameObject>();

        [Header("SFX")]
        [SerializeField]
        private AudioSource turretSourceFX;
        [SerializeField]
        private List<AudioClip> shootFX;
        #endregion

        // HIDDEN VARIABLE //
        private List<GameObject> activeTracers = new List<GameObject>();

        private void OnEnable()
        {
            transform.position = gameManager.playerCar.transform.position + hidePosition;
            heliState = HeliState.FollowPlayer;

            readyToShoot = true;
            currentBurstShotCount = Random.Range(minShotsPerBurst, maxShotsPerBurst);
        }

        private void Update()
        {
            HandleMovement();
            HandleTurretRotation();
            HandleBlades();
            HandleTurretState();
        }

        #region Movement
        private Vector3 targetPos, movementSway, dirVec3;
        private float moveSwaySpeed = 1.25f, moveSwayAmp = 1.5f;
        private void HandleMovement()
        {
            dirVec3 = targetPos - transform.position;
            Quaternion targetRot = Quaternion.LookRotation(dirVec3, Vector3.up);

            movementSway = new Vector3(Mathf.Cos(Time.time * moveSwaySpeed) * moveSwayAmp, Mathf.Sin(Time.time * moveSwaySpeed) * moveSwayAmp, 0f);

            switch (heliState)
            {
                case HeliState.FollowPlayer:
                    targetPos = gameManager.playerCar.transform.position + offsetFromPlayer + movementSway;
                    moveSpeed = gameManager.playerCar.data.autoLevelData[gameManager.playerCar.engineLevel].TopSpeed * 1.3f;

                    transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, rotateSpeed * Time.deltaTime);
                    break;
                case HeliState.Hide:
                    targetPos = gameManager.playerCar.transform.position + hidePosition + movementSway;
                    moveSpeed = gameManager.playerCar.data.autoLevelData[gameManager.playerCar.engineLevel].TopSpeed * 0.5f;

                    transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, rotateSpeed * Time.deltaTime * 0.25f);
                    break;
            }

            transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
        }

        private float moveTimer;
        public async void HideAway()
        {
            moveTimer = 0f;
            heliState = HeliState.Hide;

            while(moveTimer < 5f)
            {
                moveTimer += Time.deltaTime;

                await Task.Yield();
            }

            transform.position = targetPos;
            EXMET.RemoveSpawnable(gameObject, spawnManager.activeHelis, spawnManager.HeliPool);
        }
        #endregion

        #region Turret & Firing
        private Vector3 directionToPlayerY, directionToPlayerX;
        private Quaternion turretTargetX, turretTargetY;
        private void HandleTurretRotation()
        {
            Transform player = gameManager.playerTransform;           

            switch (heliState)
            {
                case HeliState.FollowPlayer:
                    if(Vector3.Distance(transform.position, player.position) <= 150f)
                    {
                        directionToPlayerX = transform.InverseTransformDirection(player.position - turretX.position);
                        Quaternion xRotation = Quaternion.LookRotation(directionToPlayerX);

                        directionToPlayerY = transform.InverseTransformDirection(player.position - turretY.position);
                        Quaternion yRotation = Quaternion.LookRotation(directionToPlayerY);

                        turretTargetX = ApplySway(Quaternion.Euler(xRotation.eulerAngles.x, 0f, 0f));
                        turretTargetY = ApplySway(Quaternion.Euler(0f, yRotation.eulerAngles.y, 0f));

                        turretX.localRotation = Quaternion.Lerp(turretX.localRotation, turretTargetX, 15f * Time.deltaTime);
                        turretY.localRotation = Quaternion.Lerp(turretY.localRotation, turretTargetY, 15f * Time.deltaTime);
                    }
                    else
                    {
                        turretTargetX = ApplySway(Quaternion.Euler(35f, 0f, 0f));
                        turretTargetY = ApplySway(Quaternion.Euler(0f, 0f, 0f));

                        turretX.localRotation = Quaternion.Lerp(turretX.localRotation, turretTargetX, 15f * Time.deltaTime);
                        turretY.localRotation = Quaternion.Lerp(turretY.localRotation, turretTargetY, 15f * Time.deltaTime);
                    }
                    break;
                case HeliState.Hide:
                    turretTargetX = ApplySway(Quaternion.Euler(35f, 0f, 0f));
                    turretTargetY = ApplySway(Quaternion.Euler(0f, 0f, 0f));

                    turretX.localRotation = Quaternion.Lerp(turretX.localRotation, turretTargetX, 15f * Time.deltaTime);
                    turretY.localRotation = Quaternion.Lerp(turretY.localRotation, turretTargetY, 15f * Time.deltaTime);
                    break;
            }
        }

        Quaternion ApplySway(Quaternion inputRotation)
        {
            float swayX = Mathf.Sin(Time.time * swaySpeed) * maxSwayAmount;
            float swayY = Mathf.Cos(Time.time * swaySpeed) * maxSwayAmount;

            return inputRotation * Quaternion.Euler(swayX, swayY, 0f);
        }

        #region Firing
        private bool shooting, readyToShoot;
        private int currentBurstShotCount;
        private void HandleTurretState()
        {
            shooting = heliState == 
                HeliState.FollowPlayer && 
                Vector3.Distance(transform.position, gameManager.playerTransform.position) <= 150f &&
                GameManager.gameState == GameState.InGame;

            if(readyToShoot && shooting && currentBurstShotCount > 0)
            {
                HandleTurretFire();
                Invoke(nameof(ResetTurretFire), Random.Range(minBurstDelay, maxBurstDelay));
            }
        }

        private void HandleTurretFire()
        {
            readyToShoot = false;

            Vector3 fireDirection = (gameManager.playerTransform.position + new Vector3(0f, 0f, 15f) - turretX.position).normalized;

            if (Physics.Raycast(turretX.position, fireDirection + RandomSpread(), out RaycastHit hit, 600f, layerMask))
            {
                gameManager.ShakeTheCam(0.3f);

                GameObject tracer = tracerPool[0];
                HandleTrail(tracer.GetComponent<TrailRenderer>(), hit.point);

                for (int i = 0; i < hitEffectPool.Count; i++)
                    if (!hitEffectPool[i].activeInHierarchy)
                    {
                        GameObject chosen1 = hitEffectPool[i];

                        chosen1.transform.position = hit.point;
                        chosen1.SetActive(true);
                        break;
                    }

                Damageable damageable = hit.collider.GetComponentInParent<Damageable>();
                if (damageable == null) hit.collider.GetComponent<Damageable>();
                if (damageable != null) 
                {
                    damageable.Damage(damageable.Health());
                }

                AutoMobile autoMobile = hit.collider.GetComponentInParent<AutoMobile>();
                if (autoMobile != null) autoMobile.HandleDamage(1);

                AudioClip chosenOne = shootFX[Random.Range(0, shootFX.Count)];
                turretSourceFX.PlayOneShot(chosenOne);
            }

            currentBurstShotCount--;

            if(currentBurstShotCount > 0)
                Invoke(nameof(HandleTurretFire), fireRate / 60f);
        }

        private void ResetTurretFire()
        {
            currentBurstShotCount = Random.Range(minShotsPerBurst, maxShotsPerBurst);
            readyToShoot = true;
        }

        private async void HandleTrail(TrailRenderer tracerTrail, Vector3 hitTarget)
        {
            EXMET.AddSpawnable(tracerTrail.gameObject, activeTracers, tracerPool);

            float time = 0f;

            Vector3 startPos = tracerTrail.transform.localPosition;

            while (time < 1f)
            {
                tracerTrail.transform.localPosition = Vector3.Lerp(startPos, turretX.InverseTransformPoint(hitTarget), time);
                time += Time.deltaTime / tracerTrail.time;

                await Task.Yield();
            }

            tracerTrail.transform.position = turretX.InverseTransformPoint(hitTarget);
            EXMET.RemoveSpawnable(tracerTrail.gameObject, activeTracers, tracerPool);
            tracerTrail.transform.localPosition = Vector3.zero;
        }

        private Vector3 RandomSpread()
        {
            return new Vector3(
                Random.Range(-fireSpread, fireSpread),
                Random.Range(-fireSpread, fireSpread),
                0f
                );
        }
        #endregion

        #endregion

        private void HandleBlades()
        {
            mainRotor.Rotate(Vector3.up, 360f * mainRotorRPS * Time.deltaTime);
            tailRotor.Rotate(Vector3.right, 360f * tailRotorRPS * Time.deltaTime);
        }
    }

    public enum HeliState
    {
        FollowPlayer,
        Hide,
    }
}
