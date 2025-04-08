using UnityEngine;

[System.Serializable]
public class PlayerStats {
    public static int Strength = 1;
    public static int Trading = 1;
    public static int Intelligence = 1;
    public static int Magic = 1;
    public static int Spirit = 1;
    
    public static float GetDamageMultiplier() {
        return 1f + (Strength * 0.1f);
    }

    public static float GetDropMultiplier() {
        return 1f + (Trading * 0.1f);
    }

    public static float GetXpMultiplier() {
        return 1f + (Intelligence * 0.1f);
    }
    
    public static float GetMagicMultiplier() {
        return 1f + (Magic * 0.1f);
    }

    public static float GetHealthMultiplier() {
        return 1f + (Spirit * 0.1f);
    }

    // New: Reset stats to their default values.
    public static void ResetStats() {
        Strength = 1;
        Trading = 1;
        Intelligence = 1;
        Magic = 1;
        Spirit = 1;
        Debug.Log("Player stats have been reset.");
    }
}