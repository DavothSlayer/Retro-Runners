using System.Collections.Generic;
using UnityEngine;
using System;
using Random = UnityEngine.Random;
using System.Threading.Tasks;
using Unity.Burst;
using V3CTOR;
using UnityEngine.Events;

namespace RetroCode
{
    public class GameManager : MonoBehaviour
    {
        #region Inspector Variables
        [SerializeField]
        private MainGameHUD hud;
        [SerializeField]
        private SpawnManager spawnManager;
        [SerializeField]
        private InputManager input;

        [Header("UGS")]
        [SerializeField]
        private UGSM gamingServicesManager;

        [Header("Game")]       
        public ScoreTable scoreTable;
        public HeatVariation[] heatVariations;
        [SerializeField]
        private AudioSource mainAudioSource;
        [SerializeField]
        private AudioSource nearMissSource;
        [SerializeField]
        private GameObject[] allAutos;
        [SerializeField]
        private ParticleSystem speedLinesFX;
        [Space]
        public Transform playerTransform;
        public AutoMobile playerCar;
        [SerializeField]
        private float playerMaxPower;

        [Header("Cinematics")]
        [SerializeField]
        private Camera cam;
        [Space]
        [SerializeField]
        private Vector3 gameCamPos;
        [SerializeField]
        private Quaternion gameCamRot;
        [SerializeField]
        private float gameCamFOV;
        [Space]
        [SerializeField]
        private Vector3 gameoverCamPos;
        [SerializeField]
        private Quaternion gameoverCamRot;
        [SerializeField]
        private float gameoverCamFOV;
        [Space]
        [SerializeField]
        private Transform camHolder;
        [SerializeField]
        private Canvas canvas;

        [Header("SFX")]
        [SerializeField]
        private AudioClip nearMissSFX;
        [SerializeField]
        private AudioClip boostPickupSFX;
        [SerializeField]
        private AudioClip fixerPickupSFX;
        [SerializeField]
        private AudioClip tokenPickupSFX;
        [SerializeField]
        private AudioClip heatLevelSFX;
        [SerializeField]
        private AudioClip abilityReadyClip;
        #endregion

        #region Inspector Events
        [Space]
        public UnityEvent GameOverEvent;
        public UnityEvent ScoreAdded;
        public UnityEvent ScoreMultiplierDelta;
        public UnityEvent NearMissed;
        public UnityEvent NearMissEnd;
        #endregion

        #region Hidden Variables
        public static GameState gameState;
        [HideInInspector]
        public bool gamePaused = false;

        [HideInInspector]
        public float currentRunScore;
        private int currentRunCOPsDestroyed;
        private int currentRunNMH;
        private int currentRunReward;

        private int cloudHighScore;
        private int cloudNMH;
        private int cloudCDR;        

        [HideInInspector]
        public int activeHeatLevel;
        [HideInInspector]
        public bool heat;
        [HideInInspector]
        public float heatLevelScoreOffset;

        private float playerCurrentPower;
        #endregion

        #region Events
        public delegate void HeatLevelChangeDelegate(int oldValue, int newValue);
        public event HeatLevelChangeDelegate heatLevelChanged;
        public int ActiveHeatLevel
        {
            get { return activeHeatLevel; }
            set
            {
                if (value != activeHeatLevel)
                {
                    // TRIGGER THE EVENT WHEN THE VALUE CHANGES //
                    heatLevelChanged?.Invoke(activeHeatLevel, value);

                    activeHeatLevel = value;
                }
            }
        }

        public delegate void DestroyedCOPDelegate();
        public event DestroyedCOPDelegate DestroyedCOPEvent;

        public Dictionary<string, float> activeScoreMultipliers = new Dictionary<string, float>();
        private float gameScoreMultiplier
        {
            get
            {
                float result = 1f;
                foreach (float multiplier in activeScoreMultipliers.Values)
                {
                    result *= multiplier;
                }
                return result;
            }
        }
        #endregion

        public static AutoMobile playerAutoStatic;

        private void OnEnable()
        {
            heatLevelChanged += HeatLevelChangeListener;
            InputManager.UpperDoubleTapped += AutoAbility;
        }

