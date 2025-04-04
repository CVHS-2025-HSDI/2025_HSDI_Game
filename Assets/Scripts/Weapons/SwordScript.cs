using System;
using UnityEngine;

public class SwordScript : MonoBehaviour
{
    private GameObject target;
    public GameObject projectilePrefab;      // Assign in Inspector
    public GameObject AttackRadiusPrefab;      // For enemy (assign if needed)
    
    private float attackRate;
    private float timer = 0f;
    private bool isPlayer;
    private Vector3 projPos;
    private Quaternion projRot;
    private int durability; // Should be randomly generated when picked up then set via SetDurability

    void Start()
    {
        if (isPlayer)
        {
            attackRate = 0.4f;
            durability = 100; // temporary value
        }
        else
        {
            attackRate = 1f;
            // For enemy, instantiate the attack radius if desired.
            if (AttackRadiusPrefab != null)
            {
                Instantiate(AttackRadiusPrefab, transform.position, Quaternion.identity)
                    .transform.SetParent(transform);
            }
        }
        timer = attackRate;
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (isPlayer)
        {
            // On left mouse click and if attack timer is ready, attack.
            if (Input.GetMouseButtonDown(0) && timer >= attackRate)
            {
                Attack();
                timer = 0f;
            }
        }
        
        // Ensure the sword remains unrotated relative to the world.
        transform.rotation = Quaternion.identity;
    }

    public void Attack()
    {
        if (isPlayer)
        {
            // Get the player (assumed to be the parent) and its Movement component.
            GameObject player = transform.parent.gameObject;
            Movement m = player.GetComponent<Movement>();

            // Use a new helper method (added to Movement) to get the current movement direction.
            Vector2 attackDir = m.GetMovementDirection();
            if (attackDir == Vector2.zero)
            {
                // If there's no input, default to the player's facing direction based on scale.
                attackDir = (player.transform.localScale.x >= 0) ? Vector2.right : Vector2.left;
            }
            attackDir.Normalize();
            // Compute the angle in degrees from the attack direction.
            float angle = Mathf.Atan2(attackDir.y, attackDir.x) * Mathf.Rad2Deg;
            float offsetDistance = 1.5f;
            // Position the projectile offset from the sword in the attack direction.
            projPos = transform.position + new Vector3(attackDir.x, attackDir.y, 0) * offsetDistance;
            projRot = Quaternion.Euler(new Vector3(0, 0, angle));

            SwordProjectileScript sps = Instantiate(projectilePrefab, projPos, projRot)
                .GetComponent<SwordProjectileScript>();
            sps.transform.SetParent(transform);
            sps.setIsPlayer(isPlayer);

            // Reduce durability and destroy if depleted.
            durability--;
            if (durability <= 0)
            {
                // Optionally, notify InventoryManager to remove this weapon.
                Destroy(gameObject);
            }
        }
        else
        {
            // Enemy attack: Determine the direction toward the target.
            if (target == null) return;
            Vector3 diff = target.transform.position - transform.position;
            Vector2 enemyAttackDir = new Vector2(diff.x, diff.y).normalized;
            float angle = Mathf.Atan2(enemyAttackDir.y, enemyAttackDir.x) * Mathf.Rad2Deg;
            float offsetDistance = 1.5f;
            projPos = transform.position + new Vector3(enemyAttackDir.x, enemyAttackDir.y, 0) * offsetDistance;
            projRot = Quaternion.Euler(new Vector3(0, 0, angle - 135));

            SwordProjectileScript sps = Instantiate(projectilePrefab, projPos, projRot)
                .GetComponent<SwordProjectileScript>();
            sps.transform.SetParent(transform);
            sps.setIsPlayer(isPlayer);
        }
    }

    public SwordScript SetTarget(GameObject target)
    {
        this.target = target;
        return this;
    }

    public GameObject GetTarget()
    {
        return target;
    }

    public void SetIsPlayer(bool isPlayer)
    {
        this.isPlayer = isPlayer;
    }
    
    public float GetAttackRate()
    {
        return attackRate;
    }

    public void SetDurability(int dura)
    {
        durability = dura;
    }
}
