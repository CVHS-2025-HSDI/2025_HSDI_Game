using UnityEngine;

[System.Serializable]
public class FloorData {
    public Vector3Int playerSpawn;
    public Vector3Int merchantSpawn;
    public Vector2Int[] stairPositions;
    public Vector2Int[] chestPositions;
    public Vector2Int[] roomDoors;
    public Vector2Int[] keyPositions;
    public Vector2Int[] enemyPositions;
}