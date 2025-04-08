using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class PlayerInfo : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    public float currentHealth = 100f;
    
    public PlayerStats stats;

    // Remove local UI references – they will now be accessed through SingletonManager.
    // public GameObject gameOverPanel;
    // public GameObject youDiedText;
    // public GameObject restartButton;
    // public GameObject quitToMenuButton;
    // public GameObject quitButton;
    // public GameObject toolbar;
    // public GameObject invButton;

    private SpriteRenderer sr;
    private bool isDead = false;

    public GameObject weaponPrefab; // Assign in Inspector
    private GameObject weapon;

    public GameObject DamageTextPrefab;

    void Start()
    {
        currentHealth = maxHealth;
        sr = GetComponent<SpriteRenderer>();

        // Assume HPBar is either assigned locally or found on a child.
        Slider HPBar = GetComponentInChildren<Slider>();
        if (HPBar != null)
        {
            HPBar.maxValue = maxHealth;
            HPBar.value = currentHealth;
        }
        
        // Initialize GameOver panel: set its alpha to 0 and disable the YouDied text.
        if (SingletonManager.Instance.gameOverPanel != null)
        {
            Image panelImage = SingletonManager.Instance.gameOverPanel.GetComponent<Image>();
            if (panelImage != null)
            {
                Color c = panelImage.color;
                c.a = 0f;
                panelImage.color = c;
            }
            if (SingletonManager.Instance.youDiedText != null)
                SingletonManager.Instance.youDiedText.SetActive(false);
        }

    }

    public void Heal(float amount){
        if (currentHealth != maxHealth){
    currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
    Slider HPBar = GetComponentInChildren<Slider>();
    if (HPBar != null)
        HPBar.value = currentHealth;
    Debug.Log("Healed for " + amount + ", current health: " + currentHealth);
    }else{
        return;
    }
}


    public void damage(float dmg)
    {
        if (isDead) return;
        currentHealth -= dmg;

        //Display damage taken
        GameObject text = Instantiate(DamageTextPrefab, transform.position, Quaternion.identity);
        text.GetComponent<DamageTextScript>().SetTarget(transform);
        TMP_Text textComp = text.GetComponent<TMP_Text>();
        textComp.text = "" + dmg;
        RectTransform textTransform = text.GetComponent<RectTransform>();
        textTransform.position = new Vector2(transform.position.x, transform.position.y + 0.8f);

        Debug.Log("Player hit for " + dmg + ", health now: " + currentHealth);
        
        Slider HPBar = GetComponentInChildren<Slider>();
        if (HPBar != null)
            HPBar.value = currentHealth;
            
        if (currentHealth <= 0)
            Die();
    }

    private void Die()
    {
        isDead = true;
        if (weapon != null)
            weapon.SetActive(false);
    
        // Freeze movement by disabling the Movement script.
        Movement moveScript = GetComponent<Movement>();
        if (moveScript != null)
            moveScript.enabled = false;
    
        // Hide LorePanel.
        if (SingletonManager.Instance.gameplayCanvas != null)
        {
            Transform lorePanel = SingletonManager.Instance.gameplayCanvas.transform.Find("LorePanel");
            if (lorePanel != null)
                lorePanel.gameObject.SetActive(false);
        }
        
        InventoryManager invManager = FindFirstObjectByType<InventoryManager>();
        if (invManager != null)
        {
            invManager.ClearInventoryExceptSword();
        }
    
        StartCoroutine(GameOverSequence());
    }
    
    public void Revive()
    {
        isDead = false;
        currentHealth = maxHealth;
        Slider HPBar = GetComponentInChildren<Slider>();
        if (HPBar != null)
            HPBar.value = currentHealth;
        Debug.Log("Player revived.");
    }

    private IEnumerator GameOverSequence()
    {
        // Play GameOver music if available.
        if (MusicController.Instance != null)
            MusicController.Instance.PlayGameOverMusic();
        
        float fadeDuration = 4f;
        float timer = 0f;
        Color initialPlayerColor = sr.color;

        Image panelImage = null;
        Color initialPanelColor = Color.black;
        if (SingletonManager.Instance.gameOverPanel != null)
        {
            panelImage = SingletonManager.Instance.gameOverPanel.GetComponent<Image>();
            if (panelImage != null)
            {
                initialPanelColor = panelImage.color;
                // Ensure panel starts fully transparent.
                Color temp = initialPanelColor;
                temp.a = 0f;
                panelImage.color = temp;
            }
        }
        
        // Optionally disable enemy AI.
        EnemyAI[] enemies = FindObjectsByType<EnemyAI>(FindObjectsSortMode.InstanceID);
        foreach (EnemyAI enemy in enemies)
        {
            enemy.enabled = false;
            Debug.Log("Disabled enemy AI for " + enemy.name);
        }

        // Fade out player's sprite and fade in the game over panel.
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float t = timer / fadeDuration;
            float playerAlpha = Mathf.Lerp(initialPlayerColor.a, 0f, t);
            sr.color = new Color(initialPlayerColor.r, initialPlayerColor.g, initialPlayerColor.b, playerAlpha);
            
            if (panelImage != null)
            {
                float panelAlpha = Mathf.Lerp(0f, 1f, t);
                panelImage.color = new Color(initialPanelColor.r, initialPanelColor.g, initialPanelColor.b, panelAlpha);
            }
            yield return null;
        }
        sr.color = new Color(initialPlayerColor.r, initialPlayerColor.g, initialPlayerColor.b, 0f);
        if (panelImage != null)
            panelImage.color = new Color(initialPanelColor.r, initialPanelColor.g, initialPanelColor.b, 1f);
        
        // Activate game over UI objects via SingletonManager.
        if (SingletonManager.Instance.youDiedText != null)
            SingletonManager.Instance.youDiedText.SetActive(true);
        if (SingletonManager.Instance.restartButton != null)
            SingletonManager.Instance.restartButton.SetActive(true);
        if (SingletonManager.Instance.quitToMenuButton != null)
            SingletonManager.Instance.quitToMenuButton.SetActive(true);
        if (SingletonManager.Instance.quitButton != null)
            SingletonManager.Instance.quitButton.SetActive(true);
        
        // Fade out (or hide) the Toolbar and the ShowMainInventory button.
        if (SingletonManager.Instance.toolbar != null)
            SingletonManager.Instance.toolbar.SetActive(false);
        if (SingletonManager.Instance.invButton != null)
            SingletonManager.Instance.invButton.SetActive(false);
    }
}
