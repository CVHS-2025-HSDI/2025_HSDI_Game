using UnityEngine;
using System.Collections;

public class MusicController : MonoBehaviour
{
    public static MusicController Instance;

    public AudioSource audioSource;
    public AudioClip gameOverMusic;
    public float fadeDuration = 4f; // Duration for fade-in

    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Ensure AudioSource is assigned
            if (audioSource == null)
                audioSource = GetComponent<AudioSource>();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Fades in the game over music over fadeDuration seconds.
    /// </summary>
    public void PlayGameOverMusic()
    {
        if (gameOverMusic == null || audioSource == null)
        {
            Debug.LogWarning("GameOver music or AudioSource not assigned.");
            return;
        }

        // Stop any current music
        audioSource.Stop();
        audioSource.clip = gameOverMusic;
        audioSource.volume = 0f;
        audioSource.Play();
        StartCoroutine(FadeInAudio());
    }

    private IEnumerator FadeInAudio()
    {
        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(0f, 1f, timer / fadeDuration);
            yield return null;
        }
        audioSource.volume = 1f;
    }

    /// <summary>
    /// Stop playing music immediately.
    /// </summary>
    public void StopMusic()
    {
        if (audioSource != null)
            audioSource.Stop();
    }
}