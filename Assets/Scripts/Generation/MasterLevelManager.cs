using UnityEngine;

public class MasterLevelManager : MonoBehaviour
{
    public FloorGenerator floorGenerator;
    public FloorConfig floorConfig;
    public int totalFloors = 3;
    
    public int globalSeed = 12345;

    // placeholders
    public GameObject playerPrefab;
    public GameObject merchantPrefab;

    void Start()
    {
        RandomSeed.SetSeed(globalSeed);

        // generate each floor in sequence
        for (int i = 1; i <= totalFloors; i++)
        {
            bool isFirstFloor = (i == 1);
            floorGenerator.GenerateFloor(floorConfig, isFirstFloor);

            // Now we have spawn positions
            Vector3 playerSpawnWorld = floorGenerator.floorTilemap.CellToWorld(floorGenerator.playerSpawn);
            if (isFirstFloor)
            {
                Vector3 merchantSpawnWorld = floorGenerator.floorTilemap.CellToWorld(floorGenerator.merchantSpawn);
                // Spawn the merchant
                Instantiate(merchantPrefab, merchantSpawnWorld, Quaternion.identity);
            }

            // Spawn the player
            Instantiate(playerPrefab, playerSpawnWorld, Quaternion.identity);
            
            // Todo: Figure out what we want to do between floors
            // We could offset each floor so they don't overlap:
            // floorGenerator.floorTilemap.transform.position = new Vector3(0, i * 20, 0);
            // or re-create a new FloorGenerator / new Tilemap for each floor, etc.
        }
    }
}