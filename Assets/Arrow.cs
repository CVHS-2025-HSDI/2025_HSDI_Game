using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Arrow : MonoBehaviour {
    public float speed = 15f;
    public float lifetime = 5f;
    public float damage = 10f;

    private Rigidbody2D rb;

    void Awake() {
        rb = GetComponent<Rigidbody2D>();
        
    }

   public void Fire(Vector2 direction) {
    rb.linearVelocity = direction * speed;
    Destroy(gameObject, lifetime);
}


    void OnTriggerEnter2D(Collider2D other) {
        if (other.CompareTag("Enemy")) {
            if (other.TryGetComponent(out EnemyAI enemy)) {
                enemy.damage(damage);
            }
            Destroy(gameObject);
        }
        // else if (!other.CompareTag("Player") && !other.CompareTag("Arrow")) {
        //     Destroy(gameObject);
        // }
    }
}