        private void OnDestroy()
        {
            playerAutoStatic = playerCar;

            heatLevelChanged -= HeatLevelChangeListener;
            InputManager.UpperDoubleTapped -= AutoAbility;
            playerCar.Damaged -= HandleAutoDamage;            
        }

        private void Awake()
        {
            Application.targetFrameRate = 60;

            UpdateGameScreen();
        }

        [BurstCompile]
        private void Update()
        {        
            CountScore();
            HeatLevelLogic();
            HandlePlayerPower();
            GameHUD();

            SwitchCaseMachine();
            CameraShaker();
        }

        [BurstCompile]
        private void GameHUD()
        {
            if (playerCar == null) return;

            #region HUD Text and Animator Booleans
            bool isKMH = SettingsManager.Instance.settings.SpeedUnitIsKMH;
            float speedConvertMultiplier = isKMH ? 3.6f : 2.2f;
            hud.speedoMeterText.text = $"{Mathf.RoundToInt(playerCar.rb.linearVelocity.magnitude * speedConvertMultiplier)} {(isKMH ? "KMH" : "MPH")}";
            hud.speedoMeterFill.fillAmount = playerCar.rb.linearVelocity.magnitude / playerCar.data.autoLevelData[playerCar.engineLevel].TopSpeed;

            hud.nearMissComboTimer.fillAmount = 1f - nearMissTimer / nearMissMaxTime;
            int score = Mathf.RoundToInt(currentRunScore);
            hud.scoreText.text = score.ToString(EXMET.NumForThou);

            //canvasAnimator.SetBool("ScoreXd", gameScoreMultiplier != 1f && gameState == GameState.InGame);
            //canvasAnimator.SetBool("ShowHeatLevel", ActiveHeatLevel > 0 && gameState == GameState.InGame);
            //canvasAnimator.SetBool("Near Miss", midCombo && gameState == GameState.InGame);
            #endregion

            #region Health & Power Bars
            hud.healthBarSlider.value = Mathf.Lerp(
                hud.healthBarSlider.value, ((float)playerCar.health / (float)playerCar.data.autoLevelData[playerCar.armorLevel].MaxHealth),
                Time.deltaTime * 5f
                );

            hud.powerBarSlider.value = Mathf.Lerp(
                hud.powerBarSlider.value, playerCurrentPower / playerMaxPower,
                Time.deltaTime * 5f
                );
            #endregion

            #region Pickup Markers
            /*for (int i = 0; i < spawnManager.PickupList.Count; i++)
            {
                GameObject pickup = spawnManager.PickupList[i];             
                MarkerPoint marker = hud.markerPoints[i];

                // GET THE ICON //
                PickupType pickupType = pickup.GetComponent<Pickup>().GetPickupType();
                CanvasGroup canvasG = marker.markerObject.GetComponent<CanvasGroup>();
                int index = (int)pickupType;
                Sprite icon = hud.pickUpIcons[index];

                // CALCULATE THE DISTANCE //
                float distance = Vector3.Distance(playerTransform.position, pickup.transform.position);

                // FIND MARKER WORLD POSITION ON THE SCREEN //
                RectTransform parentRect = (RectTransform)marker.markerRect.parent;              
                Vector3 viewportPoint = cam.WorldToViewportPoint(pickup.transform.position + new Vector3(0f, 20f, 0f));
                Vector3 screenPoint = canvas.worldCamera.ViewportToScreenPoint(viewportPoint);                
                RectTransformUtility.ScreenPointToWorldPointInRectangle(parentRect, screenPoint, canvas.worldCamera, out Vector3 worldPoint);
                marker.markerRect.position = worldPoint;

                // ENABLE MARKER OBJECT ACCORDING TO VARIABLES //
                marker.markerObject.SetActive(
                    pickup.activeInHierarchy 
                    && gameState == GameState.InGame 
                    && playerTransform.position.z < pickup.transform.position.z
                    );

                // SETTING THE VALUES //
                marker.icon.sprite = icon;
                marker.distance.text = $"{Mathf.RoundToInt(distance)}M";

                // PICK UP SPAWNS 1400M AWAY FROM PLAYER //
                marker.markerRect.localScale = Vector3.one * hud.markerUIScaleCurve.Evaluate(1f - distance / 400f);
                canvasG.alpha = hud.markerUIAlphaCurve.Evaluate(1f - distance / 400f) - (-Vector3.Dot(cam.transform.forward, pickup.transform.forward));
            }*/
            #endregion
        }

