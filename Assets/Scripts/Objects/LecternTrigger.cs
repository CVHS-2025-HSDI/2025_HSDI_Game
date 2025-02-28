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
    private Button closeButton; // The 'Close' button under lorePanel

    // Fade settings.
    private float fadeDuration = 0.25f;
    private float delayBetween = 0.25f;

    // Controls whether the lectern has been triggered.
    private bool triggered = false;

    private void Start()
    {
        if (LorePanelController.Instance != null)
        {
            loreTitleText = LorePanelController.Instance.loreTitleText;
            loreBodyText = LorePanelController.Instance.loreBodyText;
            // Assuming the parent of the title text is the panel.
            lorePanel = LorePanelController.Instance.loreTitleText.transform.parent.gameObject;
            panelImage = lorePanel.GetComponent<Image>();

            // Find the "Close" button within lorePanel’s children.
            closeButton = lorePanel.GetComponentInChildren<Button>(true);
            if (closeButton != null)
            {
                // Subscribe to its onClick event.
                closeButton.onClick.AddListener(OnCloseButtonClicked);
            }
            else
            {
                Debug.LogWarning("No Close button found as a child of the LorePanel!");
            }
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

            // Trigger the lore sequence.
            StartCoroutine(FadeInLoreSequence(title, body));
            triggered = true;
        }
    }

    // Option 1: Reset the trigger when the player exits the lectern area.
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            triggered = false;
        }
    }

    private IEnumerator FadeInLoreSequence(string title, string body)
    {
        if (lorePanel == null || loreTitleText == null || loreBodyText == null)
            yield break;

        // Ensure the panel is active.
        lorePanel.SetActive(true);
        lorePanel.transform.SetAsLastSibling();

        // Reactivate the close button (in case it was deactivated).
        if (closeButton != null)
            closeButton.gameObject.SetActive(true);

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
            titleColor.a = alpha;
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

    /// <summary>
    /// Called when the 'Close' button is clicked.
    /// Instantly resets the lorePanel transparency and deactivates the button.
    /// Optionally, you can also hide the lorePanel.
    /// </summary>
    private void OnCloseButtonClicked()
    {
        // Instantly reset the panel alpha.
        if (panelImage != null)
        {
            Color pc = panelImage.color;
            pc.a = 0f;
            panelImage.color = pc;
        }

        // Instantly reset the text alphas.
        if (loreTitleText != null)
        {
            Color tc = loreTitleText.color;
            tc.a = 0f;
            loreTitleText.color = tc;
        }
        if (loreBodyText != null)
        {
            Color bc = loreBodyText.color;
            bc.a = 0f;
            loreBodyText.color = bc;
        }

        // Hide the entire panel.
        lorePanel.SetActive(false);

        // Deactivate the close button so it won't be clicked again.
        if (closeButton != null)
        {
            closeButton.gameObject.SetActive(false);
        }

        // Reset the triggered flag so the lectern can be used again even if the player doesn't leave the trigger area.
        triggered = false;
    }
}
