using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Quit : MonoBehaviour
{
    // Assign this in the Inspector with your Game Over panel.
    public GameObject gameOverScreen;

    public void QuitGame() {
        Application.Quit();
        Debug.Log("Game is exiting");
    }
    
    public void ResetPlayer()
    {
        // Find the player by tag (ensure your player prefab is tagged "Player")
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            // Reset the player's health and dead state using PlayerInfo
            PlayerInfo pi = player.GetComponent<PlayerInfo>();
            if (pi != null)
            {
                pi.Revive();  // Reset isDead and currentHealth
                Debug.Log("Player health and state reset.");
            
                // Reactivate the toolbar and inventory button.
                if (pi.toolbar != null)
                    pi.toolbar.SetActive(true);
                if (pi.invButton != null)
                    pi.invButton.SetActive(true);
            }
            else
            {
                Debug.LogWarning("PlayerInfo component not found on player!");
            }

            // Re-enable the movement script.
            Movement moveScript = player.GetComponent<Movement>();
            if (moveScript != null)
            {
                moveScript.enabled = true;
                Debug.Log("Movement script re-enabled.");
            }
            else
            {
                Debug.LogWarning("Movement script not found on player!");
            }

            // Reset the sprite's alpha so the player becomes visible again.
            SpriteRenderer sr = player.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                Color col = sr.color;
                col.a = 1f;
                sr.color = col;
                Debug.Log("Player sprite alpha reset.");
            }
            else
            {
                Debug.LogWarning("SpriteRenderer not found on player!");
            }

            // Optionally, reposition the player at a spawn point.
            player.transform.position = Vector3.zero;
            Debug.Log("Player position reset.");
        }
        else
        {
            Debug.LogError("Player not found during reset!");
        }
    }

    
    /// <summary>
    /// Returns to the Main Menu.
    /// Disables gameplay UI, shows a loading message, loads the MainMenu scene,
    /// then hides the loading screen once the scene is loaded.
    /// </summary>
    public void ReturnToMainMenu() {
        StartCoroutine(ReturnToMainMenuRoutine());
    }
    
    private IEnumerator ReturnToMainMenuRoutine() {
    // Show loading screen with an appropriate message.
    if (LoadingUI.Instance != null)
        LoadingUI.Instance.ShowLoading("Returning to Main Menu...");
    else
        Debug.LogWarning("LoadingUI instance not found in ReturnToMainMenu.");

    // Reset the Game Over panel's UI elements.
    if (gameOverScreen != null) {
        // Reset the panel's alpha to 0.
        Image panelImage = gameOverScreen.GetComponent<Image>();
        if (panelImage != null) {
            Color col = panelImage.color;
            col.a = 0f;
            panelImage.color = col;
            Debug.Log("GameOver panel alpha reset.");
        } else {
            Debug.LogWarning("GameOver screen does not have an Image component!");
        }
        // Disable all child elements (buttons, text, etc.)
        foreach (Transform child in gameOverScreen.transform) {
            child.gameObject.SetActive(false);
        }
        Debug.Log("GameOver panel children disabled.");
    } else {
        Debug.LogWarning("GameOver screen not assigned!");
    }

    // Disable gameplay UI elements.
    if (SingletonManager.Instance != null && SingletonManager.Instance.gameplayCanvas != null) {
        SingletonManager.Instance.gameplayCanvas.gameObject.SetActive(false);
    }
    
    // Reset game state (leave tower mode).
    if (MasterLevelManager.Instance != null) {
        MasterLevelManager.Instance.inTower = false;
    }
    
    // Load the MainMenu scene additively so that PersistentManager and MainTower remain loaded.
    AsyncOperation op = SceneManager.LoadSceneAsync("MainMenu", LoadSceneMode.Additive);
    while (!op.isDone) {
        yield return null;
    }
    
    // Optionally, set MainMenu as the active scene.
    Scene mainMenuScene = SceneManager.GetSceneByName("MainMenu");
    if (mainMenuScene.IsValid()) {
        SceneManager.SetActiveScene(mainMenuScene);
    } else {
        Debug.LogError("MainMenu scene not found!");
    }
    
    // Disable persistent main camera by disabling its Camera and AudioListener components.
    GameObject mainCamera = GameObject.FindWithTag("MainCamera");
    if (mainCamera != null) {
        Camera cam = mainCamera.GetComponent<Camera>();
        AudioListener audioListener = mainCamera.GetComponent<AudioListener>();
        if (cam != null) {
            cam.enabled = false;
            Debug.Log("Camera component disabled.");
        } else {
            Debug.LogError("Camera component not found on MainCamera.");
        }
        if (audioListener != null) {
            audioListener.enabled = false;
            Debug.Log("AudioListener component disabled.");
        } else {
            Debug.LogError("AudioListener component not found.");
        }
    } else {
        Debug.LogError("MainCamera not found!");
    }
    
    // Disable gameplay canvas again to be safe.
    if (SingletonManager.Instance != null && SingletonManager.Instance.gameplayCanvas != null) {
        SingletonManager.Instance.gameplayCanvas.gameObject.SetActive(false);
    }
    
    // Wait a brief moment before hiding the loading UI.
    yield return new WaitForSeconds(0.5f);
    if (LoadingUI.Instance != null)
        LoadingUI.Instance.HideLoading();
    
    Debug.Log("Returned to Main Menu");
}

    
    /// <summary>
    /// Restarts the game with a new seed, bypassing the Main Menu.
    /// Resets the Game Over screen (alpha and children), generates a new seed,
    /// sets the game to tower mode, and starts floor generation.
    /// </summary>
    public void RestartGameWithNewSeed() {
        if (LoadingUI.Instance != null)
            LoadingUI.Instance.ShowLoading("Restarting...");
        else
            Debug.LogWarning("LoadingUI instance not found in RestartGameWithNewSeed.");
        
        // Reset the game over screen, if it exists.
        if (gameOverScreen != null) {
            // Reset the panel's alpha to 0.
            Image img = gameOverScreen.GetComponent<Image>();
            if (img != null) {
                Color col = img.color;
                col.a = 0f;
                img.color = col;
            }
            // Disable all children
            foreach (Transform child in gameOverScreen.transform) {
                child.gameObject.SetActive(false);
            }
        } else {
            Debug.LogWarning("GameOver screen not assigned!");
        }
        
        // Generate a new seed.
        int newSeed = Random.Range(100000, 1000000);
        
        if (MasterLevelManager.Instance != null) {
            MasterLevelManager.Instance.globalSeed = newSeed;
            MasterLevelManager.Instance.inTower = true;
            
            // Start the game at floor 1 immediately.
            MasterLevelManager.Instance.GenerateAndLoadFloor(1, true);
            Debug.Log("Restarting game with new seed: " + newSeed);
        } else {
            Debug.LogError("MasterLevelManager instance not found!");
        }
        
        // Reset the player state (health, movement, alpha, position)
        ResetPlayer();
        
        // Reset key count.
        if (KeyManager.Instance != null)
        {
            KeyManager.Instance.ResetKeys();
            Debug.Log("Key count reset.");
        }
        else
        {
            Debug.LogWarning("KeyManager instance not found!");
        }
        
        if (LoadingUI.Instance != null)
            LoadingUI.Instance.HideLoading();
    }
}
