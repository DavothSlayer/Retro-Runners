using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using V3CTOR;

namespace RetroCode
{
    public class SettingsManager : MonoBehaviour
    {
        #region Visible References
        [Header("References")]
        [Header("Tabs")]
        [SerializeField]
        private List<GameObject> optionsTabs = new List<GameObject>();

        [Header("Display Tab")]
        [SerializeField]
        private Slider resolutionSlider;
        [SerializeField]
        private TextMeshProUGUI resolutionText;
        [SerializeField]
        private BetterToggle camPosition;
        [SerializeField]
        private BetterToggle speedUnit;
        [SerializeField]
        private BetterOptions targetFPS;
        [SerializeField]
        private BetterToggle postProcess;
        [SerializeField]
        private BetterToggle overlayFPS;
        [SerializeField]
        private GameObject fpsOverlay;

        [Header("Sounds Tab")]
        [SerializeField]
        private Slider sfxSlider;
        [SerializeField]
        private TextMeshProUGUI sfxText;
        [SerializeField]
        private Slider musicSlider;
        [SerializeField]
        private TextMeshProUGUI musicText;
        [SerializeField]
        private Slider masterSlider;
        [SerializeField]
        private TextMeshProUGUI masterText;

        [Header("Rendering")]
        [SerializeField]
        private UniversalRenderPipelineAsset renderAsset;
        [SerializeField]
        private Volume volume;
        [SerializeField]
        private Camera mainCamera;

        [Header("Audio")]
        [SerializeField]
        private AudioSource backMusicSource;
        
        [Header("Notifications")]
        [SerializeField]
        private BetterToggle notifications;
        [SerializeField]
        private BetterToggle deliveryNotify;
        [SerializeField]
        private BetterToggle reminders;
        #endregion

        #region Invisible References
        [HideInInspector]
        public SettingsClass settings;
        #endregion

        public static SettingsManager Instance { get; private set; }

        private void Awake()
        {
            Instance = this;

            LoadSettings();
        }

        #region UI Methods
        public void SetOptionsTabInt(int index)
        {
            foreach (GameObject gameObject in optionsTabs)
                gameObject.SetActive(gameObject == optionsTabs[index]);
        }

        // DISPLAY TAB //
        public void UpdateResolution()
        {
            resolutionText.text = $"RESOLUTION {resolutionSlider.value}%";

            settings.Resolution = Mathf.RoundToInt(resolutionSlider.value);

            renderAsset.renderScale = settings.Resolution / 100f;
        }

        public void UpdateLowCam()
        {
            settings.LowCam = camPosition.isOn;
        }

        public void UpdateSpeedUnit()
        {
            settings.SpeedUnitIsKMH = speedUnit.isOn;
        }

        public void UpdateTargetFPS()
        {
            settings.TargetFPS = targetFPS.optionIndex;

            switch (settings.TargetFPS)
            {
                case 0:
                    Application.targetFrameRate = 30;
                    break;
                case 1:
                    Application.targetFrameRate = 45;
                    break;
                case 2:
                    Application.targetFrameRate = 60;
                    break;
            }
        }

        public void UpdatePostProcess()
        {
            settings.PostProcess = postProcess.isOn;

            // FIX PP (LOL) //
            mainCamera.GetUniversalAdditionalCameraData().renderPostProcessing = settings.PostProcess;
            volume.weight = settings.PostProcess ? 1f : 0f;
        }

        public void UpdateFPSOverlay()
        {
            settings.OverlayFPS = overlayFPS.isOn;

            fpsOverlay.SetActive(settings.OverlayFPS);
        }
        // DISPLAY TAB //

        // SOUNDS TAB //
        public void UpdateSFX()
        {
            settings.EffectsVol = (int)sfxSlider.value;

            sfxText.text = $"SFX {sfxSlider.value}%";
        }

        public void UpdateMusic()
        {
            settings.BackMusicVol = (int)musicSlider.value;

            musicText.text = $"MUSIC {musicSlider.value}%";
        }

        public void UpdateMaster()
        {
            settings.MasterVol = (int)masterSlider.value;

            masterText.text = $"MASTER {masterSlider.value}%";
        }
        // SOUNDS TAB //

        // NOTIFICATIONS //
        public void UpdateNotifications()
        {
            settings.Notifications = notifications.isOn;
        }

        public void UpdateDeliveryNotify()
        {
            settings.DeliveryNotifiy = deliveryNotify.isOn;
        }

        public void UpdateReminders()
        {
            settings.Reminders = reminders.isOn;
        }
        // NOTIFICATIONS //
        #endregion

        #region Save & Load Settings
        public void SaveSettings()
        {
            string json = JsonUtility.ToJson(settings);
            EXMET.SaveJSON(json, "settings.json");
        }

        public void LoadSettings()
        {
            string loadedJson = EXMET.LoadJSON("settings.json");
            SettingsClass loadedSettings = JsonUtility.FromJson<SettingsClass>(loadedJson);

            if (loadedSettings != null)
            {
                settings = loadedSettings;

                ApplySettings();
            }
            else 
            { 
                SetDefaultSettings();
            }
        }

        public void ApplySettings()
        {
            // DISPLAY SETTINGS //
            resolutionText.text = $"RESOLUTION {settings.Resolution}%";
            resolutionSlider.value = settings.Resolution;
            camPosition.SetValue(settings.LowCam);
            speedUnit.SetValue(settings.SpeedUnitIsKMH);
            targetFPS.SetValue(settings.TargetFPS);
            postProcess.SetValue(settings.PostProcess);
            overlayFPS.SetValue(settings.OverlayFPS);

            renderAsset.renderScale = settings.Resolution / 100f;
            mainCamera.GetUniversalAdditionalCameraData().renderPostProcessing = settings.PostProcess;
            volume.weight = settings.PostProcess ? 1f : 0f;
            fpsOverlay.SetActive(settings.OverlayFPS);
            // DISPLAY SETTINGS //
        }

        public void SetDefaultSettings()
        {
            SettingsClass defaultSettings = new SettingsClass
            {
                Resolution = 85,
                LowCam = true,
                SpeedUnitIsKMH = true,
                TargetFPS = 1,
                PostProcess = false,
                OverlayFPS = false,
                
                EffectsVol = 100,
                BackMusicVol = 80,
                MasterVol = 100,

                Notifications = true,
                DeliveryNotifiy = true,
                Reminders = true,
            };

            string json = JsonUtility.ToJson(defaultSettings);
            EXMET.SaveJSON(json, "settings.json");

            ApplySettings();
        }
        #endregion
    }

    [System.Serializable]
    public class SettingsClass
    {
        public int Resolution = 85;
        public bool LowCam = true;
        public bool SpeedUnitIsKMH = true;
        public int TargetFPS = 1;
        public bool PostProcess = false;
        public bool OverlayFPS = false;

        public int EffectsVol = 100;
        public int BackMusicVol = 80;
        public int MasterVol = 100;

        public bool Notifications = true;
        public bool DeliveryNotifiy = true;
        public bool Reminders = true;
    }
}