        #region Buttons
        private bool rearview = false;

        public void HandlePlay()
        {
            gameState = GameState.InGame;

            UpdateGameScreen();

            rearview = false;
            playerCar.abilityTimer = 0f;
            playerCar.abilityCooldownTimer = playerCar.ability.cooldownTime;
            playerCar.abilityState = AbilityState.Ready;

            //canvasAnimator.SetBool("Game Over Reward Ads", Random.Range(0f, 1f) >= 0.5f);
        }

        private float timeScale = 1f;
        public void PauseButton()
        {
            if (gameState != GameState.InGame) return;

            gamePaused = !gamePaused;

            timeScale = gamePaused ? 0f : 1f;

            LeanTween.value(Time.timeScale, timeScale, 0.6f).setIgnoreTimeScale(true).setOnUpdate((float value) =>
            {
                Time.timeScale = value;
            });
        }

        public void ReturnToMainMenu()
        {
            ResetGame();
            spawnManager.ResetGame();

            gameState = GameState.InMenu;
            UpdateGameScreen();
        }

        public void HandleDoubleRewards()
        {
            //canvasAnimator.SetBool("Game Over Reward Ads", false);

            int cloudDollars = gamingServicesManager.cloudData.RetroDollars;
            cloudDollars += currentRunReward;
            gamingServicesManager.cloudData.RetroDollars = cloudDollars;

            // DOUBLE THE REWARD, DISPLAY IT //
            currentRunReward *= 2;
            hud.earningsText.text = $"${currentRunReward.ToString("n0")}  EARNED";

            gamingServicesManager.SaveCloudData(false);
        }

        public void HandleRevive()
        {
            //canvasAnimator.SetBool("Game Over Reward Ads", false);

            playerCar.rb.linearVelocity = Vector3.zero;
            playerTransform.SetPositionAndRotation(new Vector3(3f, 0.02f, 600f), Quaternion.identity);

            playerCar.FixAuto();

            gameState = GameState.InGame;
            UpdateGameScreen();

            print("HandleRevive");
        }

        public void NoThanksButton()
        {
            //canvasAnimator.SetBool("Game Over Reward Ads", false);
        }
        #endregion

        #region Gameplay
        private void HandlePlayerPower()
        {
            if (gameState != GameState.InGame) return;

            if (playerCurrentPower > 0f)
                playerCurrentPower -= 5f * Time.deltaTime;
        }

        [BurstCompile]
        private void ResetGame()
        {
            // RESET AUTO //
            ReplaceAuto();
            playerAutoStatic = playerCar;

            // RESET GAME VALUES //
            AddMultiplier("ThrillSeeker", 1f);
            Time.timeScale = 1f;
            currentRunScore = 0;
            currentRunNMH = 0;
            currentRunCOPsDestroyed = 0;
            heatLevelScoreOffset = 0f;

            // APPLY CLOUD DATA //
            ApplyLoadedData();
        }

        [BurstCompile]
        private void ReplaceAuto()
        {
            if (playerCar != null)
            {
                GameObject currentCar = playerCar.gameObject;
                currentCar.SetActive(false);
            }

            int cloudAutoInt = gamingServicesManager.cloudData.LastSelectedCarIndex;
            GameObject newAuto = allAutos[cloudAutoInt];

            // SET PLAYER SPAWN TO ZSPAWN + 600f //
            playerCar = newAuto.GetComponent<AutoMobile>();
            playerTransform = newAuto.transform;
            playerTransform.SetPositionAndRotation(new Vector3(3f, 0.02f, 600f), Quaternion.identity);
            newAuto.SetActive(true);

            playerCar.engineLevel = gamingServicesManager.cloudData.inventoryDict[playerCar.data.ItemCode]["engine"].CurrentLevel;
            playerCar.powerLevel = gamingServicesManager.cloudData.inventoryDict[playerCar.data.ItemCode]["power"].CurrentLevel;
            playerCar.tiresLevel = gamingServicesManager.cloudData.inventoryDict[playerCar.data.ItemCode]["handling"].CurrentLevel;
            playerCar.armorLevel = gamingServicesManager.cloudData.inventoryDict[playerCar.data.ItemCode]["health"].CurrentLevel;
            playerCurrentPower = playerMaxPower;
            playerCar.FixAuto();

            playerCar.Damaged += HandleAutoDamage;
        }

