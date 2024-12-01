using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.EventSystems;

namespace RetroCode
{
    public class GarageHUD : MonoBehaviour
    {
        [Header("Universal")]
        public TextMeshProUGUI playerMoneyText;

        [Header("Garage Screen")]
        public GameObject upgradeScreen;
        [Header("Auto Selection")]
        public TextMeshProUGUI currentAutoNameText;
        public TextMeshProUGUI autoPriceText;
        public TextMeshProUGUI autoTierText;
        public GameObject autoSelectedIcon;
        public Image autoLockImage;
        public GameObject upgradeCarButton;
        public GameObject purchaseCarButton;
        public GameObject lockedCarInfo;
        public GameObject nextAutoButton, previousAutoButton;
        public Slider
            autoTopSpeedSlider,
            autoGearboxSlider,
            autoHandlingSlider,
            autoHealthSlider,
            autoPowerSlider;
        public TextMeshProUGUI
            autoTopSpeedInfo,
            autoGearboxInfo,
            autoHandlingInfo,
            autoHealthInfo,
            autoPowerInfo;
        [Space]
        [Header("Auto Upgrade")]
        public TextMeshProUGUI currentCompNameText;
        public TextMeshProUGUI compPriceText;
        public GameObject orderCompButton;
        public GameObject deliveryTimeAction, inDeliveryIcon;
        public TextMeshProUGUI deliveryTimeText;
        public GameObject openLootButtonMain;
        public GameObject nextCompButton, previousCompButton;
        public GameObject equipCompButton, equippedCompInfo, openLootButtonAux;
        public List<ParticleSystem> maxLvlFX;
        [Space]
        [Header("Confirmation Screens")]
        public GameObject carPurchaseConfirmScreen;
        public CanvasGroup curPurchaseConfirmScreenAlpha;
        public Image autoConfirmationIcon;
        public TextMeshProUGUI AutoConfirmationNameText;
        public Slider
            autoTopSpeedSlider_C,
            autoGearboxSlider_C,
            autoHandlingSlider_C,
            autoHealthSlider_C,
            autoPowerSlider_C;
        public TextMeshProUGUI
            autoTopSpeedInfo_C,
            autoGearboxInfo_C,
            autoHandlingInfo_C,
            autoHealthInfo_C,
            autoPowerInfo_C;
        public GameObject orderPartConfirmScreen;
        [Space]
        [Header("Misc")]
        public Color lockedColor;
        public Color availableColor;
        public Sprite lockedSprite, unlockedSprite;
        [Space]
        public List<CompStatSliders> compStatSliders = new List<CompStatSliders>();
    }

    [System.Serializable]
    public struct CompStatSliders
    {
        public Slider mainSlider, diffSlider;
        public TextMeshProUGUI levelText, statCurrentText, statTunedText;        
        public Image icon, arrowImage;
        public Outline outline;
    }
}
