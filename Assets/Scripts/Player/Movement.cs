using UnityEngine;
using UnityEngine.UI;

public class Movement : MonoBehaviour
{
    Rigidbody2D rb;
    public float movespeed;
    Vector2 dir;
    public float hp;
    public Text HPText;

    public float dashSpeed;
    public float dashTime;
    private bool isDashing;
    private Vector2 dashDir;

    // For double-tap detection.
    private float lastTapW, lastTapA, lastTapS, lastTapD;
    private float tapDelay = 0.3f;

    // Instead of setting transform.rotation directly,
    // we use desiredRotation and update via Rigidbody2D.
    private float desiredRotation;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        hp = 100;
    }

    private void Update()
    {
        HPText.text = "Health: " + hp.ToString();

        ProcessInputs();
        ProcessDash();

        // Set the desired rotation based on key inputs.
        // (We only update if a specific key is pressed.)
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
            desiredRotation = 0f;
        else if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            desiredRotation = 90f;
        else if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
            desiredRotation = 180f;
        else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            desiredRotation = 270f;
    }

    private void FixedUpdate()
    {
        // If not dashing, move normally.
        if (!isDashing)
        {
            Vector2 newPosition = rb.position + dir * movespeed * Time.fixedDeltaTime;
            rb.MovePosition(newPosition);
        }
        // Always update rotation via Rigidbody2D.
        rb.MoveRotation(desiredRotation);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            hp -= 10;
        }
    }

    void ProcessInputs()
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");
        dir = new Vector2(moveX, moveY).normalized;
    }

    void ProcessDash()
    {
        if (Input.GetKeyDown(KeyCode.W)) CheckDoubleTap(ref lastTapW, Vector2.up);
        if (Input.GetKeyDown(KeyCode.A)) CheckDoubleTap(ref lastTapA, Vector2.left);
        if (Input.GetKeyDown(KeyCode.S)) CheckDoubleTap(ref lastTapS, Vector2.down);
        if (Input.GetKeyDown(KeyCode.D)) CheckDoubleTap(ref lastTapD, Vector2.right);
    }

    void CheckDoubleTap(ref float lastTapTime, Vector2 inputDir)
    {
        if (Time.time - lastTapTime < tapDelay)
        {
            StartCoroutine(Dash(inputDir));
        }
        lastTapTime = Time.time;
    }

    System.Collections.IEnumerator Dash(Vector2 dashDirection)
    {
        isDashing = true;
        dashDir = dashDirection;
        float timer = 0f;
        while (timer < dashTime)
        {
            Vector2 newPosition = rb.position + dashDir * dashSpeed * Time.fixedDeltaTime;
            rb.MovePosition(newPosition);
            timer += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }
        isDashing = false;
    }
}
