using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

// PROCEDURAL GENERATION VERSION 1
// AUTHORS: Anton, Vitaly, Aron
public class FloorGenerator : MonoBehaviour
{
    public enum RoomShape{
        Rectangular,
        LShaped,
        TShaped
    }
    [Header("Tilemap Reference")]

    public Tilemap floorTilemap; // Assign in Inspector
    public GameObject keyPrefab;

    [Header("Tile Assets")]
    public TileBase floorTile;
    public TileBase wallTile;
    public TileBase stairTile;
    public TileBase cornerWallTile;
    public TileBase doorTile;  // optional; required later
    public TileBase bossDoorTile; // optional; required later
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

        GenerateRooms(width,height,config);

        GenerateKeys(width,height,4);

        // 4) Place stairs
        if(_placedStairs.Count == 0){
            GenerateStairs(width, height, 1);
        }

        // 5) Determine spawn positions
        if (isFirstFloor)
        {
            SetupFirstFloorSpawns();
        }
        else
        {
            SetupSubsequentFloorSpawns(width, height);
        }
        

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

        // Overwrite the wall tile with a door tile if needed
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

    /// <summary>
    /// True if the given tile coordinate is within floor bounds and is a floor tile (not wall/stair/etc).
    /// </summary>
    private bool IsFloorTile(Vector3Int cellPos, int width, int height)
    {
        if (cellPos.x < 0 || cellPos.x >= width || cellPos.y < 0 || cellPos.y >= height)
            return false;
        return floorTilemap.GetTile(cellPos) == floorTile;
    }

