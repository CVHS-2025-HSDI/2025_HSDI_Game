using UnityEngine;

public class PauseMenuController : MonoBehaviour
{
    // Reference to the pause menu UI panel
    public GameObject pauseMenuPanel;

    // Track the game's paused state
    private bool isPaused = false;

    void Update()
    {
        // Check for the Escape key press
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // Toggle the pause state
            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    // Activate the pause menu panel and stop time
    public void PauseGame()
    {
        pauseMenuPanel.SetActive(true);
        Time.timeScale = 0f; // Freezes the game
        isPaused = true;
    }

    // Deactivate the pause menu panel and resume time
    public void ResumeGame()
    {
        pauseMenuPanel.SetActive(false);
        Time.timeScale = 1f; // Resumes the game
        isPaused = false;
    }
}