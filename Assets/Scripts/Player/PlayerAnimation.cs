using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    public Sprite idleLeft;
    public Sprite idleRight;
    public Sprite[] runLeftSprites;  // 12 sprites for left running
    public Sprite[] runRightSprites; // 12 sprites for right running

    // Frame rate settings:
    public float normalFrameRate = 0.05f;   // Time per frame when not sprinting
    public float sprintFrameRate = 0.032f;  // Time per frame when sprinting

    private SpriteRenderer sr;
    private float timer;
    private int currentFrame;
    private bool isRunning;
    private bool facingRight = true; // Default facing direction

    // Reference to Movement to determine sprinting state.
    private Movement movement;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        // Assumes Movement is on the same GameObject.
        movement = GetComponent<Movement>();
    }

    void Update()
    {
        // Check for horizontal input.
        float move = Input.GetAxisRaw("Horizontal");
        isRunning = Mathf.Abs(move) > 0.1f;

        // Update direction based on input.
        if (move < 0)
            facingRight = false;
        else if (move > 0)
            facingRight = true;

        // Determine effective frame rate based on sprinting.
        float effectiveFrameRate = normalFrameRate;
        if (movement != null && movement.IsSprinting)
            effectiveFrameRate = sprintFrameRate;

        // Update running animation frames.
        if (isRunning)
        {
            timer += Time.deltaTime;
            if (timer >= effectiveFrameRate)
            {
                timer = 0f;
                currentFrame = (currentFrame + 1) % 12; // assuming 12 frames per cycle

                if (facingRight)
                    sr.sprite = runRightSprites[currentFrame];
                else
                    sr.sprite = runLeftSprites[currentFrame];
            }
        }
        else // Idle state.
        {
            currentFrame = 0;
            timer = 0f;
            sr.sprite = facingRight ? idleRight : idleLeft;
        }
    }
}