    /// <summary>
    /// Places rectangular rooms that do not overlap.
    /// </summary>
    private void GenerateRooms(int width, int height, FloorConfig config)
    {
        int baseRoomCount = Mathf.Clamp((width + height) / 24, 1, 10);
        bool bossRoomPlaced = false;

        for(int i = 0; i<baseRoomCount; i++){
            RoomShape shape = (RoomShape)RandomSeed.GetRandomInt(0,3);
            bool isBossRoom = (!bossRoomPlaced && i == 0);
            bool success = false;
            for(int attempt = 0;attempt < 20 && !success;attempt++){
                // Pick a random “bounding rectangle” for the room.
                int roomW = RandomSeed.GetRandomInt(4, Mathf.Min(width / 2, 8));
                int roomH = RandomSeed.GetRandomInt(4, Mathf.Min(height / 2, 8));
                int startX = RandomSeed.GetRandomInt(1, width - roomW - 1);
                int startY = RandomSeed.GetRandomInt(1, height - roomH - 1);
                Vector3Int startPos = new Vector3Int(startX, startY, 0);

                // Check if the rectangle fits (this sample simply does a bounds check;
                // in a complete implementation you might track “used” cells to avoid overlapping rooms)
                if (startX + roomW >= width - 1 || startY + roomH >= height - 1)
                    continue;

                // Depending on the shape, call a different drawing method.
                switch (shape)
                {
                    case RoomShape.Rectangular:
                        success = GenerateRectangularRoom(startPos, roomW, roomH, isBossRoom);
                        break;
                    case RoomShape.LShaped:
                        success = GenerateLShapedRoom(startPos, roomW, roomH, isBossRoom);
                        break;
                    case RoomShape.TShaped:
                        success = GenerateTShapedRoom(startPos, roomW, roomH, isBossRoom);
                        break;
                }

                if (success && isBossRoom)
                {
                    bossRoomPlaced = true;
                }
            }
        }

        if(!bossRoomPlaced){
            Vector3Int forcedBossPos = new Vector3Int(width/4,height/4,0);
            GenerateRectangularRoom(forcedBossPos,6,6,true);
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
    
    /// <summary>
    /// Generates a rectangular room. If isBossRoom is true, then place the stairs in its center and use a boss door tile.
    /// Returns true if the room was successfully placed.
    /// </summary>
    private bool GenerateRectangularRoom(Vector3Int startPos, int roomW, int roomH, bool isBossRoom)
    {
        // Draw the room perimeter walls
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
        // Optionally add corner walls
        if (cornerWallTile != null)
        {
            floorTilemap.SetTile(new Vector3Int(startPos.x - 1,     startPos.y + roomH, 0), cornerWallTile);
            floorTilemap.SetTile(new Vector3Int(startPos.x + roomW, startPos.y + roomH, 0), cornerWallTile);
            floorTilemap.SetTile(new Vector3Int(startPos.x - 1,     startPos.y - 1,     0), cornerWallTile);
            floorTilemap.SetTile(new Vector3Int(startPos.x + roomW, startPos.y - 1,     0), cornerWallTile);
        }

        // Place a door along one wall.
        Vector3Int doorPos;
        if (isBossRoom)
        {
            // For a boss room, mark the door with a special boss door tile.
            doorPos = new Vector3Int(startPos.x, startPos.y + roomH / 2, 0);
            floorTilemap.SetTile(doorPos, bossDoorTile != null ? bossDoorTile : doorTile);
        }
        else
        {
            List<Vector3Int> possibleDoorPositions = new List<Vector3Int>
            {
                new Vector3Int(RandomSeed.GetRandomInt(startPos.x, startPos.x + roomW), startPos.y - 1, 0),   // Bottom
                new Vector3Int(RandomSeed.GetRandomInt(startPos.x, startPos.x + roomW), startPos.y + roomH, 0),   // Top
                new Vector3Int(startPos.x - 1, RandomSeed.GetRandomInt(startPos.y, startPos.y + roomH), 0),      // Left
                new Vector3Int(startPos.x + roomW, RandomSeed.GetRandomInt(startPos.y, startPos.y + roomH), 0)       // Right
            };
            doorPos = possibleDoorPositions[RandomSeed.GetRandomInt(0, possibleDoorPositions.Count)];
            floorTilemap.SetTile(doorPos, doorTile);
        }
        _placedDoors.Add(doorPos);

        // Place a chest inside (we add extra chests for density)
        if (chestTile != null)
        {
            int chestX = RandomSeed.GetRandomInt(startPos.x + 1, startPos.x + roomW - 1);
            int chestY = RandomSeed.GetRandomInt(startPos.y + 1, startPos.y + roomH - 1);
            Vector3Int chestPos = new Vector3Int(chestX, chestY, 0);
            floorTilemap.SetTile(chestPos, chestTile);
            _placedChests.Add(chestPos);
        }

        // If this is the boss room, force the stairs in its center.
        if (isBossRoom)
        {
            Vector3Int bossStairPos = new Vector3Int(startPos.x + roomW / 2, startPos.y + roomH / 2, 0);
            floorTilemap.SetTile(bossStairPos, stairTile);
            _placedStairs.Add(bossStairPos);
        }

        return true;
    }

    /// <summary>
    /// Generates an L-shaped room by “carving out” one quadrant from a rectangular block.
    /// </summary>
    private bool GenerateLShapedRoom(Vector3Int startPos, int roomW, int roomH, bool isBossRoom)
    {
        // For simplicity, first draw the full rectangle:
        GenerateRectangularRoom(startPos, roomW, roomH, isBossRoom);
        // Then “carve out” one corner by replacing those wall tiles with floor tiles.
        // (Choose one of the four corners at random.)
        int carveWidth = roomW / 2;
        int carveHeight = roomH / 2;
        int corner = RandomSeed.GetRandomInt(0, 4);
        int carveStartX = (corner == 0 || corner == 3) ? startPos.x : startPos.x + roomW - carveWidth;
        int carveStartY = (corner == 0 || corner == 1) ? startPos.y : startPos.y + roomH - carveHeight;
        for (int x = carveStartX; x < carveStartX + carveWidth; x++)
        {
            for (int y = carveStartY; y < carveStartY + carveHeight; y++)
            {
                floorTilemap.SetTile(new Vector3Int(x, y, 0), floorTile);
            }
        }
        // (You may want to re-draw walls along the new “internal” edges.)
        return true;
    }

    /// <summary>
    /// Generates a T-shaped room by “removing” a section from one edge of a rectangular room.
    /// </summary>
    private bool GenerateTShapedRoom(Vector3Int startPos, int roomW, int roomH, bool isBossRoom)
    {
        // Draw the full rectangular room first.
        GenerateRectangularRoom(startPos, roomW, roomH, isBossRoom);
        // Remove a notch from one of the four edges (choose randomly).
        int notchSize = Mathf.Max(2, roomW / 4);
        int notchEdge = RandomSeed.GetRandomInt(0, 4);
        if (notchEdge == 0) // bottom edge
        {
            for (int x = startPos.x + roomW/2 - notchSize/2; x < startPos.x + roomW/2 + notchSize/2; x++)
                floorTilemap.SetTile(new Vector3Int(x, startPos.y - 1, 0), floorTile);
        }
        else if (notchEdge == 1) // top edge
        {
            for (int x = startPos.x + roomW/2 - notchSize/2; x < startPos.x + roomW/2 + notchSize/2; x++)
                floorTilemap.SetTile(new Vector3Int(x, startPos.y + roomH, 0), floorTile);
        }
        else if (notchEdge == 2) // left edge
        {
            for (int y = startPos.y + roomH/2 - notchSize/2; y < startPos.y + roomH/2 + notchSize/2; y++)
                floorTilemap.SetTile(new Vector3Int(startPos.x - 1, y, 0), floorTile);
        }
        else // right edge
        {
            for (int y = startPos.y + roomH/2 - notchSize/2; y < startPos.y + roomH/2 + notchSize/2; y++)
                floorTilemap.SetTile(new Vector3Int(startPos.x + roomW, y, 0), floorTile);
        }
        return true;
    }

    /// <summary>
    /// Randomly places a given number of key objects onto floor tiles.
    /// </summary>
    private void GenerateKeys(int width, int height, int keyCount)
    {
        int placedKeys = 0;
        int attempts = 0;
        while (placedKeys < keyCount && attempts < 100)
        {
            int keyX = RandomSeed.GetRandomInt(0, width);
            int keyY = RandomSeed.GetRandomInt(0, height);
            Vector3Int cellPos = new Vector3Int(keyX, keyY, 0);
            // Only place a key if the underlying tile is a floor tile.
            if (floorTilemap.GetTile(cellPos) == floorTile)
            {
                // Convert cell position to world position. 
                Vector3 worldPos = floorTilemap.CellToWorld(cellPos) + new Vector3(0.5f, 0.5f, 0);
            
                // Instantiate the key prefab at this world position.
                Instantiate(keyPrefab, worldPos, Quaternion.identity);
            
                placedKeys++;
            }
            attempts++;
        }
    }
}
