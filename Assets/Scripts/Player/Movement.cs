using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Movement : MonoBehaviour
{
    private Rigidbody2D rb;

    [Header("Movement Speeds")]
    public float moveSpeed = 5f;     // Normal walk speed
    public float sprintSpeed = 10f;  // Speed while sprinting
    public float dashSpeed;          // Speed during dash
    public float dashTime;           // Duration of dash

    [Header("Stamina")]
    public float maxStamina = 100f;
    public float stamina = 100f;
    public float staminaDrain = 10f;
    public float staminaRegen = 5f;
    public float staminaRecoveryDelay = 1f;
    private float timeSinceStoppedSprinting;
    private bool isSprinting;

    [Header("Health")]
    public float hp = 100f;

    [Header("UI References")]
    public Slider staminaBar;
    public Slider HPBar;

    // Double-tap detection for dash
    private float lastTapW, lastTapA, lastTapS, lastTapD;
    private float tapDelay = 0.3f;
    private bool isDashing;

    // Movement & rotation
    private Vector2 dir;            // Current movement direction
    private float desiredRotation;  // We'll rotate via rb.MoveRotation()

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // Initialize UI
        if (staminaBar != null)
        {
            staminaBar.maxValue = maxStamina;
            staminaBar.value = stamina;
        }

        if (HPBar != null)
        {
            HPBar.maxValue = 100f; // If your HP can exceed 100, adjust this
            HPBar.value = hp;
        }

        UpdateUI();
    }

    void Update()
    {
        ProcessInputs();
        ProcessDash();
        HandleRotation();    // Calculate desiredRotation
        ManageStamina();     // Drain or recover stamina
        UpdateUI();          // Update sliders/colors
    }

    void FixedUpdate()
    {
        // If not dashing, do normal movement
        if (!isDashing)
        {
            float actualSpeed = isSprinting ? sprintSpeed : moveSpeed;
            Vector2 newPosition = rb.position + dir * actualSpeed * Time.fixedDeltaTime;
            rb.MovePosition(newPosition);
        }

        // Always update rotation
        rb.MoveRotation(desiredRotation);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            hp -= 10;
            UpdateUI();
        }
    }

    /// <summary>
    /// Reads input axes, determines sprinting, etc.
    /// </summary>
    private void ProcessInputs()
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");
        dir = new Vector2(moveX, moveY).normalized;

        // Check if sprinting (shift held, some stamina left, and actually moving)
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

    /// <summary>
    /// Handles dash input (double-tap detection).
    /// </summary>
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
        rb.linearVelocity = dashDir * dashSpeed;  // or use rb.MovePosition in a loop

        yield return new WaitForSeconds(dashTime);

        isDashing = false;
        rb.linearVelocity = Vector2.zero; // or return to normal movement
    }

    /// <summary>
    /// Drains stamina while sprinting.
    /// </summary>
    private void DrainStamina()
    {
        stamina -= staminaDrain * Time.deltaTime;
        stamina = Mathf.Clamp(stamina, 0f, maxStamina);
    }

    /// <summary>
    /// Manages stamina recovery when not sprinting.
    /// </summary>
    private void ManageStamina()
    {
        // If we aren't sprinting and the user has waited long enough,
        // regenerate stamina
        if (!isSprinting && stamina < maxStamina && timeSinceStoppedSprinting >= staminaRecoveryDelay)
        {
            stamina += staminaRegen * Time.deltaTime;
            stamina = Mathf.Clamp(stamina, 0f, maxStamina);
        }
    }

    /// <summary>
    /// Updates desiredRotation based on current movement direction.
    /// We'll apply it in FixedUpdate() via rb.MoveRotation.
    /// </summary>
    private void HandleRotation()
    {
        if (dir != Vector2.zero)
        {
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            desiredRotation = angle - 90f;
        }
    }

    /// <summary>
    /// Updates the HP and Stamina sliders, plus the color of the HP bar.
    /// </summary>
    private void UpdateUI()
    {
        // HP slider
        if (HPBar != null)
        {
            HPBar.value = hp;
            // Adjust color based on HP
            Image fill = HPBar.fillRect.GetComponent<Image>();
            if (hp < 100f * 0.3f)
                fill.color = Color.red;    // Low HP
            else if (hp < 100f * 0.6f)
                fill.color = Color.yellow; // Medium HP
            else
                fill.color = Color.green;  // High HP
        }

        // Stamina slider
        if (staminaBar != null)
        {
            staminaBar.value = stamina;
        }
    }
}
