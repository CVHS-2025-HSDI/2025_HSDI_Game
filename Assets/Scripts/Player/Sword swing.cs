using UnityEngine;

public class Swordswing : MonoBehaviour
{  
    public float attackRange = 1.5f;
    public int attackDamage = 10;
    public string enemyTag = "Enemy";

    void Start()
    {
        // Check for enemies in range on creation
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(transform.position, attackRange);

        foreach (Collider2D enemy in hitEnemies)
        {
            if (enemy.CompareTag(enemyTag))
            {
                if (enemy.TryGetComponent<EnemyAI>(out EnemyAI enemyScript))
                {
                    enemyScript.damage(attackDamage);
                }
            }
        }

        Destroy(gameObject, 0.2f); // Destroy swing effect after short time
    }

    // Visualize attack range in the editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
