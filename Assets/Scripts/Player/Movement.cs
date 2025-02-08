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

    private float lastTapW, lastTapA, lastTapS, lastTapD;
    private float tapDelay = 0.3f;
    private bool isDashing;


    private void Start(){
        rb = GetComponent<Rigidbody2D>();
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

    void FixedUpdate(){
        if (!isDashing) Move();
    }

    void processInputs(){
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");

        dir = new Vector2(moveX,moveY).normalized;
    }

    void Move(){
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
        rb.velocity = dashDir * dashSpeed;
        yield return new WaitForSeconds(dashTime);
        isDashing = false;
    }
}
