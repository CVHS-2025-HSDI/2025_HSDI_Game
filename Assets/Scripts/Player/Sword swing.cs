using UnityEngine;

public class Swordswing : MonoBehaviour
{
     public float baseDamage = 10f;
    public string enemyTag = "Enemy";
    public SwordController swordOwner;

    void Start()
    {
        Destroy(gameObject, 0.5f); 
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(enemyTag) && other.TryGetComponent<EnemyAI>(out EnemyAI enemyScript))
        {
            float finalDamage = baseDamage * PlayerStats.GetDamageMultiplier();
            enemyScript.damage(finalDamage);
            swordOwner.DecreaseDurability();
        }

        Destroy(gameObject, 0.2f);
    }
}
