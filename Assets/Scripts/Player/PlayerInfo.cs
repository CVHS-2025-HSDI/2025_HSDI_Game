using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerInfo : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    public float currentHealth = 100f;

    [Header("UI References")]
    public Slider HPBar;                // Assign in Inspector
    public GameObject gameOverPanel;    // Panel under MainCanvas (covers screen)
    public GameObject youDiedText;      // Child text object (initially inactive)

    private SpriteRenderer sr;
    private bool isDead = false;

    void Start()
    {
        currentHealth = maxHealth;
        sr = GetComponent<SpriteRenderer>();

        if (HPBar != null)
        {
            HPBar.maxValue = maxHealth;
            HPBar.value = currentHealth;
        }
        // Initialize GameOver panel: set its alpha to 0 and disable the YouDied text.
        if (gameOverPanel != null)
        {
            Image panelImage = gameOverPanel.GetComponent<Image>();
            if (panelImage != null)
            {
                Color c = panelImage.color;
                c.a = 0f;
                panelImage.color = c;
            }
            if (youDiedText != null)
                youDiedText.SetActive(false);
        }
    }

    public void damage(float dmg)
    {
        if (isDead) return;
        currentHealth -= dmg;
        Debug.Log("Player hit for " + dmg + ", health now: " + currentHealth);
        if (HPBar != null)
            HPBar.value = currentHealth;
        if (currentHealth <= 0)
            Die();
    }

    private void Die()
    {
        isDead = true;
        // Freeze movement by disabling the Movement script (assumed to be on the same GameObject)
        Movement moveScript = GetComponent<Movement>();
        if (moveScript != null)
            moveScript.enabled = false;
        StartCoroutine(GameOverSequence());
    }

    private IEnumerator GameOverSequence()
    {
        // Play GameOver music
        if (MusicController.Instance != null)
            MusicController.Instance.PlayGameOverMusic();

        float fadeDuration = 4f;
        float timer = 0f;
        Color initialPlayerColor = sr.color;

        Image panelImage = null;
        Color initialPanelColor = Color.black;
        if (gameOverPanel != null)
        {
            panelImage = gameOverPanel.GetComponent<Image>();
            if (panelImage != null)
            {
                initialPanelColor = panelImage.color;
                // Ensure panel starts fully transparent
                Color temp = initialPanelColor;
                temp.a = 0f;
                panelImage.color = temp;
            }
        }

        // Simultaneously fade out the player's sprite and fade in the game over panel.
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float t = timer / fadeDuration;
            // Fade out player (alpha goes from current to 0)
            float playerAlpha = Mathf.Lerp(initialPlayerColor.a, 0f, t);
            sr.color = new Color(initialPlayerColor.r, initialPlayerColor.g, initialPlayerColor.b, playerAlpha);
            // Fade in panel (alpha goes from 0 to 1)
            if (panelImage != null)
            {
                float panelAlpha = Mathf.Lerp(0f, 1f, t);
                panelImage.color = new Color(initialPanelColor.r, initialPanelColor.g, initialPanelColor.b, panelAlpha);
            }
            yield return null;
        }
        // Ensure final states
        sr.color = new Color(initialPlayerColor.r, initialPlayerColor.g, initialPlayerColor.b, 0f);
        if (panelImage != null)
            panelImage.color = new Color(initialPanelColor.r, initialPanelColor.g, initialPanelColor.b, 1f);
        if (youDiedText != null)
            youDiedText.SetActive(true);
        // (Optionally, you might pause the game or show a reset button here.)
    }
}
