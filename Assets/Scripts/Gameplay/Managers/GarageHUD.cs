using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace RetroCode
{
    public class GarageHUD : MonoBehaviour
    {
        [Header("Universal")]
        public TextMeshProUGUI playerMoneyText;

        [Header("Garage Screen")]        
        public TextMeshProUGUI currentAutoNameText;
        public TextMeshProUGUI autoNameText_CR;
        public TextMeshProUGUI currentCompNameText_Unlocked, currentCompNameText_Locked;
        public TextMeshProUGUI autoPriceText, compPriceText;
        [Space]
        public GameObject unlockedActionParent_AR;
        public GameObject lockedActionParent_AR;
        public GameObject lockedObjectParent_AR;
        public GameObject selectButton_AR, selectedObject_AR;
        public GameObject unlockedActionParent_CR;
        public GameObject lockedActionParent_CR;
        public GameObject lockedObjectParent_CR;
        [Space]
        public TextMeshProUGUI newStatTitle;
        public Slider CompStatSlider, CompStatSliderDiff;
        public GameObject selectButton_CR, selectedObject_CR;
        public RectTransform[] compButtons;
        public float compRectDefaultX, compRectSelectedX;

        public Slider
            autoTopSpeedSlider, 
            autoAccelerationSlider,
            autoHandlingSlider,
            autoHealthSlider;
        public Image
            autoTopSpeedSliderFill,
            autoAccelerationSliderFill,
            autoHandlingSliderFill,
            autoHealthSliderFill;
        public Gradient sliderGradient;
        public TextMeshProUGUI autoNameText_C;
        public TextMeshProUGUI purchaseConfirmText;
        [Space]
        public TextMeshProUGUI abilityTitleText;
        public TextMeshProUGUI abilityInfoText;
    }
}
