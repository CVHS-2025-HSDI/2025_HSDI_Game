using UnityEngine;

public class Swordswing : MonoBehaviour
{
    public int attackDamage = 10;
    public string enemyTag = "Enemy";

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(enemyTag) && other.TryGetComponent<EnemyAI>(out EnemyAI enemyScript))
        {
            enemyScript.damage(attackDamage);
        }
        Destroy(gameObject,0.2f);
    }
}
