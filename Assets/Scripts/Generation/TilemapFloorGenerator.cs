using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class FloorGenerator : MonoBehaviour
{
    // PROCEDURAL GENERATION V3 – One-gap-per-room approach, single-tile carve, door placed in that gap.
    // AUTHORS: Vitaly, Anton, Aron (modified 2/14/2025)

    [Header("Tilemap & Prefab References")]
    public Tilemap floorTilemap;   // Assign in Inspector
    public GameObject keyPrefab;
    public GameObject chestPrefab;
    public Transform objectContainer;

    [Header("Tile Assets")]
    public TileBase floorTile;
    public TileBase wallTile;
    
    [Header("Enemy Settings")]
    public int minEnemies = 60;
    public int maxEnemies = 61;
    public Transform enemyContainer;
    public GameObject enemyPrefab;

    // Prefabs for doors and stairs
    public GameObject doorPrefab;
    public GameObject stairDownPrefab;  
    public GameObject stairUpPrefab;    

    // Spawn points set during generation
    [HideInInspector] public Vector3Int playerSpawn;
    [HideInInspector] public Vector3Int merchantSpawn;

    // Tracking special elements
    private List<Vector3Int> _placedStairs = new List<Vector3Int>();
    private List<Vector3Int> _placedDoors = new List<Vector3Int>();
    private List<Vector3Int> _placedChests = new List<Vector3Int>();
    private List<Vector3> _placedEnemies = new List<Vector3>();

    // We track rooms so we can find the largest as the "boss room"
    private List<Room> _rooms = new List<Room>();

    // Room shapes
    public enum RoomShape { Rectangular, LShaped, TShaped, JShaped }

    // Helper class
    public class Room
    {
        public int X, Y, W, H;  
        public RoomShape Shape;

        public Room(int x, int y, int w, int h, RoomShape shape)
        {
            this.X = x; 
            this.Y = y; 
            this.W = w; 
            this.H = h;
            this.Shape = shape;
        }

        public Vector2Int Center => new Vector2Int(X + W / 2, Y + H / 2);
        public int Area => W * H;

        public bool Intersects(Room other)
        {
            return (X <= other.X + other.W && X + W >= other.X &&
                    Y <= other.Y + other.H && Y + H >= other.Y);
        }
    }

    /// <summary>
    /// Generates a floor by first filling with floor, then placing walls around each room, 
    /// carving minimal shapes, and adding exactly one door gap per room.
    /// </summary>
    public FloorData GenerateFloor(FloorConfig config, bool isFirstFloor, int floorNumber, int totalFloors)
    {
        // 1) Randomize floor dimensions
        int width = RandomSeed.GetRandomInt(config.minWidth, config.maxWidth + 1);
        int height = RandomSeed.GetRandomInt(config.minHeight, config.maxHeight + 1);

        // Clear tilemap & reset lists
        floorTilemap.ClearAllTiles();
        _placedStairs.Clear();
        _placedEnemies.Clear();
        _placedDoors.Clear();
        _placedChests.Clear();
        _rooms.Clear();

        // 2) Fill entire grid with floor
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                floorTilemap.SetTile(new Vector3Int(x, y, 0), floorTile);
            }
        }

        // 3) Apply a circular boundary, then force the outer border to be walls
        MakeCircularBoundary(width, height);

        // 4) Generate random rooms
        int numRooms = RandomSeed.GetRandomInt(9, 17);
        for (int i = 0; i < numRooms; i++)
        {
            int roomW = RandomSeed.GetRandomInt(6, Mathf.Min(12, width / 2));
            int roomH = RandomSeed.GetRandomInt(6, Mathf.Min(12, height / 2));
            int roomX = RandomSeed.GetRandomInt(2, width - roomW - 2);
            int roomY = RandomSeed.GetRandomInt(2, height - roomH - 2);
            RoomShape shape = (RoomShape)RandomSeed.GetRandomInt(0, 4);

            var newRoom = new Room(roomX, roomY, roomW, roomH, shape);

            // Check overlap
            bool overlaps = false;
            foreach (Room other in _rooms)
            {
                if (newRoom.Intersects(other))
                {
                    overlaps = true;
                    break;
                }
            }
            if (overlaps)
                continue;

            _rooms.Add(newRoom);

            // Place walls around the shape
            PlaceWallsForShape(newRoom);

            // Add exactly one door gap
            AddDoorGap(newRoom);
        }

        // 5) Connect rooms with corridors
        _rooms.Sort((a, b) => a.Center.x.CompareTo(b.Center.x));
        for (int i = 1; i < _rooms.Count; i++)
        {
            Vector2Int prev = _rooms[i - 1].Center;
            Vector2Int curr = _rooms[i].Center;
            CarveCorridor(prev, curr);
        }

        // 6) Identify largest room as boss room
        Room bossRoom = null;
        int largestArea = 0;
        foreach (Room r in _rooms)
        {
            if (r.Area > largestArea)
            {
                largestArea = r.Area;
                bossRoom = r;
            }
        }

        // 7) Place stairs in the boss room
        if (bossRoom != null)
        {
            PlaceStairs(bossRoom, floorNumber, totalFloors);
        }

        // 8) Place keys, chests, enemies, etc.
        GenerateKeys(width, height, 4);
        GenerateChests(_rooms, bossRoom);
        GenerateEnemies(width, height);

        // 9) Determine spawn positions
        if (bossRoom != null)
        {
            if (isFirstFloor)
            {
                Vector3Int candidate = new Vector3Int(bossRoom.Center.x + 1, bossRoom.Center.y, 0);
                if (IsFloorTile(candidate, width, height))
                    playerSpawn = candidate;
                else
                    playerSpawn = new Vector3Int(bossRoom.Center.x, bossRoom.Center.y, 0);
            }
            else
            {
                playerSpawn = new Vector3Int(bossRoom.Center.x, bossRoom.Center.y, 0);
            }

            if (_rooms.Count > 0)
            {
                merchantSpawn = new Vector3Int(_rooms[0].Center.x, _rooms[0].Center.y, 0);
            }
            else
            {
                merchantSpawn = playerSpawn;
            }
        }
        else
        {
            // fallback
            playerSpawn = new Vector3Int(width / 2, height / 2, 0);
            merchantSpawn = playerSpawn;
        }

        // Refresh tilemap colliders
        floorTilemap.RefreshAllTiles();

        // 10) Prepare and return FloorData
        FloorData data = new FloorData
        {
            playerSpawn = playerSpawn,
            merchantSpawn = merchantSpawn,
            stairPositions = _placedStairs.ConvertAll(s => (Vector2Int)s).ToArray(),
            chestPositions = _placedChests.ConvertAll(s => (Vector2Int)s).ToArray(),
            roomDoors = _placedDoors.ConvertAll(s => (Vector2Int)s).ToArray()
        };
        return data;
    }

    /// <summary>
    /// Circular boundary + forced outer border walls.
    /// </summary>
    private void MakeCircularBoundary(int width, int height)
    {
        float centerX = width / 2f;
        float centerY = height / 2f;
        float radius = Mathf.Min(width, height) / 2f - 1f;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                float dx = x - centerX;
                float dy = y - centerY;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);
                if (dist > radius)
                {
                    floorTilemap.SetTile(new Vector3Int(x, y, 0), wallTile);
                }
            }
        }
        // Force the outer border to be walls.
        for (int x = 0; x < width; x++)
        {
            floorTilemap.SetTile(new Vector3Int(x, 0, 0), wallTile);
            floorTilemap.SetTile(new Vector3Int(x, height - 1, 0), wallTile);
        }
        for (int y = 0; y < height; y++)
        {
            floorTilemap.SetTile(new Vector3Int(0, y, 0), wallTile);
            floorTilemap.SetTile(new Vector3Int(width - 1, y, 0), wallTile);
        }
    }

    /// <summary>
    /// Places walls for a room shape. Then we do minimal carve-outs for L/T/J.
    /// </summary>
    private void PlaceWallsForShape(Room r)
    {
        switch (r.Shape)
        {
            case RoomShape.Rectangular:
                PlaceWallsRectangular(r.X, r.Y, r.W, r.H);
                break;
            case RoomShape.LShaped:
                PlaceWallsLShaped(r.X, r.Y, r.W, r.H);
                break;
            case RoomShape.TShaped:
                PlaceWallsTShaped(r.X, r.Y, r.W, r.H);
                break;
            case RoomShape.JShaped:
                PlaceWallsJShaped(r.X, r.Y, r.W, r.H);
                break;
        }
    }

    private void PlaceWallsRectangular(int x, int y, int w, int h)
    {
        for (int xx = x; xx < x + w; xx++)
        {
            floorTilemap.SetTile(new Vector3Int(xx, y + h, 0), wallTile);
            floorTilemap.SetTile(new Vector3Int(xx, y - 1, 0), wallTile);
        }
        for (int yy = y - 1; yy <= y + h; yy++)
        {
            floorTilemap.SetTile(new Vector3Int(x - 1, yy, 0), wallTile);
            floorTilemap.SetTile(new Vector3Int(x + w, yy, 0), wallTile);
        }
    }

    /// <summary>
    /// L-shape: only carve out 1 tile in one corner.
    /// </summary>
    private void PlaceWallsLShaped(int x, int y, int w, int h)
    {
        PlaceWallsRectangular(x, y, w, h);

        // Carve out exactly 1 tile in one corner
        int corner = RandomSeed.GetRandomInt(0, 4);
        int carveX = (corner == 0 || corner == 3) ? x : (x + w - 1);
        int carveY = (corner == 0 || corner == 1) ? y : (y + h - 1);

        floorTilemap.SetTile(new Vector3Int(carveX, carveY, 0), floorTile);
    }

    /// <summary>
    /// T-shape: carve exactly 1 tile as a notch from one edge.
    /// </summary>
    private void PlaceWallsTShaped(int x, int y, int w, int h)
    {
        PlaceWallsRectangular(x, y, w, h);

        int edge = RandomSeed.GetRandomInt(0, 4);
        // We carve 1 tile wide
        if (edge == 0) // bottom
        {
            int carveX = x + w / 2;
            floorTilemap.SetTile(new Vector3Int(carveX, y - 1, 0), floorTile);
        }
        else if (edge == 1) // top
        {
            int carveX = x + w / 2;
            floorTilemap.SetTile(new Vector3Int(carveX, y + h, 0), floorTile);
        }
        else if (edge == 2) // left
        {
            int carveY = y + h / 2;
            floorTilemap.SetTile(new Vector3Int(x - 1, carveY, 0), floorTile);
        }
        else // right
        {
            int carveY = y + h / 2;
            floorTilemap.SetTile(new Vector3Int(x + w, carveY, 0), floorTile);
        }
    }

    /// <summary>
    /// J-shape: carve exactly 1 tile from 2 corners, skipping 1 corner. 
    /// But let's keep it minimal: carve out exactly 1 tile from a single corner to reduce big gaps.
    /// </summary>
    private void PlaceWallsJShaped(int x, int y, int w, int h)
    {
        PlaceWallsRectangular(x, y, w, h);

        // Instead of removing two corners, remove exactly 1 tile from one corner 
        // to keep the shape minimal
        int corner = RandomSeed.GetRandomInt(0, 4);
        int carveX = (corner == 1 || corner == 3) ? (x + w - 1) : x;
        int carveY = (corner >= 2) ? y : (y + h - 1);

        floorTilemap.SetTile(new Vector3Int(carveX, carveY, 0), floorTile);
    }

    /// <summary>
    /// Creates a 1-tile corridor between two room centers.
    /// </summary>
    private void CarveCorridor(Vector2Int start, Vector2Int end)
    {
        int minX = Mathf.Min(start.x, end.x);
        int maxX = Mathf.Max(start.x, end.x);
        int minY = Mathf.Min(start.y, end.y);
        int maxY = Mathf.Max(start.y, end.y);

        for (int x = minX; x <= maxX; x++)
        {
            floorTilemap.SetTile(new Vector3Int(x, start.y, 0), floorTile);
        }
        for (int y = minY; y <= maxY; y++)
        {
            floorTilemap.SetTile(new Vector3Int(end.x, y, 0), floorTile);
        }
    }

    /// <summary>
    /// Adds exactly one door gap to the perimeter of the room:
    /// - Picks a random side
    /// - Creates a 1-tile gap by setting it to floor
    /// - Instantiates the door prefab in that tile
    /// </summary>
    private void AddDoorGap(Room r)
    {
        // 0=top, 1=bottom, 2=left, 3=right
        int side = RandomSeed.GetRandomInt(0, 4);
        Vector3Int gapCell = Vector3Int.zero;

        switch (side)
        {
            case 0: // top
                gapCell = new Vector3Int(r.X + r.W / 2, r.Y + r.H, 0);
                break;
            case 1: // bottom
                gapCell = new Vector3Int(r.X + r.W / 2, r.Y - 1, 0);
                break;
            case 2: // left
                gapCell = new Vector3Int(r.X - 1, r.Y + r.H / 2, 0);
                break;
            case 3: // right
                gapCell = new Vector3Int(r.X + r.W, r.Y + r.H / 2, 0);
                break;
        }

        // Convert that tile to floor (making a gap)
        floorTilemap.SetTile(gapCell, floorTile);

        // Place the door prefab at that location
        if (doorPrefab != null)
        {
            Vector3 worldPos = floorTilemap.CellToWorld(gapCell) + new Vector3(0.5f, 0.5f, 0);
            Instantiate(doorPrefab, worldPos, Quaternion.identity, objectContainer);
            _placedDoors.Add(gapCell);
        }
    }

     /// <summary>
    /// Called after identifying the boss room. We place multiple stairs:
    /// - If floorNumber == 1, place a "down" that leads out, plus an "up" to floor 2.
    /// - If floorNumber == totalFloors, place only a "down" from last floor.
    /// - Otherwise, place both up & down.
    /// </summary>
    public void PlaceStairs(Room bossRoom, int floorNumber, int totalFloors)
    {
        // Example: place the stairs in different corners of the boss room
        // or next to each other. We'll do corners for clarity.
        if (bossRoom == null) return;

        // "Down" corner
        Vector3Int downCell = new Vector3Int(bossRoom.X + 1, bossRoom.Y + 1, 0);
        // "Up" corner
        Vector3Int upCell   = new Vector3Int(bossRoom.X + bossRoom.W - 2, bossRoom.Y + bossRoom.H - 2, 0);

        // 1) If floorNumber == 1, place a "down" that leads out (optional), plus an "up"
        // 2) If floorNumber == totalFloors, place only "down"
        // 3) Otherwise place both

        if (floorNumber == 1)
        {
            // (Optional) place a "down" that leads out of the tower
            if (stairDownPrefab != null)
            {
                CreateStairObject(downCell, stairDownPrefab, StairType.Down, floorNumber, totalFloors);
            }
            // place an "up" to floor #2
            if (stairUpPrefab != null)
            {
                CreateStairObject(upCell, stairUpPrefab, StairType.Up, floorNumber, totalFloors);
            }
        }
        else if (floorNumber == totalFloors)
        {
            // only "down"
            if (stairDownPrefab != null)
            {
                CreateStairObject(downCell, stairDownPrefab, StairType.Down, floorNumber, totalFloors);
            }
        }
        else
        {
            // Both
            if (stairDownPrefab != null)
            {
                CreateStairObject(downCell, stairDownPrefab, StairType.Down, floorNumber, totalFloors);
            }
            if (stairUpPrefab != null)
            {
                CreateStairObject(upCell, stairUpPrefab, StairType.Up, floorNumber, totalFloors);
            }
        }
    }

    /// <summary>
    /// Helper to instantiate a stair prefab, set its StairController info, and track it in _placedStairs.
    /// </summary>
    private void CreateStairObject(Vector3Int cellPos, GameObject prefab, StairType stairType, int floorNumber, int totalFloors)
    {
        Vector3 worldPos = floorTilemap.CellToWorld(cellPos) + new Vector3(0.5f, 0.5f, 0);
        GameObject stairObj = Instantiate(prefab, worldPos, Quaternion.identity, objectContainer);

        // If the prefab has a StairController, set the floor info
        StairController sc = stairObj.GetComponent<StairController>();
        if (sc != null)
        {
            sc.stairType     = stairType;
            sc.currentFloor  = floorNumber;
            sc.totalFloors   = totalFloors;
        }
        _placedStairs.Add(cellPos);
    }

    /// <summary>
    /// Places key objects on floor tiles.
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
            if (floorTilemap.GetTile(cellPos) == floorTile)
            {
                Vector3 worldPos = floorTilemap.CellToWorld(cellPos) + new Vector3(0.5f, 0.5f, 0);
                Instantiate(keyPrefab, worldPos, Quaternion.identity, objectContainer);
                placedKeys++;
            }
            attempts++;
        }
    }

    /// <summary>
    /// Places chest prefabs in some rooms (skipping the boss room).
    /// 80% chance per eligible room; area threshold lowered to 30.
    /// </summary>
    private void GenerateChests(List<Room> rooms, Room bossRoom)
    {
        foreach (Room room in rooms)
        {
            if (room == bossRoom || room.Area < 30)
                continue;
            if (Random.value < 0.8f)
            {
                int chestX = RandomSeed.GetRandomInt(room.X + 1, room.X + room.W - 1);
                int chestY = RandomSeed.GetRandomInt(room.Y + 1, room.Y + room.H - 1);
                Vector3Int cellPos = new Vector3Int(chestX, chestY, 0);
                if (floorTilemap.GetTile(cellPos) == floorTile)
                {
                    Vector3 chestWorldPos = floorTilemap.CellToWorld(cellPos) + new Vector3(0.5f, 0.5f, 0);
                    Instantiate(chestPrefab, chestWorldPos, Quaternion.identity, objectContainer);
                    _placedChests.Add(cellPos);
                }
            }
        }
    }

    /// <summary>
    /// Spawns enemies on valid floor tiles, ensuring spacing.
    /// </summary>
    private void GenerateEnemies(int width, int height)
    {
        if (enemyPrefab == null)
        {
            Debug.LogWarning("[FloorGenerator] No enemyPrefab assigned!");
            return;
        }
        int numEnemies = RandomSeed.GetRandomInt(minEnemies, maxEnemies + 1);
        for (int i = 0; i < numEnemies; i++)
        {
            bool validSpawn = false;
            Vector3 candidatePos = Vector3.zero;
            int attempts = 0;
            float minDistance = Random.Range(3f, 6f);
            while (!validSpawn && attempts < 20)
            {
                int x = RandomSeed.GetRandomInt(0, width);
                int y = RandomSeed.GetRandomInt(0, height);
                Vector3Int cell = new Vector3Int(x, y, 0);
                if (floorTilemap.GetTile(cell) == floorTile)
                {
                    candidatePos = floorTilemap.CellToWorld(cell) + new Vector3(0.5f, 0.5f, 0);
                    validSpawn = true;
                    foreach (Vector3 pos in _placedEnemies)
                    {
                        if (Vector3.Distance(candidatePos, pos) < minDistance)
                        {
                            validSpawn = false;
                            break;
                        }
                    }
                }
                attempts++;
            }
            if (validSpawn)
            {
                Instantiate(enemyPrefab, candidatePos, Quaternion.identity, enemyContainer);
                _placedEnemies.Add(candidatePos);
            }
        }
    }

    /// <summary>
    /// Checks if a given cell is floor.
    /// </summary>
    private bool IsFloorTile(Vector3Int cellPos, int width, int height)
    {
        if (cellPos.x < 0 || cellPos.x >= width || cellPos.y < 0 || cellPos.y >= height)
            return false;
        return floorTilemap.GetTile(cellPos) == floorTile;
    }
}
