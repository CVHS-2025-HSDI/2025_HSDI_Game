using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

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

    // New flag that tells us whether we are inside the tower.
    public bool inTower = false;

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        // Set the global seed.
        RandomSeed.SetSeed(globalSeed);

        player   = GameObject.FindWithTag("Player");
        merchant = GameObject.FindWithTag("Merchant");
        
        SceneManager.sceneLoaded += OnSceneLoaded;

        // If inTower is true (set in the editor) then immediately trigger floor generation.
        if (inTower)
        {
            EnterTower();
        }
        else
        {
            Debug.Log("Not in tower yet; floor generation will be disabled until EnterTower() is called.");
        }
    }

    // This method is called when the player “enters” the tower.
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

        LoadingUI.Instance.ShowLoading("Loading floor " + floorNumber + "...");
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

            CameraFollow camFollow = FindAnyObjectByType<CameraFollow>();
            if (camFollow != null && player != null)
                camFollow.SetTarget(player.transform);
            
            LoadingUI.Instance.HideLoading();
            Debug.Log($"[MasterLevelManager] Floor {_currentFloorNumber} loaded. First Floor? {_isFirstFloorLoad}");
        }
    }
}
