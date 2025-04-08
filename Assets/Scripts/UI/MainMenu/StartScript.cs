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

    // In this version we no longer need the cutsceneText reference
    // private Text cutsceneText;

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

        // --- SKIP THE BED CUTSCENE ---
        // Instead of loading the "Bed" scene and showing the cutscene,
        // we immediately set up gameplay.
        SetupGameplayAndTower();
    }

    private void SetupGameplayAndTower()
    {
        // --- 7) Enable the GameplayCanvas & elements ---
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
            
            SingletonManager.Instance.showCharacter.gameObject.SetActive(true);
            SingletonManager.Instance.xpText.gameObject.SetActive(true);
            
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

        // Generate a new seed and signal the level manager to generate the tower.
        int newSeed = Random.Range(100000, 1000000);

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

        // (Optional) Unload MainMenu scene and hide MainMenuCanvas if you want.
        // if (SingletonManager.Instance != null && mainMenuCanvas != null)
        // {
        //     mainMenuCanvas.SetActive(false);
        // }
        // SceneManager.UnloadSceneAsync("MainMenu");
    }
}
