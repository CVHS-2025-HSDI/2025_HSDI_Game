using UnityEngine;
using UnityEngine.SceneManagement;

public class TowerSceneLoader : MonoBehaviour
{
    private bool _loaded = false;

    void Start()
    {
        // Load PersistentManager & UIOverlay if not already loaded
        if (!_loaded)
        {
            SceneManager.LoadScene("PersistentManager", LoadSceneMode.Additive);
            SceneManager.LoadScene("UIOverlay", LoadSceneMode.Additive);
            // Optionally load an initial floor; could leave this alone
            // SceneManager.LoadScene("TowerFloorTemplate", LoadSceneMode.Additive);
            _loaded = true;
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