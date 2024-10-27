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
        public TextMeshProUGUI currentAutoNameText;
        public TextMeshProUGUI currentCompNameText;
        public TextMeshProUGUI autoPriceText, compPriceText;
        [Space]
        public GameObject upgradeCarButton;
        public GameObject purchaseCarButton;
        public GameObject lockedCarInfo;
        public GameObject nextAutoButton, previousAutoButton;
        public GameObject orderCompButton;
        public GameObject deliveryTimeAction, inDeliveryIcon;
        public TextMeshProUGUI deliveryTimeText;
        public GameObject openLootButtonMain;
        public GameObject nextCompButton, previousCompButton;
        public GameObject equipCompButton, equippedCompInfo, openLootButtonAux;
        public List<ParticleSystem> maxLvlFX;
        public Color lockedColor, availableColor;

        [Space]
        public List<CompStatSliders> compStatSliders = new List<CompStatSliders>();

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
        public TextMeshProUGUI autoNameText_C;
        public TextMeshProUGUI purchaseConfirmText;
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
