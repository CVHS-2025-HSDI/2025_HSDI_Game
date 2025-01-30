using UnityEngine;

[System.Serializable]
public class FloorConfig
{
    public int minWidth = 8;
    public int maxWidth = 15;
    
    public int minHeight = 8;
    public int maxHeight = 15;

    public bool hasCornerWalls = true;
    public bool isMainDoors = true;
    public int stairCount = 1;
    public int seed = 0; // For reproducible random generation
}
