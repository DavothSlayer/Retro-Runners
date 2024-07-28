using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;
using V3CTOR;
using UnityEngine.Events;
using Pinwheel.Jupiter;
using System.Threading.Tasks;
using Unity.VisualScripting;

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
        [SerializeField]
        private ParticleSystem nearMissFX;
        [Space]
        public Transform playerTransform;
        public AutoMobile playerCar;

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
        [SerializeField]
        private Animator canvasAnimator;
        [SerializeField]
        private Animator effectBarAnimator;

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

        [Header("Tiles")]
        public RoadVariation[] roadVariations;
        [SerializeField]
        private float tileSafeZone;
        public UnityEvent<float> RoadTileSpawned;
        [SerializeField]
        private Transform background;
        #endregion

        #region Hidden Variables
        public static GameState gameState;
        [HideInInspector]
        public bool gamePaused = false;

        private float tileZSpawn = 0f;
        private float railZSpawn;
        private float tileLength = 400f;
        private float tileLineForPlayer = 1200f;

        [HideInInspector]
        public float currentRunScore;
        private int currentRunCOPsDestroyed;
        private int currentRunNMH;
        private int currentRunReward;

        private int cloudHighScore;
        private int cloudNMH;
        private int cloudCDR;
        
        [HideInInspector]
        public int nextRoadVar = 0;
        [HideInInspector]
        public int activeRoadVar = 0;
        [HideInInspector]
        public List<GameObject> activeTiles = new List<GameObject>();
        private List<GameObject> activeRails = new List<GameObject>();

        [HideInInspector]
        public int activeHeatLevel;
        [HideInInspector]
        public bool heat;
        [HideInInspector]
        public float heatLevelScoreOffset;        
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
            InputManager.LowerDoubleTapped += RearviewMethod;
            playerCar.Damaged += HandleAutoDamage;
        }

        private void OnDestroy()
        {
            playerAutoStatic = playerCar;

            heatLevelChanged -= HeatLevelChangeListener;
            InputManager.UpperDoubleTapped -= AutoAbility;
            InputManager.LowerDoubleTapped -= RearviewMethod;
            playerCar.Damaged -= HandleAutoDamage;
        }

        private void Awake()
        {
            Application.targetFrameRate = 60;

            UpdateGameScreen();
        }
        
        private void Update()
        {
            TileHandler();
            GuardRailHandler();
            
            CountScore();
            HeatLevelLogic();
            GameHUD();

            SwitchCaseMachine();
            CameraShaker();
        }

        private void GameHUD()
        {
            #region HUD Text and Animator Booleans
            hud.speedoMeterText.text = $"{Mathf.RoundToInt(playerCar.rb.velocity.magnitude * 2.2f)} MPH";
            hud.speedoMeterFill.fillAmount = playerCar.rb.velocity.magnitude / playerCar.data.autoLevelData[playerCar.engineLevel].TopSpeed;

            hud.nearMissComboTimer.fillAmount = 1f - nearMissTimer / nearMissMaxTime;
            int score = Mathf.RoundToInt(currentRunScore);
            hud.scoreText.text = score.ToString("n0");

            canvasAnimator.SetBool("ScoreXd", gameScoreMultiplier != 1f && gameState == GameState.InGame);
            canvasAnimator.SetBool("ShowHeatLevel", ActiveHeatLevel > 0 && gameState == GameState.InGame);
            canvasAnimator.SetBool("Near Miss", midCombo && gameState == GameState.InGame);
            #endregion

            #region Heat Level Stars
            if (heat)
            {
                for (int i = 0; i < hud.heatLevelStars.Length; i++)
                    hud.heatLevelStars[i].gameObject.SetActive(i <= activeHeatLevel - 1);
            }
            else
            {
                for (int i = 0; i < hud.heatLevelStars.Length; i++)
                    hud.heatLevelStars[i].gameObject.SetActive(false);
            }
            #endregion

            #region Pickup Markers
            for (int i = 0; i < spawnManager.PickupList.Count; i++)
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
            }
            #endregion

            #region Effect Bar Items
            effectBarAnimator.SetBool("Boost", playerCar.boost && GameManager.gameState == GameState.InGame);
            effectBarAnimator.SetBool("Ability", playerCar.abilityState != AbilityState.Ready && GameManager.gameState == GameState.InGame);

            for (int i = 0; i < hud.effectBarItems.Count; i++)
            {
                EffectBarItem barItem = hud.effectBarItems[i];

                switch (barItem.itemType)
                {
                    case EffectBarItemType.Boost:
                        barItem.timerFill.fillAmount = 1f - boostTimer / boostDuration;
                        break;

                    case EffectBarItemType.Ability:
                        if (playerCar.abilityState == AbilityState.Active)
                            barItem.timerFill.fillAmount = Mathf.Lerp(barItem.timerFill.fillAmount, 1f - playerCar.abilityTimer / playerCar.ability.duration, 10f * Time.deltaTime);
                        if (playerCar.abilityState == AbilityState.Cooldown)
                            barItem.timerFill.fillAmount = Mathf.Lerp(barItem.timerFill.fillAmount, playerCar.abilityCooldownTimer / playerCar.ability.cooldownTime, 10f * Time.deltaTime);
                        break;
                }
            }
            #endregion

            #region Health Bar
            hud.healthBarFill.fillAmount = Mathf.Lerp(
                hud.healthBarFill.fillAmount, 1f - (float)playerCar.health / (float)playerCar.data.autoLevelData[playerCar.armorLevel].MaxHealth,
                Time.deltaTime * 10f
                );
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

            canvasAnimator.SetBool("Game Over Reward Ads", Random.Range(0f, 1f) >= 0.5f);
        }

        public void PauseButton()
        {
            Time.timeScale = gamePaused ?
                Time.timeScale = 1f :
                Time.timeScale = 0f;           

            gamePaused = !gamePaused;
            hud.pauseGraphic.sprite = gamePaused ? hud.continueIcon : hud.pauseIcon;

            print(gamePaused);
        }

        public void ReturnToMainMenu()
        {
            ResetGame();

            gameState = GameState.InMenu;
            UpdateGameScreen();
        }

        public void HandleDoubleRewards()
        {
            canvasAnimator.SetBool("Game Over Reward Ads", false);

            int cloudDollars = gamingServicesManager.cloudData.retroDollars;
            cloudDollars += currentRunReward;
            gamingServicesManager.cloudData.retroDollars = cloudDollars;

            // DOUBLE THE REWARD, DISPLAY IT //
            currentRunReward *= 2;
            hud.earnings.text = $"${currentRunReward.ToString("n0")}  EARNED";

            gamingServicesManager.SaveCloudData(false);
        }

        public void HandleRevive()
        {
            canvasAnimator.SetBool("Game Over Reward Ads", false);

            playerCar.rb.velocity = Vector3.zero;
            playerTransform.SetPositionAndRotation(new Vector3(3f, 0.02f, 600f), Quaternion.identity);

            playerCar.FixAuto();

            tileZSpawn = 0f;
            tileLineForPlayer = 1000f;
            railZSpawn = 0f;
            activeRoadVar = 0;
            nextRoadVar = 0;

            // DESPAWN ACTIVE TILES //
            while (activeTiles.Count > 0)
                RemoveRoadTile(0);

            // SPAWN IN NEW ROAD TILES //
            for (int i = 0; i < 5; i++)
                SpawnRoadTile();

            // DESPAWN ACTIVE RAILS //
            while (activeRails.Count > 0)
                RemoveRail(0);

            // SPAWN IN NEW RAILS //
            for (int i = 0; i < 6; i++)
                SpawnGuardRail();        

            spawnManager.roadLanes[0].active = false;
            spawnManager.roadLanes[5].active = false;
            spawnManager.KillEmAll();
            spawnManager.InitializeNPCs();

            gameState = GameState.InGame;
            UpdateGameScreen();

            print("HandleRevive");
        }

        public void NoThanksButton()
        {
            canvasAnimator.SetBool("Game Over Reward Ads", false);
        }        
        #endregion

        #region Gameplay
        private void ResetGame()
        {
            // RESET AUTO //
            ReplaceAuto();
            playerAutoStatic = playerCar;

            // RESET GAME VALUES //
            AddMultiplier("ThrillSeeker", 1f);
            Time.timeScale = 1f;
            tileZSpawn = 0f;
            tileLineForPlayer = 1000f;
            railZSpawn = 0f;
            activeRoadVar = 0;
            nextRoadVar = 0;
            currentRunScore = 0;
            currentRunNMH = 0;
            currentRunCOPsDestroyed = 0;
            heatLevelScoreOffset = 0f;

            // DESPAWN ACTIVE TILES //
            while (activeTiles.Count > 0)
                RemoveRoadTile(0);

            // SPAWN IN NEW ROAD TILES //
            for (int i = 0; i < 5; i++)
                SpawnRoadTile();

            // DESPAWN ACTIVE RAILS //
            while (activeRails.Count > 0)
                RemoveRail(0);

            // SPAWN IN NEW RAILS //
            for (int i = 0; i < 6; i++)
                SpawnGuardRail();

            spawnManager.roadLanes[0].active = false;
            spawnManager.roadLanes[5].active = false;

            spawnManager.KillEmAll();
            spawnManager.InitializeNPCs();

            // APPLY CLOUD DATA //
            ApplyLoadedData();
        }

        private void ReplaceAuto()
        {
            if (playerCar != null)
            {
                GameObject currentCar = playerCar.gameObject;
                currentCar.SetActive(false);
            }

            int cloudAutoInt = gamingServicesManager.cloudData.lastSelectedCarInt;
            GameObject newAuto = allAutos[cloudAutoInt];

            // SET PLAYER SPAWN TO ZSPAWN + 600f //
            playerCar = newAuto.GetComponent<AutoMobile>();
            playerTransform = newAuto.transform;
            playerTransform.SetPositionAndRotation(new Vector3(3f, 0.02f, 600f), Quaternion.identity);
            newAuto.SetActive(true);

            playerCar.engineLevel = gamingServicesManager.cloudData.unlockedCarsDict[playerCar.data.ItemCode].lastSelectedCompList[0];
            playerCar.gearboxLevel = gamingServicesManager.cloudData.unlockedCarsDict[playerCar.data.ItemCode].lastSelectedCompList[1];
            playerCar.tiresLevel = gamingServicesManager.cloudData.unlockedCarsDict[playerCar.data.ItemCode].lastSelectedCompList[2];
            playerCar.armorLevel = gamingServicesManager.cloudData.unlockedCarsDict[playerCar.data.ItemCode].lastSelectedCompList[3];
            playerCar.FixAuto();
        }

        private void HandleAutoDamage()
        {
            HealthBarRoutine();
            canvasAnimator.SetTrigger("HealthBar Event");
        }

        private float barTime = 3f;
        private float barTimer;
        private async void HealthBarRoutine()
        {
            barTimer = 0f;

            canvasAnimator.SetBool("ShowHealthBar", true);

            while (barTimer < barTime && GameManager.gameState == GameState.InGame)
            {
                barTimer += Time.deltaTime;

                await Task.Yield();
            }

            canvasAnimator.SetBool("ShowHealthBar", false);
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
        public void NearMiss()
        {
            canvasAnimator.SetTrigger("Near Miss Combo");

            nearMissTimer = 0f;
            nearMissComboCount++;

            AddExternalScore(scoreTable.scorePerNearMiss * nearMissComboCount * gameScoreMultiplier);

            if(nearMissComboCount > 1)
                hud.nearMissText.text = $"NEAR  MISS {nearMissComboCount}X";
            else
                hud.nearMissText.text = $"NEAR  MISS";

            if (!midCombo) { NearMissTimer(); }

            nearMissSource.pitch = 0.8f + 0.05f * nearMissComboCount;
            nearMissSource.pitch = Mathf.Clamp(nearMissSource.pitch, 0.8f, 1.4f);

            nearMissSource.PlayOneShot(nearMissSFX);
            nearMissFX.Play();
        }

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
                gamingServicesManager.cloudData.highestNearMissCombo = currentRunNMH;

            nearMissComboCount = 0;
            midCombo = false;
        }
        // NEAR MISS //

        // REARVIEW MIRROR //
        public void RearviewMethod()
        {
            rearview = !rearview;
        }

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
                gamingServicesManager.cloudData.highScore = score;

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
                    canvasAnimator.SetTrigger("ScoreXd Event");
                }
            }
            else
            {
                // ADD A NEW MULTIPLIER //
                activeScoreMultipliers.Add(source, newMultiplier);

                hud.scoreMultiplierText.text = $"{gameScoreMultiplier}X";
                canvasAnimator.SetTrigger("ScoreXd Event");
            }
        }

        public void RemoveMultiplier(string source)
        {
            activeScoreMultipliers.Remove(source);

            canvasAnimator.SetTrigger("ScoreXd Event");
        }

        public void AddExternalScore(float amount)
        {
            currentRunScore += amount;
            hud.scoreEventText.text = $"+ {amount.ToString("n0")}";
            canvasAnimator.SetTrigger("Score Added Event");
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

            HealthBarRoutine();
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
        public delegate void DestroyedCOPDelegate();
        public event DestroyedCOPDelegate DestroyedCOPEvent;
        public void DestroyedCOP()
        {
            currentRunCOPsDestroyed++;

            if (currentRunCOPsDestroyed > cloudCDR)
                gamingServicesManager.cloudData.mostCOPsDestroyed = currentRunCOPsDestroyed;

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

            tileZSpawn = 0f;
            tileLineForPlayer = 1000f;
            railZSpawn = 0f;
            activeRoadVar = 0;
            nextRoadVar = 0;
            currentRunScore *= 2f;

            // DESPAWN ACTIVE TILES //
            while (activeTiles.Count > 0)
                RemoveRoadTile(0);

            // SPAWN IN NEW ROAD TILES //
            for (int i = 0; i < 5; i++)
                SpawnRoadTile();

            // DESPAWN ACTIVE RAILS //
            while (activeRails.Count > 0)
                RemoveRail(0);

            // SPAWN IN NEW RAILS //
            for (int i = 0; i < 6; i++)
                SpawnGuardRail();

            spawnManager.roadLanes[0].active = false;
            spawnManager.roadLanes[5].active = false;
            spawnManager.KillEmAll();
            spawnManager.InitializeNPCs();
        }
        // AUTO ABILITIES //
        #endregion

        #region Game Over
        public void GameOver()
        {
            gameState = GameState.GameOver;
            
            int cloudDollars = gamingServicesManager.cloudData.retroDollars;
            cloudDollars += currentRunReward;
            gamingServicesManager.cloudData.retroDollars = cloudDollars;

            // DISPLAY EARNINGS AND THE SCORE //
            hud.earnings.text = $"${currentRunReward.ToString("n0")}  EARNED";
            hud.finalScore.text = $"SCORE \n {currentRunScore.ToString("n0")}";
            hud.finalNMH.text = $"HIGHEST  NEAR  MISS  COMBO \n {currentRunNMH}X";
            hud.COPKillCount.text = $"{currentRunCOPsDestroyed}  COPs  Destroyed!";

            UpdateGameScreen();
            gamingServicesManager.SaveCloudData(false);
        }
        #endregion

        #region Road Generation
        // ROAD TILE METHODS //
        private void TileHandler()
        {
            if (playerCar == null) { return; }

            background.position = new Vector3(0f, 0f, playerTransform.position.z + 1300f);

            if(playerTransform.position.z >= tileLineForPlayer)
            {
                nextRoadVar = NextRoadVar();

                SpawnRoadTile();

                tileLineForPlayer += tileLength;
            }
            
            if(activeTiles.Count <= 5) { return; }
            
            if(playerTransform.position.z >= activeTiles[0].transform.position.z + tileSafeZone)
            {
                RemoveRoadTile(0);
            }
        }

        private void SpawnRoadTile()
        {
            // SPAWN NEXT TILE //
            var tileArray = roadVariations[activeRoadVar].roadTiles;
            GameObject nextTile = NextTile();

            if (activeRoadVar != nextRoadVar)
            {
                nextTile.transform.SetPositionAndRotation(transform.forward * tileZSpawn, transform.rotation);
                EXMET.AddSpawnable(nextTile, activeTiles, tileArray);

                activeRoadVar = nextRoadVar;
            }
            else
            {
                nextTile.transform.SetPositionAndRotation(transform.forward * tileZSpawn, transform.rotation);
                if (activeTiles.Contains(nextTile)) 
                {
                    int duplicateInt = activeTiles.IndexOf(nextTile);
                    RemoveRoadTile(duplicateInt);
                }
                EXMET.AddSpawnable(nextTile, activeTiles, tileArray);
            }

            spawnManager.roadLanes[0].active = nextTile.name.Contains("Level2");
            spawnManager.roadLanes[5].active = nextTile.name.Contains("Level2");

            tileZSpawn += tileLength;
           
            if(gameState == GameState.InGame)
                RoadTileSpawned?.Invoke(tileZSpawn);
        }

        private GameObject NextTile()
        {
            // GET NEXT TILE //
            var tileArray = roadVariations[activeRoadVar].roadTiles;
            GameObject tile;

            if (activeRoadVar != nextRoadVar)
            {
                tile = roadVariations[nextRoadVar].transitionTile;
            }
            else
            {
                if (tileArray.Count == 0)
                    tile = activeTiles[0];
                else
                    tile = tileArray[0];
            }

            return tile;
        }

        private int NextRoadVar()
        {
            for (int i = 0; i < activeTiles.Count; i++)
                if (activeTiles[i].name.Contains("Start"))
                    return activeRoadVar;

            int nextVar = Random.Range(0f, 1f) > 0.5f ? 1 : 0;
            return nextVar;
        }

        private void RemoveRoadTile(int integer)
        {
            if (activeTiles.Count == 0) { return; }

            if (!activeTiles[integer].name.Contains("Start"))
            {
                GameObject obj = activeTiles[integer];
                int index = obj.name.Contains("Level1") ? 0 : 1;

                EXMET.RemoveSpawnable(obj, activeTiles, roadVariations[index].roadTiles);
            }            
            else if(activeTiles[integer].name.Contains("Start"))
            {
                GameObject obj = activeTiles[integer];

                obj.SetActive(false);
                activeTiles.Remove(obj);
            }
        }
        // ROAD TILE METHODS //

        // GUARD RAIL METHODS //
        private void GuardRailHandler()
        {
            if (playerCar == null) { return; }

            if (playerTransform.position.z > railZSpawn - 800f)
                SpawnGuardRail();

            if (activeRails.Count == 0) return;
            if (playerTransform.position.z > activeRails[0].transform.position.z + 800f)
                RemoveRail(0);
        }

        private void SpawnGuardRail()
        {
            // SPAWN NEXT RAIL //
            var railArray = roadVariations[activeRoadVar].rails;
            GameObject nextRail = NextRail();

            nextRail.transform.SetPositionAndRotation(transform.forward * railZSpawn, transform.rotation);
            EXMET.AddSpawnable(nextRail, activeRails, railArray);

            railZSpawn += 200f;
        }

        private GameObject NextRail()
        {
            if (roadVariations[activeRoadVar].rails.Count == 0)
                return activeRails[0];
            else
                return roadVariations[activeRoadVar].rails[0];
        }

        private void RemoveRail(int index)
        {
            GameObject obj = activeRails[index];

            int railIndex = obj.name.Contains("Rail1") ? 0 : 1;
            EXMET.RemoveSpawnable(obj, activeRails, roadVariations[railIndex].rails);
        }

        public void RemoveDeadRail(GameObject obj)
        {
            int railIndex = obj.name.Contains("Rail1") ? 0 : 1;
            EXMET.RemoveSpawnable(obj, activeRails, roadVariations[railIndex].rails);
        }
        // GUARD RAIL METHODS //
        #endregion

        #region Cinematics
        private Vector3 playerPos;
        private Quaternion playerRot;
        private Quaternion rearviewRot;
        private Vector3 rearviewPos;
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
                    camPosTarget = Vector3.Slerp(camPosTarget, gameCamPos, 3.5f * Time.deltaTime);
                    camRotTarget = Quaternion.Slerp(camRotTarget, gameCamRot, 6f * Time.deltaTime);
                    
                    Quaternion carRot = Quaternion.Euler(0f, 5f * input.xTouchLerp, 6f * input.xTouchLerp);
                    Vector3 cameraOffset = new(1.5f * input.xTouchLerp, 0f, 0f);
                    
                    rearviewRot = rearview && gameState == GameState.InGame ?
                        Quaternion.Slerp(rearviewRot, Quaternion.Euler(0f, 180f, 0f), 6f * Time.deltaTime) :
                        Quaternion.Slerp(rearviewRot, Quaternion.Euler(0f, 0f, 0f), 8f * Time.deltaTime);

                    rearviewPos = rearview && gameState == GameState.InGame ?
                        Vector3.Slerp(rearviewPos, playerCar.data.rearviewOffset, 6f * Time.deltaTime) :
                        Vector3.Slerp(rearviewPos, Vector3.zero, 8f * Time.deltaTime);

                    camHolder.position = playerPos + camPosTarget + cameraOffset + rearviewPos;
                    camHolder.rotation = playerRot * camRotTarget * carRot * rearviewRot;

                    speedFOV = (playerCar.rb.velocity.magnitude / playerCar.data.autoLevelData[playerCar.engineLevel].TopSpeed) * 30f;

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
        }

        public void ApplyLoadedData()
        {
            cloudNMH = gamingServicesManager.cloudData.highestNearMissCombo;
            cloudHighScore = gamingServicesManager.cloudData.highScore;
            cloudCDR = gamingServicesManager.cloudData.mostCOPsDestroyed;

            hud.highScoreText.text = $"HIGH  SCORE  {cloudHighScore.ToString("n0")}";
            hud.highestMissComboText.text = $"NEAR  MISS  COMBO  RECORD  {cloudNMH}X!";
            hud.COPKillCountRecord.text = $"COPS  DESTROYED  RECORD  {cloudCDR}!";
            hud.playerMoneyText.text = $"${gamingServicesManager.cloudData.retroDollars.ToString("n0")}";
        }

        public void UpdateGameScreen()
        {
            canvasAnimator.SetInteger("GameState", (int)gameState);
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
    public class RoadVariation
    {
        public List<GameObject> roadTiles = new List<GameObject>();
        public GameObject transitionTile;
        public List<GameObject> rails = new List<GameObject>();
        [Space]
        public int maxNPCCountRL;
        public int maxNPCCountLL;
        [Space]
        public float NPCSpawnDistance;
        public float distanceBetweenNPC;
        [Space]
        public int maxPickupCount;
        [Range(0, 100f)]
        public float pickupSpawnChance;
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