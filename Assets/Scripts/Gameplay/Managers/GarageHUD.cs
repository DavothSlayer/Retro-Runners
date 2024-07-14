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
        public TextMeshProUGUI currentAutoNameText;
        public TextMeshProUGUI currentCompNameText;
        public TextMeshProUGUI currentAutoNameTextInComp;
        public TextMeshProUGUI autoPriceText, compPriceText;
        [Space]
        public GameObject unlockedActionParent_AR;
        public GameObject lockedActionParent_AR;
        public GameObject lockedObjectParent_AR;
        public GameObject selectButton_AR, selectedObject_AR;
        public GameObject unlockedActionParent_CR;
        public GameObject lockedActionParent_CR;
        public GameObject lockedObjectParent_CR;

        public List<CompStatSliders> compStatSliders = new List<CompStatSliders>();

        public GameObject selectButton_CR, selectedObject_CR, nextAutoButton, previousAutoButton, nextCompButton, previousCompButton;
        public RectTransform[] compButtons;
        public float compRectDefaultX, compRectSelectedX;

        public Slider
            autoTopSpeedSlider, 
            autoAccelerationSlider,
            autoTurningSlider,
            autoHealthSlider;
        public Image
            autoTopSpeedSliderFill,
            autoAccelerationSliderFill,
            autoTurningSliderFill,
            autoHealthSliderFill;
        public TextMeshProUGUI
            autoTopSpeedInfo,
            autoAccelerationInfo,
            autoTurningInfo,
            autoHealthInfo;
        public Gradient sliderGradient;
        public TextMeshProUGUI autoNameText_C;
        public TextMeshProUGUI purchaseConfirmText;
        [Space]
        public TextMeshProUGUI abilityTitleText;
        public TextMeshProUGUI abilityInfoText;
    }

    [System.Serializable]
    public struct CompStatSliders
    {
        public Slider mainSlider, diffSlider;
    }
}
