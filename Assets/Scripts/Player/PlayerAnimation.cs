using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    public Sprite idleLeft;
    public Sprite idleRight;
    public Sprite[] runLeftSprites;  // 12 sprites for left running
    public Sprite[] runRightSprites; // 12 sprites for right running
    public float frameRate = 0.1f;   // Time per frame

    private SpriteRenderer sr;
    private float timer;
    private int currentFrame;
    private bool isRunning;
    private bool facingRight = true; // Default facing direction

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        // Check for movement input
        float move = Input.GetAxisRaw("Horizontal");
        isRunning = Mathf.Abs(move) > 0.1f;

        // Update direction based on input
        if (move < 0)
            facingRight = false;
        else if (move > 0)
            facingRight = true;

        // If running, update running animation frames
        if (isRunning)
        {
            timer += Time.deltaTime;
            if (timer >= frameRate)
            {
                timer = 0f;
                currentFrame = (currentFrame + 1) % 12; // assuming 12 frames per cycle

                if (facingRight)
                    sr.sprite = runRightSprites[currentFrame];
                else
                    sr.sprite = runLeftSprites[currentFrame];
            }
        }
        else // Idle state
        {
            currentFrame = 0;
            timer = 0f;
            sr.sprite = facingRight ? idleRight : idleLeft;
        }
    }
}