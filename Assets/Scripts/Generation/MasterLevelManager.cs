using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class MasterLevelManager : MonoBehaviour
{
    public FloorConfig floorConfig;
    public int totalFloors = 5; // Can be changed; maybe load from global settings later?
    public int globalSeed = 12345;

    // Prefabs for player and merchant (only used if they don't already exist)
    public GameObject playerPrefab;
    public GameObject merchantPrefab;

    // Dictionary storing floor data
    private Dictionary<int, FloorData> floorsData = new Dictionary<int, FloorData>();

    // Cached references to player and merchant if they exist in the scene
    private GameObject player;
    private GameObject merchant;

    // Store state to know which floor we’re generating, if it's first, etc.
    private int _currentFloorNumber;
    private bool _isFirstFloorLoad = false;

    void Awake()
    {
        // Ensure this manager persists across scene changes
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        // Set global seed
        RandomSeed.SetSeed(globalSeed);

        // Try to find the existing player and merchant (from PersistentManager)
        player = GameObject.FindWithTag("Player");
        merchant = GameObject.FindWithTag("Merchant");

        // Listen for scene load events
        SceneManager.sceneLoaded += OnSceneLoaded;

        // Generate the first floor right away
        GenerateAndLoadFloor(1, isFirstFloor: true);
    }

    /// <summary>
    /// Called when we want to load a new floor.
    /// We unload the old "TowerFloorTemplate" and then load a fresh one.
    /// Afterwards, OnSceneLoaded(...) will do the actual floor generation.
    /// </summary>
    public void GenerateAndLoadFloor(int floorNumber, bool isFirstFloor)
    {
        _currentFloorNumber = floorNumber;
        _isFirstFloorLoad = isFirstFloor;

        // Unload old "TowerFloorTemplate" if it exists
        var oldFloor = SceneManager.GetSceneByName("TowerFloorTemplate");
        if (oldFloor.IsValid())
        {
            SceneManager.UnloadSceneAsync(oldFloor);
        }

        // Now load a fresh "TowerFloorTemplate"
        SceneManager.LoadScene("TowerFloorTemplate", LoadSceneMode.Additive);
    }

    /// <summary>
    /// Unity callback fired whenever a scene finishes loading.
    /// We'll look for "TowerFloorTemplate" here and, if loaded, generate the floor.
    /// </summary>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "TowerFloorTemplate")
        {
            // Find the FloorGenerator
            FloorGenerator floorGen = FindAnyObjectByType<FloorGenerator>(); // Unity 2023+ version of FindObjectOfType
            if (floorGen == null)
            {
                Debug.LogError("[MasterLevelManager] No FloorGenerator found in TowerFloorTemplate scene!");
                return;
            }

            // Generate the floor
            FloorData data = floorGen.GenerateFloor(floorConfig, _isFirstFloorLoad);
            floorsData[_currentFloorNumber] = data;

            // Convert tile coordinates to world (with 0.5 offset)
            Vector3 playerSpawnWorld = floorGen.floorTilemap.CellToWorld(data.playerSpawn) + new Vector3(0.5f, 0.5f, 0);
            Vector3 merchantSpawnWorld = floorGen.floorTilemap.CellToWorld(data.merchantSpawn) + new Vector3(0.5f, 0.5f, 0);

            // Find existing Player and Merchant
            player = GameObject.FindWithTag("Player");
            merchant = GameObject.FindWithTag("Merchant");

            // Move player to new spawn position
            if (player != null)
            {
                player.transform.position = playerSpawnWorld;
            }

            // Move merchant (only if it's the first floor)
            if (_isFirstFloorLoad && merchant != null)
            {
                merchant.transform.position = merchantSpawnWorld;
            }

            Debug.Log($"[MasterLevelManager] Floor {_currentFloorNumber} loaded. First Floor? {_isFirstFloorLoad}");
        }
    }
}
