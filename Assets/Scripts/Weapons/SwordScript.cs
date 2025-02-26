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

        transform.rotation = Quaternion.identity;
    }

    public void Attack()
    {
        if (isPlayer)
        {
            GameObject player = gameObject.transform.parent.gameObject;
            Movement m = player.GetComponent<Movement>();

            // Changed back to old method because new one wasn't working correctly
            if (m.GetDesiredRotation() == 0)
            {
                projPos = new Vector3(transform.position.x, transform.position.y + 1.5f);
                projRot = Quaternion.Euler(new Vector3(0, 0, 315));
            }
            else if (m.GetDesiredRotation() == -180)
            {
                projPos = new Vector3(transform.position.x, transform.position.y - 1.5f);
                projRot = Quaternion.Euler(new Vector3(0, 0, 135));
            }
            else if (m.GetDesiredRotation() == -90)
            {
                projPos = new Vector3(transform.position.x + 1.5f, transform.position.y);
                projRot = Quaternion.Euler(new Vector3(0, 0, 225));
            }
            else if (m.GetDesiredRotation() == 90)
            {
                projPos = new Vector3(transform.position.x - 1.5f, transform.position.y);
                projRot = Quaternion.Euler(new Vector3(0, 0, 45));
            }
            else if (m.GetDesiredRotation() == 45)
            {
                projPos = new Vector3(transform.position.x - 1f, transform.position.y + 1f);
                projRot = Quaternion.Euler(new Vector3(0, 0, 0));
            }
            else if (m.GetDesiredRotation() == -45)
            {
                projPos = new Vector3(transform.position.x + 1f, transform.position.y + 1f);
                projRot = Quaternion.Euler(new Vector3(0, 0, 270));
            }
            else if (m.GetDesiredRotation() == -225)
            {
                projPos = new Vector3(transform.position.x - 1f, transform.position.y - 1f);
                projRot = Quaternion.Euler(new Vector3(0, 0, 90));
            }
            else if (m.GetDesiredRotation() == -135)
            {
                projPos = new Vector3(transform.position.x + 1f, transform.position.y - 1f);
                projRot = Quaternion.Euler(new Vector3(0, 0, 180));
            }

            SwordProjectileScript sps = Instantiate(projectilePrefab, projPos, projRot)
                .GetComponent<SwordProjectileScript>();
            sps.gameObject.transform.SetParent(transform);
            sps.setIsPlayer(isPlayer);
        }
        else
        {
            if (target == null) return;
            // For enemy attacks, determine the direction towards the target.
            if (Math.Abs(target.transform.position.x - transform.position.x) >= 1 && Math.Abs(target.transform.position.y - transform.position.y) >= 1)
            {
                //up and right
                if (target.transform.position.x > transform.position.x && target.transform.position.y > transform.position.y)
                {
                    projPos = new Vector3(transform.position.x + 1f, transform.position.y + 1f);
                    projRot = Quaternion.Euler(new Vector3(0, 0, 270));
                }
                //up and left
                else if (target.transform.position.x < transform.position.x && target.transform.position.y > transform.position.y)
                {
                    projPos = new Vector3(transform.position.x - 1f, transform.position.y + 1f);
                    projRot = Quaternion.Euler(new Vector3(0, 0, 0));
                }
                //down and right
                else if (target.transform.position.x > transform.position.x && target.transform.position.y < transform.position.y)
                {
                    projPos = new Vector3(transform.position.x + 1f, transform.position.y - 1f);
                    projRot = Quaternion.Euler(new Vector3(0, 0, 180));
                }
                //down and left
                else if (target.transform.position.x < transform.position.x && target.transform.position.y < transform.position.y)
                {
                    projPos = new Vector3(transform.position.x - 1f, transform.position.y - 1f);
                    projRot = Quaternion.Euler(new Vector3(0, 0, 90));
                }
            }
            else if (Math.Abs(target.transform.position.x - transform.position.x) > Math.Abs(target.transform.position.y - transform.position.y))
            {
                //left
                if (target.transform.position.x < transform.position.x)
                {
                    projPos = new Vector3(transform.position.x - 1.5f, transform.position.y);
                    projRot = Quaternion.Euler(new Vector3(0, 0, 45));
                }
                //right
                else if (target.transform.position.x > transform.position.x)
                {
                    projPos = new Vector3(transform.position.x + 1.5f, transform.position.y);
                    projRot = Quaternion.Euler(new Vector3(0, 0, 225));
                }
            }
            else
            {
                //down
                if (target.transform.position.y < transform.position.y)
                {
                    projPos = new Vector3(transform.position.x, transform.position.y - 1.5f);
                    projRot = Quaternion.Euler(new Vector3(0, 0, 135));
                }
                //up
                else if (target.transform.position.y > transform.position.y)
                {
                    projPos = new Vector3(transform.position.x, transform.position.y + 1.5f);
                    projRot = Quaternion.Euler(new Vector3(0, 0, 315));
                }
            }

            SwordProjectileScript sps = Instantiate(projectilePrefab, projPos, projRot)
                .GetComponent<SwordProjectileScript>();
            sps.gameObject.transform.SetParent(transform);
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
}