        private void HandleAutoDamage()
        {
            // DO SMTH?? //            
        }

        // PRIVATE VARIABLES //
        private float nearMissMaxTime = 2f;
        private float nearMissTimer;
        private int nearMissComboCount;
        private bool midCombo;
        private float boostDuration = 4f;
        private float boostTimer;
        // PRIVATE VARIABLES //

        // NEAR MISS //
        [BurstCompile]
        public void NearMiss()
        {
            nearMissTimer = 0f;
            nearMissComboCount++;
            NearMissed?.Invoke();

            AddExternalScore(scoreTable.scorePerNearMiss * nearMissComboCount * gameScoreMultiplier);

            hud.nearMissText.text = $"NEAR  MISS {nearMissComboCount}X";

            if (!midCombo) { NearMissTimer(); }

            nearMissSource.pitch = 0.8f + 0.05f * nearMissComboCount;
            nearMissSource.pitch = Mathf.Clamp(nearMissSource.pitch, 0.8f, 1.4f);

            nearMissSource.PlayOneShot(nearMissSFX);

            playerCurrentPower += 7.5f;
        }

        [BurstCompile]
        public async void NearMissTimer()
        {
            midCombo = true;

            while (nearMissMaxTime > nearMissTimer)
            {
                nearMissTimer += Time.deltaTime;

                await Task.Yield();
            }

            if (nearMissComboCount > currentRunNMH)
                currentRunNMH = nearMissComboCount;

            if (currentRunNMH > cloudNMH)
                gamingServicesManager.cloudData.HighestNearMissCount = currentRunNMH;

            NearMissEnd?.Invoke();

            nearMissComboCount = 0;
            midCombo = false;
        }
        // NEAR MISS //

        // SCORE //
        private void CountScore()
        {
            if (gameState != GameState.InGame) { return; }

            if (playerTransform.position.x <= -0.5f)
                AddMultiplier("ThrillSeeker", scoreTable.thrillSeekerMultiplier);
            else
                AddMultiplier("ThrillSeeker", 1f);

            currentRunScore += Time.deltaTime * scoreTable.scorePerSecond * gameScoreMultiplier;

            int score = Mathf.RoundToInt(currentRunScore);

            if (score > cloudHighScore)
                gamingServicesManager.cloudData.HighScore = score;

            currentRunReward = Mathf.RoundToInt(score * scoreTable.rewardRatio);
        }

        public void AddMultiplier(string source, float newMultiplier)
        {
            if (activeScoreMultipliers.ContainsKey(source))
            {
                // UPDATE THE EXISTING MULTIPLIER //
                if (activeScoreMultipliers[source] != newMultiplier)
                {
                    activeScoreMultipliers[source] = newMultiplier;

                    hud.scoreMultiplierText.text = $"{gameScoreMultiplier}X";
                    ScoreMultiplierDelta?.Invoke();
                }
            }
            else
            {
                // ADD A NEW MULTIPLIER //
                activeScoreMultipliers.Add(source, newMultiplier);

                hud.scoreMultiplierText.text = $"{gameScoreMultiplier}X";
                ScoreMultiplierDelta?.Invoke();
            }
        }

        public void RemoveMultiplier(string source)
        {
            activeScoreMultipliers.Remove(source);

            ScoreMultiplierDelta?.Invoke();
        }

        public void AddExternalScore(float amount)
        {
            ScoreAdded?.Invoke();

            currentRunScore += amount;
            hud.scoreEventText.text = $"+ {amount.ToString(EXMET.NumForThou)}";
        }
        // SCORE //

