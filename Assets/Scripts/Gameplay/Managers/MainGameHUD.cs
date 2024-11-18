using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RetroCode
{
    public class MainGameHUD : MonoBehaviour
    {
        [Header("Universal")]
        public TextMeshProUGUI playerMoneyText;

        [Header("Menu Screen")]
        public TextMeshProUGUI highScoreText;
        public TextMeshProUGUI highestMissComboText;
        public TextMeshProUGUI COPKillCountRecord;

        [Header("Game Screen")]
        public Transform mainGameParent;
        public TextMeshProUGUI scoreText;
        public TextMeshProUGUI scoreEventText;
        public TextMeshProUGUI scoreMultiplierText;
        public TextMeshProUGUI nearMissText;
        public Image nearMissComboTimer;
        public TextMeshProUGUI speedoMeterText;
        public Image speedoMeterFill;
        public Slider healthBarSlider;
        public Slider powerBarSlider;
        [Space]
        public AnimationCurve markerUIScaleCurve;
        public AnimationCurve markerUIAlphaCurve;
        [Space]
        public Sprite[] pickUpIcons;

        [Header("Game Over Screen")]
        public TextMeshProUGUI gameOverText;
        public TextMeshProUGUI earningsText;
        public TextMeshProUGUI finalScoreText;
        public TextMeshProUGUI finalNMHText;
        public TextMeshProUGUI COPKillCountText;
    }
}
