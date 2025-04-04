using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class OptionsMenuController : MonoBehaviour {
    [Header("Volume Settings")]
    public Slider masterAudioSlider;
    public Slider musicAudioSlider;
    public Slider sfxAudioSlider;

    public TextMeshProUGUI masterVolumeText;
    public TextMeshProUGUI musicVolumeText;
    public TextMeshProUGUI sfxVolumeText;

    [Header("Display Settings")]
    public TMP_Dropdown resolutionDropdown;
    public Button fullscreenButton;
    public Button closeButton;
    public Image fullscreenCheckmark; 

    // Store fullscreen state (true = on, false = off)
    private bool isFullscreen = true;
    private List<Resolution> allowedResolutions;
    
    public GameObject mainMenuButtons;

    void Start() {
        // Initialize volume sliders (max 100, default 100)
        if (masterAudioSlider != null) {
            masterAudioSlider.maxValue = 100;
            masterAudioSlider.value = 100;
            masterAudioSlider.onValueChanged.AddListener(UpdateMasterVolume);
            UpdateMasterVolume(masterAudioSlider.value);
        }
        if (musicAudioSlider != null) {
            musicAudioSlider.maxValue = 100;
            musicAudioSlider.value = 100;
            musicAudioSlider.onValueChanged.AddListener(UpdateMusicVolume);
            UpdateMusicVolume(musicAudioSlider.value);
        }
        if (sfxAudioSlider != null) {
            sfxAudioSlider.maxValue = 100;
            sfxAudioSlider.value = 100;
            sfxAudioSlider.onValueChanged.AddListener(UpdateSFXVolume);
            UpdateSFXVolume(sfxAudioSlider.value);
        }

        // Populate resolution dropdown with only resolutions 1280x720 or higher (we only scaled to 720p)
        if (resolutionDropdown != null) {
            Resolution[] allResolutions = Screen.resolutions;
            allowedResolutions = new List<Resolution>();
            List<string> options = new List<string>();

            // Filter resolutions to those at least 1280x720
            for (int i = 0; i < allResolutions.Length; i++) {
                Resolution res = allResolutions[i];
                if (res.width >= 1280 && res.height >= 720) {
                    allowedResolutions.Add(res);
                    options.Add(res.width + " x " + res.height);
                }
            }

            // Default to 1920x1080
            int defaultResIndex = 0;
            for (int i = 0; i < allowedResolutions.Count; i++) {
                Resolution res = allowedResolutions[i];
                if (res.width == 1920 && res.height == 1080) {
                    defaultResIndex = i;
                    break;
                }
            }

            resolutionDropdown.ClearOptions();
            resolutionDropdown.AddOptions(options);
            resolutionDropdown.value = defaultResIndex;
            resolutionDropdown.RefreshShownValue();
            resolutionDropdown.onValueChanged.AddListener(SetResolution);
        }

        // Setup fullscreen button as a checkbox
        if (fullscreenButton != null) {
            UpdateFullscreenButtonVisual();
            fullscreenButton.onClick.AddListener(ToggleFullscreen);
        }

        // Close button disables Options panel and re-enables main menu buttons.
        if (closeButton != null) {
            closeButton.onClick.AddListener(CloseOptions);
        }
    }

    #region Volume Updates
    void UpdateMasterVolume(float value) {
        AudioManager.Instance.SetMasterVolume(value / 100f);
    }
    void UpdateMusicVolume(float value) {
        AudioManager.Instance.SetMusicVolume(value / 100f);
    }
    void UpdateSFXVolume(float value) {
        AudioManager.Instance.SetSFXVolume(value / 100f);
    }
    #endregion

    #region Resolution and Fullscreen
    void SetResolution(int resIndex) {
        if (allowedResolutions != null && allowedResolutions.Count > resIndex) {
            Resolution res = allowedResolutions[resIndex];
            Screen.SetResolution(res.width, res.height, Screen.fullScreen);
        }
    }

    void ToggleFullscreen() {
        isFullscreen = !isFullscreen;
        Screen.fullScreen = isFullscreen;
        UpdateFullscreenButtonVisual();
    }

    void UpdateFullscreenButtonVisual() {
        // If using a checkmark image, show or hide it.
        if (fullscreenCheckmark != null) {
            fullscreenCheckmark.enabled = isFullscreen;
        }
        // Also update the text if you want:
        Text btnText = fullscreenButton.GetComponentInChildren<Text>();
        if (btnText != null) {
            btnText.text = isFullscreen ? "Fullscreen: On" : "Fullscreen: Off";
        }
    }
    #endregion

    void CloseOptions() {
        if (mainMenuButtons != null)
            mainMenuButtons.SetActive(true);
        // Disable the Options panel.
        gameObject.SetActive(false);
    }
}
