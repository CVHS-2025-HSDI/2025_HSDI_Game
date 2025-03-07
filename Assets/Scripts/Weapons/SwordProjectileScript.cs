using System;
using UnityEngine;

public class SwordProjectileScript : MonoBehaviour
{
    private GameObject target;
    private int damage;
    private float timer = 0;
    private float timeVisible = 0.15f;
    private bool attack = true;
    private bool isPlayer;
    private float knockbackForce = 25f;

    void Start()
    {
        damage = (int)((UnityEngine.Random.value * 4) + 1);
    }

    void Update()
    {
        // If visible for timeVisible seconds, destory
        if (timer >= timeVisible)
        {
            Destroy(gameObject);
        }
        else
        {
            timer += Time.deltaTime;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (attack)
        {
            target = collision.gameObject;

            // If projectile collided with a valid target, remove 'damage' health from target
            if ((isPlayer && target.GetComponent<EnemyAI>() != null) || (!isPlayer && target.GetComponent<PlayerInfo>() != null))
            {
                if (isPlayer)
                {
                    target.GetComponent<EnemyAI>().damage(damage);

                    // Add knockback vector to enemy's movement direction vector
                    Vector2 direction = (collision.transform.position - transform.parent.transform.position).normalized;
                    target.GetComponent<EnemyAI>().SetKnockbackForceVector(direction * knockbackForce);
                }
                else
                {
                    target.GetComponent<PlayerInfo>().damage(damage);

                    // Add knockback vector to player's movement direction vector
                    knockbackForce = 15f;
                    Vector2 direction = (collision.transform.position - transform.parent.transform.position).normalized;
                    target.GetComponent<Movement>().SetKnockbackForceVector(direction * knockbackForce);
                }
            }
            attack = false;
        }
    }

    public void setIsPlayer(bool isPlayer)
    {
        this.isPlayer = isPlayer;
    }
}
