using UnityEngine;

[System.Serializable]
public class PlayerStats {
    public int strength = 1;
    public int trading = 1;
    public int intelligence = 1;
    public int magic = 1;
    public int spirit = 1;
    
    public float GetDamageMultiplier() {
        return 1f + (strength * 0.1f);
    }

    public float GetDropMultiplier()
    {
        return 1f + (trading * 0.1f);
    }

    public float GetXpMultiplier()
    {
        return 1f + (intelligence * 0.1f);
    }
    
    public float GetMagicMultiplier()
    {
        return 1f + (magic * 0.1f);
    }

    public float GetHealthMultiplier()
    {
        return 1f + (spirit * 0.1f);
    }
}
