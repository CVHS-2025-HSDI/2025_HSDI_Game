using UnityEngine;
using UnityEngine.UI;   

public class Movement : MonoBehaviour
{
    Rigidbody2D rb;
    public float movespeed;
    Vector2 dir;
    public float hp;
    public Text HPText;



    private void Start(){
        rb = GetComponent<Rigidbody2D>();
        hp = 100;
      
    }

    private void Update(){

        HPText.text = "Health: " + hp.ToString();

        rb.AddForce(dir * movespeed,ForceMode2D.Force);

        processInputs();

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
