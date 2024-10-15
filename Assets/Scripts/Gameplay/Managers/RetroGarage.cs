 using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
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
        private List<CompList> compLists;
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
        public GarageState garageState = GarageState.AutoReview;
        [HideInInspector]
        public ComponentState compState = ComponentState.EngineReview;

        private void Awake()
        {
            Application.targetFrameRate = 60;
        }

        private void Update()
        {
            HandleAutoStats();
            HandleUICodedAnim();

            camOffset.Rotate(Vector3.up * 25f * Time.deltaTime);
            camOffset.rotation *= Quaternion.Euler(Vector3.up * swipeSelect.inputX * Time.deltaTime);

            compRotationParent.Rotate(Vector3.up * 25f * Time.deltaTime);
        }

        #region Core Garage Functionality

        #region Inventory Info & Stats
        private float statSliderLerpSpeed = 5f;
        private float autoTopSpeedVar, autoPowerVar, autoHandlingVar, autoHealthVar;
        private float bestTopSpeed, bestPower, bestHandling, bestHealth;
        private float worstTopSpeed, worstPower, worstHandling, worstHealth;
        private void HandleAutoStats()
        {
            AutoData currentAutodata = autoProps[selectedAutoInt].data;
            
            // FIND THE BEST & WORST VALUES AMONG THE CARS //
            for(int i = 0; i < autoProps.Length; i++)
                foreach(AutoLevelData ald in autoProps[i].data.autoLevelData)
                {
                    bestTopSpeed = Mathf.Max(bestTopSpeed, ald.TopSpeed);
                    bestPower = Mathf.Max(bestPower, ald.Power);
                    bestHandling = Mathf.Max(bestHandling, ald.Handling);
                    bestHealth = Mathf.Max(bestHealth, ald.MaxHealth);

                    worstTopSpeed = Mathf.Min(worstTopSpeed, ald.TopSpeed);
                    worstPower = Mathf.Min(worstPower, ald.Power);
                    worstHandling = Mathf.Min(worstHandling, ald.Handling);
                    worstHealth = Mathf.Min(worstHealth, ald.MaxHealth);
                }
            // FIND THE BEST & WORST VALUES AMONG THE CARS //

            // SET THE SLIDER MAX & MIN VALUES //
            hud.autoTopSpeedSlider.maxValue = Mathf.Lerp(hud.autoTopSpeedSlider.maxValue, bestTopSpeed, statSliderLerpSpeed * Time.deltaTime);
            hud.autoPowerSlider.maxValue = Mathf.Lerp(hud.autoPowerSlider.maxValue, bestPower, statSliderLerpSpeed * Time.deltaTime);
            hud.autoHandlingSlider.maxValue = Mathf.Lerp(hud.autoHandlingSlider.maxValue, bestHandling, statSliderLerpSpeed * Time.deltaTime);
            hud.autoHealthSlider.maxValue = Mathf.Lerp(hud.autoHealthSlider.maxValue, bestHealth, statSliderLerpSpeed * Time.deltaTime);

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

            // DOES THE PLAYER OWN THE CURRENT CAR? //
            if (gamingServicesManager.cloudData.inventoryDict.ContainsKey(currentAutodata.ItemCode))
            {
                autoTopSpeedVar = currentAutodata.autoLevelData[gamingServicesManager.cloudData.inventoryDict[currentAutodata.ItemCode]["engine"].currentLevel].TopSpeed;                
                autoPowerVar = currentAutodata.autoLevelData[gamingServicesManager.cloudData.inventoryDict[currentAutodata.ItemCode]["power"].currentLevel].Power;             
                autoHandlingVar = currentAutodata.autoLevelData[gamingServicesManager.cloudData.inventoryDict[currentAutodata.ItemCode]["handling"].currentLevel].Handling;
                autoHealthVar = currentAutodata.autoLevelData[gamingServicesManager.cloudData.inventoryDict[currentAutodata.ItemCode]["health"].currentLevel].MaxHealth;

                hud.compStatSliders[0].mainSlider.value = autoTopSpeedVar;
                hud.compStatSliders[0].diffSlider.value = compState == 0 ? 
                    currentAutodata.autoLevelData[gamingServicesManager.cloudData.inventoryDict[currentAutodata.ItemCode]["engine"].currentLevel + 1].TopSpeed :
                    autoTopSpeedVar;

                hud.compStatSliders[1].mainSlider.value = autoPowerVar;
                hud.compStatSliders[1].diffSlider.value = (int)compState == 1 ?
                    currentAutodata.autoLevelData[gamingServicesManager.cloudData.inventoryDict[currentAutodata.ItemCode]["power"].currentLevel + 1].Power :
                    autoPowerVar;

                hud.compStatSliders[2].mainSlider.value = autoHandlingVar;
                hud.compStatSliders[2].diffSlider.value = (int)compState == 2 ?
                    currentAutodata.autoLevelData[gamingServicesManager.cloudData.inventoryDict[currentAutodata.ItemCode]["handling"].currentLevel + 1].Handling :
                    autoHandlingVar;

                hud.compStatSliders[3].mainSlider.value = currentAutodata.autoLevelData[0].MaxHealth;
                hud.compStatSliders[3].diffSlider.value = (int)compState == 3 ?
                    currentAutodata.autoLevelData[gamingServicesManager.cloudData.inventoryDict[currentAutodata.ItemCode]["health"].currentLevel + 1].MaxHealth :
                    autoHealthVar;

                hud.autoTopSpeedInfo.text = $"{Mathf.RoundToInt(autoTopSpeedVar * 2.2f)} MPH";
                hud.autoPowerInfo.text = $"{autoPowerVar} UNITS";
                hud.autoHandlingInfo.text = $"{autoHandlingVar} M/S";
                hud.autoHealthInfo.text = $"{autoHealthVar} HP";
            }
            else
            {
                hud.autoTopSpeedInfo.text = $"{Mathf.RoundToInt(currentAutodata.autoLevelData[0].TopSpeed * 2.2f)} MPH";
                hud.autoPowerInfo.text = $"{currentAutodata.autoLevelData[0].Power} UNITS";
                hud.autoHandlingInfo.text = $"{currentAutodata.autoLevelData[0].Handling} M/S";
                hud.autoHealthInfo.text = $"{currentAutodata.autoLevelData[0].MaxHealth} HP";
            }

            // VALUES //
            hud.autoTopSpeedSlider.value = Mathf.Lerp(hud.autoTopSpeedSlider.value, autoTopSpeedVar, statSliderLerpSpeed * Time.deltaTime);
            hud.autoPowerSlider.value = Mathf.Lerp(hud.autoPowerSlider.value, autoPowerVar, statSliderLerpSpeed * Time.deltaTime);
            hud.autoHandlingSlider.value = Mathf.Lerp(hud.autoHandlingSlider.value, autoHandlingVar, statSliderLerpSpeed * Time.deltaTime);
            hud.autoHealthSlider.value = Mathf.Lerp(hud.autoHealthSlider.value, autoHealthVar, statSliderLerpSpeed * Time.deltaTime);
            // VALUES //
        }

        public void NextOption()
        {
            if (selectedAutoInt != autoProps.Length - 1) { selectedAutoInt++; }

            CheckInventory();
        }

        public void PreviousOption()
        {
            if (selectedAutoInt != 0) { selectedAutoInt--; }

            CheckInventory();
        }

        public void CheckInventory()
        {
            hud.previousAutoButton.SetActive(selectedAutoInt != 0);
            hud.nextAutoButton.SetActive(selectedAutoInt != autoProps.Length - 1);

            hud.playerMoneyText.text = $"R$ {gamingServicesManager.cloudData.retroDollars.ToString("N", EXMET.NumForThou)}";
            //UpdatePlayerMoneyText();

            #region Check Cars
            // SET ACTIVE ONLY SELECTED CAR, HIDE REST //
            for (int i = 0; i < autoProps.Length; i++)
                autoProps[i].gameObject.SetActive(autoProps[i] == autoProps[selectedAutoInt]);

            AutoData currentAutoData = autoProps[selectedAutoInt].data;

            if (gamingServicesManager.cloudData.inventoryDict.ContainsKey(currentAutoData.ItemCode))
            {
                hud.currentAutoNameText.text = currentAutoData.AutoName;

                autoProps[selectedAutoInt].lockedModel.SetActive(false);
                autoProps[selectedAutoInt].unlockedModels[0].SetActive(true);

                hud.unlockedActionParent_AR.SetActive(true);
                hud.lockedObjectParent_AR.SetActive(false);

                #region Check Components
                for (int i = 0; i < compLists.Count; i++)
                {
                    if (((int)compState) == i)
                    {
                        for (int j = 0; j < compLists[i].Comps.Count; j++)
                        {
                            ComponentProp prop = compLists[i].Comps[j];
                            AutoPartData partData = gamingServicesManager.cloudData.inventoryDict[currentAutoData.ItemCode][CompStateToPartsKey((int)compState)];
                            prop.gameObject.SetActive(j == partData.NextLevel());

                            ComponentProp currentProp = compLists[i].Comps[partData.currentLevel];
                            hud.currentCompNameText.text = currentProp.itemData.ItemName;

                            // CHECK ORDER STATUS //
                            if (gamingServicesManager.cloudData.inventoryDict[currentAutoData.ItemCode][CompStateToPartsKey((int)compState)].isDelivered)
                            {
                                hud.lockedActionParent_CR.SetActive(false);
                                hud.lockedObjectParent_CR.SetActive(false);
                            }
                            else
                            {
                                hud.lockedActionParent_CR.SetActive(true);
                                hud.lockedObjectParent_CR.SetActive(true);

                                int CompPrice = Mathf.RoundToInt(currentProp.itemData.DefaultPrice * autoProps[selectedAutoInt].data.CompPriceMultiplier);
                                hud.compPriceText.text = $"${CompPrice.ToString("N", EXMET.NumForThou)}";
                            }
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

                hud.autoPriceText.text = $"R$ {autoProps[selectedAutoInt].data.Price.ToString("N", EXMET.NumForThou)}";
            }
            #endregion
        }

        public string CompStateToPartsKey(int compState)
        {
            switch (compState)
            {
                case 0:
                    return "engine";

                case 1:
                    return "power";

                case 2:
                    return "handling";

                case 3:
                    return "health";
            }

            return "";
        }

        public void OnCloudDataReceived()
        {
            selectedAutoInt = gamingServicesManager.cloudData.lastSelectedCarInt;

            for (int i = 0; i < autoProps.Length; i++)
            {
                autoProps[i].gameObject.SetActive(autoProps[i] == autoProps[selectedAutoInt]);
            }

            CheckInventory();
        }

        #endregion

        #region Purchasing & Upgrading & Selecting
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
                    hud.lockedObjectParent_CR.SetActive(false);

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

            CheckInventory();
        }

        private void HandleUICodedAnim()
        {
            #region Component Buttons
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
            #endregion
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
    public class CompList
    {
        public List<ComponentProp> Comps = new List<ComponentProp>(0);
    }
}
