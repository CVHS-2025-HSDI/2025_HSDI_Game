using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class MasterLevelManager : MonoBehaviour
{
    public FloorConfig floorConfig;
    public int totalFloors = 5; // Can be changed; maybe load from global settings later?
    public int globalSeed = 12345;

    // Prefabs for the player and merchant
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
        // If this is your global manager, you might do:
        // DontDestroyOnLoad(gameObject);
        // And store a static Instance reference, etc.
    }

    void Start()
    {
        // Set global seed
        RandomSeed.SetSeed(globalSeed);

        // Find existing player/merchant in scene if they exist
        player = GameObject.FindWithTag("Player");
        merchant = GameObject.FindWithTag("Merchant");

        // listen for scene load events
        SceneManager.sceneLoaded += OnSceneLoaded;

        // For demonstration: generate the first floor right away
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
        // Once loaded, OnSceneLoaded() will be called
        SceneManager.LoadScene("TowerFloorTemplate", LoadSceneMode.Additive);
    }

    /// <summary>
    /// Unity callback fired whenever a scene finishes loading.
    /// We'll look for "TowerFloorTemplate" here and, if loaded, generate the floor.
    /// </summary>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // We only care if the newly loaded scene is "TowerFloorTemplate"
        if (scene.name == "TowerFloorTemplate")
        {
            // At this point, the floor scene is in memory. Let's find the FloorGenerator.
            FloorGenerator floorGen = Object.FindFirstObjectByType<FloorGenerator>();
            if (floorGen == null)
            {
                Debug.LogError("[MasterLevelManager] No FloorGenerator found in TowerFloorTemplate scene!");
                return;
            }

            // Generate the floor
            FloorData data = floorGen.GenerateFloor(floorConfig, _isFirstFloorLoad);

            // Store the result
            floorsData[_currentFloorNumber] = data;

            // Convert tile coordinates to world (with 0.5 offset)
            Vector3 playerSpawnWorld = floorGen.floorTilemap.CellToWorld(data.playerSpawn) + new Vector3(0.5f, 0.5f, 0);
            Vector3 merchantSpawnWorld = floorGen.floorTilemap.CellToWorld(data.merchantSpawn) + new Vector3(0.5f, 0.5f, 0);

            // Move or instantiate the player
            if (player != null)
            {
                player.transform.position = playerSpawnWorld;
            }
            else
            {
                player = Instantiate(playerPrefab, playerSpawnWorld, Quaternion.identity);
            }

            // If it's the first floor, also handle the merchant
            if (_isFirstFloorLoad)
            {
                if (merchant != null)
                {
                    merchant.transform.position = merchantSpawnWorld;
                }
                else
                {
                    merchant = Instantiate(merchantPrefab, merchantSpawnWorld, Quaternion.identity);
                }
            }

            Debug.Log($"[MasterLevelManager] Floor {_currentFloorNumber} generated. FirstFloor? {_isFirstFloorLoad}");
        }
    }
}
