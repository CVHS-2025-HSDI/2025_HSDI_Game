using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class FloorGenerator : MonoBehaviour
{
    // PROCEDURAL GENERATION V3 – One-gap-per-room approach with lectern and enclosed boss room.
    // Up stairs will always be placed in the boss room.
    // Down stairs will be placed in a random non-boss room (if available).
    // AUTHORS: Vitaly, Anton, Aron (modified 2/14/2025)

    [Header("Tilemap & Prefab References")]
    public Tilemap floorTilemap;   // Assign in Inspector
    public GameObject keyPrefab;
    public GameObject chestPrefab;
    public Transform objectContainer;
    public GameObject lecternPrefab;  // Lectern prefab to be placed once per floor

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
    public GameObject stairDownPrefab;  // Down stair (for floor 1/exiting or for middle/last floors)
    public GameObject stairUpPrefab;    // Up stair (always in boss room for floors > 1)

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

    // Helper class – now with a doorCell field to record its door gap.
    public class Room
    {
        public int X, Y, W, H;  
        public RoomShape Shape;
        public Vector3Int doorCell; // stores the door gap cell

        public Room(int x, int y, int w, int h, RoomShape shape)
        {
            X = x; Y = y; W = w; H = h; Shape = shape;
            doorCell = new Vector3Int(-1, -1, 0);
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
    /// Generates a floor by filling with floor tiles, applying a circular boundary,
    /// generating rooms (each with one door gap) and connecting them,
    /// then placing a lectern and designating the largest room as the boss room.
    /// In the boss room, a dedicated boss door is placed and the up stairs are placed.
    /// The down stairs are placed in a random room that is not the boss room.
    /// </summary>
    public FloorData GenerateFloor(FloorConfig config, bool isFirstFloor, int floorNumber, int totalFloors)
    {
        int width  = RandomSeed.GetRandomInt(config.minWidth, config.maxWidth + 1);
        int height = RandomSeed.GetRandomInt(config.minHeight, config.maxHeight + 1);

        floorTilemap.ClearAllTiles();
        _placedStairs.Clear();
        _placedEnemies.Clear();
        _placedDoors.Clear();
        _placedChests.Clear();
        _rooms.Clear();
        KeyManager.Instance.ResetKeys();

        // Fill entire grid with floor.
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                floorTilemap.SetTile(new Vector3Int(x, y, 0), floorTile);

        MakeCircularBoundary(width, height);

        // Generate random rooms.
        int numRooms = RandomSeed.GetRandomInt(12, 20);
        for (int i = 0; i < numRooms; i++)
        {
            int roomW = RandomSeed.GetRandomInt(6, Mathf.Min(12, width / 2));
            int roomH = RandomSeed.GetRandomInt(6, Mathf.Min(12, height / 2));
            int roomX = RandomSeed.GetRandomInt(2, width - roomW - 2);
            int roomY = RandomSeed.GetRandomInt(2, height - roomH - 2);
            RoomShape shape = (RoomShape)RandomSeed.GetRandomInt(0, 4);

            var newRoom = new Room(roomX, roomY, roomW, roomH, shape);
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
            PlaceWallsForShape(newRoom);
            // For normal rooms, add one door gap.
            AddDoorGap(newRoom);
        }

        // Determine the boss room as the largest room.
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
        // For the boss room, remove its previously placed door (if any) and add a dedicated boss door.
        if (bossRoom != null)
        {
            if (_placedDoors.Contains(bossRoom.doorCell))
            {
                _placedDoors.Remove(bossRoom.doorCell);
                floorTilemap.SetTile(bossRoom.doorCell, wallTile);
            }
            AddBossRoomDoor(bossRoom);
        }

        // Connect rooms with corridors.
        _rooms.Sort((a, b) => a.Center.x.CompareTo(b.Center.x));
        for (int i = 1; i < _rooms.Count; i++)
        {
            CarveCorridor(_rooms[i - 1].Center, _rooms[i].Center);
        }
        
        // Choose a spawn room (for the player) that is not the boss room.
        Room spawnRoom = ChooseRandomRoomExcluding(bossRoom);
        playerSpawn = new Vector3Int(spawnRoom.Center.x, spawnRoom.Center.y, 0);

        // Choose a different room for the merchant if available.
        Room merchantRoom = ChooseRandomRoomExcluding(bossRoom, spawnRoom);
        if (merchantRoom == null)
            merchantRoom = spawnRoom;
        merchantSpawn = new Vector3Int(merchantRoom.Center.x, merchantRoom.Center.y, 0);
        
        // Place up stairs in the boss room.
        if (bossRoom != null)
        {
            PlaceUpStairs(bossRoom, floorNumber, totalFloors);
        }
        // Place down stairs in a room different from the boss room.
        Room downRoom = ChooseRandomRoomExcluding(bossRoom);
        if (downRoom != null)
        {
            PlaceDownStairs(downRoom, floorNumber, totalFloors);
        }
        else if (bossRoom != null)
        {
            // Fallback if only boss room exists.
            PlaceDownStairs(bossRoom, floorNumber, totalFloors);
        }
        // --- END STAIRCASE PLACEMENT ---

        // Place lectern once per floor.
        if (lecternPrefab != null)
        {
            Vector3Int lecternCell = new Vector3Int(width / 2, height / 2, 0);
            if (floorTilemap.GetTile(lecternCell) == floorTile)
            {
                Vector3 lecternWorldPos = floorTilemap.CellToWorld(lecternCell) + new Vector3(0.5f, 0.5f, 0);
                Instantiate(lecternPrefab, lecternWorldPos, Quaternion.identity, objectContainer);
            }
        }

        // Place keys, chests, and enemies.
        GenerateKeys(width, height, 4);
        GenerateChests(_rooms, bossRoom);
        GenerateEnemies(width, height);

        floorTilemap.RefreshAllTiles();

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

    private Room ChooseRandomRoomExcluding(Room bossRoom, Room additionalExclusion = null)
    {
        List<Room> valid = _rooms.FindAll(r => r != bossRoom && r != additionalExclusion);
        if (valid.Count == 0)
            return bossRoom;
        int index = RandomSeed.GetRandomInt(0, valid.Count);
        return valid[index];
    }

    private void MakeCircularBoundary(int width, int height)
    {
        float centerX = width / 2f;
        float centerY = height / 2f;
        float radius = Mathf.Min(width, height) / 2f - 1f;
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                float dx = x - centerX;
                float dy = y - centerY;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);
                if (dist > radius)
                    floorTilemap.SetTile(new Vector3Int(x, y, 0), wallTile);
            }
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

    private void PlaceWallsLShaped(int x, int y, int w, int h)
    {
        PlaceWallsRectangular(x, y, w, h);
        int corner = RandomSeed.GetRandomInt(0, 4);
        int carveX = (corner == 0 || corner == 3) ? x : (x + w - 1);
        int carveY = (corner == 0 || corner == 1) ? y : (y + h - 1);
        floorTilemap.SetTile(new Vector3Int(carveX, carveY, 0), floorTile);
    }

    private void PlaceWallsTShaped(int x, int y, int w, int h)
    {
        PlaceWallsRectangular(x, y, w, h);
        int edge = RandomSeed.GetRandomInt(0, 4);
        if (edge == 0)
        {
            int carveX = x + w / 2;
            floorTilemap.SetTile(new Vector3Int(carveX, y - 1, 0), floorTile);
        }
        else if (edge == 1)
        {
            int carveX = x + w / 2;
            floorTilemap.SetTile(new Vector3Int(carveX, y + h, 0), floorTile);
        }
        else if (edge == 2)
        {
            int carveY = y + h / 2;
            floorTilemap.SetTile(new Vector3Int(x - 1, carveY, 0), floorTile);
        }
        else
        {
            int carveY = y + h / 2;
            floorTilemap.SetTile(new Vector3Int(x + w, carveY, 0), floorTile);
        }
    }

    private void PlaceWallsJShaped(int x, int y, int w, int h)
    {
        PlaceWallsRectangular(x, y, w, h);
        int corner = RandomSeed.GetRandomInt(0, 4);
        int carveX = (corner == 1 || corner == 3) ? (x + w - 1) : x;
        int carveY = (corner >= 2) ? y : (y + h - 1);
        floorTilemap.SetTile(new Vector3Int(carveX, carveY, 0), floorTile);
    }

    private void CarveCorridor(Vector2Int start, Vector2Int end)
    {
        int minX = Mathf.Min(start.x, end.x);
        int maxX = Mathf.Max(start.x, end.x);
        int minY = Mathf.Min(start.y, end.y);
        int maxY = Mathf.Max(start.y, end.y);
        for (int x = minX; x <= maxX; x++)
            floorTilemap.SetTile(new Vector3Int(x, start.y, 0), floorTile);
        for (int y = minY; y <= maxY; y++)
            floorTilemap.SetTile(new Vector3Int(end.x, y, 0), floorTile);
    }

    private void AddDoorGap(Room r)
    {
        int side = RandomSeed.GetRandomInt(0, 4);
        Vector3Int gapCell = Vector3Int.zero;
        switch (side)
        {
            case 0:
                gapCell = new Vector3Int(r.X + r.W / 2, r.Y + r.H, 0);
                break;
            case 1:
                gapCell = new Vector3Int(r.X + r.W / 2, r.Y - 1, 0);
                break;
            case 2:
                gapCell = new Vector3Int(r.X - 1, r.Y + r.H / 2, 0);
                break;
            case 3:
                gapCell = new Vector3Int(r.X + r.W, r.Y + r.H / 2, 0);
                break;
        }
        floorTilemap.SetTile(gapCell, floorTile);
        if (doorPrefab != null)
        {
            Vector3 worldPos = floorTilemap.CellToWorld(gapCell) + new Vector3(0.5f, 0.5f, 0);
            Instantiate(doorPrefab, worldPos, Quaternion.identity, objectContainer);
            _placedDoors.Add(gapCell);
        }
        r.doorCell = gapCell;
    }

    private void AddBossRoomDoor(Room r)
    {
        // Always use the bottom wall for the boss room door.
        Vector3Int doorCell = new Vector3Int(r.X + r.W / 2, r.Y - 1, 0);
        floorTilemap.SetTile(doorCell, floorTile);
        if (doorPrefab != null)
        {
            Vector3 worldPos = floorTilemap.CellToWorld(doorCell) + new Vector3(0.5f, 0.5f, 0);
            Instantiate(doorPrefab, worldPos, Quaternion.identity, objectContainer);
            _placedDoors.Add(doorCell);
        }
        r.doorCell = doorCell;
    }

    // --- New methods for separate staircase placement ---
    private void PlaceUpStairs(Room r, int floorNumber, int totalFloors)
    {
        // Up stairs go in the boss room.
        // For up stairs, choose a candidate corner from r.
        Vector3Int[] candidates = new Vector3Int[4];
        candidates[0] = new Vector3Int(r.X + 1, r.Y + r.H - 2, 0);      // top-left
        candidates[1] = new Vector3Int(r.X + r.W - 2, r.Y + r.H - 2, 0);  // top-right
        candidates[2] = new Vector3Int(r.X + 1, r.Y + 1, 0);             // bottom-left
        candidates[3] = new Vector3Int(r.X + r.W - 2, r.Y + 1, 0);         // bottom-right
        // For example, choose a random candidate.
        int idx = RandomSeed.GetRandomInt(0, candidates.Length);
        CreateStairObject(candidates[idx], stairUpPrefab, StairType.Up, floorNumber, totalFloors);
    }

    private void PlaceDownStairs(Room r, int floorNumber, int totalFloors)
    {
        // Down stairs: choose a fixed position in the room (e.g. bottom-left).
        Vector3Int downCell = new Vector3Int(r.X + 1, r.Y + 1, 0);
        CreateStairObject(downCell, stairDownPrefab, StairType.Down, floorNumber, totalFloors);
    }

    private void CreateStairObject(Vector3Int cellPos, GameObject prefab, StairType stairType, int floorNumber, int totalFloors)
    {
        Vector3 worldPos = floorTilemap.CellToWorld(cellPos) + new Vector3(0.5f, 0.5f, 0);
        GameObject stairObj = Instantiate(prefab, worldPos, Quaternion.identity, objectContainer);
        StairController sc = stairObj.GetComponent<StairController>();
        if (sc != null)
        {
            sc.stairType = stairType;
            sc.currentFloor = floorNumber;
            sc.totalFloors = totalFloors;
        }
        _placedStairs.Add(cellPos);
    }

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

    private bool IsFloorTile(Vector3Int cellPos, int width, int height)
    {
        if (cellPos.x < 0 || cellPos.x >= width || cellPos.y < 0 || cellPos.y >= height)
            return false;
        return floorTilemap.GetTile(cellPos) == floorTile;
    }
}
