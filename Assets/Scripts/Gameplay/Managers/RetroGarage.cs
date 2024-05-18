using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RetroCode
{
    public class RetroGarage : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField]
        private SwipeSelect swipeSelect;
        [SerializeField]
        private GarageHUD hud;
        [SerializeField]
        private Animator garageScreenAnimator;

        [Header("Garage")]
        [SerializeField]
        private GameObject autoPropParent;
        [SerializeField]
        private GameObject compPropParent;
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
        public int selectedCompInt = 0;
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
        }

        #region Core Garage Functionality

        #region Inventory Info & Stats
        private float statLerpSpeed = 5f;
        private float topSpeedFlt, accelerationFlt, handlingFlt, armorFlt;
        private float maxTopSpeedFlt, maxAccelerationFlt, maxHandlingFlt, maxArmorFlt;
        private void HandleAutoStats()
        {
            AutoData data = autoProps[selectedAutoInt].data;

            //maxTopSpeedFlt = data.autoLevelData[data.autoLevelData.Length - 1].TopSpeed;
            //maxAccelerationFlt = -data.autoLevelData[data.autoLevelData.Length - 1].Acceleration;
            //maxHandlingFlt = data.autoLevelData[data.autoLevelData.Length - 1].TurnSpeed;
            //maxArmorFlt = data.autoLevelData[data.autoLevelData.Length - 1].MaxHealth;

            for(int i = 0; i < autoProps.Length; i++)
                foreach(AutoLevelData aLD in autoProps[i].data.autoLevelData)
                {
                    maxTopSpeedFlt = Mathf.Max(maxTopSpeedFlt, aLD.TopSpeed);
                    maxAccelerationFlt = Mathf.Max(maxAccelerationFlt, -aLD.Acceleration);
                    maxHandlingFlt = Mathf.Max(maxHandlingFlt, aLD.TurnSpeed);
                    maxArmorFlt = Mathf.Max(maxArmorFlt, aLD.MaxHealth);
                }

            hud.autoTopSpeedSlider.maxValue = Mathf.Lerp(hud.autoTopSpeedSlider.maxValue, maxTopSpeedFlt, statLerpSpeed * Time.deltaTime);
            hud.autoAccelerationSlider.maxValue = Mathf.Lerp(hud.autoAccelerationSlider.maxValue, maxAccelerationFlt, statLerpSpeed * Time.deltaTime);
            hud.autoHandlingSlider.maxValue = Mathf.Lerp(hud.autoHandlingSlider.maxValue, maxHandlingFlt, statLerpSpeed * Time.deltaTime);
            hud.autoHealthSlider.maxValue = Mathf.Lerp(hud.autoHealthSlider.maxValue, maxArmorFlt, statLerpSpeed * Time.deltaTime);

            if (gamingServicesManager.cloudData.unlockedCarsDict.ContainsKey(data.ItemCode))
            {
                topSpeedFlt = data.autoLevelData[gamingServicesManager.cloudData.unlockedCarsDict[data.ItemCode].lastSelectedCompList[0]].TopSpeed;                
                accelerationFlt = -data.autoLevelData[gamingServicesManager.cloudData.unlockedCarsDict[data.ItemCode].lastSelectedCompList[1]].Acceleration;             
                handlingFlt = data.autoLevelData[gamingServicesManager.cloudData.unlockedCarsDict[data.ItemCode].lastSelectedCompList[2]].TurnSpeed;
                armorFlt = data.autoLevelData[gamingServicesManager.cloudData.unlockedCarsDict[data.ItemCode].lastSelectedCompList[3]].MaxHealth;

                switch (compState)
                {
                    case ComponentState.EngineReview:
                        hud.CompStatSlider.minValue = 0f;
                        hud.CompStatSlider.maxValue = hud.autoTopSpeedSlider.maxValue;
                        hud.CompStatSlider.value = data.autoLevelData[0].TopSpeed;
                        hud.CompStatSliderDiff.minValue = 0f;
                        hud.CompStatSliderDiff.maxValue = hud.autoTopSpeedSlider.maxValue;
                        hud.CompStatSliderDiff.value = data.autoLevelData[selectedCompInt].TopSpeed;
                        break;
                    case ComponentState.GearboxReview:
                        hud.CompStatSlider.minValue = -10f;
                        hud.CompStatSlider.maxValue = hud.autoAccelerationSlider.maxValue;
                        hud.CompStatSlider.value = -data.autoLevelData[0].Acceleration;
                        hud.CompStatSliderDiff.minValue = -10f;
                        hud.CompStatSliderDiff.maxValue = hud.autoAccelerationSlider.maxValue;                       
                        hud.CompStatSliderDiff.value = -data.autoLevelData[selectedCompInt].Acceleration;
                        break;
                    case ComponentState.TiresReview:
                        hud.CompStatSlider.minValue = 0f;
                        hud.CompStatSlider.maxValue = hud.autoHandlingSlider.maxValue;
                        hud.CompStatSlider.value = data.autoLevelData[0].TurnSpeed;
                        hud.CompStatSliderDiff.minValue = 0f;
                        hud.CompStatSliderDiff.maxValue = hud.autoHandlingSlider.maxValue;
                        hud.CompStatSliderDiff.value = data.autoLevelData[selectedCompInt].TurnSpeed;
                        break;
                    case ComponentState.ArmorReview:
                        hud.CompStatSlider.minValue = 0f;
                        hud.CompStatSlider.maxValue = hud.autoHealthSlider.maxValue;
                        hud.CompStatSlider.value = data.autoLevelData[0].MaxHealth;
                        hud.CompStatSliderDiff.minValue = 0f;
                        hud.CompStatSliderDiff.maxValue = hud.autoHealthSlider.maxValue;
                        hud.CompStatSliderDiff.value = data.autoLevelData[selectedCompInt].MaxHealth;
                        break;
                }
            }
            else
            {
                topSpeedFlt = data.autoLevelData[0].TopSpeed;
                accelerationFlt = -data.autoLevelData[0].Acceleration;
                handlingFlt = data.autoLevelData[0].TurnSpeed;
                armorFlt = data.autoLevelData[0].MaxHealth;
            }

            // VALUES //
            hud.autoTopSpeedSlider.value = Mathf.Lerp(hud.autoTopSpeedSlider.value, topSpeedFlt, statLerpSpeed * Time.deltaTime);
            hud.autoAccelerationSlider.value = Mathf.Lerp(hud.autoAccelerationSlider.value, accelerationFlt, statLerpSpeed * Time.deltaTime);
            hud.autoHandlingSlider.value = Mathf.Lerp(hud.autoHandlingSlider.value, handlingFlt, statLerpSpeed * Time.deltaTime);
            hud.autoHealthSlider.value = Mathf.Lerp(hud.autoHealthSlider.value, armorFlt, statLerpSpeed * Time.deltaTime);

            // COLORS //
            hud.autoTopSpeedSliderFill.color = 
                hud.sliderGradient.Evaluate(hud.autoTopSpeedSlider.value / hud.autoTopSpeedSlider.maxValue);
            hud.autoAccelerationSliderFill.color =
                hud.sliderGradient.Evaluate(1f - (-hud.autoAccelerationSlider.value / -hud.autoAccelerationSlider.minValue));
            hud.autoHandlingSliderFill.color =
                hud.sliderGradient.Evaluate(hud.autoHandlingSlider.value / hud.autoHandlingSlider.maxValue);
            hud.autoHealthSliderFill.color =
                hud.sliderGradient.Evaluate(hud.autoHealthSlider.value / hud.autoHealthSlider.maxValue);
        }

        public void SwipeNext()
        {
            if(garageState == GarageState.AutoReview)
            {
                if (selectedAutoInt != autoProps.Length - 1) 
                { 
                    selectedAutoInt++;
                    CheckInventory();
                    return;
                }
            }

            if(garageState == GarageState.ComponentReview)
            {
                if (selectedCompInt != compLists[((int)compState)].Comps.Count - 1) 
                { 
                    selectedCompInt++;
                    CheckInventory();
                    return;
                }
            }
        }

        public void SwipePrevious()
        {
            if(garageState == GarageState.AutoReview)
            {
                if (selectedAutoInt != 0) { selectedAutoInt--; }
            }

            if (garageState == GarageState.ComponentReview)
            {
                if (selectedCompInt != 0) { selectedCompInt--; }
            }

            CheckInventory();
        }

        public void CheckInventory()
        {
            hud.playerMoneyText.text = $"${gamingServicesManager.cloudData.retroDollars.ToString("n0")}";

            #region Check Cars
            for (int i = 0; i < autoProps.Length; i++)
            {
                autoProps[i].gameObject.SetActive(autoProps[i] == autoProps[selectedAutoInt]);
            }

            AutoData c_autoD = autoProps[selectedAutoInt].data;

            hud.selectButton_AR.SetActive(selectedAutoInt != gamingServicesManager.cloudData.lastSelectedCarInt);
            hud.selectedObject_AR.SetActive(selectedAutoInt == gamingServicesManager.cloudData.lastSelectedCarInt);

            if (gamingServicesManager.cloudData.unlockedCarsDict.ContainsKey(c_autoD.ItemCode))
            {
                hud.currentAutoNameText.text = c_autoD.AutoName;

                hud.autoNameText_CR.text = c_autoD.AutoName;
                autoProps[selectedAutoInt].lockedModel.SetActive(false);
                autoProps[selectedAutoInt].unlockedModels[0].SetActive(true);

                hud.unlockedActionParent_AR.SetActive(true);
                hud.lockedActionParent_AR.SetActive(false);
                hud.lockedObjectParent_AR.SetActive(false);

                #region Check Components
                for (int i = 0; i < compLists.Count; i++)
                {
                    if (((int)compState) == i)
                    {
                        for (int j = 0; j < compLists[i].Comps.Count; j++)
                        {
                            ComponentProp prop = compLists[i].Comps[j];
                            prop.gameObject.SetActive(j == selectedCompInt);

                            ComponentProp selectedProp = compLists[i].Comps[selectedCompInt];
                            hud.currentCompNameText_Unlocked.text = selectedProp.itemData.ItemName;
                            hud.currentCompNameText_Locked.text = selectedProp.itemData.ItemName;

                            hud.selectButton_CR.SetActive(selectedCompInt != gamingServicesManager.cloudData.unlockedCarsDict[c_autoD.ItemCode].lastSelectedCompList[(int)compState]);
                            hud.selectedObject_CR.SetActive(selectedCompInt == gamingServicesManager.cloudData.unlockedCarsDict[c_autoD.ItemCode].lastSelectedCompList[(int)compState]);

                            // CHECK OWNERSHIP //
                            if (gamingServicesManager.cloudData.unlockedCarsDict[c_autoD.ItemCode].compCodes[((int)compState)].Contains($"{selectedProp.itemData.ItemCode}"))
                            {
                                hud.unlockedActionParent_CR.SetActive(true);
                                hud.lockedActionParent_CR.SetActive(false);
                                hud.lockedObjectParent_CR.SetActive(false);
                            }
                            else
                            {
                                hud.unlockedActionParent_CR.SetActive(false);
                                hud.lockedActionParent_CR.SetActive(true);
                                hud.lockedObjectParent_CR.SetActive(true);

                                int CompPrice = Mathf.RoundToInt(selectedProp.itemData.DefaultPrice * autoProps[selectedAutoInt].data.CompPriceMultiplier);
                                hud.compPriceText.text = $"${CompPrice.ToString("n0")}";
                            }
                        }

                        switch (i)
                        {
                            case 0:
                                hud.newStatTitle.text = "Top Speed";
                                break;
                            case 1:
                                hud.newStatTitle.text = "Acceleration";
                                break;
                            case 2:
                                hud.newStatTitle.text = "Handling";
                                break;
                            case 3:
                                hud.newStatTitle.text = "Health";
                                break;
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
                hud.currentAutoNameText.text = c_autoD.AutoName;
                autoProps[selectedAutoInt].lockedModel.SetActive(true);
                autoProps[selectedAutoInt].unlockedModels[0].SetActive(false);

                hud.unlockedActionParent_AR.SetActive(false);
                hud.lockedActionParent_AR.SetActive(true);
                hud.lockedObjectParent_AR.SetActive(true);

                hud.autoPriceText.text = $"${autoProps[selectedAutoInt].data.Price.ToString("n0")}";
            }
            #endregion
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

        public void PlayerMoneyChanged()
        {
            StartCoroutine(PlayerMoneyChangedRoutine());
        }

        private IEnumerator PlayerMoneyChangedRoutine()
        {
            while (true)
            {
                // CBT //
            }
        }

        public void UpdateAbilityInfo()
        {
            AutoData c_autoD = autoProps[selectedAutoInt].data;
            hud.abilityTitleText.text = c_autoD.Ability.abilityName;
            hud.abilityInfoText.text = c_autoD.AbilityInfo;
        }
        #endregion

        #region Purchasing & Upgrading & Selecting
        public void PrepTransaction()
        {
            bool component = garageState == GarageState.ComponentReview;

            ItemData iData = compLists[((int)compState)].Comps[selectedCompInt].itemData;
            AutoData aData = autoProps[selectedAutoInt].data;

            if (!component)
                hud.purchaseConfirmText.text = $"Purchase {aData.AutoName} for ${aData.Price.ToString("n0")}?";
            else
                hud.purchaseConfirmText.text = $"Purchase {iData.ItemName} for ${Mathf.RoundToInt(iData.DefaultPrice * aData.CompPriceMultiplier).ToString("n0")}?";
        }

        public void ProcessTransaction()
        {
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
        }

        private void CompleteTransaction(bool component, string AutoCode, string CompCode)
        {
            if (!component)
            {
                gamingServicesManager.cloudData.unlockedCarsDict.Add(AutoCode, new CloudAutoData());

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
        }

        private void TransactionFailed()
        {
            garageScreenAnimator.SetTrigger("Purchase Insufficient");
            audioSource.PlayOneShot(transactionFailed);

            swipeSelect.SetSwipeState(true);
        }
        
        public void SelectComp()
        {
            AutoData aData = autoProps[selectedAutoInt].data;

            gamingServicesManager.cloudData.unlockedCarsDict[aData.ItemCode].lastSelectedCompList[((int)compState)] = selectedCompInt;

            gamingServicesManager.SaveCloudData(false);

            garageScreenAnimator.SetTrigger("Selected");

            CheckInventory();
        }

        public void SelectAuto()
        {
            gamingServicesManager.cloudData.lastSelectedCarInt = selectedAutoInt;

            gamingServicesManager.SaveCloudData(false);

            garageScreenAnimator.SetTrigger("Selected");

            CheckInventory();
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

                    autoPropParent.SetActive(true);
                    compPropParent.SetActive(false);
                    break;
                case GarageState.ComponentReview:
                    compState = 0;

                    SetUIState(1);
                    SetCameraState(1);

                    autoPropParent.SetActive(false);
                    compPropParent.SetActive(true);
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
            selectedCompInt = gamingServicesManager.cloudData.unlockedCarsDict[autoProps[selectedAutoInt].data.ItemCode].lastSelectedCompList[((int)compState)];

            CheckInventory();
        }

        private void HandleUICodedAnim()
        {
            #region Component Buttons
            for (int i = 0; i < hud.compButtons.Length; i++)
            {
                if (i == ((int)compState))
                {
                    RectTransform rT = hud.compButtons[i];
                    float rTY = rT.transform.localPosition.y;

                    rT.transform.localPosition = Vector3.Lerp(
                        rT.transform.localPosition,
                        new Vector3(hud.compRectSelectedX, rTY, 0f),
                        4f * Time.deltaTime
                        );
                }
                else
                {
                    RectTransform rT = hud.compButtons[i];
                    float rTY = rT.transform.localPosition.y;

                    rT.transform.localPosition = Vector3.Lerp(
                        rT.transform.localPosition,
                        new Vector3(hud.compRectDefaultX, rTY, 0f),
                        4f * Time.deltaTime
                        );
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
