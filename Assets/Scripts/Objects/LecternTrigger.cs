using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class LecternTrigger : MonoBehaviour
{
    [Header("Floor Info")]
    [Tooltip("Set the current floor number for this lectern.")]
    public int currentFloor;

    // These references are obtained via the singleton.
    private GameObject lorePanel;
    private TextMeshProUGUI loreTitleText;
    private TextMeshProUGUI loreBodyText;
    private Image panelImage; // For fading the panel background

    // Fade settings.
    private float fadeDuration = 0.25f;
    private float delayBetween = 0.25f;

    private bool triggered = false;

    private void Start()
    {
        if (LorePanelController.Instance != null)
        {
            lorePanel = LorePanelController.Instance.gameObject;
            loreTitleText = LorePanelController.Instance.loreTitleText;
            loreBodyText = LorePanelController.Instance.loreBodyText;
            panelImage = lorePanel.GetComponent<Image>();
        }
        else
        {
            Debug.LogWarning("LorePanelController instance not found. Make sure your LorePanel is in the scene and has the LorePanelController script attached.");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (triggered)
            return;

        if (other.CompareTag("Player"))
        {
            string fullLore = LecternManager.Instance.GetLoreForFloor(currentFloor);
            if (string.IsNullOrEmpty(fullLore))
            {
                Debug.LogWarning("No lore found for floor " + currentFloor);
                return;
            }

            // Split lore into title and body (assuming a newline separator).
            string[] parts = fullLore.Split('\n');
            string title = parts.Length > 0 ? parts[0] : "";
            string body = parts.Length > 1 ? fullLore.Substring(fullLore.IndexOf('\n') + 1) : "";

            StartCoroutine(FadeInLoreSequence(title, body));
            triggered = true;
        }
    }

    private IEnumerator FadeInLoreSequence(string title, string body)
    {
        if (lorePanel == null || loreTitleText == null || loreBodyText == null)
            yield break;

        // Ensure the panel is active.
        lorePanel.SetActive(true);

        // Fade in the panel's image from 0 to 168/255.
        if (panelImage != null)
        {
            Color panelColor = panelImage.color;
            panelColor.a = 0f;
            panelImage.color = panelColor;
            float timer = 0f;
            while (timer < fadeDuration)
            {
                timer += Time.deltaTime;
                float alpha = Mathf.Clamp01(timer / fadeDuration) * (168f / 255f);
                panelColor.a = alpha;
                panelImage.color = panelColor;
                yield return null;
            }
            panelImage.color = new Color(panelColor.r, panelColor.g, panelColor.b, 168f / 255f);
        }

        // Reset text alpha values.
        Color titleColor = loreTitleText.color;
        titleColor.a = 0f;
        loreTitleText.color = titleColor;

        Color bodyColor = loreBodyText.color;
        bodyColor.a = 0f;
        loreBodyText.color = bodyColor;

        // Set text values.
        loreTitleText.text = title;
        loreBodyText.text = body;

        // Fade in the title.
        float textTimer = 0f;
        while (textTimer < fadeDuration)
        {
            textTimer += Time.deltaTime;
            float alpha = Mathf.Clamp01(textTimer / fadeDuration);
            titleColor.a = alpha; // Target for text is 1 (fully opaque)
            loreTitleText.color = titleColor;
            yield return null;
        }
        loreTitleText.color = new Color(titleColor.r, titleColor.g, titleColor.b, 1f);

        // Wait for the delay between title and body.
        yield return new WaitForSeconds(delayBetween);

        // Fade in the body.
        textTimer = 0f;
        while (textTimer < fadeDuration)
        {
            textTimer += Time.deltaTime;
            float alpha = Mathf.Clamp01(textTimer / fadeDuration);
            bodyColor.a = alpha;
            loreBodyText.color = bodyColor;
            yield return null;
        }
        loreBodyText.color = new Color(bodyColor.r, bodyColor.g, bodyColor.b, 1f);
    }
}
