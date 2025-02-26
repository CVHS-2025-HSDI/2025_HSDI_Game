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
        }
        else
        {
            Debug.LogWarning("LoadingUI or SingletonManager instance not found!");
        }
        
        // 2. Signal MasterLevelManager to enter the tower.
        MasterLevelManager mlm = MasterLevelManager.Instance;
        if (mlm != null)
        {
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
        
        // 4. Disable the MainMenuCanvas.
        // if (SingletonManager.Instance != null && mainMenuCanvas != null)
        // {
        //     mainMenuCanvas.SetActive(false);
        // }
        // else
        // {
        //     Debug.LogWarning("MainMenuCanvas or SingletonManager not found!");
        // }
        //
        // // 5. Finally, unload the MainMenu scene.
        // SceneManager.UnloadSceneAsync("MainMenu");
    }
}
