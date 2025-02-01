using UnityEngine;

public class Movement : MonoBehaviour
{
    Rigidbody2D rb;
    public float movespeed;
    Vector2 dir;
    private void Start(){
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update(){
        rb.AddForce(dir * movespeed,ForceMode2D.Force);
        processInputs();

        if(Input.GetKey(KeyCode.W)){
            transform.rotation = Quaternion.Euler(transform.rotation.x, transform.rotation.y, 0f);
        }if(Input.GetKey(KeyCode.A)){
            transform.rotation = Quaternion.Euler(transform.rotation.x, transform.rotation.y, 90f);
        }if(Input.GetKey(KeyCode.S)){
            transform.rotation = Quaternion.Euler(transform.rotation.x, transform.rotation.y, 180f);
        }if(Input.GetKey(KeyCode.D)){
            transform.rotation = Quaternion.Euler(transform.rotation.x, transform.rotation.y, 270f);
        }

    }

    void FixedUpdate(){
        Move();
    }

    void processInputs(){
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");

        dir = new Vector2(moveX,moveY).normalized;
    }

    void Move(){
        rb.linearVelocity = new Vector2(dir.x * movespeed,dir.y * movespeed);
    }
}
