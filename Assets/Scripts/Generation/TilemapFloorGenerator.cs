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
    public TileBase doorTile; // optional

    // Spawn points found or set during generation
    public Vector3Int playerSpawn;
    public Vector3Int merchantSpawn;

    /// <summary>
    /// Generates a floor using the given config and randomization.
    /// </summary>
    public void GenerateFloor(FloorConfig config, bool isFirstFloor)
    {
        // 1) Randomize floor dimensions
        int width  = RandomSeed.GetRandomInt(config.minWidth, config.maxWidth + 1);
        int height = RandomSeed.GetRandomInt(config.minHeight, config.maxHeight + 1);

        // 2) Generate floor tiles
        GenerateFloorTiles(width, height);

        // 3) Generate perimeter walls (with optional corners, door if needed)
        GenerateWalls(width, height, config.useCornerWalls, isFirstFloor && config.hasDoor);

        // 4) Place stairs
        GenerateStairs(width, height, config.stairsCount);

        // 5) Determine spawn positions
        if (isFirstFloor)
        {
            // We place the door along a wall. Let's say on the left side.
            // Ensure there's an opening for a door, and a bit of space inside
            // so we can place merchant/player spawn. We'll assume the door is at (0, 0).
            // Then the player spawns at (1, 0) and merchant at (2, 0), for example.

            Vector3Int doorPosition   = new Vector3Int(0, 0, 0);
            Vector3Int playerPosition = new Vector3Int(1, 0, 0);
            Vector3Int merchantPosition = new Vector3Int(2, 0, 0);

            // Overwrite the wall tile with a door tile if needed
            if (doorTile != null)
            {
                floorTilemap.SetTile(doorPosition, doorTile);
            }

            // Save spawn positions
            playerSpawn   = playerPosition;
            merchantSpawn = merchantPosition;
        }
        else
        {
            // On subsequent floors, place player near the top of the stairs from the previous level
            // For demonstration, let's pick the position of the last placed stair or near it
            Vector3Int spawnStair = GetRandomStairPosition(width, height);
            playerSpawn = spawnStair + new Vector3Int(1, 0, 0); // just offset it by 1 for example
        }
    }

    private void GenerateFloorTiles(int width, int height)
    {
        // Fill the region [0..width-1, 0..height-1] with floorTile
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                floorTilemap.SetTile(new Vector3Int(x, y, 0), floorTile);
            }
        }
    }

    private void GenerateWalls(int width, int height, bool useCorners, bool placeDoor)
    {
        // We'll place walls around the perimeter. 
        // The region is from x in [0..width-1], y in [0..height-1].

        // Top and bottom walls
        for (int x = 0; x < width; x++)
        {
            // top row
            floorTilemap.SetTile(new Vector3Int(x, height, 0), wallTile);
            // bottom row
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
            // top-left corner
            floorTilemap.SetTile(new Vector3Int(-1, height, 0), cornerWallTile);
            // bottom-left corner
            floorTilemap.SetTile(new Vector3Int(-1, -1, 0), cornerWallTile);
            // top-right corner
            floorTilemap.SetTile(new Vector3Int(width, height, 0), cornerWallTile);
            // bottom-right corner
            floorTilemap.SetTile(new Vector3Int(width, -1, 0), cornerWallTile);
        }
    }

    private void GenerateStairs(int width, int height, int stairsCount)
    {
        // Place a certain number of stairs inside the floor bounds
        // We can choose random positions that don't conflict with walls.
        for (int i = 0; i < stairsCount; i++)
        {
            int stairX = RandomSeed.GetRandomInt(0, width);
            int stairY = RandomSeed.GetRandomInt(0, height);

            floorTilemap.SetTile(new Vector3Int(stairX, stairY, 0), stairTile);
        }
    }

    /// <summary>
    /// Returns the position of one of the placed stairs, for example the last one.
    /// </summary>
    private Vector3Int GetRandomStairPosition(int width, int height)
    {
        // In a real scenario, you'd store all placed stair positions in a list
        // then pick one. Here, let's just pick a random one again.
        int stairX = RandomSeed.GetRandomInt(0, width);
        int stairY = RandomSeed.GetRandomInt(0, height);
        return new Vector3Int(stairX, stairY, 0);
    }
}
