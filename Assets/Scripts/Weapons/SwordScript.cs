using UnityEngine;

public class SwordScript : MonoBehaviour
{
    private GameObject target;
    public GameObject projectilePrefab;      // Assign in Inspector
    public GameObject AttackRadiusPrefab;      // For enemy (assign if needed)
    
    private float attackRate;
    private float timer = 0f;
    private bool isPlayer;

    void Start()
    {
        if (isPlayer)
        {
            attackRate = 0.4f;
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
        else
        {
            // For enemy, you might trigger attacks via AI logic.
            // For demonstration, we auto-attack when the timer is ready.
            if (target != null && timer >= attackRate)
            {
                Attack();
                timer = 0f;
            }
        }
    }

    public void Attack()
    {
        if (isPlayer)
        {
            // Use the parent's up vector as the facing direction.
            Vector3 direction = transform.parent.up;
            // Calculate projectile spawn position offset from the sword.
            Vector3 projPos = transform.position + direction * 1.5f;
            // Create a rotation that makes the projectile point in that direction.
            Quaternion projRot = Quaternion.LookRotation(Vector3.forward, direction);
            SwordProjectileScript sps = Instantiate(projectilePrefab, projPos, projRot)
                .GetComponent<SwordProjectileScript>();
            sps.gameObject.transform.SetParent(transform);
            sps.setIsPlayer(isPlayer);
        }
        else
        {
            if (target == null) return;
            // For enemy attacks, determine the direction towards the target.
            Vector3 diff = target.transform.position - transform.position;
            diff.Normalize();
            Vector3 projPos = transform.position + diff * 1.5f;
            Quaternion projRot = Quaternion.LookRotation(Vector3.forward, diff);
            SwordProjectileScript sps = Instantiate(projectilePrefab, projPos, projRot)
                .GetComponent<SwordProjectileScript>();
            sps.gameObject.transform.SetParent(transform);
            sps.setIsPlayer(isPlayer);
        }
    }

    public SwordScript setTarget(GameObject target)
    {
        this.target = target;
        return this;
    }

    public GameObject getTarget()
    {
        return target;
    }

    public void setIsPlayer(bool isPlayer)
    {
        this.isPlayer = isPlayer;
    }
    
    public float GetAttackRate()
    {
        return attackRate;
    }

    public GameObject GetTarget()
    {
        return target;
    }

}
