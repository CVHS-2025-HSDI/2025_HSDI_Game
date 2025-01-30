using UnityEngine;

[System.Serializable]
public class FloorConfig
{
    public int minWidth  = 8;
    public int maxWidth  = 15;
    public int minHeight = 8;
    public int maxHeight = 15;

    public bool useCornerWalls = true;
    public bool hasDoor        = true;
    public int stairsCount     = 1;
}
