using UnityEngine;

[System.Serializable]
public class PlayerStats {
    public static SingletonManager Instance { get; private set; } 
    
    public static int Strength = 1;
    public static int Trading = 1;
    public static int Intelligence = 1;
    public static int Magic = 1;
    public static int Spirit = 1;
    
    public static float GetDamageMultiplier() 
    {
        return 1f + (Strength * 0.1f);
    }

    public static float GetDropMultiplier() 
    {
        return 1f + (Trading * 0.1f);
    }

    public static float GetXpMultiplier()
    {
        return 1f + (Intelligence * 0.1f);
    }
    
    public static float GetMagicMultiplier()
    {
        return 1f + (Magic * 0.1f);
    }

    public static float GetHealthMultiplier()
    {
        return 1f + (Spirit * 0.1f);
    }
}
