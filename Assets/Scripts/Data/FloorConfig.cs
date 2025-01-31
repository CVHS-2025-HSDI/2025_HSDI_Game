using UnityEngine;

[System.Serializable]
public class FloorConfig
{
    public int minWidth  = 16;
    public int maxWidth  = 95;
    public int minHeight = 16;
    public int maxHeight = 95;

    public bool useCornerWalls = true;
    public bool hasDoor        = true;
    public int stairsCount     = 1;
}
