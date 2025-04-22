using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Movement : MonoBehaviour
{
    private Rigidbody2D rb;
    private PlayerInfo playerInfo;
    private Animator animator;  // Reference to Animator component

    [Header("Movement Speeds")]
    public float moveSpeed = 5f;
    public float sprintSpeed = 10f;
    public float dashSpeed;
    public float dashTime;

    [Header("Stamina")]
    public float maxStamina = 100f;
    public float stamina = 100f;
    public float staminaDrain = 10f;
    public float staminaRegen = 5f;
    public float staminaRecoveryDelay = 1f;

    private float timeSinceStoppedSprinting;
    private bool isSprinting;

    [Header("UI References")]
    public Slider staminaBar;
    public Slider HPBar;

    // Double-tap detection for dash
    private float lastTapW, lastTapA, lastTapS, lastTapD;
    private float tapDelay = 0.3f;
    private bool isDashing;

    // Movement
    private Vector2 dir;
    // To remember the last horizontal direction (true means right, false means left)
    private bool lastFacingRight = true;
    private Vector2 knockbackForceVector;

    // NEW: Tracking still-running state.
    private bool wasRunning = false;
    private bool stillRunning = false;

    public bool IsSprinting {
        get { return isSprinting; }
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        playerInfo = GetComponent<PlayerInfo>();
        animator = GetComponent<Animator>();  // Get Animator component
        Debug.Log("Start: Components initialized. Animator: " + (animator != null));
        Debug.Log("Start: Initial stamina: " + stamina + ", Health: " + (playerInfo != null ? playerInfo.currentHealth.ToString() : "No player info"));


        // Initialize stamina UI
        if (staminaBar != null)
        {
            staminaBar.maxValue = maxStamina;
            staminaBar.value = stamina;
        }

        // Initialize HP UI from PlayerInfo
        if (playerInfo != null && HPBar != null)
        {
            HPBar.maxValue = playerInfo.maxHealth;
            HPBar.value = playerInfo.currentHealth;
        }
        UpdateUI();
    }

    void Update()
    {
        ProcessInputs();
        ProcessDash();
        ManageStamina();
        UpdateUI();
        // UpdateAnimator();
    }

    void FixedUpdate()
    {
        if (!isDashing)
        {
            float actualSpeed = isSprinting ? sprintSpeed : moveSpeed;
            Vector2 newPos = rb.position + dir * actualSpeed * Time.fixedDeltaTime;

            // Apply knockback force if any
            if (knockbackForceVector != Vector2.zero)
            {
                newPos += knockbackForceVector * Time.fixedDeltaTime;
                knockbackForceVector = Vector2.Lerp(knockbackForceVector, Vector2.zero, 0.5f);
            }

            rb.MovePosition(newPos);
        }
    }

    void ProcessInputs()
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");
        dir = new Vector2(moveX, moveY).normalized;

        isSprinting = Input.GetKey(KeyCode.LeftShift) && stamina > 0f && dir != Vector2.zero;
        if (isSprinting)
        {
            DrainStamina();
            timeSinceStoppedSprinting = 0f;
        }
        else
        {
            timeSinceStoppedSprinting += Time.deltaTime;
        }
    }

    // Update the Animator parameters including stillRunning.
    void UpdateAnimator()
    {
        bool isMoving = dir != Vector2.zero;
        Debug.Log("Is Moving: " + isMoving);
        animator.SetBool("isMoving", isMoving);

        // Set stillRunning to true if the player is moving now and was already moving last frame.
        if (isMoving && wasRunning)
            stillRunning = true;
        else
            stillRunning = false;
        Debug.Log("Is running: " + stillRunning);
        animator.SetBool("stillRunning", stillRunning);

        // Update the facing direction.
        if (dir.x > 0)
        {
            Debug.Log("Facing Right: " + true);
            animator.SetBool("FacingRight", true);
            lastFacingRight = true;
        }
        else if (dir.x < 0)
        {
            Debug.Log("Facing Left: " + !false);
            animator.SetBool("FacingRight", false);
            lastFacingRight = false;
        }
        else
        {
            // If no horizontal input, keep previous facing.
            Debug.Log("Still facing right: " + lastFacingRight);
            animator.SetBool("FacingRight", lastFacingRight);
        }

        // Save current moving state for next frame comparison.
        wasRunning = isMoving;
    }

    private void ProcessDash()
    {
        if (Input.GetKeyDown(KeyCode.W)) CheckDoubleTap(ref lastTapW, Vector2.up);
        if (Input.GetKeyDown(KeyCode.A)) CheckDoubleTap(ref lastTapA, Vector2.left);
        if (Input.GetKeyDown(KeyCode.S)) CheckDoubleTap(ref lastTapS, Vector2.down);
        if (Input.GetKeyDown(KeyCode.D)) CheckDoubleTap(ref lastTapD, Vector2.right);
    }

    private void CheckDoubleTap(ref float lastTapTime, Vector2 dashDir)
    {
        if (Time.time - lastTapTime < tapDelay)
        {
            StartCoroutine(Dash(dashDir));
        }
        lastTapTime = Time.time;
    }

    private IEnumerator Dash(Vector2 dashDir)
    {
        isDashing = true;
        rb.linearVelocity = dashDir * dashSpeed;
        yield return new WaitForSeconds(dashTime);
        isDashing = false;
        rb.linearVelocity = Vector2.zero;
    }

    private void DrainStamina()
    {
        stamina -= staminaDrain * Time.deltaTime;
        stamina = Mathf.Clamp(stamina, 0f, maxStamina);
    }

    private void ManageStamina()
    {
        if (!isSprinting && stamina < maxStamina && timeSinceStoppedSprinting >= staminaRecoveryDelay)
        {
            stamina += staminaRegen * Time.deltaTime;
            stamina = Mathf.Clamp(stamina, 0f, maxStamina);
        }
    }

    private void UpdateUI()
    {
        if (HPBar != null && playerInfo != null)
        {
            HPBar.value = playerInfo.currentHealth;
            Image fill = HPBar.fillRect.GetComponent<Image>();
            if (playerInfo.currentHealth < playerInfo.maxHealth * 0.3f)
                fill.color = Color.red;
            else if (playerInfo.currentHealth < playerInfo.maxHealth * 0.6f)
                fill.color = Color.yellow;
            else
                fill.color = Color.green;
        }

        if (staminaBar != null)
            staminaBar.value = stamina;
    }

    public void SetKnockbackForceVector(Vector2 v)
    {
        knockbackForceVector = v;
    }
    
    public Vector2 GetMovementDirection()
    {
        return dir;
    }
}
