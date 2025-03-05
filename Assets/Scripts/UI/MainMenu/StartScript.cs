using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartScript : MonoBehaviour
{
    // These references can be assigned in the Inspector if needed.
    public GameObject mainMenuCanvas; 
    public Camera mainCamera;  // This is the camera in the MainMenu scene.

    public void OnStartGameClicked()
    {
        // Check if the player is "dead" (for example, health is 0 or movement disabled)
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            PlayerInfo playerInfo = player.GetComponent<PlayerInfo>();
            Movement playerMovement = player.GetComponent<Movement>();
            if (playerInfo != null && (playerInfo.currentHealth <= 0 || (playerMovement != null && !playerMovement.enabled)))
            {
                // Find the Quit component that holds ResetPlayer
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
        
        // 1. Enable the GameplayCanvas and its sliders.
        if (SingletonManager.Instance != null && LoadingUI.Instance != null)
        {
            SingletonManager.Instance.gameplayCanvas.gameObject.SetActive(true);
            // Show the loading panel.
            LoadingUI.Instance.ShowLoading("Loading Tower...");
            
            Transform sprintSlider = SingletonManager.Instance.gameplayCanvas.transform.Find("SprintSlider");
            if (sprintSlider != null)
                sprintSlider.gameObject.SetActive(true);
            else
                Debug.LogWarning("SprintSlider not found under GameplayCanvas.");
            
            Transform healthSlider = SingletonManager.Instance.gameplayCanvas.transform.Find("HealthSlider");
            if (healthSlider != null)
                healthSlider.gameObject.SetActive(true);
            else
                Debug.LogWarning("HealthSlider not found under GameplayCanvas.");
            
            Transform invButton = SingletonManager.Instance.gameplayCanvas.transform.Find("ShowMainInventory");
            if (invButton != null)
                invButton.gameObject.SetActive(true);
            else
                Debug.LogWarning("ShowMainInventory not found under GameplayCanvas.");
            
            Transform toolbar = SingletonManager.Instance.gameplayCanvas.transform.Find("Toolbar");
            if (toolbar != null)
                toolbar.gameObject.SetActive(true);
            else
                Debug.LogWarning("Toolbar not found under GameplayCanvas.");
            
            Transform lorePanel = SingletonManager.Instance.gameplayCanvas.transform.Find("LorePanel");
            if (lorePanel != null)
                lorePanel.gameObject.SetActive(true);
            else
                Debug.LogWarning("LorePanel not found under GameplayCanvas.");
        }
        else
        {
            Debug.LogWarning("LoadingUI or SingletonManager instance not found!");
        }
        
        // Generate a new seed value.
        int newSeed = Random.Range(100000, 1000000);
        
        // 2. Signal MasterLevelManager to enter the tower.
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
        
        // 3. Switch cameras:
        //    Deactivate the MainMenu camera (assigned via mainCamera) and activate the persistent one.
        if (SingletonManager.Instance != null)
        {
            if (SingletonManager.Instance.mainCamera != null)
                SingletonManager.Instance.mainCamera.gameObject.SetActive(true);
            if (mainCamera != null)
                mainCamera.gameObject.SetActive(false);
        }
        
        // 4. Optionally, disable the MainMenuCanvas and unload the MainMenu scene.
        // (Your current code for this is commented out.)
        // if (SingletonManager.Instance != null && mainMenuCanvas != null)
        // {
        //     mainMenuCanvas.SetActive(false);
        // }
        // else
        // {
        //     Debug.LogWarning("MainMenuCanvas or SingletonManager not found!");
        // }
        //
        // SceneManager.UnloadSceneAsync("MainMenu");
    }
}
