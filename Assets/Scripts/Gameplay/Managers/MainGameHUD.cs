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
        public TextMeshProUGUI highScoreRecord;
        public TextMeshProUGUI nearMissChainRecord;
        public TextMeshProUGUI COPKillCountRecord;
        public TextMeshProUGUI NPCKillCountRecord;

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
        public CanvasGroup gameOverNormalScreen;
        public CanvasGroup gameOverAdsScreen;
        public TextMeshProUGUI gameOverText;
        public TextMeshProUGUI earningsText;
        public TextMeshProUGUI finalScoreText;
        public TextMeshProUGUI bestNearMissChainText;
        public TextMeshProUGUI COPKillCountText;
        public TextMeshProUGUI NPCKillCountText;
        public CanvasGroup playAgainButtonCanvasGroup;
    }
}
