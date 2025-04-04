using UnityEngine;
using UnityEngine.SceneManagement;

public class Bootstrapper : MonoBehaviour {
    [Tooltip("Name of the PersistentManager scene to load additively.")]
    public string mainMenuScene = "MainMenu";

    [Tooltip("Name of the MainTower scene to load additively.")]
    public string mainTowerScene = "MainTower";

    void Awake() {
        // Load PersistentManager scene if not loaded.
        if (!IsSceneLoaded(mainMenuScene)) {
            SceneManager.LoadScene(mainMenuScene, LoadSceneMode.Additive);
        }
        // Load MainTower scene if not loaded.
        if (!IsSceneLoaded(mainTowerScene)) {
            SceneManager.LoadScene(mainTowerScene, LoadSceneMode.Additive);
        }
    }

    // Helper function to check if a scene is already loaded.
    bool IsSceneLoaded(string sceneName) {
        for (int i = 0; i < SceneManager.sceneCount; i++) {
            Scene loadedScene = SceneManager.GetSceneAt(i);
            if (loadedScene.name == sceneName)
                return true;
        }
        return false;
    }
}