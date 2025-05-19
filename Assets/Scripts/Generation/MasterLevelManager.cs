using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class MasterLevelManager : MonoBehaviour
{
    public static MasterLevelManager Instance;  // Singleton instance

    public FloorConfig floorConfig;
    public int totalFloors = 16;
    public int globalSeed = 1337420;
    public int highestFloorReached = 1;  // Starting at floor 1.
    
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
        Debug.Log("MasterLevelManager: Waiting for player to enter the tower.");
    }
    
    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    
    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void EnterTower()
    {
        inTower = true;
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
        highestFloorReached = Mathf.Max(highestFloorReached, floorNumber);
        _currentFloorNumber = floorNumber;
        _isFirstFloorLoad = isFirstFloor;

        // Use a floor-specific seed.
        int floorSeed = globalSeed + floorNumber * 12345;
        RandomSeed.SetSeed(floorSeed);

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

            // Always generate the static geometry.
            FloorData generatedData = floorGen.GenerateFloor(floorConfig, _isFirstFloorLoad, _currentFloorNumber, totalFloors);

            // If saved data exists (regardless of floor number), load dynamic objects.
            if (floorsData.ContainsKey(_currentFloorNumber))
            {
                FloorData savedData = floorsData[_currentFloorNumber];
                floorGen.LoadFloorFromData(savedData);
                // Carry over flags such as traversable.
                generatedData.traversable = savedData.traversable;
                Debug.Log($"[MasterLevelManager] Loaded existing FloorData for floor {_currentFloorNumber}.");
            }
            else
            {
                Debug.Log($"[MasterLevelManager] Generated new FloorData for floor {_currentFloorNumber}.");
            }
            
            floorsData[_currentFloorNumber] = generatedData;

            Vector3 playerSpawnWorld = floorGen.floorTilemap.CellToWorld(generatedData.playerSpawn) + new Vector3(0.5f, 0.5f, 0);
            Vector3 merchantSpawnWorld = floorGen.floorTilemap.CellToWorld(generatedData.merchantSpawn) + new Vector3(0.5f, 0.5f, 0);

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

            Scene mainMenu = SceneManager.GetSceneByName("MainMenu");
            if (mainMenu.IsValid())
                SceneManager.UnloadSceneAsync("MainMenu");

            Debug.Log($"[MasterLevelManager] Floor {_currentFloorNumber} loaded. First Floor? {_isFirstFloorLoad}");
        }
    }
    
    public void MarkCurrentFloorTraversable()
    {
        if (floorsData.ContainsKey(_currentFloorNumber))
        {
            floorsData[_currentFloorNumber].traversable = true;
            Debug.Log($"Floor {_currentFloorNumber} marked as traversable.");
        }
    }
    
    public bool IsFloorTraversable(int floorNumber)
    {
        if (floorsData.ContainsKey(floorNumber))
        {
            return floorsData[floorNumber].traversable;
        }
        return false;
    }
    
    public void ClearFloorData()
    {
        floorsData.Clear();
        Debug.Log("Floor data cleared for restart.");
    }

}
