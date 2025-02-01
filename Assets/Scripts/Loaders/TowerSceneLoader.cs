using UnityEngine;
using UnityEngine.SceneManagement;

public class TowerSceneLoader : MonoBehaviour
{
    
    // Load PersistentManager & UIOverlay if not already loaded
    void Start()
    {
        // Check if PersistentManager is already loaded
        if (!SceneManager.GetSceneByName("PersistentManager").isLoaded)
        {
            SceneManager.LoadScene("PersistentManager", LoadSceneMode.Additive);
        }

        // Do the same for UIOverlay
        if (!SceneManager.GetSceneByName("UIOverlay").isLoaded)
        {
            SceneManager.LoadScene("UIOverlay", LoadSceneMode.Additive);
        }
    }

    public void LoadNextFloor()
    {
        // Unload the old floor as an example
        Scene oldFloor = SceneManager.GetSceneByName("TowerFloorTemplate");
        if (oldFloor.IsValid())
        {
            SceneManager.UnloadSceneAsync(oldFloor);
        }

        // Load a new fresh floor template
        SceneManager.LoadScene("TowerFloorTemplate", LoadSceneMode.Additive);
    }
}