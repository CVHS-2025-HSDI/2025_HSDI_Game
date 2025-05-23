using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class DoorTrigger : MonoBehaviour
{
    [Tooltip("Name of the scene to load (single or additive)")]
    public string sceneToLoad;

    [Tooltip("If true, start in-tower gameplay; otherwise just load town")]
    public bool startTowerMode = false;

    [Tooltip("How long to fade in/out (optional)")]
    public float fadeDuration = 0.5f;

    bool _isLoading = false;
    GameObject _player;  // cache the player that triggered us

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_isLoading) return;
        if (!other.CompareTag("Player")) return;

        _player = other.gameObject;
        _isLoading = true;
        StartCoroutine(TransitionRoutine());
    }

    private IEnumerator TransitionRoutine()
    {
        // 1) Fade out
        if (LoadingUI.Instance != null)
            LoadingUI.Instance.ShowLoading("Entering " + sceneToLoad + "…");

        // 2) Load the target scene additively
        if (sceneToLoad.Equals("TowerFloorTemplate"))
        {
            startTowerMode = true; // Not loading TowerFloorTemplate since it will be loaded by MasterLevelManager
        }
        else
        {
            var asyncLoad = SceneManager.LoadSceneAsync(sceneToLoad, LoadSceneMode.Additive);
            while (!asyncLoad.isDone)
                yield return null;
        }

        // 3) Make the new scene active so that FindGameObject calls target it
        Scene newScene = SceneManager.GetSceneByName(sceneToLoad);
        if (newScene.IsValid())
            SceneManager.SetActiveScene(newScene);

        // 4) Re-configure cameras & UI
        if (SingletonManager.Instance != null)
        {
            SingletonManager.Instance.townCamera.gameObject.SetActive(true);
            SingletonManager.Instance.mainCamera.gameObject.SetActive(false);
            SingletonManager.Instance.gameplayCanvas.gameObject.SetActive(true);
        }

        // 5) For non-tower: reset player state, then move to a Town spawn point
        if (!startTowerMode)
        {
            var quitScript = FindFirstObjectByType<Quit>();
            quitScript?.ResetPlayer();

            _player.transform.position = new Vector3(20.5f, 20.5f, 0);
        }
        else
        {
            // 5b) Tower mode: wire up UI + kick off Tower generation
            LoaderManager.SetupGameplayAndTower();
        }

        // 6) Hide loading
        if (LoadingUI.Instance != null)
            LoadingUI.Instance.HideLoading();

        _isLoading = false;
    }
}