        public void HandlePickup(PickupType pickupType)
        {
            if (gameState == GameState.GameOver) return;

            switch (pickupType)
            {
                case PickupType.PickupBonus:
                    PickupBonusMethod();
                    PlayAudioOneShot(tokenPickupSFX);
                    break;
                case PickupType.Boost:
                    BoostMethod();
                    PlayAudioOneShot(boostPickupSFX);
                    break;
                case PickupType.Fixer:
                    FixerMethod();
                    PlayAudioOneShot(fixerPickupSFX);
                    break;
            }
        }

        // BOOST METHOD //
        private async void BoostMethod()
        {
            AddExternalScore(100f);
            boostTimer = 0;
            playerCar.boost = true;
            ShakeTheCam(2.5f);

            while(boostTimer < boostDuration)
            {
                boostTimer += Time.deltaTime;
                await Task.Yield();
            }

            playerCar.boost = false;
        }

        // FIXER METHOD //
        private void FixerMethod()
        {
            AddExternalScore(50f);

            playerCar.HandleFixer();
        }

        // TOKEN METHOD //
        private void PickupBonusMethod()
        {
            AddExternalScore(scoreTable.pickUpBonus);

            playerCar.HandlePickupBonus();
        }

        // HEAT LEVEL LOGIC //
        private void HeatLevelLogic()
        {
            if (currentRunScore < heatVariations[0].startsFromScore + heatLevelScoreOffset || gameState != GameState.InGame)
            {
                heat = false;
                ActiveHeatLevel = 0;
                return;
            }

            heat = true;
             
            for(int i = 0; i < heatVariations.Length - 1; i++)
            {
                if (currentRunScore >= heatVariations[heatVariations.Length - 1].startsFromScore)
                {
                    ActiveHeatLevel = heatVariations.Length;
                    return;
                }
                else
                {
                    if (currentRunScore >= heatVariations[i].startsFromScore + heatLevelScoreOffset && currentRunScore < heatVariations[i + 1].startsFromScore + heatLevelScoreOffset)
                    {
                        ActiveHeatLevel = i + 1;
                    }
                }
            }
        }

        private void HeatLevelChangeListener(int oldHeatLevel, int newHeatLevel)
        {
            if (newHeatLevel > oldHeatLevel)
            {
                PlayAudioOneShot(heatLevelSFX);
            }

            if (heat == false)
            {
                AddMultiplier("Heat Level", 1f);
            }
            else
            {
                AddMultiplier("Heat Level", newHeatLevel + 1f);

                spawnManager.HeatLevelCheck();
            }
        }
        // HEAT LEVEL LOGIC

        // DESTROYED COP EVENT //
        public void DestroyedCOP()
        {
            currentRunCOPsDestroyed++;

            if (currentRunCOPsDestroyed > cloudCDR)
                gamingServicesManager.cloudData.MostCOPsDestroyed = currentRunCOPsDestroyed;

            DestroyedCOPEvent?.Invoke();
        }
        // DESTROYED COP EVENT //

        // AUTO ABILITIES //
        public void AutoAbility()
        {
            playerCar.HandleActivateAbility();
        }

        public void HandleAbilityReady()
        {
            mainAudioSource.PlayOneShot(abilityReadyClip);
        }

        public void HandleTimeless()
        {
            playerCar.FixAuto();

            spawnManager.KillEmAll();
            spawnManager.InitializeNPCs();
        }
        // AUTO ABILITIES //
        #endregion

        #region Game Over
        public void GameOver()
        {
            gameState = GameState.GameOver;
            
            int cloudDollars = gamingServicesManager.cloudData.RetroDollars;
            cloudDollars += currentRunReward;
            gamingServicesManager.cloudData.RetroDollars = cloudDollars;

            GameOverEvent?.Invoke();

            // DISPLAY EARNINGS AND THE SCORE //
            hud.earningsText.text = $"R$ {currentRunReward.ToString(EXMET.NumForThou)} EARNED";
            hud.finalScoreText.text = $"SCORE {currentRunScore.ToString(EXMET.NumForThou)}";
            hud.finalNMHText.text = $"HIGHEST NEAR MISS COMBO {currentRunNMH}X";
            hud.COPKillCountText.text = $"{currentRunCOPsDestroyed} COPs Destroyed!";

            UpdateGameScreen();
            gamingServicesManager.SaveCloudData(false);
        }
        #endregion

