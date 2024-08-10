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
        [SerializeField]
        private Toggle targetFrameToggle;
        [Space]
        [SerializeField]
        private Slider renderScaleSlider;
        [SerializeField]
        private TextMeshProUGUI renderScaleText;
        [Space]
        [SerializeField]
        private Toggle displayFrameToggle;
        [SerializeField]
        private GameObject frameRateCounter;
        [Space]
        [SerializeField]
        private Toggle postProcessToggle;
        [SerializeField]
        private Volume postProcessVolume;
        [SerializeField]
        private Camera cam;
        [Space]
        [SerializeField]
        private Slider backMusicSlider;
        [SerializeField]
        private TextMeshProUGUI backMusicText;
        [SerializeField]
        private Slider gameSoundSlider;
        [SerializeField]
        private TextMeshProUGUI gameSoundText;

        [Header("Rendering")]
        [SerializeField]
        private UniversalRenderPipelineAsset renderAsset;
        [Space]
        [Header("Audio")]
        [SerializeField]
        private AudioSource backMusicSource;
        [SerializeField]
        private AudioSource[] gameSoundSource;
        #endregion

        #region Invisible References
        [HideInInspector]
        public SettingsClass settings;
        private int deviceRefreshRate;
        private List<float> gameSoundVolumes = new List<float>();
        #endregion

        private void Awake()
        {
            for (int i = 0; i < gameSoundSource.Length; i++)
                gameSoundVolumes.Add(gameSoundSource[i].volume);

            LoadSettings();
        }

        #region UI Methods
        public void TargetFrameToggleChange()
        {
            if (targetFrameToggle.isOn)
                Application.targetFrameRate = deviceRefreshRate;
            else
                Application.targetFrameRate = deviceRefreshRate / 2;

            settings.MaxFPSToggle = targetFrameToggle.isOn;
        }

        public void RenderScaleSliderChange()
        {
            renderScaleText.text = $"{renderScaleSlider.value}%";
            renderAsset.renderScale = renderScaleSlider.value / 100f;

            settings.RenderScale = renderScaleSlider.value / 100f;
        }

        public void DisplayFrameToggleChange()
        {
            settings.DisplayFPS = displayFrameToggle.isOn;
            frameRateCounter.SetActive(settings.DisplayFPS);
        }

        public void PostProcessToggleChange()
        {
            settings.PostProcess = postProcessToggle.isOn;
            postProcessVolume.weight = postProcessToggle.isOn == true ? 1f : 0f;
            cam.GetUniversalAdditionalCameraData().renderPostProcessing = postProcessToggle.isOn;
        }

        public void BackMusicSliderChange()
        {
            backMusicSource.volume = backMusicSlider.value / 100f;
            backMusicText.text = $"{backMusicSlider.value}%";

            settings.BackMusicVol = backMusicSlider.value;
        }

        public void GameSoundSliderChange()
        {
            for(int i = 0; i < gameSoundSource.Length; i++)
                gameSoundSource[i].volume = gameSoundVolumes[i] * (gameSoundSlider.value / 100f);

            gameSoundText.text = $"{gameSoundSlider.value}%";

            settings.EffectsVol = gameSoundSlider.value;
        }
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
                print("Settings found. Applying...");

                ApplySettings();
            }
            else 
            { 
                SetDefaultSettings();
                print("Settings not found, applying default settings...");
            }
        }

        public void ApplySettings()
        {
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
                renderScaleText.text = $"{renderScaleSlider.value}%";
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
        }
        
        public void SetDefaultSettings()
        {
            SettingsClass defaultSettings = new SettingsClass
            {
                MaxFPSToggle = true,
                RenderScale = 1.00f,
                DisplayFPS = false,
                PostProcess = false,
                BackMusicVol = 80f,
                EffectsVol = 100f,
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
        public bool MaxFPSToggle = true;
        public float RenderScale = 0.75f;
        public bool DisplayFPS = false;
        public bool PostProcess = false;

        public float BackMusicVol = 80f;
        public float EffectsVol = 100f;
    }
}
