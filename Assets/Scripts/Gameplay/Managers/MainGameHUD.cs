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
        public Image healthBarFill;
        public Image powerBarFill;
        [Space]
        public List<MarkerPoint> markerPoints = new List<MarkerPoint>();
        [Space]
        public List<EffectBarItem> effectBarItems = new List<EffectBarItem>();
        [Space]
        public AnimationCurve markerUIScaleCurve;
        public AnimationCurve markerUIAlphaCurve;
        [Space]
        public Sprite[] pickUpIcons;
        [Space]
        public Image pauseGraphic;
        public Sprite pauseIcon, continueIcon;

        [Header("Game Over Screen")]
        public TextMeshProUGUI earnings;
        public TextMeshProUGUI finalScore;
        public TextMeshProUGUI finalNMH;
        public TextMeshProUGUI COPKillCount;
    }

    [Serializable]
    public struct MarkerPoint
    {
        public GameObject markerObject;
        public RectTransform markerRect;
        public Image icon;
        public TextMeshProUGUI distance;
    }

    [Serializable]
    public struct EffectBarItem
    {
        public EffectBarItemType itemType;
        public Image timerFill;
    }

    public enum EffectBarItemType
    {
        Boost,
        Ability,
        Deez,
    }
}
