using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

// PROCEDURAL GENERATION VERSION 1
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

    // Spawn points found or set during generation
    [HideInInspector] public Vector3Int playerSpawn;
    [HideInInspector] public Vector3Int merchantSpawn;

    // Keep track of all special elements we generate, in case we need them later
    private List<Vector3Int> _placedStairs = new List<Vector3Int>();
    private List<Vector3Int> _placedChests = new List<Vector3Int>();
    private List<Vector3Int> _placedDoors = new List<Vector3Int>();

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

        // 7) Prepare the FloorData to store or return
        FloorData data = new FloorData
        {
            playerSpawn   = (Vector3Int)playerSpawn,
            merchantSpawn = (Vector3Int)merchantSpawn,
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
                // if it's a wall, skip placing stairs here
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
        playerSpawn = new Vector3Int(0, 0, 0);  // Adjusted to match (0.5, 0.5) world position
        merchantSpawn = new Vector3Int(3, 0, 0); // Adjusted to match (3.5, 0.5) world position

        // Overwrite the wall tile with a door tile if needed
        if (doorTile != null)
        {
            floorTilemap.SetTile(doorPosition, doorTile);
        }
    }


    /// <summary>
    /// For subsequent floors, place player near a randomly picked stair. We check adjacent tiles for walls or out-of-bounds.
    /// If the immediate right is invalid, we try left. If both are invalid, we just place the player on the stair tile.
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

        playerSpawn = stairPos; // Default case: on the stairs
    }

    /// <summary>
    /// True if the given tile coordinate is within floor bounds and is a floor tile (not wall/stair/etc).
    /// </summary>
    private bool IsFloorTile(Vector3Int cellPos, int width, int height)
    {
        // Check bounds
        if (cellPos.x < 0 || cellPos.x >= width || cellPos.y < 0 || cellPos.y >= height)
            return false;

        TileBase tileAt = floorTilemap.GetTile(cellPos);
        if (tileAt == floorTile)
            return true;

        return false;
    }

    // /// <summary>
    // /// Returns a random stair position. We do a naive approach here, but ideally you'd store all placed stairs.
    // /// This is used *internally* before we changed to the approach of storing them in _placedStairs.
    // /// NOW UNUSED. LEAVING COMMENTED
    // /// </summary>
    // private Vector3Int GetRandomStairPosition(int width, int height)
    // {
    //     // For demonstration, pick any random position. 
    //     // In a real scenario, you'd pick from _placedStairs for guaranteed stairs.
    //     int stairX = RandomSeed.GetRandomInt(0, width);
    //     int stairY = RandomSeed.GetRandomInt(0, height);
    //     return new Vector3Int(stairX, stairY, 0);
    // }

    /// <summary>
    /// Places rectangular rooms that do not overlap. Each room has walls, corners, a door, and optionally a chest.
    /// </summary>
    private void GenerateRooms(int width, int height, FloorConfig config)
    {
        // If the floor is REALLY tiny, skip. Otherwise, we do at least 1.
        // Example threshold: if (width + height) < 16 => no rooms.
        if ((width + height) < 16)
        {
            // Debug.Log($"[FloorGenerator] Floor too small ({width}x{height}), skipping rooms.");
            return;
        }

        // For bigger floors, create up to (width+height)/32 rooms, but ensure at least 1.
        int numRooms = (width + height) / 32;
        numRooms = Mathf.Clamp(numRooms, 1, (config.maxWidth + config.maxHeight) / 32);

        // Tracks which cells are "taken" by rooms to prevent overlap
        HashSet<Vector3Int> usedTiles = new HashSet<Vector3Int>();

        Debug.Log($"[FloorGenerator] Attempting to create {numRooms} room(s) in a {width}x{height} floor.");

        for (int i = 0; i < numRooms; i++)
        {
            // Room size. 
            // We'll clamp max to ensure the room fits comfortably within the floor
            int maxRoomWidth = Mathf.Max(4, width - 4);
            int maxRoomHeight = Mathf.Max(4, height - 4);

            // If there's no space for even a 4x4, skip
            if (maxRoomWidth < 4 || maxRoomHeight < 4)
            {
                // Debug.Log($"[FloorGenerator] Not enough space for room #{i} in {width}x{height} floor.");
                break;
            }

            int roomW = RandomSeed.GetRandomInt(4, Mathf.Min(maxRoomWidth, width / 2));
            int roomH = RandomSeed.GetRandomInt(4, Mathf.Min(maxRoomHeight, height / 2));

            bool validRoom = false;
            Vector3Int startPos = Vector3Int.zero;
            int attempts = 0;

            while (!validRoom && attempts < 20)
            {
                // Start positions range from [1..(width-roomW-1)] so there's room for the walls
                int startX = RandomSeed.GetRandomInt(1, width - roomW - 1);
                int startY = RandomSeed.GetRandomInt(1, height - roomH - 1);
                startPos = new Vector3Int(startX, startY, 0);

                validRoom = true;
                // Check if any tile in the rectangle is already used
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
                // Something went wrong. May be hard to debug, good luck
                Debug.Log($"[FloorGenerator] Could not place room #{i} after {attempts} attempts. Most likely, the floor is full.");
                continue;
            }

            // Mark the area as used before generation
            for (int x = startPos.x - 1; x <= startPos.x + roomW; x++)
            {
                for (int y = startPos.y - 1; y <= startPos.y + roomH; y++)
                {
                    usedTiles.Add(new Vector3Int(x, y, 0));
                }
            }

            // Draw perimeter walls
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

            // Place corner walls
            if (cornerWallTile != null)
            {
                floorTilemap.SetTile(new Vector3Int(startPos.x - 1,     startPos.y + roomH, 0), cornerWallTile);
                floorTilemap.SetTile(new Vector3Int(startPos.x + roomW, startPos.y + roomH, 0), cornerWallTile);
                floorTilemap.SetTile(new Vector3Int(startPos.x - 1,     startPos.y - 1,     0), cornerWallTile);
                floorTilemap.SetTile(new Vector3Int(startPos.x + roomW, startPos.y - 1,     0), cornerWallTile);
            }

            // Pick a random wall for the door (if available)
            if (doorTile != null)
            {
                List<Vector3Int> possibleDoorPositions = new List<Vector3Int>
                {
                    new Vector3Int(RandomSeed.GetRandomInt(startPos.x, startPos.x + roomW), startPos.y - 1, 0),   // Bottom
                    new Vector3Int(RandomSeed.GetRandomInt(startPos.x, startPos.x + roomW), startPos.y + roomH, 0),// Top
                    new Vector3Int(startPos.x - 1, RandomSeed.GetRandomInt(startPos.y, startPos.y + roomH), 0),   // Left
                    new Vector3Int(startPos.x + roomW, RandomSeed.GetRandomInt(startPos.y, startPos.y + roomH), 0)// Right
                };
                Vector3Int doorPos = possibleDoorPositions[RandomSeed.GetRandomInt(0, possibleDoorPositions.Count)];
                floorTilemap.SetTile(doorPos, doorTile);
                _placedDoors.Add(doorPos);
            }

            // Place a chest inside
            if (chestTile != null)
            {
                int chestX = RandomSeed.GetRandomInt(startPos.x + 1, startPos.x + roomW - 1);
                int chestY = RandomSeed.GetRandomInt(startPos.y + 1, startPos.y + roomH - 1);
                Vector3Int chestPos = new Vector3Int(chestX, chestY, 0);
                floorTilemap.SetTile(chestPos, chestTile);
                _placedChests.Add(chestPos);
            }

            // Log the final success for debug purposes:
            // Debug.Log($"[FloorGenerator] Placed room #{i} at {startPos} (size {roomW}x{roomH}).");
        }
    }

}
