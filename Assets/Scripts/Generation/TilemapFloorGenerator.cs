using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class FloorGenerator : MonoBehaviour
{
    [Header("Tilemap Reference")]
    public Tilemap floorTilemap; // Assign in Inspector

    [Header("Tile Assets")]
    public TileBase floorTile;
    public TileBase wallTile;
    public TileBase stairTile;
    public TileBase cornerWallTile;
    public TileBase doorTile;  // optional; required later
    public TileBase chestTile; // optional; required later

    [Header("Enemy Settings")]
    public int minEnemies = 60;        // Minimum number of enemies to spawn on the floor
    public int maxEnemies = 61;        // Maximum number of enemies to spawn on the floor

    // Spawn points found or set during generation
    [HideInInspector] public Vector3Int playerSpawn;
    [HideInInspector] public Vector3Int merchantSpawn;

    // Keep track of all special elements we generate, in case we need them later
    private List<Vector3Int> _placedStairs = new List<Vector3Int>();
    private List<Vector3Int> _placedChests = new List<Vector3Int>();
    private List<Vector3Int> _placedDoors = new List<Vector3Int>();

    // New: list to store enemy world positions (for spacing checks)
    private List<Vector3> _placedEnemies = new List<Vector3>();

    public Transform enemyContainer;
    public GameObject enemyPrefab;

    /// <summary>
    /// Generates a floor using the given config and randomization, then returns FloorData for later reference.
    /// </summary>
    public FloorData GenerateFloor(FloorConfig config, bool isFirstFloor)
    {
        // 1) Randomize floor dimensions
        int width  = RandomSeed.GetRandomInt(config.minWidth,  config.maxWidth  + 1);
        int height = RandomSeed.GetRandomInt(config.minHeight, config.maxHeight + 1);

        // Clear any existing tiles/stairs from a prior generation
        floorTilemap.ClearAllTiles();
        _placedStairs.Clear();
        _placedEnemies.Clear();

        // 2) Generate floor tiles
        GenerateFloorTiles(width, height);

        // 3) Generate perimeter walls (with optional corners)
        GenerateWalls(width, height, config.useCornerWalls);

        // 4) Place stairs
        GenerateStairs(width, height, config.stairsCount);

        // 5) Determine spawn positions
        if (isFirstFloor)
        {
            SetupFirstFloorSpawns();
        }
        else
        {
            SetupSubsequentFloorSpawns(width, height);
        }
        
        // 6) Generate rooms
        GenerateRooms(width, height, config);

        // 7) Generate enemies
        GenerateEnemies(width, height);

        // 8) Prepare the FloorData to store or return
        FloorData data = new FloorData
        {
            playerSpawn   = playerSpawn,
            merchantSpawn = merchantSpawn,
            stairPositions = _placedStairs.ConvertAll(s => (Vector2Int)s).ToArray(),
            chestPositions = _placedChests.ConvertAll(s => (Vector2Int)s).ToArray(),
            roomDoors = _placedDoors.ConvertAll(s => (Vector2Int)s).ToArray()
            // You might add enemy positions later if needed.
        };

        return data;
    }

    private void GenerateFloorTiles(int width, int height)
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                floorTilemap.SetTile(new Vector3Int(x, y, 0), floorTile);
            }
        }
    }

    private void GenerateWalls(int width, int height, bool useCorners)
    {
        // Top and bottom walls
        for (int x = 0; x < width; x++)
        {
            floorTilemap.SetTile(new Vector3Int(x, height, 0), wallTile);
            floorTilemap.SetTile(new Vector3Int(x, -1, 0), wallTile);
        }

        // Left and right walls
        for (int y = -1; y <= height; y++)
        {
            floorTilemap.SetTile(new Vector3Int(-1, y, 0), wallTile);
            floorTilemap.SetTile(new Vector3Int(width, y, 0), wallTile);
        }

        // Corner walls (if requested)
        if (useCorners && cornerWallTile != null)
        {
            floorTilemap.SetTile(new Vector3Int(-1, height,  0), cornerWallTile);
            floorTilemap.SetTile(new Vector3Int(-1, -1,     0), cornerWallTile);
            floorTilemap.SetTile(new Vector3Int(width, height,  0), cornerWallTile);
            floorTilemap.SetTile(new Vector3Int(width, -1,     0), cornerWallTile);
        }
    }

    private void GenerateStairs(int width, int height, int stairsCount)
    {
        for (int i = 0; i < stairsCount; i++)
        {
            int stairX = RandomSeed.GetRandomInt(0, width);
            int stairY = RandomSeed.GetRandomInt(0, height);
            Vector3Int pos = new Vector3Int(stairX, stairY, 0);

            // Simple check to avoid overwriting walls
            TileBase existing = floorTilemap.GetTile(pos);
            if (existing == wallTile || existing == cornerWallTile)
            {
                i--;
                continue;
            }

            floorTilemap.SetTile(pos, stairTile);
            _placedStairs.Add(pos);
        }
    }

    /// <summary>
    /// On the first floor, we place a "door" at (-1, 0) and spawn the player at (1,0), merchant at (2,0).
    /// </summary>
    private void SetupFirstFloorSpawns()
    {
        Vector3Int doorPosition = new Vector3Int(-1, 0, 0);
        playerSpawn = new Vector3Int(0, 0, 0);  // For (0.5, 0.5) world position when offset is added
        merchantSpawn = new Vector3Int(3, 0, 0); // For (3.5, 0.5) world position

        if (doorTile != null)
        {
            floorTilemap.SetTile(doorPosition, doorTile);
        }
    }

    /// <summary>
    /// For subsequent floors, place player near a randomly picked stair.
    /// </summary>
    private void SetupSubsequentFloorSpawns(int width, int height)
    {
        if (_placedStairs.Count == 0)
        {
            playerSpawn = new Vector3Int(0, 0, 0);
            return;
        }

        Vector3Int stairPos = _placedStairs[RandomSeed.GetRandomInt(0, _placedStairs.Count)];

        Vector3Int rightTile = stairPos + Vector3Int.right;
        if (IsFloorTile(rightTile, width, height))
        {
            playerSpawn = rightTile;
            return;
        }

        Vector3Int leftTile = stairPos + Vector3Int.left;
        if (IsFloorTile(leftTile, width, height))
        {
            playerSpawn = leftTile;
            return;
        }

        playerSpawn = stairPos;
    }

    private bool IsFloorTile(Vector3Int cellPos, int width, int height)
    {
        if (cellPos.x < 0 || cellPos.x >= width || cellPos.y < 0 || cellPos.y >= height)
            return false;

        TileBase tileAt = floorTilemap.GetTile(cellPos);
        return tileAt == floorTile;
    }

    /// <summary>
    /// Places rectangular rooms that do not overlap.
    /// </summary>
    private void GenerateRooms(int width, int height, FloorConfig config)
    {
        if ((width + height) < 16)
            return;

        int numRooms = (width + height) / 32;
        numRooms = Mathf.Clamp(numRooms, 1, (config.maxWidth + config.maxHeight) / 32);
        HashSet<Vector3Int> usedTiles = new HashSet<Vector3Int>();

        Debug.Log($"[FloorGenerator] Attempting to create {numRooms} room(s) in a {width}x{height} floor.");

        for (int i = 0; i < numRooms; i++)
        {
            int maxRoomWidth = Mathf.Max(4, width - 4);
            int maxRoomHeight = Mathf.Max(4, height - 4);
            if (maxRoomWidth < 4 || maxRoomHeight < 4)
                break;

            int roomW = RandomSeed.GetRandomInt(4, Mathf.Min(maxRoomWidth, width / 2));
            int roomH = RandomSeed.GetRandomInt(4, Mathf.Min(maxRoomHeight, height / 2));

            bool validRoom = false;
            Vector3Int startPos = Vector3Int.zero;
            int attempts = 0;
            while (!validRoom && attempts < 20)
            {
                int startX = RandomSeed.GetRandomInt(1, width - roomW - 1);
                int startY = RandomSeed.GetRandomInt(1, height - roomH - 1);
                startPos = new Vector3Int(startX, startY, 0);

                validRoom = true;
                for (int x = startX - 1; x <= startX + roomW; x++)
                {
                    for (int y = startY - 1; y <= startY + roomH; y++)
                    {
                        if (usedTiles.Contains(new Vector3Int(x, y, 0)))
                        {
                            validRoom = false;
                            break;
                        }
                    }
                    if (!validRoom) break;
                }
                attempts++;
            }

            if (!validRoom)
            {
                Debug.Log($"[FloorGenerator] Could not place room #{i} after {attempts} attempts. Floor may be full.");
                continue;
            }

            for (int x = startPos.x - 1; x <= startPos.x + roomW; x++)
            {
                for (int y = startPos.y - 1; y <= startPos.y + roomH; y++)
                {
                    usedTiles.Add(new Vector3Int(x, y, 0));
                }
            }

            for (int x = startPos.x; x < startPos.x + roomW; x++)
            {
                floorTilemap.SetTile(new Vector3Int(x, startPos.y + roomH, 0), wallTile);
                floorTilemap.SetTile(new Vector3Int(x, startPos.y - 1, 0), wallTile);
            }
            for (int y = startPos.y - 1; y <= startPos.y + roomH; y++)
            {
                floorTilemap.SetTile(new Vector3Int(startPos.x - 1, y, 0), wallTile);
                floorTilemap.SetTile(new Vector3Int(startPos.x + roomW, y, 0), wallTile);
            }

            if (cornerWallTile != null)
            {
                floorTilemap.SetTile(new Vector3Int(startPos.x - 1, startPos.y + roomH, 0), cornerWallTile);
                floorTilemap.SetTile(new Vector3Int(startPos.x + roomW, startPos.y + roomH, 0), cornerWallTile);
                floorTilemap.SetTile(new Vector3Int(startPos.x - 1, startPos.y - 1, 0), cornerWallTile);
                floorTilemap.SetTile(new Vector3Int(startPos.x + roomW, startPos.y - 1, 0), cornerWallTile);
            }

            if (doorTile != null)
            {
                List<Vector3Int> possibleDoorPositions = new List<Vector3Int>
                {
                    new Vector3Int(RandomSeed.GetRandomInt(startPos.x, startPos.x + roomW), startPos.y - 1, 0),
                    new Vector3Int(RandomSeed.GetRandomInt(startPos.x, startPos.x + roomW), startPos.y + roomH, 0),
                    new Vector3Int(startPos.x - 1, RandomSeed.GetRandomInt(startPos.y, startPos.y + roomH), 0),
                    new Vector3Int(startPos.x + roomW, RandomSeed.GetRandomInt(startPos.y, startPos.y + roomH), 0)
                };
                Vector3Int doorPos = possibleDoorPositions[RandomSeed.GetRandomInt(0, possibleDoorPositions.Count)];
                floorTilemap.SetTile(doorPos, doorTile);
                _placedDoors.Add(doorPos);
            }

            if (chestTile != null)
            {
                int chestX = RandomSeed.GetRandomInt(startPos.x + 1, startPos.x + roomW - 1);
                int chestY = RandomSeed.GetRandomInt(startPos.y + 1, startPos.y + roomH - 1);
                Vector3Int chestPos = new Vector3Int(chestX, chestY, 0);
                floorTilemap.SetTile(chestPos, chestTile);
                _placedChests.Add(chestPos);
            }
        }
    }
    private void GenerateEnemies(int width, int height){
        Debug.Log("Generate Enemies called!");
        if (enemyPrefab == null)
        {
            Debug.LogWarning("[FloorGenerator] No enemyPrefab assigned!");
            return;
        }

        // Determine how many enemies to spawn; this can be based on floor area or a fixed range.
        int numEnemies = RandomSeed.GetRandomInt(minEnemies, maxEnemies + 1);



        for (int i = 0; i < numEnemies; i++)
        {
            bool validSpawn = false;
            Vector3 candidatePos = Vector3.zero;
            int attempts = 0;

            // Randomly choose a minimum distance for this enemy (between 3 and 6)
            float minDistance = Random.Range(3f, 6f);

            while (!validSpawn && attempts < 20)
            {
                // Choose a random tile coordinate on the floor (assume floor area spans from (0,0) to (width-1,height-1))
                int x = RandomSeed.GetRandomInt(0, width);
                int y = RandomSeed.GetRandomInt(0, height);
                // Convert cell position to world position (adding 0.5 to center on the tile)
                candidatePos = floorTilemap.CellToWorld(new Vector3Int(x, y, 0)) + new Vector3(0.5f, 0.5f, 0);
                Vector3Int candidatePosVec3Int = new Vector3Int(x, y, 0);

                validSpawn = true;
                foreach (Vector3 pos in _placedEnemies)
                {
                    if (Vector3.Distance(candidatePos, pos) < minDistance || floorTilemap.GetTile(candidatePosVec3Int) != floorTile)
                    {
                        validSpawn = false;
                        break;
                    }
                }
                attempts++;
            }

            if (validSpawn)
            {
                GameObject enemy = Instantiate(enemyPrefab, candidatePos, Quaternion.identity, enemyContainer);
                _placedEnemies.Add(candidatePos);
            }
            else
            {
                Debug.LogWarning($"[FloorGenerator] Could not find valid enemy spawn after {attempts} attempts for enemy {i}.");
            }
        }
    }
}
