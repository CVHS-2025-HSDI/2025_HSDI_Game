using UnityEngine;
using TMPro;

public class PlayerXP : MonoBehaviour {
    public int currentXP = 0;
    public int currentLevel = 1;
    public int xpToNextLevel = 100;
    public int availableStatPoints = 0;
    public TMP_Text xpUIText; // Reference to an on-screen UI text element for XP
    public TMP_Text levelUIText;

    // Call this method to add XP to the player
    public void AddXP(int baseXP) {
        Debug.Log("AddXP called with baseXP: " + baseXP);
    
        // Retrieve the player's stats from PlayerInfo
        PlayerStats stats = GetComponent<PlayerInfo>()?.stats;
        if (stats == null) {
            Debug.LogWarning("PlayerStats is null in AddXP!");
        }
    
        // Use the instance method instead of a static call.
        float multiplier = stats != null ? PlayerStats.GetXpMultiplier() : 1f;
        int xpGained = Mathf.RoundToInt(baseXP * multiplier);
        Debug.Log("XP gained: " + xpGained + " (multiplier: " + multiplier + ")");
    
        currentXP += xpGained;
        UpdateXPUI();
    
        // Check for level up
        while (currentXP >= xpToNextLevel) {
            LevelUp();
        }
    }

    private void LevelUp() {
        currentLevel++;
        currentXP -= xpToNextLevel;
        xpToNextLevel = CalculateXPForNextLevel();
        availableStatPoints++; // Increase available stat points (could be more than 1)
        Debug.Log($"Level Up! Now at level {currentLevel}. Available stat points increased to {availableStatPoints}.");
        UpdateXPUI();
    }

    private int CalculateXPForNextLevel() {
        // Simple formula; you can adjust this as needed.
        return currentLevel * 100;
    }
    
    private void UpdateXPUI() {
        if (levelUIText != null)
        {
            levelUIText.text = $"Level: {currentLevel}";
            Debug.Log("Level UI Updated: " + levelUIText.text);
        }
        else
        {
            Debug.LogWarning("LevelUIText is null in UpdateXPUI!");
        }
        
        if (xpUIText != null) {
            xpUIText.text = $"XP: {currentXP}/{xpToNextLevel}";
            Debug.Log("XP UI Updated: " + xpUIText.text);
        } else {
            Debug.LogWarning("xpUIText is null in UpdateXPUI!");
        }
    }
}