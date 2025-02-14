using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class FloorGenerator : MonoBehaviour
{
    // PROCEDURAL GENERATION V3 – Floor-first approach.
    // AUTHORS: Vitaly, Anton, Aron (modified)

    [Header("Tilemap & Prefab References")]
    public Tilemap floorTilemap;  // Assign in Inspector
    public GameObject keyPrefab;
    public GameObject chestPrefab;

    [Header("Tile Assets")]
    public TileBase floorTile;
    public TileBase wallTile;
    public TileBase doorTile;       // optional
    public TileBase stairDownTile;  // For first floor
    public TileBase stairUpTile;    // For non-first floors

    [Header("Enemy Settings")]
    public int minEnemies = 60;
    public int maxEnemies = 61;
    public Transform enemyContainer;
    public GameObject enemyPrefab;

    // Spawn points set during generation.
    [HideInInspector] public Vector3Int playerSpawn;
    [HideInInspector] public Vector3Int merchantSpawn;

    // Lists for special elements we generate
    private List<Vector3Int> _placedStairs = new List<Vector3Int>();
    private List<Vector3Int> _placedDoors = new List<Vector3Int>();
    private List<Vector3Int> _placedChests = new List<Vector3Int>();
    private List<Vector3> _placedEnemies = new List<Vector3>();

    // We track rooms so we can find the largest as the "boss room"
    private List<Room> _rooms = new List<Room>();

    // A helper class to represent rooms
    private class Room
    {
        public int x, y, w, h;
        public Room(int x, int y, int w, int h)
        {
            this.x = x; this.y = y; this.w = w; this.h = h;
        }
        public Vector2Int Center => new Vector2Int(x + w / 2, y + h / 2);
        public int Area => w * h;
        public bool Intersects(Room other)
        {
            return (x <= other.x + other.w && x + w >= other.x &&
                    y <= other.y + other.h && y + h >= other.y);
        }
    }

    /// <summary>
    /// Generates a floor by first filling with floor, then placing walls to define rooms and corridors.
    /// </summary>
    public FloorData GenerateFloor(FloorConfig config, bool isFirstFloor)
    {
        // 1) Randomize overall dimensions
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

        // 3) Apply a non-rectangular boundary shape
        //    For example, carve a circular boundary so the edges are “outside” the map
        //    (We’ll place wall tiles at the edges)
        MakeCircularBoundary(width, height);

        // 4) Generate random rooms by placing walls around them.
        //    We treat each “room” as a rectangular region we want to keep open,
        //    then we place walls on the perimeter.
        int numRooms = RandomSeed.GetRandomInt(5, 10);
        for (int i = 0; i < numRooms; i++)
        {
            // Choose random size. Feel free to tweak min/max
            int roomW = RandomSeed.GetRandomInt(6, Mathf.Min(12, width / 2));
            int roomH = RandomSeed.GetRandomInt(6, Mathf.Min(12, height / 2));
            int roomX = RandomSeed.GetRandomInt(2, width - roomW - 2);
            int roomY = RandomSeed.GetRandomInt(2, height - roomH - 2);

            var newRoom = new Room(roomX, roomY, roomW, roomH);

            // Check for overlap
            bool overlaps = false;
            foreach (var other in _rooms)
            {
                if (newRoom.Intersects(other))
                {
                    overlaps = true;
                    break;
                }
            }
            if (overlaps) continue;

            // Mark the perimeter of the room as walls
            // (But keep the inside as floor)
            _rooms.Add(newRoom);
            PlaceWallsAroundRoom(newRoom);
        }

        // 5) Connect the rooms with corridors (just fill them with floor or remove perimeter walls).
        //    Because we already have floor, we can optionally place walls to define corridors or
        //    keep them open. For demonstration, we do a simple “connect centers in a line” approach:
        _rooms.Sort((a, b) => a.Center.x.CompareTo(b.Center.x));
        for (int i = 1; i < _rooms.Count; i++)
        {
            Vector2Int prev = _rooms[i - 1].Center;
            Vector2Int curr = _rooms[i].Center;
            CarveCorridor(prev, curr);
        }

        // 6) Identify the largest room as the boss room
        Room bossRoom = _rooms.Count > 0 ? _rooms[0] : null;
        foreach (Room r in _rooms)
        {
            if (bossRoom == null || r.Area > bossRoom.Area)
                bossRoom = r;
        }

        if (bossRoom != null)
        {
            // Place stairs in the boss room center
            Vector3Int bossCenter = new Vector3Int(bossRoom.Center.x, bossRoom.Center.y, 0);
            if (isFirstFloor)
                floorTilemap.SetTile(bossCenter, stairDownTile);
            else
                floorTilemap.SetTile(bossCenter, stairUpTile);
            _placedStairs.Add(bossCenter);
        }

        // 7) Place keys, chests, enemies, etc.
        GenerateKeys(width, height, 4);
        GenerateChests(_rooms, bossRoom);
        GenerateEnemies(width, height);

        // 8) Spawn positions
        if (bossRoom != null)
        {
            if (isFirstFloor)
            {
                // e.g. spawn the player next to the downward stairs
                Vector3Int candidate = new Vector3Int(bossRoom.Center.x + 1, bossRoom.Center.y, 0);
                if (IsFloorTile(candidate, width, height))
                    playerSpawn = candidate;
                else
                    playerSpawn = new Vector3Int(bossRoom.Center.x, bossRoom.Center.y, 0);
            }
            else
            {
                // spawn on the ascending stairs
                playerSpawn = new Vector3Int(bossRoom.Center.x, bossRoom.Center.y, 0);
            }
            // Merchant spawn in first room center (or fallback)
            merchantSpawn = new Vector3Int(_rooms[0].Center.x, _rooms[0].Center.y, 0);
        }
        else
        {
            // Fallback if no rooms
            playerSpawn = new Vector3Int(width / 2, height / 2, 0);
            merchantSpawn = playerSpawn;
        }

        // Refresh tilemap colliders
        floorTilemap.RefreshAllTiles();

        // 9) Prepare and return FloorData
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
    /// Example of carving a circular boundary by placing walls outside a circle.
    /// </summary>
    private void MakeCircularBoundary(int width, int height)
    {
        // Center of the map
        float centerX = width / 2f;
        float centerY = height / 2f;
        float radius = Mathf.Min(width, height) / 2f - 1f;

        // For each tile, if it’s outside the circle, set it to wall
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
    }

    /// <summary>
    /// Surround the edges of a room with walls. Leaves the inside as floor.
    /// </summary>
    private void PlaceWallsAroundRoom(Room r)
    {
        // Top & bottom edges
        for (int x = r.x; x < r.x + r.w; x++)
        {
            floorTilemap.SetTile(new Vector3Int(x, r.y + r.h, 0), wallTile);
            floorTilemap.SetTile(new Vector3Int(x, r.y - 1, 0), wallTile);
        }
        // Left & right edges
        for (int y = r.y - 1; y <= r.y + r.h; y++)
        {
            floorTilemap.SetTile(new Vector3Int(r.x - 1, y, 0), wallTile);
            floorTilemap.SetTile(new Vector3Int(r.x + r.w, y, 0), wallTile);
        }
    }

    /// <summary>
    /// Simple corridor “carving”: we remove walls in a straight line horizontally, then vertically.
    /// </summary>
    private void CarveCorridor(Vector2Int start, Vector2Int end)
    {
        int minX = Mathf.Min(start.x, end.x);
        int maxX = Mathf.Max(start.x, end.x);
        int minY = Mathf.Min(start.y, end.y);
        int maxY = Mathf.Max(start.y, end.y);

        // Horizontal pass
        for (int x = minX; x <= maxX; x++)
        {
            floorTilemap.SetTile(new Vector3Int(x, start.y, 0), floorTile);
        }
        // Vertical pass
        for (int y = minY; y <= maxY; y++)
        {
            floorTilemap.SetTile(new Vector3Int(end.x, y, 0), floorTile);
        }
    }

    /// <summary>
    /// Randomly places a given number of key objects on floor tiles.
    /// </summary>
    private void GenerateKeys(int width, int height, int keyCount)
    {
        // (Same as before)
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
                Instantiate(keyPrefab, worldPos, Quaternion.identity);
                placedKeys++;
            }
            attempts++;
        }
    }

    /// <summary>
    /// Places chest prefabs in some of the rooms (skipping the boss room).
    /// </summary>
    private void GenerateChests(List<Room> rooms, Room bossRoom)
    {
        // (Same as before, but we now have "rooms" that are walled rectangles.)
        foreach (Room room in rooms)
        {
            if (room == bossRoom || room.Area < 50)
                continue;
            // 50% chance
            if (Random.value > 0.5f)
            {
                int chestX = RandomSeed.GetRandomInt(room.x + 1, room.x + room.w - 1);
                int chestY = RandomSeed.GetRandomInt(room.y + 1, room.y + room.h - 1);
                Vector3Int cellPos = new Vector3Int(chestX, chestY, 0);
                if (floorTilemap.GetTile(cellPos) == floorTile)
                {
                    Vector3 chestWorldPos = floorTilemap.CellToWorld(cellPos) + new Vector3(0.5f, 0.5f, 0);
                    Instantiate(chestPrefab, chestWorldPos, Quaternion.identity);
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
        // (Same as your code.)
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
                var enemy = Instantiate(enemyPrefab, candidatePos, Quaternion.identity, enemyContainer);
                _placedEnemies.Add(candidatePos);
            }
        }
    }

    /// <summary>
    /// Check if a given cell is floor (i.e., not out of bounds and not a wall tile).
    /// </summary>
    private bool IsFloorTile(Vector3Int cellPos, int width, int height)
    {
        if (cellPos.x < 0 || cellPos.x >= width || cellPos.y < 0 || cellPos.y >= height)
            return false;
        return floorTilemap.GetTile(cellPos) == floorTile;
    }
}
