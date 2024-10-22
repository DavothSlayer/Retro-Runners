 using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using UnityEditor.ShaderKeywordFilter;
using UnityEngine;
using UnityEngine.UI;
using V3CTOR;

namespace RetroCode
{
    public class RetroGarage : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField]
        private ScreenInteract swipeSelect;
        [SerializeField]
        private GarageHUD hud;
        [SerializeField]
        private Animator garageScreenAnimator;

        [Header("Garage")]
        [SerializeField]
        private Transform camOffset;
        [SerializeField]
        private GameObject autoPropParent;
        [SerializeField]
        private Transform compRotationParent;
        [SerializeField]
        private Animator cameraAnimator;
        public AutoProp[] autoProps;
        [SerializeField]
        private List<Partlist> compLists;
        [Space] 
        [SerializeField]
        private UGSM gamingServicesManager;

        [Header("SFX")]
        public AudioSource audioSource;
        [SerializeField]
        private AudioClip transactionSuccessful;
        [SerializeField]
        private AudioClip transactionFailed;

        // HIDDEN //
        [HideInInspector]
        public int selectedAutoInt = 0;
        [HideInInspector]
        public int selectedCompInt = 0;
        [HideInInspector]
        public GarageState garageState = GarageState.AutoReview;
        [HideInInspector]
        public ComponentState compState = ComponentState.EngineReview;

        private void Awake()
        {
            Application.targetFrameRate = 60;

            InitConstants();
        }

        private void Update()
        {
            HandleAutoSliders();

            camOffset.Rotate(Vector3.up * 25f * Time.deltaTime);
            camOffset.rotation *= Quaternion.Euler(Vector3.up * swipeSelect.inputX * Time.deltaTime);

            /*Vector3 currentRot = camOffset.eulerAngles;
            float rotX = currentRot.x * swipeSelect.inputY * Time.deltaTime;
            rotX = Mathf.Clamp(rotX, 15f, 90f);

            camOffset.eulerAngles = new Vector3(rotX, currentRot.y * swipeSelect.inputX * Time.deltaTime, currentRot.z);*/

            compRotationParent.Rotate(Vector3.up * 25f * Time.deltaTime);

            hud.deliveryTimeText.text = DeliveryTimeText(autoProps[selectedAutoInt].data, (int)compState);
        }

        #region Core Garage Functionality

        #region Inventory Info & Stats
        private float autoTopSpeedVar, autoPowerVar, autoHandlingVar, autoHealthVar;
        private float bestTopSpeed, bestPower, bestHandling, bestHealth;
        private void InitConstants()
        {
            AutoData currentAutoData = autoProps[selectedAutoInt].data;

            // FIND THE BEST & WORST VALUES AMONG THE CARS //
            for (int i = 0; i < autoProps.Length; i++)
                foreach (AutoLevelData ald in autoProps[i].data.autoLevelData)
                {
                    bestTopSpeed = Mathf.Max(bestTopSpeed, ald.TopSpeed);
                    bestPower = Mathf.Max(bestPower, ald.Power);
                    bestHandling = Mathf.Max(bestHandling, ald.Handling);
                    bestHealth = Mathf.Max(bestHealth, ald.MaxHealth);
                }
            // FIND THE BEST & WORST VALUES AMONG THE CARS //

            // SET THE SLIDER MAX & MIN VALUES //
            hud.autoTopSpeedSlider.maxValue = bestTopSpeed;
            hud.autoPowerSlider.maxValue = bestPower;
            hud.autoHandlingSlider.maxValue = bestHandling;
            hud.autoHealthSlider.maxValue = bestHealth;

            hud.compStatSliders[0].mainSlider.minValue = 0f;
            hud.compStatSliders[0].mainSlider.maxValue = bestTopSpeed;
            hud.compStatSliders[0].diffSlider.minValue = 0f;
            hud.compStatSliders[0].diffSlider.maxValue = bestTopSpeed;

            hud.compStatSliders[1].mainSlider.minValue = 0f;
            hud.compStatSliders[1].mainSlider.maxValue = bestPower;
            hud.compStatSliders[1].diffSlider.minValue = 0f;
            hud.compStatSliders[1].diffSlider.maxValue = bestPower;

            hud.compStatSliders[2].mainSlider.minValue = 0f;
            hud.compStatSliders[2].mainSlider.maxValue = bestHandling;
            hud.compStatSliders[2].diffSlider.minValue = 0f;
            hud.compStatSliders[2].diffSlider.maxValue = bestHandling;

            hud.compStatSliders[3].mainSlider.minValue = 0f;
            hud.compStatSliders[3].mainSlider.maxValue = bestHealth;
            hud.compStatSliders[3].diffSlider.minValue = 0f;
            hud.compStatSliders[3].diffSlider.maxValue = bestHealth;
            // SET THE SLIDER MAX & MIN VALUES //
        }

        private float statSliderLerpSpeed = 5f;
        private void HandleAutoSliders()
        {
            // VALUES //
            hud.autoTopSpeedSlider.value = Mathf.Lerp(hud.autoTopSpeedSlider.value, autoTopSpeedVar, statSliderLerpSpeed * Time.deltaTime);
            hud.autoPowerSlider.value = Mathf.Lerp(hud.autoPowerSlider.value, autoPowerVar, statSliderLerpSpeed * Time.deltaTime);
            hud.autoHandlingSlider.value = Mathf.Lerp(hud.autoHandlingSlider.value, autoHandlingVar, statSliderLerpSpeed * Time.deltaTime);
            hud.autoHealthSlider.value = Mathf.Lerp(hud.autoHealthSlider.value, autoHealthVar, statSliderLerpSpeed * Time.deltaTime);
            // VALUES //
        }

        public void NextOption()
        {
            if (hud.upgradeScreen.activeInHierarchy)
            {
                if (selectedCompInt != gamingServicesManager.cloudData.inventoryDict[autoProps[selectedAutoInt].data.ItemCode][EXMET.IntToCompClass((int)compState)].NextLevel()) selectedCompInt++;
            }
            else
            {
                if (selectedAutoInt != autoProps.Length - 1) selectedAutoInt++;
            }

            CheckInventory();
        }

        public void PreviousOption()
        {
            if (hud.upgradeScreen.activeInHierarchy)
            {
                if (selectedCompInt != 0) selectedCompInt--;
            }
            else
            {
                if (selectedAutoInt != 0) selectedAutoInt--;
            }                

            CheckInventory();
        }

        public void CheckInventory()
        {
            AutoData currentAutoData = autoProps[selectedAutoInt].data;

            hud.previousAutoButton.SetActive(selectedAutoInt != 0);
            hud.nextAutoButton.SetActive(selectedAutoInt != autoProps.Length - 1);
            hud.openCratesButton.SetActive(gamingServicesManager.lootingDictionary.Keys.Count != 0);

            hud.playerMoneyText.text = $"R$ {gamingServicesManager.cloudData.retroDollars.ToString("N", EXMET.NumForThou)}";
            //UpdatePlayerMoneyText();

            #region Check Cars
            // SET ACTIVE ONLY SELECTED CAR, HIDE REST //
            for (int i = 0; i < autoProps.Length; i++)
                autoProps[i].gameObject.SetActive(autoProps[i] == autoProps[selectedAutoInt]);

            if (gamingServicesManager.cloudData.inventoryDict.ContainsKey(currentAutoData.ItemCode))
            {
                AutoPartData partData = gamingServicesManager.cloudData.inventoryDict[currentAutoData.ItemCode][EXMET.IntToCompClass((int)compState)];

                hud.currentAutoNameText.text = currentAutoData.AutoName;

                autoProps[selectedAutoInt].lockedModel.SetActive(false);
                autoProps[selectedAutoInt].unlockedModels[0].SetActive(true);

                hud.unlockedActionParent_AR.SetActive(true);
                hud.lockedObjectParent_AR.SetActive(false);

                autoTopSpeedVar = currentAutoData.autoLevelData[gamingServicesManager.cloudData.inventoryDict[currentAutoData.ItemCode]["engine"].equippedLevel].TopSpeed;
                autoPowerVar = currentAutoData.autoLevelData[gamingServicesManager.cloudData.inventoryDict[currentAutoData.ItemCode]["power"].equippedLevel].Power;
                autoHandlingVar = currentAutoData.autoLevelData[gamingServicesManager.cloudData.inventoryDict[currentAutoData.ItemCode]["handling"].equippedLevel].Handling;
                autoHealthVar = currentAutoData.autoLevelData[gamingServicesManager.cloudData.inventoryDict[currentAutoData.ItemCode]["health"].equippedLevel].MaxHealth;

                // AUTO VIEW STATS //
                hud.compStatSliders[0].mainSlider.value = autoTopSpeedVar;
                hud.compStatSliders[0].diffSlider.value = currentAutoData.autoLevelData[gamingServicesManager.cloudData.inventoryDict[currentAutoData.ItemCode]["engine"].NextLevel()].TopSpeed;

                hud.compStatSliders[1].mainSlider.value = autoPowerVar;
                hud.compStatSliders[1].diffSlider.value = currentAutoData.autoLevelData[gamingServicesManager.cloudData.inventoryDict[currentAutoData.ItemCode]["power"].NextLevel()].Power;

                hud.compStatSliders[2].mainSlider.value = autoHandlingVar;
                hud.compStatSliders[2].diffSlider.value = currentAutoData.autoLevelData[gamingServicesManager.cloudData.inventoryDict[currentAutoData.ItemCode]["handling"].NextLevel()].Handling;

                hud.compStatSliders[3].mainSlider.value = autoHealthVar;
                hud.compStatSliders[3].diffSlider.value = currentAutoData.autoLevelData[gamingServicesManager.cloudData.inventoryDict[currentAutoData.ItemCode]["health"].NextLevel()].MaxHealth;

                hud.autoTopSpeedInfo.text = $"{Mathf.RoundToInt(autoTopSpeedVar * 2.2f)} MPH";
                hud.autoPowerInfo.text = $"{autoPowerVar} UNITS";
                hud.autoHandlingInfo.text = $"{autoHandlingVar} M/S";
                hud.autoHealthInfo.text = $"{autoHealthVar} HP";
                // AUTO VIEW STATS //

                // COMPONENT VIEW STATS //
                hud.compStatSliders[0].statCurrentText.text = $"{Mathf.RoundToInt(currentAutoData.autoLevelData[gamingServicesManager.cloudData.inventoryDict[currentAutoData.ItemCode]["engine"].equippedLevel].TopSpeed * 2.2f)} MPH";
                hud.compStatSliders[0].statTunedText.text = $"{Mathf.RoundToInt(currentAutoData.autoLevelData[gamingServicesManager.cloudData.inventoryDict[currentAutoData.ItemCode]["engine"].NextLevel()].TopSpeed * 2.2f)} MPH";
                hud.compStatSliders[0].levelText.text = $"MK. {EXMET.LevelAsRoman(gamingServicesManager.cloudData.inventoryDict[currentAutoData.ItemCode]["engine"].equippedLevel)}";
                hud.compStatSliders[0].icon.sprite = compLists[0].Comps[gamingServicesManager.cloudData.inventoryDict[currentAutoData.ItemCode]["engine"].equippedLevel].itemData.icon;

                hud.compStatSliders[1].statCurrentText.text = $"{currentAutoData.autoLevelData[gamingServicesManager.cloudData.inventoryDict[currentAutoData.ItemCode]["power"].equippedLevel].Power} UNITS";
                hud.compStatSliders[1].statTunedText.text = $"{currentAutoData.autoLevelData[gamingServicesManager.cloudData.inventoryDict[currentAutoData.ItemCode]["power"].NextLevel()].Power} UNITS";
                hud.compStatSliders[1].levelText.text = $"MK. {EXMET.LevelAsRoman(gamingServicesManager.cloudData.inventoryDict[currentAutoData.ItemCode]["power"].equippedLevel)}";
                hud.compStatSliders[1].icon.sprite = compLists[1].Comps[gamingServicesManager.cloudData.inventoryDict[currentAutoData.ItemCode]["power"].equippedLevel].itemData.icon;

                hud.compStatSliders[2].statCurrentText.text = $"{currentAutoData.autoLevelData[gamingServicesManager.cloudData.inventoryDict[currentAutoData.ItemCode]["handling"].equippedLevel].Handling} M/S";
                hud.compStatSliders[2].statTunedText.text = $"{currentAutoData.autoLevelData[gamingServicesManager.cloudData.inventoryDict[currentAutoData.ItemCode]["handling"].NextLevel()].Handling} M/S";
                hud.compStatSliders[2].levelText.text = $"MK. {EXMET.LevelAsRoman(gamingServicesManager.cloudData.inventoryDict[currentAutoData.ItemCode]["handling"].equippedLevel)}";
                hud.compStatSliders[2].icon.sprite = compLists[2].Comps[gamingServicesManager.cloudData.inventoryDict[currentAutoData.ItemCode]["handling"].equippedLevel].itemData.icon;

                hud.compStatSliders[3].statCurrentText.text = $"{currentAutoData.autoLevelData[gamingServicesManager.cloudData.inventoryDict[currentAutoData.ItemCode]["health"].equippedLevel].MaxHealth} HP";
                hud.compStatSliders[3].statTunedText.text = $"{currentAutoData.autoLevelData[gamingServicesManager.cloudData.inventoryDict[currentAutoData.ItemCode]["health"].NextLevel()].MaxHealth} HP";
                hud.compStatSliders[3].levelText.text = $"MK. {EXMET.LevelAsRoman(gamingServicesManager.cloudData.inventoryDict[currentAutoData.ItemCode]["health"].equippedLevel)}";
                hud.compStatSliders[3].icon.sprite = compLists[3].Comps[gamingServicesManager.cloudData.inventoryDict[currentAutoData.ItemCode]["health"].equippedLevel].itemData.icon;
                // COMPONENT VIEW STATS //

                hud.previousCompButton.SetActive(selectedCompInt != 0);
                hud.nextCompButton.SetActive(selectedCompInt != partData.NextLevel());
                hud.compPriceText.gameObject.SetActive(selectedCompInt == partData.NextLevel());
                hud.deliveryTimeAction.SetActive(DeliveryInProgress(currentAutoData, (int)compState) && selectedCompInt == partData.NextLevel());
                hud.orderCompButton.SetActive(!DeliveryInProgress(currentAutoData, (int)compState) && selectedCompInt == partData.NextLevel());

                #region Check Components
                for (int i = 0; i < compLists.Count; i++)
                {
                    if (((int)compState) == i)
                    {
                        for (int j = 0; j < compLists[i].Comps.Count; j++)
                        {
                            ComponentProp prop = compLists[i].Comps[j];
                            prop.gameObject.SetActive(j == selectedCompInt);

                            ComponentProp currentProp = compLists[i].Comps[selectedCompInt];
                            hud.currentCompNameText.text = currentProp.itemData.ItemName;
                            
                            foreach (ParticleSystem fx in hud.maxLvlFX) fx.gameObject.SetActive(selectedCompInt == 4);

                            int CompPrice = Mathf.RoundToInt(currentProp.itemData.DefaultPrice * autoProps[selectedAutoInt].data.CompPriceMultiplier);
                            hud.compPriceText.text = $"R$ {CompPrice.ToString("N", EXMET.NumForThou)}";
                        }
                    }
                    else
                    {
                        foreach (ComponentProp compProp in compLists[i].Comps)
                        {
                            compProp.gameObject.SetActive(false);
                        }
                    }
                }
                #endregion
            }
            else
            {
                hud.currentAutoNameText.text = currentAutoData.AutoName;
                autoProps[selectedAutoInt].lockedModel.SetActive(true);
                autoProps[selectedAutoInt].unlockedModels[0].SetActive(false);

                hud.unlockedActionParent_AR.SetActive(false);
                hud.lockedObjectParent_AR.SetActive(true);

                autoTopSpeedVar = currentAutoData.autoLevelData[0].TopSpeed;
                autoPowerVar = currentAutoData.autoLevelData[0].Power;
                autoHandlingVar = currentAutoData.autoLevelData[0].Handling;
                autoHealthVar = currentAutoData.autoLevelData[0].MaxHealth;

                hud.autoTopSpeedInfo.text = $"{Mathf.RoundToInt(currentAutoData.autoLevelData[0].TopSpeed * 2.2f)} MPH";
                hud.autoPowerInfo.text = $"{currentAutoData.autoLevelData[0].Power} UNITS";
                hud.autoHandlingInfo.text = $"{currentAutoData.autoLevelData[0].Handling} M/S";
                hud.autoHealthInfo.text = $"{currentAutoData.autoLevelData[0].MaxHealth} HP";

                hud.autoPriceText.text = $"R$ {autoProps[selectedAutoInt].data.Price.ToString("N", EXMET.NumForThou)}";
            }
            #endregion
        }

        private bool DeliveryInProgress(AutoData currentAutoData, int compClass)
        {
            if (!gamingServicesManager.deliveryDictionary.ContainsKey(currentAutoData.ItemCode)) return false;

            if (gamingServicesManager.deliveryDictionary[currentAutoData.ItemCode].ContainsKey(EXMET.IntToCompClass(compClass)))
            {
                return true;
            }
            else { return false; }
        }

        private string DeliveryTimeText(AutoData currentAutoData, int compClass)
        {
            if (!DeliveryInProgress(currentAutoData, compClass)) return "00:00:00";
            else
            {
                DateTime currentTime = DateTime.UtcNow;
                DateTime expectedDeliveryDate = gamingServicesManager.deliveryDictionary[currentAutoData.ItemCode][EXMET.IntToCompClass(compClass)];

                TimeSpan difference = expectedDeliveryDate - currentTime;

                string deliveryTime = difference.ToString(@"hh\:mm\:ss");

                return deliveryTime;
            }
        }

        public void OnCloudDataReceived()
        {
            selectedAutoInt = gamingServicesManager.cloudData.lastSelectedCarInt;

            CheckInventory();
        }
        #endregion

        #region Purchasing & Upgrading & Selecting
        public void OrderAutoPart()
        {
            AutoData autoData = autoProps[selectedAutoInt].data;
            AutoPartData autoPartData = gamingServicesManager.cloudData.inventoryDict[autoData.ItemCode][EXMET.IntToCompClass((int)compState)];

            autoPartData.OrderPart(autoPartData.NextLevel(), DateTime.UtcNow);

            print($"Auto Part ordered: Current LVL: {autoPartData.currentLevel}, Ordered LVL: {autoPartData.orderedLevel}");

            gamingServicesManager.SaveCloudData(false);
            gamingServicesManager.CheckDeliveryTimers();
            gamingServicesManager.CheckAvailableLoot();

            CheckInventory();
        }

        public void PrepTransaction()
        {
            /*bool component = garageState == GarageState.ComponentReview;

            ItemData iData = compLists[((int)compState)].Comps[selectedCompInt].itemData;
            AutoData aData = autoProps[selectedAutoInt].data;

            if (!component)
                hud.purchaseConfirmText.text = $"Purchase {aData.AutoName} for ${aData.Price.ToString("n0")}?";
            else
                hud.purchaseConfirmText.text = $"Purchase {iData.ItemName} for ${Mathf.RoundToInt(iData.DefaultPrice * aData.CompPriceMultiplier).ToString("n0")}?";*/
        }

        public void ProcessTransaction()
        {
            /*
            bool component = garageState == GarageState.ComponentReview;

            ItemData iData = compLists[((int)compState)].Comps[selectedCompInt].itemData;
            AutoData aData = autoProps[selectedAutoInt].data;

            if (!component)
            {
                if (gamingServicesManager.cloudData.retroDollars >= aData.Price)
                {
                    // SUCCESS //
                    CompleteTransaction(false, aData.ItemCode, iData.ItemCode);
                }
                else
                {
                    // FAILED //
                    TransactionFailed();
                }
            }
            else
            {
                if (gamingServicesManager.cloudData.retroDollars >= Mathf.RoundToInt(iData.DefaultPrice * aData.CompPriceMultiplier))
                {
                    // SUCCESS //
                    CompleteTransaction(true, aData.ItemCode, iData.ItemCode);
                }
                else
                {
                    // FAILED //
                    TransactionFailed();
                }
            }
            */
        }

        private void CompleteTransaction(bool component, string AutoCode, string CompCode)
        {
            /*
            if (!component)
            {
                gamingServicesManager.cloudData.unlockedCarsDict.Add(AutoCode, new AutoPartsData());

                gamingServicesManager.cloudData.unlockedCarsDict[AutoCode].compCodes.Add("Engine_LVL1");
                gamingServicesManager.cloudData.unlockedCarsDict[AutoCode].compCodes.Add("Gearbox_LVL1");
                gamingServicesManager.cloudData.unlockedCarsDict[AutoCode].compCodes.Add("Tires_LVL1");
                gamingServicesManager.cloudData.unlockedCarsDict[AutoCode].compCodes.Add("Armor_LVL1");

                for (int i = 0; i < 4; i++)
                    gamingServicesManager.cloudData.unlockedCarsDict[AutoCode].lastSelectedCompList.Add(0);

                gamingServicesManager.cloudData.lastSelectedCarInt = selectedAutoInt;

                //gamingServicesManager.cloudData.retroDollars = -autoProps[selectedAutoInt].data.Price;

                hud.autoNameText_C.text = autoProps[selectedAutoInt].data.AutoName;
                autoProps[selectedAutoInt].PlayUnlockFX();
                SetGarageState(2);
            }
            else
            {
                gamingServicesManager.cloudData.unlockedCarsDict[AutoCode].compCodes[((int)compState)] += CompCode;

                gamingServicesManager.cloudData.unlockedCarsDict[AutoCode].lastSelectedCompList[((int)compState)] = selectedCompInt;

                //gamingServicesManager.cloudData.retroDollars =- compLists[((int)compState)].Comps[selectedCompInt].itemData.DefaultPrice;

                compLists[((int)compState)].Comps[selectedCompInt].PlayUnlockFX();
                garageScreenAnimator.SetTrigger("Purchase Success");
            }

            swipeSelect.SetSwipeState(true);
            audioSource.PlayOneShot(transactionSuccessful);

            gamingServicesManager.SaveCloudData(false);

            CheckInventory();
            */
        }

        private void TransactionFailed()
        {
            garageScreenAnimator.SetTrigger("Purchase Insufficient");
            audioSource.PlayOneShot(transactionFailed);

            swipeSelect.SetSwipeState(true);
        }
        
        public void SelectComp()
        {
            /*
            AutoData aData = autoProps[selectedAutoInt].data;

            gamingServicesManager.cloudData.unlockedCarsDict[aData.ItemCode].lastSelectedCompList[((int)compState)] = selectedCompInt;

            gamingServicesManager.SaveCloudData(false);

            garageScreenAnimator.SetTrigger("Selected");

            CheckInventory();
            */
        }

        public void SelectAuto()
        {
            /*
            gamingServicesManager.cloudData.lastSelectedCarInt = selectedAutoInt;

            gamingServicesManager.SaveCloudData(false);

            garageScreenAnimator.SetTrigger("Selected");

            CheckInventory();
            */
        }
        #endregion

        #endregion

        #region Loading Screens & UI States
        public void LoadingDoneMethod(Animator animator)
        {
            animator.SetBool("Loading", false);
        }

        public void SetLoadingScreenState(int i)
        {
            garageScreenAnimator.SetInteger("Loading Screen State", i);
        }

        private void SetCameraState(int i)
        {
            cameraAnimator.SetInteger("State", i);
        }

        private void SetUIState(int i)
        {
            garageScreenAnimator.SetInteger("State", i);
        }

        public void SetGarageState(int stateInt)
        {
            garageState = (GarageState)stateInt;          

            switch (garageState)
            {
                case GarageState.AutoReview:
                    SetUIState(0);
                    SetCameraState(0);
                    break;
                case GarageState.ComponentReview:
                    compState = 0;             
                    SetUIState(1);
                    SetCameraState(1);
                    break;
                case GarageState.CinematicView:
                    SetUIState(2);
                    SetCameraState(2);
                    break;
            }
        }

        public void SetComponentState(int stateInt)
        {
            compState = (ComponentState)stateInt;

            selectedCompInt = gamingServicesManager.cloudData.inventoryDict[autoProps[selectedAutoInt].data.ItemCode][EXMET.IntToCompClass((int)compState)].NextLevel();

            for (int i = 0; i < hud.compStatSliders.Count; i++)
            {
                if (i == ((int)compState))
                {
                    Outline outline = hud.compStatSliders[i].outline;
                    outline.enabled = true;
                }
                else
                {
                    Outline outline = hud.compStatSliders[i].outline;
                    outline.enabled = false;
                }
            }

            CheckInventory();
        }
        #endregion

        #region Utils
        private void CloudTransaction(int price)
        {
            int dollars = gamingServicesManager.cloudData.retroDollars;
            dollars -= price;

            gamingServicesManager.cloudData.retroDollars = dollars;
        }
        #endregion
    }

    public enum GarageState
    {
        AutoReview,
        ComponentReview,
        CinematicView,
    }

    public enum ComponentState
    {
        EngineReview,
        GearboxReview,
        TiresReview,
        ArmorReview,
    }

    [Serializable]
    public class Partlist
    {
        public List<ComponentProp> Comps = new List<ComponentProp>(0);
    }
}
