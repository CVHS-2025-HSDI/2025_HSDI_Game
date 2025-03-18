using UnityEngine;
using UnityEngine.UI;

public class LevelUpMenu : MonoBehaviour
{
    public PlayerInfo playerInfo;
    public Text strengthText;
    public Text tradingText;
    public Text intelligenceText;
    public Text magicText;
    public Text spiritText;
    public int availablePoints = 5;

    void Start()
    {
        UpdateUI();
    }

    public void AddStrength()
    {
        if (availablePoints > 0)
        {
            playerInfo.stats.strength++;
            availablePoints--;
            UpdateUI();
        }
    }

    public void AddTrading()
    {
        if (availablePoints > 0)
        {
            playerInfo.stats.trading++;
            availablePoints--;
            UpdateUI();
        }
    }

    public void AddIntelligence()
    {
        if (availablePoints > 0)
        {
            playerInfo.stats.intelligence++;
            availablePoints--;
            UpdateUI();
        }
    }

    public void AddMagic()
    {
        if (availablePoints > 0)
        {
            playerInfo.stats.magic++;
            availablePoints--;
            UpdateUI();
        }
    }

    public void AddSpirit()
    {
        if (availablePoints > 0)
        {
            playerInfo.stats.spirit++;
            availablePoints--;
            UpdateUI();
        }
    }

    void UpdateUI()
    {
        if (strengthText != null)
            strengthText.text = "STR: " + playerInfo.stats.strength;
        if (tradingText != null)
            tradingText.text = "TRD: " + playerInfo.stats.trading;
        if (intelligenceText != null)
            intelligenceText.text = "INT: " + playerInfo.stats.intelligence;
        if (magicText != null)
            magicText.text = "MAG: " + playerInfo.stats.magic;
        if (spiritText != null)
            spiritText.text = "SPR: " + playerInfo.stats.spirit;
    }
}