using UnityEngine;

public static class LoaderManager
{
    public static void SetupGameplayAndTower()
    {
        // --- 1) Enable the GameplayCanvas & elements ---
        if (SingletonManager.Instance != null && LoadingUI.Instance != null)
        {
            var ui = SingletonManager.Instance.gameplayCanvas.gameObject;
            ui.SetActive(true);
            LoadingUI.Instance.ShowLoading("Loading Tower...");

            // activate sub-elements by name (or cache references instead)
            Transform sprint = ui.transform.Find("SprintSlider");
            if (sprint  != null) sprint .gameObject.SetActive(true);
            Transform health = ui.transform.Find("HealthSlider");
            if (health  != null) health .gameObject.SetActive(true);
            Transform invBtn = ui.transform.Find("ShowMainInventory");
            if (invBtn  != null) invBtn .gameObject.SetActive(true);
            Transform toolbar = ui.transform.Find("Toolbar");
            if (toolbar  != null) toolbar .gameObject.SetActive(true);
            Transform lore  = ui.transform.Find("LorePanel");
            if (lore     != null) lore    .gameObject.SetActive(true);

            SingletonManager.Instance.showCharacter.gameObject.SetActive(true);
            SingletonManager.Instance.xpText       .gameObject.SetActive(true);
            var eventSys = SingletonManager.Instance.eventSystem;
            if (eventSys != null)
                eventSys.SetActive(true);
        }
        else
        {
            Debug.LogWarning("LoadingUI or SingletonManager instance not found!");
        }

        // --- 2) Actually start the tower run ---
        var mlm = MasterLevelManager.Instance;
        if (mlm != null)
        {
            mlm.globalSeed = Random.Range(100000, 1000000);
            mlm.inTower     = true;
            mlm.GenerateAndLoadFloor(1, true);
        }
        else
        {
            Debug.LogError("MasterLevelManager instance not found!");
        }
    }
}
