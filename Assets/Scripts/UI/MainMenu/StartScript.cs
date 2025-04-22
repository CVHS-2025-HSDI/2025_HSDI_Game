using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Playables;
using System.Collections;

public class StartScript : MonoBehaviour
{
    public GameObject mainMenuCanvas;
    public GameObject mainMenuCamera;
    public GameObject currentEventSystem;

    private Movement playerMovement;

    public void OnStartGameClicked()
    {
        Debug.Log("[Cutscene] OnStartGameClicked → starting coroutine");
        StartCoroutine(PlayBedCutsceneAndStartGame());
    }

    private IEnumerator PlayBedCutsceneAndStartGame()
    {
        Debug.Log("[Cutscene] Coroutine begin");

        // ---- 0) reset dead player (unchanged) ----
        var player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            var info = player.GetComponent<PlayerInfo>();
            playerMovement = player.GetComponent<Movement>();
            if (info != null && (info.currentHealth <= 0 || (playerMovement != null && !playerMovement.enabled)))
            {
                var quitScript = FindFirstObjectByType<Quit>();
                if (quitScript != null)
                    quitScript.ResetPlayer();
            }
        }

        // ---- 1) load "Bed" additively ----
        Debug.Log("[Cutscene] Loading 'Bed' scene…");
        yield return SceneManager.LoadSceneAsync("Bed", LoadSceneMode.Additive);

        Scene bedScene = SceneManager.GetSceneByName("Bed");
        Debug.Log($"[Cutscene] bedScene.isLoaded? {bedScene.isLoaded}");

        // ---- 2) swap cameras/UI ----
        var mgr = SingletonManager.Instance;
        if (mgr != null)
        {
            Debug.Log("[Cutscene] Activating bedroom camera + cutscene UI");
            mainMenuCamera.SetActive(false);
            // disable rendering & raycasts but keep GO active, otherwise cutscene shits itself
            var canvas = mainMenuCanvas.GetComponent<Canvas>();
            if (canvas != null)
                canvas.enabled = false;
            else
                mainMenuCanvas.SetActive(false);
            mgr.bedroomCamera.gameObject.SetActive(true);
            mgr.cutsceneCanvas.gameObject.SetActive(true);
        }

        // ---- 3) find the director & decide wait duration ----
        var timelineGO = GameObject.FindWithTag("BedTimeline");
        PlayableDirector director = timelineGO?.GetComponent<PlayableDirector>();
        float waitDuration = 12f;

        if (director != null)
        {
            waitDuration = (float)director.duration;
            Debug.Log($"[Cutscene] Director found. Duration = {waitDuration:0.00}s. Playing…");
            director.Play();
        }
        else
        {
            Debug.Log("[Cutscene] No director found; using fallback 12s wait");
        }

        // ---- 4) manual unscaled-time loop ----
        float timer = 0f;
        while (timer < waitDuration && !Input.anyKeyDown)
        {
            timer += Time.unscaledDeltaTime;
            Debug.Log($"[Cutscene] waiting… {timer:0.00}/{waitDuration:0.00}");
            yield return null;
        }
        Debug.Log($"[Cutscene] Wait loop exited: timer={timer:0.00}, anyKeyDown={Input.anyKeyDown}");

        // ---- 5) cleanup director if still playing ----
        if (director != null && director.state == PlayState.Playing)
        {
            Debug.Log("[Cutscene] Stopping director manually");
            director.Stop();
        }

        // ---- 6) unload Bed scene ----
        Debug.Log("[Cutscene] Unloading 'Bed' scene…");
        yield return SceneManager.UnloadSceneAsync(bedScene);
        Debug.Log("[Cutscene] Unload complete");

        // ---- 7) restore cameras/UI ----
        if (mgr != null)
        {
            Debug.Log("[Cutscene] Restoring main camera + UI");
            mgr.mainCamera.gameObject.SetActive(true);
            mgr.bedroomCamera.gameObject.SetActive(false);
            mgr.cutsceneCanvas.gameObject.SetActive(false);
        }

        // ---- 8) continue to game ----
        Debug.Log("[Cutscene] Calling SetupGameplayAndTower");
        SetupGameplayAndTower();
    }
    
    private void SetupGameplayAndTower()
    {
        // --- 7) Enable the GameplayCanvas & elements ---
        if (SingletonManager.Instance != null && LoadingUI.Instance != null)
        {
            var ui = SingletonManager.Instance.gameplayCanvas.gameObject;
            ui.SetActive(true);
            LoadingUI.Instance.ShowLoading("Loading Tower...");

            // you can still find and activate any sub‑elements as before
            Transform sprintSlider = SingletonManager.Instance.gameplayCanvas.transform.Find("SprintSlider");
            if (sprintSlider != null) sprintSlider.gameObject.SetActive(true);
            Transform healthSlider = SingletonManager.Instance.gameplayCanvas.transform.Find("HealthSlider");
            if (healthSlider != null) healthSlider.gameObject.SetActive(true);
            Transform invButton = SingletonManager.Instance.gameplayCanvas.transform.Find("ShowMainInventory");
            if (invButton != null) invButton.gameObject.SetActive(true);
            Transform toolbar = SingletonManager.Instance.gameplayCanvas.transform.Find("Toolbar");
            if (toolbar != null) toolbar.gameObject.SetActive(true);
            Transform lorePanel = SingletonManager.Instance.gameplayCanvas.transform.Find("LorePanel");
            if (lorePanel != null) lorePanel.gameObject.SetActive(true);

            SingletonManager.Instance.showCharacter.gameObject.SetActive(true);
            SingletonManager.Instance.xpText.gameObject.SetActive(true);

            // Switch event systems
            if (currentEventSystem != null)
                currentEventSystem.SetActive(false);
            var eventSys = SingletonManager.Instance.eventSystem;
            if (eventSys != null)
                eventSys.SetActive(true);
        }
        else Debug.LogWarning("LoadingUI or SingletonManager instance not found!");

        // Generate a new seed and signal the level manager to generate the tower.
        int newSeed = Random.Range(100000, 1000000);
        var mlm = MasterLevelManager.Instance;
        if (mlm != null)
        {
            mlm.globalSeed = newSeed;
            mlm.inTower = true;
            mlm.GenerateAndLoadFloor(1, true);
        }
        else Debug.LogError("MasterLevelManager instance not found!");
    }
}
