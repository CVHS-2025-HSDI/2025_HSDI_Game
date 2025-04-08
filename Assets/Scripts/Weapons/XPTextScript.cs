using UnityEngine;
using TMPro;

public class XPTextScript : MonoBehaviour
{
    private float timer = 0f;
    private Transform target;
    private float moveSpeedIncrease = 0.4f;
    private TMP_Text xpText;

    void Awake()
    {
        xpText = GetComponent<TMP_Text>();
    }

    void Update()
    {
        // Destroy after a short duration
        if (timer > 0.3f)
        {
            Destroy(gameObject);
            return;
        }
        
        // If a target is provided, track its position then add upward movement
        if (target != null)
        {
            // Position above the target with a base offset
            transform.position = new Vector2(target.position.x, target.position.y + 0.8f);
            // Move upward gradually
            transform.position = new Vector2(transform.position.x, transform.position.y + moveSpeedIncrease * Time.deltaTime);
            moveSpeedIncrease += 0.4f;
        }
        else
        {
            // Continue moving upward if target is gone
            transform.position = new Vector2(transform.position.x, transform.position.y + 0.4f * Time.deltaTime);
        }
        
        // Fade out effect: gradually decrease text alpha over time
        if (xpText != null)
        {
            Color currentColor = xpText.color;
            currentColor.a = Mathf.Lerp(1f, 0f, timer / 0.3f);
            xpText.color = currentColor;
        }
        
        timer += Time.deltaTime;
    }

    // Set a target so the text can track its position (if needed)
    public void SetTarget(Transform t)
    {
        target = t;
    }

    // Call this method when creating the XP text to display the XP gained.
    public void Setup(int xpAmount)
    {
        if (xpText == null)
            xpText = GetComponent<TMP_Text>();
        xpText.text = xpAmount.ToString();
    }
}