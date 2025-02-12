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

    public float dashSpeed;
    public float dashTime;

    private float lastTapW, lastTapA, lastTapS, lastTapD;
    private float tapDelay = 0.3f;
    private bool isDashing;

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
        hp = 100;
    }

    private void Update(){

        HPText.text = "Health: " + hp.ToString();

        rb.AddForce(dir * movespeed,ForceMode2D.Force);

        processInputs();
        processDash();

        if(Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)){
            transform.rotation = Quaternion.Euler(transform.rotation.x, transform.rotation.y, 0f);
        }if(Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)){
            transform.rotation = Quaternion.Euler(transform.rotation.x, transform.rotation.y, 90f);
        }if(Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)){
            transform.rotation = Quaternion.Euler(transform.rotation.x, transform.rotation.y, 180f);
        }if(Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)){
            transform.rotation = Quaternion.Euler(transform.rotation.x, transform.rotation.y, 270f);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision){
        if (collision.gameObject.tag == "Enemy"){
            hp-=10;
        }

    }

    private void Update()
    {
        ProcessInputs();
        HandleRotation();
        ManageStamina();
        UpdateUI();
    }

    void FixedUpdate(){
        if (!isDashing) Move();
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

        if (!isDashing) rb.linearVelocity = new Vector2(dir.x * movespeed,dir.y * movespeed);
    }

    void processDash(){
        if (Input.GetKeyDown(KeyCode.W)) CheckDoubleTap(ref lastTapW, Vector2.up);
        if (Input.GetKeyDown(KeyCode.A)) CheckDoubleTap(ref lastTapA, Vector2.left);
        if (Input.GetKeyDown(KeyCode.S)) CheckDoubleTap(ref lastTapS, Vector2.down);
        if (Input.GetKeyDown(KeyCode.D)) CheckDoubleTap(ref lastTapD, Vector2.right);
    }

    void CheckDoubleTap(ref float lastTapTime, Vector2 dashDir){
        if (Time.time - lastTapTime < tapDelay){
            StartCoroutine(Dash(dashDir));
        }
        lastTapTime = Time.time;
    }

    System.Collections.IEnumerator Dash(Vector2 dashDir){
        isDashing = true;
        rb.linearVelocity = dashDir * dashSpeed;
        yield return new WaitForSeconds(dashTime);
        isDashing = false;
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
