using UnityEngine;
using System.Collections.Generic;

public class MasterLevelManager : MonoBehaviour
{
    public FloorGenerator floorGenerator;
    public FloorConfig floorConfig;
    public int totalFloors = 5; // Can be changed as needed; maybe load from global settings later?

    public int globalSeed = 12345;

    // Placeholders
    public GameObject playerPrefab;
    public GameObject merchantPrefab;

    // Store the FloorData for each floor
    private Dictionary<int, FloorData> floorsData = new Dictionary<int, FloorData>();

    // Cached references to player and merchant if they exist in the scene
    private GameObject player;
    private GameObject merchant;

    void Start()
    {
        RandomSeed.SetSeed(globalSeed);

        // Find existing player/merchant in scene if they exist
        player = GameObject.FindWithTag("Player");
        merchant = GameObject.FindWithTag("Merchant");

        // Imagine we do an intro cutscene here...
        // Then, once the cutscene is done, we generate the first floor:
        GenerateAndLoadFloor(1, isFirstFloor: true);
    }

    /// <summary>
    /// Called when we need to generate or load a floor.
    /// If we haven't generated it yet, we do so now.
    /// Then we move or spawn the player/merchant accordingly.
    /// </summary>
    public void GenerateAndLoadFloor(int floorNumber, bool isFirstFloor)
    {
        // If we already have data for this floor, maybe we'd just load from that
        // TODO: Implement saving floors
        FloorData data = floorGenerator.GenerateFloor(floorConfig, isFirstFloor);

        // Store the result
        floorsData[floorNumber] = data;

        // Convert tile coordinates to world and apply a 0.5 offset for correct positioning
        Vector3 playerSpawnWorld = floorGenerator.floorTilemap.CellToWorld((Vector3Int)data.playerSpawn) + new Vector3(0.5f, 0.5f, 0);
        Vector3 merchantSpawnWorld = floorGenerator.floorTilemap.CellToWorld((Vector3Int)data.merchantSpawn) + new Vector3(0.5f, 0.5f, 0);

        // Move existing player if found; otherwise, instantiate
        if (player != null)
        {
            player.transform.position = playerSpawnWorld;
        }
        else
        {
            player = Instantiate(playerPrefab, playerSpawnWorld, Quaternion.identity);
        }

        // If it's the first floor, also handle the merchant
        if (isFirstFloor)
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

        // If we want to offset floors in the same scene, do it here:
        // floorGenerator.floorTilemap.transform.position = new Vector3(0, floorNumber * 30, 0);
        // Or each floor might be in a separate scene. Up to you, green team reader!
    }
}