        #region Cinematics
        private Vector3 playerPos;
        private Quaternion playerRot;
        private float speedFOV;
        private Vector3 proSwayPos;
        private Vector3 proSwayRot;
        private Vector3 camPosTarget;
        private Quaternion camRotTarget;
        private void SwitchCaseMachine()
        {
            if(playerCar == null) { return; }

            playerPos = new(playerTransform.position.x, 0f, playerTransform.position.z);
            playerRot = Quaternion.identity;

            switch (gameState)
            {
                case GameState.InMenu:
                    camPosTarget = Vector3.Slerp(camPosTarget, playerCar.data.CameraPositionInMenu, 3.5f * Time.deltaTime);
                    camRotTarget = Quaternion.Slerp(camRotTarget, playerCar.data.CameraRotationInMenu, 6f * Time.deltaTime);

                    proSwayPos = new(5f * Mathf.Sin(Time.time * .1f), 5f * Mathf.Sin(Time.time * .1f), 0f);
                    proSwayRot = new(.5f * Mathf.Sin(Time.time * 3f), .5f * Mathf.Cos(Time.time * 3f), 0f);

                    camHolder.position = playerPos + camPosTarget;
                    camHolder.rotation = playerRot * camRotTarget * Quaternion.Euler(proSwayRot);

                    cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, playerCar.data.InMenuFOV, 5f * Time.deltaTime);
                    break;

                case GameState.InGame:
                    camPosTarget = Vector3.Slerp(camPosTarget, playerCar.data.CameraPositionInGame, 3.5f * Time.deltaTime);
                    camRotTarget = Quaternion.Slerp(camRotTarget, playerCar.data.CameraRotationInGame, 6f * Time.deltaTime);
                    
                    Quaternion carRot = Quaternion.Euler(0f, 5f * input.xTouchLerp, 6f * input.xTouchLerp);
                    Vector3 cameraOffset = new(1.5f * input.xTouchLerp, 0f, 0f);
                    Vector3 lowCamOffset = SettingsManager.Instance.settings.LowCam ? Vector3.zero : new(0f, 1f, 0f);

                    camHolder.position = playerPos + camPosTarget + lowCamOffset + cameraOffset;
                    camHolder.rotation = playerRot * camRotTarget * carRot;

                    speedFOV = (playerCar.rb.linearVelocity.magnitude / playerCar.data.autoLevelData[playerCar.engineLevel].TopSpeed) * 30f;

                    cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, gameCamFOV + speedFOV, 5f * Time.deltaTime);

                    hud.mainGameParent.localRotation = Quaternion.Euler(0f, 2.5f * input.xTouchLerp, -3f * input.xTouchLerp);

                    break;

                case GameState.GameOver:
                    camPosTarget = Vector3.Slerp(camPosTarget, gameoverCamPos, 0.85f * Time.deltaTime);
                    camRotTarget = Quaternion.Slerp(camRotTarget, gameoverCamRot, 8f * Time.deltaTime);

                    camHolder.position = playerPos + camPosTarget;
                    camHolder.rotation = camRotTarget;

