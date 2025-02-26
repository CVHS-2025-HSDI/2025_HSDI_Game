using UnityEngine;
using UnityEngine.SceneManagement;

public class TowerSceneLoader : MonoBehaviour
{
    void Start()
    {
        if (!SceneManager.GetSceneByName("PersistentManager").isLoaded)
        {
            SceneManager.LoadScene("PersistentManager", LoadSceneMode.Additive);
        }
    }

    public void LoadNextFloor()
    {
        Scene oldFloor = SceneManager.GetSceneByName("TowerFloorTemplate");
        if (oldFloor.IsValid())
            SceneManager.UnloadSceneAsync(oldFloor);

        SceneManager.LoadScene("TowerFloorTemplate", LoadSceneMode.Additive);
    }
}