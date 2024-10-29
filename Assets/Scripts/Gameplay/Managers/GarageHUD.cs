using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

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
            autoPowerSlider,
            autoHandlingSlider,
            autoHealthSlider;
        public TextMeshProUGUI
            autoTopSpeedInfo,
            autoPowerInfo,
            autoHandlingInfo,
            autoHealthInfo;
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