                    cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, gameoverCamFOV, Time.deltaTime);
                    break;
            }
        }

        private float shakeDuration = 0.5f;
        private float startShakeMagnitude = 0.1f;
        private float endShakeMagnitude = 0.01f;
        private float shakeTimer = 0f;
        private void CameraShaker()
        {
            if (shakeTimer > 0)
            {
                float currentMagnitude = Mathf.Lerp(startShakeMagnitude, endShakeMagnitude, 1 - (shakeTimer / shakeDuration));
                Vector3 offset = Random.insideUnitSphere * currentMagnitude;
                cam.transform.localPosition = offset;
                shakeTimer -= Time.deltaTime;
            }
            else
            {
                cam.transform.localPosition = Vector3.Lerp(cam.transform.localPosition, Vector3.zero, Time.deltaTime * 10f);
            }
        }

        public void ShakeTheCam(float duration)
        {
            shakeDuration = duration;
            shakeTimer = shakeDuration;
        }
        #endregion

        #region Utils
        public void CloudDataReceive()
        {
            print("Cloud Data Received.");

            ResetGame();

            // THIS IS ONLY FOR LOOKING AT THE CLOUD DATA //
            /*for (int i = 0; i < allAutos.Length; i++)
            {
                print($"Contains {allAutos[i].GetComponent<AutoMobile>().data.AutoName}? {gamingServicesManager.cloudData.unlockedCarsDict.ContainsKey(allAutos[i].GetComponent<AutoMobile>().data.ItemCode)}");
                if (gamingServicesManager.cloudData.unlockedCarsDict.ContainsKey(allAutos[i].GetComponent<AutoMobile>().data.ItemCode))
                {
                    for (int j = 0; j < gamingServicesManager.cloudData.unlockedCarsDict[allAutos[i].GetComponent<AutoMobile>().data.ItemCode].compCodes.Count; j++)
                    {
                        print($"{allAutos[i].GetComponent<AutoMobile>().data.AutoName} CompCodes: {gamingServicesManager.cloudData.unlockedCarsDict[allAutos[i].GetComponent<AutoMobile>().data.ItemCode].compCodes[j]}");
                    }

                    for (int k = 0; k < gamingServicesManager.cloudData.unlockedCarsDict[allAutos[i].GetComponent<AutoMobile>().data.ItemCode].compCodes.Count; k++)
                    {
                        print($"{allAutos[i].GetComponent<AutoMobile>().data.AutoName} SelectedComp: {gamingServicesManager.cloudData.unlockedCarsDict[allAutos[i].GetComponent<AutoMobile>().data.ItemCode].lastSelectedCompList[k]}");
                    }
                }
            }*/
        }

        public void ApplyLoadedData()
        {
            cloudNMH = gamingServicesManager.cloudData.HighestNearMissCount;
            cloudHighScore = gamingServicesManager.cloudData.HighScore;
            cloudCDR = gamingServicesManager.cloudData.MostCOPsDestroyed;

            hud.highScoreText.text = $"HIGH SCORE {cloudHighScore.ToString(EXMET.NumForThou)}";
            hud.highestMissComboText.text = $"NEAR MISS COMBO RECORD {cloudNMH}X!";
            hud.COPKillCountRecord.text = $"COPS DESTROYED RECORD {cloudCDR}!";
            hud.playerMoneyText.text = $"R${gamingServicesManager.cloudData.RetroDollars.ToString(EXMET.NumForThou)}";
        }

        public void UpdateGameScreen()
        {
            //canvasAnimator.SetInteger("GameState", (int)gameState);
        }

        public void PlayAudioOneShot(AudioClip clip)
        {
            mainAudioSource.PlayOneShot(clip);
        }
        #endregion

        #region Spawn Positions
        public static float[] xSpawnPositions()
        {
            float[] spawns = new float[4];

            spawns[0] = -9f;
            spawns[1] = -3f;
            spawns[2] = 3f;
            spawns[3] = 9f;

            return spawns;
        }

        public static float RandomXSpawnPosition()
        {
            return xSpawnPositions()[Random.Range(0, xSpawnPositions().Length)];
        }

        public static float[] xSpawnPositionsMenu()
        {
            float[] spawns = new float[3];

            spawns[0] = -9f;
            spawns[1] = -3f;
            spawns[2] = 9f;

            return spawns;
        }

        public static float RandomXSpawnPositionMenu()
        {
            return xSpawnPositionsMenu()[UnityEngine.Random.Range(0, xSpawnPositionsMenu().Length)];
        }
        #endregion
    }

    public enum GameState
    {
        InMenu,
        InGame,
        GameOver,
    }

    [Serializable]
    public struct HeatVariation
    {
        public int maxHeliCount;
        [Space]
        public int maxCOPCount;
        [Range(0f, 100f)]
        public float copSpawnChance;
        [Space]
        public float startsFromScore;
    }
}