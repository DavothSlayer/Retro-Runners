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
        private BetterToggle sixtyFPS;
        [SerializeField]
        private BetterToggle postProcess;

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
        private UniversalRendererData renderData;
        [SerializeField]
        private Volume volume;
        [Space]
        [Header("Audio")]
        [SerializeField]
        private AudioSource backMusicSource;
        #endregion

        #region Invisible References
        [HideInInspector]
        public SettingsClass settings;
        #endregion

        private void Awake()
        {
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

        public void UpdateSixtyFPS()
        {
            settings.SixtyFPS = sixtyFPS.isOn;

            Application.targetFrameRate = settings.SixtyFPS ? 60 : 30;
        }

        public void UpdatePostProcess()
        {
            settings.PostProcess = postProcess.isOn;

            // FIX PP (LOL) //
            volume.gameObject.SetActive(settings.PostProcess);
            volume.weight = settings.PostProcess ? 1f : 0f;
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
            /*
            string loadedJson = EXMET.LoadJSON("settings.json");
            SettingsClass loadedSettings = JsonUtility.FromJson<SettingsClass>(loadedJson);

            if (loadedSettings != null)
            {
                settings = loadedSettings;
                print("Settings found. Applying...");

                ApplySettings();
            }
            else 
            { 
                SetDefaultSettings();
                print("Settings not found, applying default settings...");
            }
            */
        }

        public void ApplySettings()
        {
            /*
            deviceRefreshRate = (int)Screen.currentResolution.refreshRateRatio.value;

            // MAX FPS //
            targetFrameToggle.isOn = settings.MaxFPSToggle;

            if (settings.MaxFPSToggle)
                Application.targetFrameRate = deviceRefreshRate;
            else
                Application.targetFrameRate = deviceRefreshRate / 2;
            // MAX FPS //

            // RENDER SCALE //
            renderAsset.renderScale = settings.RenderScale;
            if(renderScaleSlider != null)
            {
                renderScaleSlider.value = settings.RenderScale * 100f;
                renderScaleText.text = $"Resolution {renderScaleSlider.value}%";
            }
            // RENDER SCALE //

            // FPS COUNTER //
            displayFrameToggle.isOn = settings.DisplayFPS;

            frameRateCounter.SetActive(settings.DisplayFPS);
            // FPS COUNTER //

            // POST PROCESSING //
            postProcessToggle.isOn = settings.PostProcess;

            postProcessVolume.weight = settings.PostProcess == true ? 1f : 0f;
            cam.GetUniversalAdditionalCameraData().renderPostProcessing = postProcessToggle.isOn;
            // POST PROCESSING //

            // AUDIO //
            backMusicSlider.value = settings.BackMusicVol;
            gameSoundSlider.value = settings.EffectsVol;

            backMusicSource.volume = settings.BackMusicVol / 100f;
            for (int i = 0; i < gameSoundSource.Length; i++)
                gameSoundSource[i].volume = gameSoundVolumes[i] * (settings.EffectsVol / 100f);
            // AUDIO //
            */
        }
        
        public void SetDefaultSettings()
        {
            SettingsClass defaultSettings = new SettingsClass
            {
                Resolution = 80,
                LowCam = true,
                SpeedUnitIsKMH = true,
                SixtyFPS = true,
                PostProcess = false,
                BackMusicVol = 80,
                EffectsVol = 100,
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
        public int Resolution = 100;
        public bool LowCam = true;
        public bool SpeedUnitIsKMH = true;
        public bool SixtyFPS = true;
        public bool PostProcess = false;

        public int EffectsVol = 100;
        public int BackMusicVol = 80;
        public int MasterVol = 100;
    }
}
