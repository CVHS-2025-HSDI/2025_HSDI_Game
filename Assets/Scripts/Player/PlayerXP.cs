using UnityEngine;
using TMPro;

public class PlayerXP : MonoBehaviour {
    public static PlayerXP Instance;  // Singleton instance
    
    public int currentXP = 0;
    public int currentLevel = 1;
    public int xpToNextLevel = 500;
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
        availableStatPoints += 5;
        Debug.Log($"Level Up! Now at level {currentLevel}. Available stat points increased to {availableStatPoints}.");
    
        // Update the XP UI as usual.
        UpdateXPUI();

        // Directly update the CharacterStats UI if it exists.
        CharacterStatsUI statsUI = FindFirstObjectByType<CharacterStatsUI>();
        if (statsUI != null) {
            statsUI.UpdateUI();
        }
    }


    private int CalculateXPForNextLevel() {
        // Simple formula!
        return currentLevel * 500;
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
    
    public void ResetXP() {
        currentXP = 0;
        currentLevel = 1;
        xpToNextLevel = 500;
        availableStatPoints = 0;
        UpdateXPUI();
        Debug.Log("Player XP has been reset.");
    }
}