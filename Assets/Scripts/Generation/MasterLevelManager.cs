using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class MasterLevelManager : MonoBehaviour
{
    public FloorConfig floorConfig;
    public int totalFloors = 8;
    public int globalSeed = 1337420;

    public GameObject playerPrefab;
    public GameObject merchantPrefab;

    private Dictionary<int, FloorData> floorsData = new Dictionary<int, FloorData>();

    private GameObject player;
    private GameObject merchant;
    
    private int _currentFloorNumber;
    private bool _isFirstFloorLoad = false;

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        // Set the global seed once.
        RandomSeed.SetSeed(globalSeed);

        player   = GameObject.FindWithTag("Player");
        merchant = GameObject.FindWithTag("Merchant");
        
        SceneManager.sceneLoaded += OnSceneLoaded;

        // Start with floor 1
        GenerateAndLoadFloor(1, true);
    }

    public void GenerateAndLoadFloor(int floorNumber, bool isFirstFloor)
    {
        LoadingUI.Instance.ShowLoading("Loading floor " + floorNumber + "...");
        _currentFloorNumber = floorNumber;
        _isFirstFloorLoad   = isFirstFloor;

        // Instead of always using the same global seed,
        // compute a floor-specific seed (for example, add a constant multiple of the floor number)
        int floorSeed = globalSeed + floorNumber * 12345;
        RandomSeed.SetSeed(floorSeed);

        // Unload the old floor if it exists.
        var oldFloor = SceneManager.GetSceneByName("TowerFloorTemplate");
        if (oldFloor.IsValid())
        {
            SceneManager.UnloadSceneAsync(oldFloor);
        }
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

            // Generate the floor layout (with our floor-specific seed)
            FloorData data = floorGen.GenerateFloor(
                floorConfig, 
                _isFirstFloorLoad, 
                _currentFloorNumber, 
                totalFloors
            );
            floorsData[_currentFloorNumber] = data;

            // Convert tile coords to world positions.
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

            // Hook camera
            var playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null)
            {
                CameraFollow camFollow = FindAnyObjectByType<CameraFollow>();
                if (camFollow != null)
                {
                    camFollow.SetTarget(playerObj.transform);
                }
            }
            
            LoadingUI.Instance.HideLoading();
            Debug.Log($"[MasterLevelManager] Floor {_currentFloorNumber} loaded. First Floor? {_isFirstFloorLoad}");
        }
    }
}
