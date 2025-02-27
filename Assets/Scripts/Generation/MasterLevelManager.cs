using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class MasterLevelManager : MonoBehaviour
{
    public static MasterLevelManager Instance;  // Singleton instance

    public FloorConfig floorConfig;
    public int totalFloors = 16;
    public int globalSeed = 1337420;
    
    public GameObject playerPrefab;
    public GameObject merchantPrefab;

    private Dictionary<int, FloorData> floorsData = new Dictionary<int, FloorData>();
    private GameObject player;
    private GameObject merchant;
    
    private int _currentFloorNumber;
    private bool _isFirstFloorLoad = false;

    // Flag to indicate whether we’re inside the tower.
    public bool inTower = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Set the global seed.
        RandomSeed.SetSeed(globalSeed);

        // Initially, if you start in the MainMenu, you don’t generate a floor.
        // inTower remains false until EnterTower() is called.
        Debug.Log("MasterLevelManager: Waiting for player to enter the tower.");
    }
    
    void OnEnable()
    {
        // Subscribe to Unity’s sceneLoaded event
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    
    void OnDisable()
    {
        // Unsubscribe to avoid memory leaks or multiple calls
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // Called from MainMenuController when Start Game is pressed.
    public void EnterTower()
    {
        inTower = true;
        // Now start with floor 1 inside the tower.
        GenerateAndLoadFloor(1, true);
    }

    public void GenerateAndLoadFloor(int floorNumber, bool isFirstFloor)
    {
        if (!inTower)
        {
            Debug.Log("Not in tower; floor generation disabled.");
            return;
        }

        if (LoadingUI.Instance != null)
            LoadingUI.Instance.ShowLoading("Loading floor " + floorNumber + "...");
        else
            Debug.LogError("LoadingUI instance is null in GenerateAndLoadFloor!");
        _currentFloorNumber = floorNumber;
        _isFirstFloorLoad = isFirstFloor;

        // Use a floor-specific seed.
        int floorSeed = globalSeed + floorNumber * 12345;
        RandomSeed.SetSeed(floorSeed);

        // Unload the old floor if it exists.
        var oldFloor = SceneManager.GetSceneByName("TowerFloorTemplate");
        if (oldFloor.IsValid())
            SceneManager.UnloadSceneAsync(oldFloor);

        SceneManager.LoadScene("TowerFloorTemplate", LoadSceneMode.Additive);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "TowerFloorTemplate")
        {
            FloorGenerator floorGen = FindAnyObjectByType<FloorGenerator>();
            if (floorGen == null)
            {
                Debug.LogError("[MasterLevelManager] No FloorGenerator found in TowerFloorTemplate!");
                return;
            }

            FloorData data = floorGen.GenerateFloor(
                floorConfig, 
                _isFirstFloorLoad, 
                _currentFloorNumber, 
                totalFloors
            );
            floorsData[_currentFloorNumber] = data;

            Vector3 playerSpawnWorld = floorGen.floorTilemap.CellToWorld(data.playerSpawn) + new Vector3(0.5f, 0.5f, 0);
            Vector3 merchantSpawnWorld = floorGen.floorTilemap.CellToWorld(data.merchantSpawn) + new Vector3(0.5f, 0.5f, 0);

            player = GameObject.FindWithTag("Player");
            merchant = GameObject.FindWithTag("Merchant");

            if (player == null)
            {
                player = Instantiate(playerPrefab, playerSpawnWorld, Quaternion.identity);
                player.tag = "Player";
            }
            else
            {
                player.transform.position = playerSpawnWorld;
            }
            
            if (merchant == null && _isFirstFloorLoad)
            {
                merchant = Instantiate(merchantPrefab, merchantSpawnWorld, Quaternion.identity);
                merchant.tag = "Merchant";
            }
            else if (_isFirstFloorLoad)
            {
                merchant.transform.position = merchantSpawnWorld;
            }

            // Set the camera to follow the player.
            CameraFollow camFollow = FindAnyObjectByType<CameraFollow>();
            if (camFollow != null && player != null)
                camFollow.SetTarget(player.transform);
            
            // Hide the loading panel (assumed to be managed by LoadingUI).
            LoadingUI.Instance.HideLoading();

            // Unload the MainMenu scene now that the game is ready, if we're going from the main menu
            Scene mainMenu = SceneManager.GetSceneByName("MainMenu");
            if (mainMenu.IsValid())
                SceneManager.UnloadSceneAsync("MainMenu");

            Debug.Log($"[MasterLevelManager] Floor {_currentFloorNumber} loaded. First Floor? {_isFirstFloorLoad}");
        }
    }
}
