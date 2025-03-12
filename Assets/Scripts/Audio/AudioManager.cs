using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour {
    public static AudioManager Instance { get; private set; }

    [Header("Audio Mixer")]
    public AudioMixer masterMixer;

    void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // volume is normalized between 0 and 1; converted to decibels.
    public void SetMasterVolume(float volume) {
        float dB = volume > 0 ? Mathf.Log10(volume) * 20f : -80f;
        masterMixer.SetFloat("Master", dB);
    }

    public void SetMusicVolume(float volume) {
        float dB = volume > 0 ? Mathf.Log10(volume) * 20f : -80f;
        masterMixer.SetFloat("Music", dB);
    }

    public void SetSFXVolume(float volume) {
        float dB = volume > 0 ? Mathf.Log10(volume) * 20f : -80f;
        masterMixer.SetFloat("SFX", dB);
    }
}