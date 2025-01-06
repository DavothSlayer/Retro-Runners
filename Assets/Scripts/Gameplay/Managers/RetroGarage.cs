using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using UnityEditor.Sequences;
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

        [Header("Garage")]
        [SerializeField]
        private Transform camOffset;
        [SerializeField]
        private GameObject autoPropParent;
        [SerializeField]
        private Transform compRotationParent;
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
        private float autoTopSpeedVar, autoTorqueVar, autoHandlingVar, autoHealthVar, autoPowerVar;
        private float bestTopSpeed, bestTorque, bestHandling, bestHealth, bestPower;
        private void InitConstants()
        {
            AutoData currentAutoData = autoProps[selectedAutoInt].data;

            // FIND THE BESTVALUES AMONG(US) THE CARS //
            for (int i = 0; i < autoProps.Length; i++)
                foreach (AutoLevelData ald in autoProps[i].data.autoLevelData)
                {
                    bestTopSpeed = Mathf.Max(bestTopSpeed, ald.TopSpeed);
                    bestTorque = Mathf.Max(bestTorque, ald.Torque);
                    bestHandling = Mathf.Max(bestHandling, ald.Handling);
                    bestHealth = Mathf.Max(bestHealth, ald.MaxHealth);
                    bestPower = Mathf.Max(bestPower, ald.Power);
                }
            // FIND THE BEST VALUES AMONG(US) THE CARS //

            // SET THE SLIDER MAX & MIN VALUES //
            hud.autoTopSpeedSlider.maxValue = bestTopSpeed;
            hud.autoGearboxSlider.maxValue = bestTorque;
            hud.autoHandlingSlider.maxValue = bestHandling;
            hud.autoHealthSlider.maxValue = bestHealth;
            hud.autoPowerSlider.maxValue = bestPower;

            hud.compStatSliders[0].mainSlider.maxValue = bestTopSpeed;
            hud.compStatSliders[0].diffSlider.maxValue = bestTopSpeed;

            hud.compStatSliders[1].mainSlider.maxValue = bestTorque;
            hud.compStatSliders[1].diffSlider.maxValue = bestTorque;

            hud.compStatSliders[2].mainSlider.maxValue = bestHandling;
            hud.compStatSliders[2].diffSlider.maxValue = bestHandling;

            hud.compStatSliders[3].mainSlider.maxValue = bestHealth;
            hud.compStatSliders[3].diffSlider.maxValue = bestHealth;

            hud.compStatSliders[4].mainSlider.maxValue = bestPower;
            hud.compStatSliders[4].diffSlider.maxValue = bestPower;

            hud.autoTopSpeedSlider_C.maxValue = bestTopSpeed;
            hud.autoGearboxSlider_C.maxValue = bestTorque;
            hud.autoHandlingSlider_C.maxValue = bestHandling;
            hud.autoHealthSlider_C.maxValue = bestHealth;
            hud.autoPowerSlider_C.maxValue = bestPower;
            // SET THE SLIDER MAX & MIN VALUES //
        }

        #region Inventory Info & Stats
        private float statSliderLerpSpeed = 5f;
        private void HandleAutoSliders()
        {
            // VALUES //
            hud.autoTopSpeedSlider.value = Mathf.Lerp(hud.autoTopSpeedSlider.value, autoTopSpeedVar, statSliderLerpSpeed * Time.deltaTime);
            hud.autoGearboxSlider.value = Mathf.Lerp(hud.autoGearboxSlider.value, autoTorqueVar, statSliderLerpSpeed * Time.deltaTime);
            hud.autoHandlingSlider.value = Mathf.Lerp(hud.autoHandlingSlider.value, autoHandlingVar, statSliderLerpSpeed * Time.deltaTime);
            hud.autoHealthSlider.value = Mathf.Lerp(hud.autoHealthSlider.value, autoHealthVar, statSliderLerpSpeed * Time.deltaTime);
            hud.autoPowerSlider.value = Mathf.Lerp(hud.autoPowerSlider.value, autoPowerVar, statSliderLerpSpeed * Time.deltaTime);
            // VALUES //
        }

        public void CheckInventory()
        {
            AutoData currentAutoData = autoProps[selectedAutoInt].data;

            hud.previousAutoButton.SetActive(selectedAutoInt != 0);
            hud.nextAutoButton.SetActive(selectedAutoInt != autoProps.Length - 1);
            hud.openLootButtonMain.SetActive(gamingServicesManager.lootingDictionary.Keys.Count != 0);
            hud.autoTierText.text = $"TIER {EXMET.LevelAsRoman(currentAutoData.Tier)}";

            UpdatePlayerMoney(gamingServicesManager.cloudData.RetroDollars);

            #region Check Cars
            // SET ACTIVE ONLY SELECTED CAR, HIDE REST //
            for (int i = 0; i < autoProps.Length; i++)
                autoProps[i].gameObject.SetActive(autoProps[i] == autoProps[selectedAutoInt]);

            if (gamingServicesManager.cloudData.inventoryDict.ContainsKey(currentAutoData.ItemCode))
            {
                AutoPartData partData = gamingServicesManager.cloudData.inventoryDict[currentAutoData.ItemCode][EXMET.IntToCompClass((int)compState)];

                hud.currentAutoNameText.text = currentAutoData.AutoName;

                autoProps[selectedAutoInt].lockedModel.SetActive(false);
                autoProps[selectedAutoInt].unlockedModel.SetActive(true);

                hud.upgradeCarButton.SetActive(true);
                hud.purchaseCarButton.SetActive(false);
                hud.lockedCarInfo.SetActive(false);
                //hud.autoSelectedIcon.SetActive(gamingServicesManager.cloudData.LastSelectedCarIndex == selectedAutoInt);

                autoTopSpeedVar = currentAutoData.autoLevelData[gamingServicesManager.cloudData.inventoryDict[currentAutoData.ItemCode]["topspeed"].EquippedLevel].TopSpeed;
                autoTorqueVar = currentAutoData.autoLevelData[gamingServicesManager.cloudData.inventoryDict[currentAutoData.ItemCode]["torque"].EquippedLevel].Torque;
                autoHandlingVar = currentAutoData.autoLevelData[gamingServicesManager.cloudData.inventoryDict[currentAutoData.ItemCode]["handling"].EquippedLevel].Handling;
                autoHealthVar = currentAutoData.autoLevelData[gamingServicesManager.cloudData.inventoryDict[currentAutoData.ItemCode]["health"].EquippedLevel].MaxHealth;
                autoPowerVar = currentAutoData.autoLevelData[gamingServicesManager.cloudData.inventoryDict[currentAutoData.ItemCode]["power"].EquippedLevel].Power;

                // AUTO VIEW STATS //
                hud.compStatSliders[0].mainSlider.value = autoTopSpeedVar;
                hud.compStatSliders[0].diffSlider.value = currentAutoData.autoLevelData[gamingServicesManager.cloudData.inventoryDict[currentAutoData.ItemCode]["topspeed"].NextLevel()].TopSpeed;

                hud.compStatSliders[1].mainSlider.value = autoTorqueVar;
                hud.compStatSliders[1].diffSlider.value = currentAutoData.autoLevelData[gamingServicesManager.cloudData.inventoryDict[currentAutoData.ItemCode]["torque"].NextLevel()].Torque;

                hud.compStatSliders[2].mainSlider.value = autoHandlingVar;
                hud.compStatSliders[2].diffSlider.value = currentAutoData.autoLevelData[gamingServicesManager.cloudData.inventoryDict[currentAutoData.ItemCode]["handling"].NextLevel()].Handling;

                hud.compStatSliders[3].mainSlider.value = autoHealthVar;
                hud.compStatSliders[3].diffSlider.value = currentAutoData.autoLevelData[gamingServicesManager.cloudData.inventoryDict[currentAutoData.ItemCode]["health"].NextLevel()].MaxHealth;

                hud.compStatSliders[4].mainSlider.value = autoPowerVar;
                hud.compStatSliders[4].diffSlider.value = currentAutoData.autoLevelData[gamingServicesManager.cloudData.inventoryDict[currentAutoData.ItemCode]["power"].NextLevel()].Power;

                hud.autoTopSpeedInfo.text = $"{Mathf.RoundToInt(autoTopSpeedVar * (SettingsManager.Instance.settings.SpeedUnitIsKMH ? 3.6f : 2.2f))} {(SettingsManager.Instance.settings.SpeedUnitIsKMH ? "KMH" : "MPH")}";
                hud.autoGearboxInfo.text = $"{autoTorqueVar} TQ";
                hud.autoHandlingInfo.text = $"{autoHandlingVar} GR";
                hud.autoHealthInfo.text = $"{autoHealthVar} HP";
                hud.autoPowerInfo.text = $"{autoPowerVar} CORES";
                // AUTO VIEW STATS //

                // COMPONENT VIEW STATS //
                hud.compStatSliders[0].statTunedText.gameObject.SetActive(
                    gamingServicesManager.cloudData.inventoryDict[currentAutoData.ItemCode]["topspeed"].EquippedLevel <
                    gamingServicesManager.cloudData.inventoryDict[currentAutoData.ItemCode]["topspeed"].NextLevel()
                    );
                hud.compStatSliders[0].arrowImage.gameObject.SetActive(
                    gamingServicesManager.cloudData.inventoryDict[currentAutoData.ItemCode]["topspeed"].EquippedLevel <
                    gamingServicesManager.cloudData.inventoryDict[currentAutoData.ItemCode]["topspeed"].NextLevel()
                    );
                hud.compStatSliders[0].statCurrentText.text = $"{Mathf.RoundToInt(currentAutoData.autoLevelData[gamingServicesManager.cloudData.inventoryDict[currentAutoData.ItemCode]["topspeed"].EquippedLevel].TopSpeed * (SettingsManager.Instance.settings.SpeedUnitIsKMH ? 3.6f : 2.2f))} {(SettingsManager.Instance.settings.SpeedUnitIsKMH ? "KMH" : "MPH")}";
                hud.compStatSliders[0].statTunedText.text = $"{Mathf.RoundToInt(currentAutoData.autoLevelData[gamingServicesManager.cloudData.inventoryDict[currentAutoData.ItemCode]["topspeed"].NextLevel()].TopSpeed * (SettingsManager.Instance.settings.SpeedUnitIsKMH ? 3.6f : 2.2f))} {(SettingsManager.Instance.settings.SpeedUnitIsKMH ? "KMH" : "MPH")}";
                hud.compStatSliders[0].levelText.text = $"MK. {EXMET.LevelAsRoman(gamingServicesManager.cloudData.inventoryDict[currentAutoData.ItemCode]["topspeed"].EquippedLevel)}";
                hud.compStatSliders[0].icon.sprite = compLists[0].Comps[gamingServicesManager.cloudData.inventoryDict[currentAutoData.ItemCode]["topspeed"].EquippedLevel].itemData.icon;

                hud.compStatSliders[1].statTunedText.gameObject.SetActive(
                    gamingServicesManager.cloudData.inventoryDict[currentAutoData.ItemCode]["torque"].EquippedLevel <
                    gamingServicesManager.cloudData.inventoryDict[currentAutoData.ItemCode]["torque"].NextLevel()
                    );
                hud.compStatSliders[1].arrowImage.gameObject.SetActive(
                    gamingServicesManager.cloudData.inventoryDict[currentAutoData.ItemCode]["torque"].EquippedLevel <
                    gamingServicesManager.cloudData.inventoryDict[currentAutoData.ItemCode]["torque"].NextLevel()
                    );
                hud.compStatSliders[1].statCurrentText.text = $"{currentAutoData.autoLevelData[gamingServicesManager.cloudData.inventoryDict[currentAutoData.ItemCode]["torque"].EquippedLevel].Torque} TQ";
                hud.compStatSliders[1].statTunedText.text = $"{currentAutoData.autoLevelData[gamingServicesManager.cloudData.inventoryDict[currentAutoData.ItemCode]["torque"].NextLevel()].Torque} TQ";
                hud.compStatSliders[1].levelText.text = $"MK. {EXMET.LevelAsRoman(gamingServicesManager.cloudData.inventoryDict[currentAutoData.ItemCode]["torque"].EquippedLevel)}";
                hud.compStatSliders[1].icon.sprite = compLists[1].Comps[gamingServicesManager.cloudData.inventoryDict[currentAutoData.ItemCode]["torque"].EquippedLevel].itemData.icon;

                hud.compStatSliders[2].statTunedText.gameObject.SetActive(
                    gamingServicesManager.cloudData.inventoryDict[currentAutoData.ItemCode]["handling"].EquippedLevel <
                    gamingServicesManager.cloudData.inventoryDict[currentAutoData.ItemCode]["handling"].NextLevel()
                    );
                hud.compStatSliders[2].arrowImage.gameObject.SetActive(
                    gamingServicesManager.cloudData.inventoryDict[currentAutoData.ItemCode]["handling"].EquippedLevel <
                    gamingServicesManager.cloudData.inventoryDict[currentAutoData.ItemCode]["handling"].NextLevel()
                    );
                hud.compStatSliders[2].statCurrentText.text = $"{currentAutoData.autoLevelData[gamingServicesManager.cloudData.inventoryDict[currentAutoData.ItemCode]["handling"].EquippedLevel].Handling} GR";
                hud.compStatSliders[2].statTunedText.text = $"{currentAutoData.autoLevelData[gamingServicesManager.cloudData.inventoryDict[currentAutoData.ItemCode]["handling"].NextLevel()].Handling} GR";
                hud.compStatSliders[2].levelText.text = $"MK. {EXMET.LevelAsRoman(gamingServicesManager.cloudData.inventoryDict[currentAutoData.ItemCode]["handling"].EquippedLevel)}";
                hud.compStatSliders[2].icon.sprite = compLists[2].Comps[gamingServicesManager.cloudData.inventoryDict[currentAutoData.ItemCode]["handling"].EquippedLevel].itemData.icon;

                hud.compStatSliders[3].statTunedText.gameObject.SetActive(
                    gamingServicesManager.cloudData.inventoryDict[currentAutoData.ItemCode]["health"].EquippedLevel <
                    gamingServicesManager.cloudData.inventoryDict[currentAutoData.ItemCode]["health"].NextLevel()
                    );
                hud.compStatSliders[3].arrowImage.gameObject.SetActive(
                    gamingServicesManager.cloudData.inventoryDict[currentAutoData.ItemCode]["health"].EquippedLevel <
                    gamingServicesManager.cloudData.inventoryDict[currentAutoData.ItemCode]["health"].NextLevel()
                    );
                hud.compStatSliders[3].statCurrentText.text = $"{currentAutoData.autoLevelData[gamingServicesManager.cloudData.inventoryDict[currentAutoData.ItemCode]["health"].EquippedLevel].MaxHealth} HP";
                hud.compStatSliders[3].statTunedText.text = $"{currentAutoData.autoLevelData[gamingServicesManager.cloudData.inventoryDict[currentAutoData.ItemCode]["health"].NextLevel()].MaxHealth} HP";
                hud.compStatSliders[3].levelText.text = $"MK. {EXMET.LevelAsRoman(gamingServicesManager.cloudData.inventoryDict[currentAutoData.ItemCode]["health"].EquippedLevel)}";
                hud.compStatSliders[3].icon.sprite = compLists[3].Comps[gamingServicesManager.cloudData.inventoryDict[currentAutoData.ItemCode]["health"].EquippedLevel].itemData.icon;

                hud.compStatSliders[4].statTunedText.gameObject.SetActive(
                    gamingServicesManager.cloudData.inventoryDict[currentAutoData.ItemCode]["power"].EquippedLevel <
                    gamingServicesManager.cloudData.inventoryDict[currentAutoData.ItemCode]["power"].NextLevel()
                    );
                hud.compStatSliders[4].arrowImage.gameObject.SetActive(
                    gamingServicesManager.cloudData.inventoryDict[currentAutoData.ItemCode]["power"].EquippedLevel <
                    gamingServicesManager.cloudData.inventoryDict[currentAutoData.ItemCode]["power"].NextLevel()
                    );
                hud.compStatSliders[4].statCurrentText.text = $"{currentAutoData.autoLevelData[gamingServicesManager.cloudData.inventoryDict[currentAutoData.ItemCode]["power"].EquippedLevel].Power} CORES";
                hud.compStatSliders[4].statTunedText.text = $"{currentAutoData.autoLevelData[gamingServicesManager.cloudData.inventoryDict[currentAutoData.ItemCode]["power"].NextLevel()].Power} CORES";
                hud.compStatSliders[4].levelText.text = $"MK. {EXMET.LevelAsRoman(gamingServicesManager.cloudData.inventoryDict[currentAutoData.ItemCode]["power"].EquippedLevel)}";
                hud.compStatSliders[4].icon.sprite = compLists[4].Comps[gamingServicesManager.cloudData.inventoryDict[currentAutoData.ItemCode]["power"].EquippedLevel].itemData.icon;
                // COMPONENT VIEW STATS //

                hud.previousCompButton.SetActive(selectedCompInt != 0);
                hud.nextCompButton.SetActive(selectedCompInt != partData.NextLevel());

                hud.deliveryTimeAction.SetActive(DeliveryInProgress(currentAutoData, (int)compState) && selectedCompInt > partData.CurrentLevel && !partData.isLooted);
                hud.inDeliveryIcon.SetActive(hud.deliveryTimeAction.activeInHierarchy);

                hud.orderCompButton.SetActive(!DeliveryInProgress(currentAutoData, (int)compState) && selectedCompInt > partData.CurrentLevel && partData.isLooted);
                hud.compPriceText.gameObject.SetActive(hud.orderCompButton.activeInHierarchy);

                hud.equipCompButton.SetActive(selectedCompInt != partData.EquippedLevel && selectedCompInt <= partData.CurrentLevel);
                hud.equippedCompInfo.SetActive(selectedCompInt == partData.EquippedLevel && selectedCompInt <= partData.CurrentLevel);

                hud.openLootButtonAux.SetActive(!DeliveryInProgress(currentAutoData, (int)compState) && selectedCompInt > partData.CurrentLevel && !partData.isLooted);

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
                            
                            hud.compPriceText.color = gamingServicesManager.cloudData.RetroDollars >= CompPrice ? hud.availableColor : hud.lockedColor;
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
                autoProps[selectedAutoInt].lockedModel.SetActive(gamingServicesManager.cloudData.RetroDollars < autoProps[selectedAutoInt].data.Price);
                autoProps[selectedAutoInt].unlockedModel.SetActive(gamingServicesManager.cloudData.RetroDollars >= autoProps[selectedAutoInt].data.Price);

                hud.upgradeCarButton.SetActive(false);
                hud.purchaseCarButton.SetActive(true);
                hud.lockedCarInfo.SetActive(true);
                //hud.autoSelectedIcon.SetActive(false);

                hud.autoLockImage.sprite = gamingServicesManager.cloudData.RetroDollars >= currentAutoData.Price ? hud.unlockedSprite : hud.lockedSprite;
                hud.autoLockImage.color = gamingServicesManager.cloudData.RetroDollars >= currentAutoData.Price ? hud.availableColor : hud.lockedColor;
                hud.autoPriceText.color = gamingServicesManager.cloudData.RetroDollars >= currentAutoData.Price ? hud.availableColor : hud.lockedColor;

                autoTopSpeedVar = currentAutoData.autoLevelData[0].TopSpeed;
                autoTorqueVar = currentAutoData.autoLevelData[0].Torque;
                autoHandlingVar = currentAutoData.autoLevelData[0].Handling;
                autoHealthVar = currentAutoData.autoLevelData[0].MaxHealth;
                autoPowerVar = currentAutoData.autoLevelData[0].Power;

                hud.autoTopSpeedInfo.text = $"{Mathf.RoundToInt(currentAutoData.autoLevelData[0].TopSpeed * (SettingsManager.Instance.settings.SpeedUnitIsKMH ? 3.6f : 2.2f))} {(SettingsManager.Instance.settings.SpeedUnitIsKMH ? "KMH" : "MPH")}";
                hud.autoGearboxInfo.text = $"{currentAutoData.autoLevelData[0].Torque} TQ";
                hud.autoHandlingInfo.text = $"{currentAutoData.autoLevelData[0].Handling} GR";
                hud.autoHealthInfo.text = $"{currentAutoData.autoLevelData[0].MaxHealth} HP";
                hud.autoPowerInfo.text = $"{currentAutoData.autoLevelData[0].Power} CORES";

                hud.autoPriceText.text = $"R$ {autoProps[selectedAutoInt].data.Price.ToString("N", EXMET.NumForThou)}";
            }
            #endregion
        }

        private int lastAmount = 0;
        public void UpdatePlayerMoney(int newAmount)
        {
            LeanTween.value(lastAmount, newAmount, 1f)
                .setOnUpdate((float value) =>
                {
                    hud.playerMoneyText.text = $"R$ {Mathf.RoundToInt(value).ToString("N", EXMET.NumForThou)}";
                });

            lastAmount = newAmount;
        }
        #endregion

        #region Purchasing & Upgrading & Selecting
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

        private float bestVar;
        public void OrderAutoPart()
        {
            AutoData autoData = autoProps[selectedAutoInt].data;
            AutoPartData autoPartData = gamingServicesManager.cloudData.inventoryDict[autoData.ItemCode][EXMET.IntToCompClass((int)compState)];

            hud.upgradeConfAutoNameText.text = autoData.AutoName;
            hud.upgradeConfCompTitleText.text = EXMET.IntToCompClass((int)compState);

            hud.upgradeConfCurrentIcon.sprite = compLists[(int)compState].Comps[autoPartData.CurrentLevel].itemData.icon;
            hud.upgradeConfCurrentLvlText.text = $"MK. {EXMET.LevelAsRoman(autoPartData.CurrentLevel)}";

            hud.upgradeConfNextIcon.sprite = compLists[(int)compState].Comps[autoPartData.NextLevel()].itemData.icon;
            hud.upgradeConfNextLvlText.text = $"MK. {EXMET.LevelAsRoman(autoPartData.NextLevel())}";

            switch ((int)compState)
            {
                case 0:
                    bestVar = bestTopSpeed;

                    hud.upgradeConfMainSlider.maxValue = bestVar;
                    hud.upgradeConfDiffSlider.maxValue = bestVar;

                    hud.upgradeConfCurrentStatText.text = $"{Mathf.RoundToInt(autoData.autoLevelData[autoPartData.EquippedLevel].TopSpeed * (SettingsManager.Instance.settings.SpeedUnitIsKMH ? 3.6f : 2.2f))} {(SettingsManager.Instance.settings.SpeedUnitIsKMH ? "KMH" : "MPH")}";
                    hud.upgradeConfNextStatText.text = $"{Mathf.RoundToInt(autoData.autoLevelData[autoPartData.NextLevel()].TopSpeed * (SettingsManager.Instance.settings.SpeedUnitIsKMH ? 3.6f : 2.2f))} {(SettingsManager.Instance.settings.SpeedUnitIsKMH ? "KMH" : "MPH")}";

                    hud.upgradeConfMainSlider.value = autoData.autoLevelData[autoPartData.EquippedLevel].TopSpeed;
                    hud.upgradeConfDiffSlider.value = autoData.autoLevelData[autoPartData.NextLevel()].TopSpeed;
                    break;
                case 1:
                    bestVar = bestTorque;

                    hud.upgradeConfMainSlider.maxValue = bestVar;
                    hud.upgradeConfDiffSlider.maxValue = bestVar;

                    hud.upgradeConfCurrentStatText.text = $"{Mathf.RoundToInt(autoData.autoLevelData[autoPartData.EquippedLevel].Torque)} TQ";
                    hud.upgradeConfNextStatText.text = $"{Mathf.RoundToInt(autoData.autoLevelData[autoPartData.NextLevel()].Torque)} TQ";

                    hud.upgradeConfMainSlider.value = autoData.autoLevelData[autoPartData.EquippedLevel].Torque;
                    hud.upgradeConfDiffSlider.value = autoData.autoLevelData[autoPartData.NextLevel()].Torque;
                    break;
                case 2:
                    bestVar = bestHandling;

                    hud.upgradeConfMainSlider.maxValue = bestVar;
                    hud.upgradeConfDiffSlider.maxValue = bestVar;

                    hud.upgradeConfCurrentStatText.text = $"{Mathf.RoundToInt(autoData.autoLevelData[autoPartData.EquippedLevel].Handling)} GR";
                    hud.upgradeConfNextStatText.text = $"{Mathf.RoundToInt(autoData.autoLevelData[autoPartData.NextLevel()].Handling)} GR";

                    hud.upgradeConfMainSlider.value = autoData.autoLevelData[autoPartData.EquippedLevel].Handling;
                    hud.upgradeConfDiffSlider.value = autoData.autoLevelData[autoPartData.NextLevel()].Handling;
                    break;
                case 3:
                    bestVar = bestHealth;

                    hud.upgradeConfMainSlider.maxValue = bestVar;
                    hud.upgradeConfDiffSlider.maxValue = bestVar;

                    hud.upgradeConfCurrentStatText.text = $"{Mathf.RoundToInt(autoData.autoLevelData[autoPartData.EquippedLevel].MaxHealth)} HP";
                    hud.upgradeConfNextStatText.text = $"{Mathf.RoundToInt(autoData.autoLevelData[autoPartData.NextLevel()].MaxHealth)} HP";

                    hud.upgradeConfMainSlider.value = autoData.autoLevelData[autoPartData.EquippedLevel].MaxHealth;
                    hud.upgradeConfDiffSlider.value = autoData.autoLevelData[autoPartData.NextLevel()].MaxHealth;
                    break;
                case 4:
                    bestVar = bestPower;

                    hud.upgradeConfMainSlider.maxValue = bestVar;
                    hud.upgradeConfDiffSlider.maxValue = bestVar;

                    hud.upgradeConfCurrentStatText.text = $"{Mathf.RoundToInt(autoData.autoLevelData[autoPartData.EquippedLevel].Power)} CORES";
                    hud.upgradeConfNextStatText.text = $"{Mathf.RoundToInt(autoData.autoLevelData[autoPartData.NextLevel()].Power)} CORES";

                    hud.upgradeConfMainSlider.value = autoData.autoLevelData[autoPartData.EquippedLevel].Power;
                    hud.upgradeConfDiffSlider.value = autoData.autoLevelData[autoPartData.NextLevel()].Power;
                    break;
            }
        }

        public void OrderAutoPartConfirm()
        {
            AutoData autoData = autoProps[selectedAutoInt].data;
            AutoPartData autoPartData = gamingServicesManager.cloudData.inventoryDict[autoData.ItemCode][EXMET.IntToCompClass((int)compState)];
            
            autoPartData.OrderPart(autoPartData.NextLevel(), DateTime.UtcNow);

            print($"Auto Part ordered: Current LVL: {autoPartData.CurrentLevel}, Ordered LVL: {autoPartData.OrderedLevel}");

            gamingServicesManager.SaveCloudData(false);
            gamingServicesManager.CheckDeliveryTimers();

            CheckInventory();
        }

        public void PurchaseCar()
        {
            AutoData autoData = autoProps[selectedAutoInt].data;

            hud.autoConfirmationIcon.sprite = autoData.Icon;
            hud.AutoConfirmationNameText.text = autoData.AutoName;

            hud.autoTopSpeedSlider_C.value = autoTopSpeedVar;
            hud.autoGearboxSlider_C.value = autoTorqueVar;
            hud.autoHandlingSlider_C.value = autoHandlingVar;
            hud.autoHealthSlider_C.value = autoHealthVar;
            hud.autoPowerSlider_C.value = autoPowerVar;

            hud.autoTopSpeedInfo_C.text = $"{Mathf.RoundToInt(autoData.autoLevelData[0].TopSpeed * (SettingsManager.Instance.settings.SpeedUnitIsKMH ? 3.6f : 2.2f))} {(SettingsManager.Instance.settings.SpeedUnitIsKMH ? "KMH" : "MPH")}";
            hud.autoGearboxInfo_C.text = $"{autoData.autoLevelData[0].Torque} TQ";
            hud.autoHandlingInfo_C.text = $"{autoData.autoLevelData[0].Handling} GR";
            hud.autoHealthInfo_C.text = $"{autoData.autoLevelData[0].MaxHealth} HP";
            hud.autoPowerInfo_C.text = $"{autoData.autoLevelData[0].Power} CORES";
        }

        public void PurchaseCarConfirm()
        {
            AutoData autoData = autoProps[selectedAutoInt].data;

            gamingServicesManager.cloudData.PurchaseCar(autoData.ItemCode, (byte)selectedAutoInt);

            gamingServicesManager.SaveCloudData(false);

            CheckInventory();
        }

        public void SelectComp()
        {
            AutoData autoData = autoProps[selectedAutoInt].data;

            gamingServicesManager.cloudData.inventoryDict[autoData.ItemCode][EXMET.IntToCompClass((int)compState)].EquippedLevel = (byte)selectedCompInt;

            gamingServicesManager.SaveCloudData(false);

            CheckInventory();
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
            int dollars = gamingServicesManager.cloudData.RetroDollars;
            dollars -= price;

            gamingServicesManager.cloudData.RetroDollars = dollars;
        }

        public void OnCloudDataReceived()
        {
            selectedAutoInt = gamingServicesManager.cloudData.LastSelectedCarIndex;

            CheckInventory();
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
