using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class StartScript : MonoBehaviour
{
    // References assigned in the Inspector if needed
    public GameObject mainMenuCanvas;
    public Camera mainCamera;        // The camera in the MainMenu scene
    public GameObject currentEventSystem;

    // Store a reference to the player’s movement so we can disable/enable it
    private Movement playerMovement;
    // We'll dynamically find the cutsceneText once the CutsceneCanvas is active.
    private Text cutsceneText;

    public void OnStartGameClicked()
    {
        // --- 0) Check and reset dead player (existing code) ---
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            PlayerInfo playerInfo = player.GetComponent<PlayerInfo>();
            playerMovement = player.GetComponent<Movement>();
            if (playerInfo != null && (playerInfo.currentHealth <= 0 
                || (playerMovement != null && !playerMovement.enabled)))
            {
                // Reset the dead player
                Quit quitScript = FindFirstObjectByType<Quit>();
                if (quitScript != null)
                {
                    quitScript.ResetPlayer();
                    Debug.Log("Dead player detected and reset.");
                }
                else
                {
                    Debug.LogWarning("Quit script instance not found; cannot reset dead player.");
                }
            }
        }
        else
        {
            Debug.LogWarning("Player not found in OnStartGameClicked.");
        }

        // --- 1) Load the "Bed" scene (additive) for the cutscene ---
        SceneManager.LoadScene("Bed", LoadSceneMode.Additive);

        // --- 2) Show the CutsceneCanvas ---
        if (SingletonManager.Instance != null && SingletonManager.Instance.cutsceneCanvas != null)
        {
            SingletonManager.Instance.cutsceneCanvas.gameObject.SetActive(true);

            // Find the Text component within the cutscene canvas (if not already cached)
            if (cutsceneText == null)
            {
                cutsceneText = SingletonManager.Instance.cutsceneCanvas.GetComponentInChildren<Text>();
                if (cutsceneText == null)
                {
                    Debug.LogWarning("No Text component found under CutsceneCanvas!");
                }
            }
        }

        // Disable player movement until the cutscene ends
        if (playerMovement != null)
        {
            playerMovement.enabled = false;
        }

        // Start the cutscene flow (fade in text, wait, skip if Enter, etc.)
        StartCoroutine(CutsceneSequence());
    }

    private IEnumerator CutsceneSequence()
    {
        // --- 3) Fade in the cutscene text over 1 second ---
        yield return StartCoroutine(FadeInCutsceneText(1f));

        // Now wait for the cutscene to finish or for player to skip
        float cutsceneDuration = 5f; // Example: cutscene lasts 5 seconds
        float timer = 0f;

        bool cutsceneEnded = false;
        while (!cutsceneEnded)
        {
            timer += Time.deltaTime;

            // --- 4) If the player presses Enter, skip immediately ---
            if (Input.GetKeyDown(KeyCode.Return))
            {
                cutsceneEnded = true;
                break;
            }

            // If the timer hits the cutscene duration, end naturally
            if (timer >= cutsceneDuration)
            {
                cutsceneEnded = true;
            }

            yield return null;
        }

        // --- 5) On cutscene's end, go straight to the game ---
        EndCutscene();
    }

    private void EndCutscene()
    {
        // --- 6) Disable the CutsceneCanvas, enable the GameplayCanvas ---
        if (SingletonManager.Instance != null && SingletonManager.Instance.cutsceneCanvas != null)
        {
            SingletonManager.Instance.cutsceneCanvas.gameObject.SetActive(false);
        }

        // Reset the text alpha for next time
        if (cutsceneText != null)
        {
            Color c = cutsceneText.color;
            c.a = 0f;
            cutsceneText.color = c;
        }

        // Enable the persistent camera *now*, at the end of the cutscene
        if (SingletonManager.Instance != null)
        {
            if (SingletonManager.Instance.mainCamera != null)
            {
                SingletonManager.Instance.mainCamera.gameObject.SetActive(true);
            }
            if (mainCamera != null)
            {
                mainCamera.gameObject.SetActive(false);
            }
        }

        // Re-enable player movement
        if (playerMovement != null)
        {
            playerMovement.enabled = true;
        }

        // Now we enable the gameplay UI, generate the tower, etc.
        SetupGameplayAndTower();
    }

    private void SetupGameplayAndTower()
    {
        // --- 7) Enable the GameplayCanvas & elements (existing code from your snippet) ---
        if (SingletonManager.Instance != null && LoadingUI.Instance != null)
        {
            SingletonManager.Instance.gameplayCanvas.gameObject.SetActive(true);
            // Show the loading panel
            LoadingUI.Instance.ShowLoading("Loading Tower...");

            Transform sprintSlider = SingletonManager.Instance.gameplayCanvas.transform.Find("SprintSlider");
            if (sprintSlider != null)
                sprintSlider.gameObject.SetActive(true);

            Transform healthSlider = SingletonManager.Instance.gameplayCanvas.transform.Find("HealthSlider");
            if (healthSlider != null)
                healthSlider.gameObject.SetActive(true);

            Transform invButton = SingletonManager.Instance.gameplayCanvas.transform.Find("ShowMainInventory");
            if (invButton != null)
                invButton.gameObject.SetActive(true);

            Transform toolbar = SingletonManager.Instance.gameplayCanvas.transform.Find("Toolbar");
            if (toolbar != null)
                toolbar.gameObject.SetActive(true);

            Transform lorePanel = SingletonManager.Instance.gameplayCanvas.transform.Find("LorePanel");
            if (lorePanel != null)
                lorePanel.gameObject.SetActive(true);

            // Switch systems
            if (currentEventSystem != null)
                currentEventSystem.gameObject.SetActive(false);
            GameObject eventSystem = SingletonManager.Instance.eventSystem;
            if (eventSystem != null)
                eventSystem.gameObject.SetActive(true);
        }
        else
        {
            Debug.LogWarning("LoadingUI or SingletonManager instance not found!");
        }

        // Generate a new seed
        int newSeed = Random.Range(100000, 1000000);

        // Signal MasterLevelManager to enter the tower
        MasterLevelManager mlm = MasterLevelManager.Instance;
        if (mlm != null)
        {
            mlm.globalSeed = newSeed;
            mlm.inTower = true;
            mlm.GenerateAndLoadFloor(1, true);
        }
        else
        {
            Debug.LogError("MasterLevelManager instance not found!");
        }

        // (Optional) Unload MainMenu scene and hide MainMenuCanvas if you want
        // if (SingletonManager.Instance != null && mainMenuCanvas != null)
        // {
        //     mainMenuCanvas.SetActive(false);
        // }
        // SceneManager.UnloadSceneAsync("MainMenu");
    }

    /// <summary>
    /// Smoothly fade in the cutsceneText alpha from 0 to 1 over <paramref name="duration"/> seconds.
    /// </summary>
    private IEnumerator FadeInCutsceneText(float duration)
    {
        if (cutsceneText == null)
        {
            yield break;
        }

        // Start fully transparent
        Color startColor = cutsceneText.color;
        startColor.a = 0f;
        cutsceneText.color = startColor;

        // Target fully opaque
        Color endColor = cutsceneText.color;
        endColor.a = 1f;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            cutsceneText.color = Color.Lerp(startColor, endColor, t);
            yield return null;
        }
    }
}
