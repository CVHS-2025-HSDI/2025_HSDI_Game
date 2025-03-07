using UnityEngine;

public class Options : MonoBehaviour
{
    public GameObject optionsMenu;
    public GameObject mainMenuButtons;
    
    public void OpenOptions() {
        // Enable the Options panel.
        if (optionsMenu != null)
            optionsMenu.gameObject.SetActive(true);
        if (mainMenuButtons != null)
            mainMenuButtons.gameObject.SetActive(false);
    }
}
