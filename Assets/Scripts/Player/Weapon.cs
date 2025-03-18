using UnityEngine;

public class Weapon : MonoBehaviour
{
    public Transform attackPoint; // Position where the attack originates
    public float attackRange = 1f;
    public int attackDamage = 10;
    public string enemyTag = "Enemy"; // Set this in the Inspector or hardcode it

    public float attackCooldown = 0.5f;
    private bool canAttack = true;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0) && canAttack) // Change input as needed
        {
            Attack();
        }
    }

    void Attack()
    {
        canAttack = false;
        Invoke(nameof(ResetAttack), attackCooldown);

        //Animation stuff 
        //     ||
        //     VV

        //Animator animator = GetComponent<Animator>();
        // if (animator != null)
        // {
        //     animator.SetTrigger("Attack");
        // }

       

        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(attackPoint.position, attackRange);

        foreach (Collider2D collider in hitColliders)
        {
            if (collider.CompareTag(enemyTag)) // Check if the object has the correct tag
            {
                if (collider.TryGetComponent<EnemyAI>(out EnemyAI enemyScript)) 
                {
                    enemyScript.damage(attackDamage); // Calls TakeDamage() from EnemyAI
                }
            }
        }
    }

    void ResetAttack()
    {
        canAttack = true;
    }


}
