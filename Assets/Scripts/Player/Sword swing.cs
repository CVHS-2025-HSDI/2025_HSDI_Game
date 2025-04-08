using UnityEngine;

public class Swordswing : MonoBehaviour
{
    public int baseDamage = 10;
    public string enemyTag = "Enemy";

    // If there's no collision, self-destruct after X seconds
    void Start()
    {
        Destroy(gameObject, 0.5f); 
        // Adjust the time as needed (0.5, 1.0, etc.)
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(enemyTag) && other.TryGetComponent<EnemyAI>(out EnemyAI enemyScript))
        {
            float finalDamage = baseDamage * PlayerStats.GetDamageMultiplier();
            enemyScript.damage(finalDamage);
        }
        // Destroy the effect shortly after collision
        Destroy(gameObject, 0.2f);
    }
}
