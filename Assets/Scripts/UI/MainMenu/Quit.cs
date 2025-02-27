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
        
        // Disable gameplay UI elements.
        if (SingletonManager.Instance != null && SingletonManager.Instance.gameplayCanvas != null) {
            SingletonManager.Instance.gameplayCanvas.gameObject.SetActive(false);
        }
        
        // Reset game state (e.g., leave tower mode).
        if (MasterLevelManager.Instance != null) {
            MasterLevelManager.Instance.inTower = false;
        }
        
        // Load the MainMenu scene.
        AsyncOperation op = SceneManager.LoadSceneAsync("MainMenu");
        while (!op.isDone) {
            yield return null;
        }
        
        // Optionally disable the persistent main camera if needed.
        if (SingletonManager.Instance != null && SingletonManager.Instance.mainCamera != null) {
            SingletonManager.Instance.mainCamera.gameObject.SetActive(false);
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
            Debug.LogWarning("LoadingUI instance not found in ReturnToMainMenu.");
        
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
        
        if (LoadingUI.Instance != null)
            LoadingUI.Instance.HideLoading();
    }
}
