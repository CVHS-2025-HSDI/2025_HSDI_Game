using UnityEngine;
using UnityEngine.UI;

public class Movement : MonoBehaviour
{
    private Rigidbody2D rb;
    public float moveSpeed = 5f;
    public float sprintSpeed = 8f;

    public float maxStamina = 100f;
    public float stamina = 100f;
    public float staminaDrain = 10f;
    public float staminaRegen = 5f;
    public float staminaRecoveryDelay = 1f;

    private float timeSinceStoppedSprinting;
    private bool isSprinting;

    private Vector2 dir;
    public float hp = 100f;
    public Text HPText;
    public Slider staminaBar;
    public Slider HPBar;


    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        UpdateUI();

        if (staminaBar != null){
        staminaBar.maxValue = maxStamina;
        staminaBar.value = stamina;
        }

        if (HPBar != null){
        HPBar.maxValue = 100;
        HPBar.value = stamina;
        }

    }

    private void Update()
    {
        ProcessInputs();
        HandleRotation();
        ManageStamina();
        UpdateUI();
    }

    private void FixedUpdate()
    {
        Move();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            hp -= 10;
            UpdateUI();
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

    private void Move()
    {
        if (!isSprinting && stamina < maxStamina && timeSinceStoppedSprinting >= staminaRecoveryDelay){
            ManageStamina();
        }
        else{
            timeSinceStoppedSprinting += Time.deltaTime;
        }

        rb.linearVelocity = dir * moveSpeed;
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

    void HandleRotation()
    {
        if (dir != Vector2.zero)
        {
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle - 90);
        }
    }

    void UpdateUI()
    {
        if (HPBar != null)
        {
            HPBar.value = hp;
        }

        Image fill = HPBar.fillRect.GetComponent<Image>();
        if (hp < 100 * 0.3f)
            fill.color = Color.red; // Low stamina
        else if (hp < 100 * 0.6f)
            fill.color = Color.yellow; // Medium stamina
        else
            fill.color = Color.green; // Healthy stamina
        
        if (staminaBar != null){
            staminaBar.value = stamina; 
        }
    }

}
