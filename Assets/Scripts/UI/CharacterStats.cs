using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CharacterStatsUI : MonoBehaviour {
    [Header("UI References")]
    public TMP_Text availablePointsText;

    [Header("Strength UI")]
    public TMP_Text strengthValueText;
    public Button strengthPlusButton;
    public Button strengthMinusButton;

    [Header("Trading UI")]
    public TMP_Text tradingValueText;
    public Button tradingPlusButton;
    public Button tradingMinusButton;

    [Header("Intelligence UI")]
    public TMP_Text intelligenceValueText;
    public Button intelligencePlusButton;
    public Button intelligenceMinusButton;

    [Header("Magic UI")]
    public TMP_Text magicValueText;
    public Button magicPlusButton;
    public Button magicMinusButton;

    [Header("Spirit UI")]
    public TMP_Text spiritValueText;
    public Button spiritPlusButton;
    public Button spiritMinusButton;

    private PlayerXP playerXP;

    void Start() {
        // Find the player and get the necessary components.
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null) {
            playerXP = player.GetComponent<PlayerXP>();
            // Assuming the PlayerStats are part of the PlayerInfo component.
        }

        // Set up button click listeners.
        strengthPlusButton.onClick.AddListener(() => ModifyStat("Strength", true));
        strengthMinusButton.onClick.AddListener(() => ModifyStat("Strength", false));
        
        tradingPlusButton.onClick.AddListener(() => ModifyStat("Trading", true));
        tradingMinusButton.onClick.AddListener(() => ModifyStat("Trading", false));
        
        intelligencePlusButton.onClick.AddListener(() => ModifyStat("Intelligence", true));
        intelligenceMinusButton.onClick.AddListener(() => ModifyStat("Intelligence", false));
        
        magicPlusButton.onClick.AddListener(() => ModifyStat("Magic", true));
        magicMinusButton.onClick.AddListener(() => ModifyStat("Magic", false));
        
        spiritPlusButton.onClick.AddListener(() => ModifyStat("Spirit", true));
        spiritMinusButton.onClick.AddListener(() => ModifyStat("Spirit", false));
        
        // Initial UI update.
        UpdateUI();
    }

    /// <summary>
    /// Handles modifying stats based on the button pressed.
    /// </summary>
    /// <param name="statName">Name of the stat ("Strength", "Trading", etc.)</param>
    /// <param name="isIncrement">True to add a point, false to remove a point</param>
    private void ModifyStat(string statName, bool isIncrement) {
        if (playerXP == null)
            return;

        Debug.Log($"Before modifying {statName}: availableStatPoints = {playerXP.availableStatPoints}");

        // For adding a point.
        if (isIncrement && playerXP.availableStatPoints > 0) {
            switch(statName) {
                case "Strength":
                    PlayerStats.Strength++;
                    break;
                case "Trading":
                    PlayerStats.Trading++;
                    break;
                case "Intelligence":
                    PlayerStats.Intelligence++;
                    break;
                case "Magic":
                    PlayerStats.Magic++;
                    break;
                case "Spirit":
                    PlayerStats.Spirit++;
                    break;
            }
            playerXP.availableStatPoints--;
        }
        // For removing a point (only if stat is above the starting value, assumed to be 1).
        else if (!isIncrement) {
            switch(statName) {
                case "Strength":
                    if (PlayerStats.Strength > 1) { PlayerStats.Strength--; playerXP.availableStatPoints++; }
                    break;
                case "Trading":
                    if (PlayerStats.Trading > 1) { PlayerStats.Trading--; playerXP.availableStatPoints++; }
                    break;
                case "Intelligence":
                    if (PlayerStats.Intelligence > 1) { PlayerStats.Intelligence--; playerXP.availableStatPoints++; }
                    break;
                case "Magic":
                    if (PlayerStats.Magic > 1) { PlayerStats.Magic--; playerXP.availableStatPoints++; }
                    break;
                case "Spirit":
                    if (PlayerStats.Spirit > 1) { PlayerStats.Spirit--; playerXP.availableStatPoints++; }
                    break;
            }
        }

        Debug.Log($"After modifying {statName}: availableStatPoints = {playerXP.availableStatPoints}");
        UpdateUI();
    }


    /// <summary>
    /// Updates the UI elements based on current player stats and available points.
    /// </summary>
    private void UpdateUI() {
        if (playerXP != null && availablePointsText != null) {
            availablePointsText.text = "Available Points: " + playerXP.availableStatPoints.ToString();
            Debug.Log("UI updated: " + availablePointsText.text);
        }

        if (PlayerStats.Instance != null) {
            strengthValueText.text = PlayerStats.Strength.ToString();
            tradingValueText.text = PlayerStats.Trading.ToString();
            intelligenceValueText.text = PlayerStats.Intelligence.ToString();
            magicValueText.text = PlayerStats.Magic.ToString();
            spiritValueText.text = PlayerStats.Spirit.ToString();
        }
    }

    // Optionally, if you implement events in your PlayerXP or PlayerStats,
    // subscribe to them here to update the UI automatically when changes occur.
}
