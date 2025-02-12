using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

// PROCEDURAL GENERATION VERSION 1
public class FloorGenerator : MonoBehaviour
{
    public enum FloorShape{
        Rectangular,
        LShaped,
        TShaped,
    }
    [Header("Tilemap Reference")]

    public Tilemap floorTilemap; // Assign in Inspector

    [Header("Tile Assets")]
    public TileBase floorTile;
    public TileBase wallTile;
    public TileBase stairTile;
    public TileBase cornerWallTile;
    public TileBase doorTile;  // optional; required later
    public TileBase chestTile; // optional; required later
    public TileBase keyTile;

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
        int baseRoomCount = Mathf.Clamp((width + height) / 24, 1, 10);
        bool bossRoomPlaced = false;

        for(int i = 0;i<baseRoomCount;i++){
            RoomShape shape = (RoomShape)RandomSeed.GetRandomInt(0,3);
            bool isBossRoom = (!bossRoomPlaced && i == 0)
            bool success = false;
            for(int attempt = 0;attempt < 20 && !success;attempt++){

            }
        }

        if(!bossRoomPlaced){
            Vector3Int forcedBossPos = new Vector3Int(width/4,height/4,0);
            GenerateRectangularRoom(forcedBossPos,6,6,true);
        }
        
        
    }

}
