using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class FloorGenerator : MonoBehaviour
{
    // PROCEDURAL GENERATION V3 – One-gap-per-room approach with lectern and enclosed boss room.
    // Up stairs will always be placed in the boss room.
    // Down stairs will be placed in a random non-boss room (if available).
    // State (keys, chests, enemies) is recorded and reloaded.
    // AUTHORS: Vitaly, Anton, Aron (modified 2/26/2025)

    [Header("Tilemap & Prefab References")]
    public Tilemap floorTilemap;       // Assign in Inspector
    public GameObject keyPrefab;
    public GameObject chestPrefab;
    public Transform objectContainer;
    public GameObject lecternPrefab;   // Lectern prefab to be placed once per floor

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
    public GameObject stairDownPrefab; // Down stair (for floor 1/exiting or for middle/last floors)
    public GameObject stairUpPrefab;   // Up stair (always in boss room for floors > 1)

    // Spawn points set during generation
    [HideInInspector] public Vector3Int playerSpawn;
    [HideInInspector] public Vector3Int merchantSpawn;

    // Tracking special elements
    private List<Vector3Int> _placedStairs = new List<Vector3Int>();
    private List<Vector3Int> _placedDoors = new List<Vector3Int>();
    private List<Vector3Int> _placedChests = new List<Vector3Int>();
    private List<Vector3Int> _placedKeys = new List<Vector3Int>();
    private List<Vector3Int> _placedEnemies = new List<Vector3Int>();

    // We track rooms so we can find the largest as the "boss room"
    private List<Room> _rooms = new List<Room>();

    // Room shapes
    public enum RoomShape { Rectangular, LShaped, TShaped, JShaped }

    // Helper class – includes a doorCell to record its door gap.
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
    /// Generates a new floor and returns a FloorData instance that contains all dynamic object positions.
    /// (Keys and enemies remaining on the floor will be saved in FloorData.)
    /// </summary>
    public FloorData GenerateFloor(FloorConfig config, bool isFirstFloor, int floorNumber, int totalFloors)
    {
        int width  = RandomSeed.GetRandomInt(config.minWidth, config.maxWidth + 1);
        int height = RandomSeed.GetRandomInt(config.minHeight, config.maxHeight + 1);

        floorTilemap.ClearAllTiles();
        _placedStairs.Clear();
        _placedDoors.Clear();
        _placedChests.Clear();
        _placedKeys.Clear();
        _placedEnemies.Clear();
        _rooms.Clear();
        KeyManager.Instance.ResetKeys();

        // Fill grid with floor.
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
            AddDoorGap(newRoom);
        }

        // Determine the boss room (largest room).
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
            CarveCorridor(_rooms[i - 1].Center, _rooms[i].Center);

        // Choose spawn rooms.
        Room spawnRoom = ChooseRandomRoomExcluding(bossRoom);
        playerSpawn = new Vector3Int(spawnRoom.Center.x, spawnRoom.Center.y, 0);
        Room merchantRoom = ChooseRandomRoomExcluding(bossRoom, spawnRoom);
        if (merchantRoom == null)
            merchantRoom = spawnRoom;
        merchantSpawn = new Vector3Int(merchantRoom.Center.x, merchantRoom.Center.y, 0);

        // Place stairs.
        if (bossRoom != null)
        {
            PlaceUpStairs(bossRoom, floorNumber, totalFloors);
        }
        Room downRoom = ChooseRandomRoomExcluding(bossRoom);
        if (downRoom != null)
        {
            PlaceDownStairs(downRoom, floorNumber, totalFloors);
        }
        else if (bossRoom != null)
        {
            PlaceDownStairs(bossRoom, floorNumber, totalFloors);
        }

        // Place lectern at center.
        if (lecternPrefab != null)
        {
            Vector3Int lecternCell = new Vector3Int(width / 2, height / 2, 0);
            if (floorTilemap.GetTile(lecternCell) == floorTile)
            {
                Vector3 lecternWorldPos = floorTilemap.CellToWorld(lecternCell) + new Vector3(0.5f, 0.5f, 0);
                Instantiate(lecternPrefab, lecternWorldPos, Quaternion.identity, objectContainer);
            }
        }

        // Generate keys, chests, and enemies.
        List<Vector2Int> keyPositions = GenerateKeys(width, height, 4);
        List<Vector2Int> chestPositions = GenerateChests(_rooms, bossRoom);
        List<Vector2Int> enemyPositions = GenerateEnemies(width, height);

        floorTilemap.RefreshAllTiles();

        FloorData data = new FloorData
        {
            playerSpawn = playerSpawn,
            merchantSpawn = merchantSpawn,
            stairPositions = _placedStairs.ConvertAll(s => (Vector2Int)s).ToArray(),
            chestPositions = chestPositions.ToArray(),
            roomDoors = _placedDoors.ConvertAll(s => (Vector2Int)s).ToArray(),
            keyPositions = keyPositions.ToArray(),
            enemyPositions = enemyPositions.ToArray()
        };
        return data;
    }

    // --- LOAD FLOOR FROM DATA ---
    // This method re-instantiates keys, chests, and enemies based on stored FloorData.
    // If keys were collected or enemies defeated, their positions would have been removed.
    public void LoadFloorFromData(FloorData data)
    {
        // Clear previous dynamic objects.
        foreach (Transform child in objectContainer)
            Destroy(child.gameObject);
        foreach (Transform child in enemyContainer)
            Destroy(child.gameObject);

        // Re-instantiate keys.
        foreach (Vector2Int keyPos in data.keyPositions)
        {
            Vector3Int cell = new Vector3Int(keyPos.x, keyPos.y, 0);
            Vector3 worldPos = floorTilemap.CellToWorld(cell) + new Vector3(0.5f, 0.5f, 0);
            Instantiate(keyPrefab, worldPos, Quaternion.identity, objectContainer);
        }
        // Re-instantiate chests.
        foreach (Vector2Int chestPos in data.chestPositions)
        {
            Vector3Int cell = new Vector3Int(chestPos.x, chestPos.y, 0);
            Vector3 worldPos = floorTilemap.CellToWorld(cell) + new Vector3(0.5f, 0.5f, 0);
            Instantiate(chestPrefab, worldPos, Quaternion.identity, objectContainer);
        }
        // Re-instantiate enemies.
        foreach (Vector2Int enemyPos in data.enemyPositions)
        {
            Vector3Int cell = new Vector3Int(enemyPos.x, enemyPos.y, 0);
            Vector3 worldPos = floorTilemap.CellToWorld(cell) + new Vector3(0.5f, 0.5f, 0);
            Instantiate(enemyPrefab, worldPos, Quaternion.identity, enemyContainer);
        }
        // (Stairs, lectern, and door objects are assumed static and already present.)
        playerSpawn = data.playerSpawn;
        merchantSpawn = data.merchantSpawn;
    }

    // --- Helper Methods ---
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
    
        // Set the gap cell to floor tile as a starting point.
        floorTilemap.SetTile(gapCell, floorTile);

        // Check the eight neighbors of gapCell.
        bool surroundedByWalls = true;
        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                // Skip the center cell.
                if (dx == 0 && dy == 0)
                    continue;
            
                Vector3Int neighbor = gapCell + new Vector3Int(dx, dy, 0);
                // If any neighbor is not a wall, the gap is not sealed.
                if (floorTilemap.GetTile(neighbor) != wallTile)
                {
                    surroundedByWalls = false;
                    break;
                }
            }
            if (!surroundedByWalls)
                break;
        }
    
        // If the gap is fully surrounded by walls, seal it.
        if (surroundedByWalls)
        {
            floorTilemap.SetTile(gapCell, wallTile);
            return;
        }
    
        // Otherwise, instantiate a door at the gap.
        if (doorPrefab != null)
        {
            if (floorTilemap.GetTile(gapCell) != floorTile)
            {
                floorTilemap.SetTile(gapCell, floorTile);
                // Fix for doors spawned in walls and other stuff (hopefully)
            }
            Vector3 worldPos = floorTilemap.CellToWorld(gapCell) + new Vector3(0.5f, 0.5f, 0);
            Instantiate(doorPrefab, worldPos, Quaternion.identity, objectContainer);
            _placedDoors.Add(gapCell);
        }
        r.doorCell = gapCell;
    }

    private void AddBossRoomDoor(Room r)
    {
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
        // Up stairs always go in the boss room.
        // For simplicity, choose a random candidate corner.
        Vector3Int[] candidates = new Vector3Int[4];
        candidates[0] = new Vector3Int(r.X + 1, r.Y + r.H - 2, 0);      // top-left
        candidates[1] = new Vector3Int(r.X + r.W - 2, r.Y + r.H - 2, 0);  // top-right
        candidates[2] = new Vector3Int(r.X + 1, r.Y + 1, 0);             // bottom-left
        candidates[3] = new Vector3Int(r.X + r.W - 2, r.Y + 1, 0);         // bottom-right
        int idx = RandomSeed.GetRandomInt(0, candidates.Length);
        CreateStairObject(candidates[idx], stairUpPrefab, StairType.Up, floorNumber, totalFloors);
    }

    private void PlaceDownStairs(Room r, int floorNumber, int totalFloors)
    {
        // Down stairs: choose a fixed position in a non-boss room.
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

    private List<Vector2Int> GenerateKeys(int width, int height, int keyCount)
    {
        List<Vector2Int> keyPositions = new List<Vector2Int>();
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
                keyPositions.Add(new Vector2Int(cellPos.x, cellPos.y));
                placedKeys++;
            }
            attempts++;
        }
        return keyPositions;
    }

    private List<Vector2Int> GenerateChests(List<Room> rooms, Room bossRoom)
    {
        List<Vector2Int> chestPositions = new List<Vector2Int>();
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
                    chestPositions.Add(new Vector2Int(cellPos.x, cellPos.y));
                    _placedChests.Add(cellPos);
                }
            }
        }
        return chestPositions;
    }

    private List<Vector2Int> GenerateEnemies(int width, int height)
    {
        List<Vector2Int> enemyPositions = new List<Vector2Int>();
        if (enemyPrefab == null)
        {
            Debug.LogWarning("[FloorGenerator] No enemyPrefab assigned!");
            return enemyPositions;
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
                TileBase tileAtCell = floorTilemap.GetTile(cell);
                if (tileAtCell == floorTile && !_placedDoors.Contains(cell))
                {
                    candidatePos = floorTilemap.CellToWorld(cell) + new Vector3(0.5f, 0.5f, 0);
                    validSpawn = true;
                    foreach (Vector2Int pos in enemyPositions)
                    {
                        if (Vector2.Distance(new Vector2(candidatePos.x, candidatePos.y), pos) < minDistance)
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
                enemyPositions.Add(new Vector2Int((int)candidatePos.x, (int)candidatePos.y));
                _placedEnemies.Add(new Vector3Int((int)candidatePos.x, (int)candidatePos.y, 0));
            }
        }
        return enemyPositions;
    }

    private bool IsFloorTile(Vector3Int cellPos, int width, int height)
    {
        if (cellPos.x < 0 || cellPos.x >= width || cellPos.y < 0 || cellPos.y >= height)
            return false;
        return floorTilemap.GetTile(cellPos) == floorTile;
    }
}
