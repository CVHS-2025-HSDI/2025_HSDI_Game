using UnityEngine;
using System.Collections;

public class ScrollingCredits : MonoBehaviour
{
    // Speed at which the credits scroll
    public float scrollSpeed = 50f;
    // The RectTransform of the credits text
    public RectTransform creditsContent;
    // The parent GameObject of the buttons that should hide/show
    public GameObject buttonsParent;
    // The target Y position when the credits have fully scrolled out
    public float scrollEndY = 1000f;
    
    // Flag to indicate whether the credits are scrolling
    private bool isScrolling = false;

    // Call this method from your button's OnClick event
    public void StartCredits()
    {
        // Hide the buttons
        if (buttonsParent != null)
            buttonsParent.SetActive(false);

        // Optionally reset the credits position to the starting position
        creditsContent.anchoredPosition = new Vector2(creditsContent.anchoredPosition.x, 0);

        // Begin scrolling
        isScrolling = true;
    }

    void Update()
    {
        if (isScrolling)
        {
            // Move the credits upward each frame
            creditsContent.anchoredPosition += new Vector2(0, scrollSpeed * Time.deltaTime);

            // Check if the credits have scrolled past the threshold
            if (creditsContent.anchoredPosition.y >= scrollEndY)
            {
                // Stop scrolling
                isScrolling = false;

                // Optionally, reset credits position if you want to scroll again later
                creditsContent.anchoredPosition = new Vector2(creditsContent.anchoredPosition.x, 0);

                // Show the buttons again
                if (buttonsParent != null)
                    buttonsParent.SetActive(true);
            }
        }
    }
}