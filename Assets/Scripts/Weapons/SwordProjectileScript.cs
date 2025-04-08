using System;
using UnityEngine;

public class SwordProjectileScript : MonoBehaviour
{
    private GameObject target;
    private float damage;
    private float timer = 0f;
    private float timeVisible = 0.15f;
    private bool attack = true;
    private bool isPlayer;
    private float knockbackForce = 25f;

    private int baseDamage = 5;

    void Start()
    {
        damage = CalculateDamage(baseDamage);
    }

    void Update()
    {
        // Destroy this projectile after the visible time expires.
        if (timer >= timeVisible)
        {
            Destroy(gameObject);
        }
        else
        {
            timer += Time.deltaTime;
        }
    }
    
    public float CalculateDamage(int baseDamage)
    {
        // Retrieve the player's existing damage multiplier (e.g., from their Strength stat).
        float damageMultiplier = PlayerStats.GetDamageMultiplier();
        
        // Retrieve the player's current level from PlayerXP.
        int playerLevel = 1;
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
        {
            PlayerXP xpComponent = playerObj.GetComponent<PlayerXP>();
            if (xpComponent != null)
            {
                playerLevel = xpComponent.currentLevel;
            }
        }
        
        // For example, add 5% more damage per level above level 1.
        float levelMultiplier = 1f + ((playerLevel - 1) * 0.05f);
        
        Debug.Log($"Calculating damage: baseDamage = {baseDamage}, " +
                  $"damageMultiplier = {damageMultiplier}, " +
                  $"playerLevel = {playerLevel}, " +
                  $"levelMultiplier = {levelMultiplier}");
        
        return baseDamage * damageMultiplier * levelMultiplier;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (attack)
        {
            target = collision.gameObject;

            // Check whether the target is valid based on who fired the projectile.
            if ((isPlayer && target.GetComponent<EnemyAI>() != null) ||
                (!isPlayer && target.GetComponent<PlayerInfo>() != null))
            {
                if (isPlayer)
                {
                    target.GetComponent<EnemyAI>().damage(damage);
                    
                    // Add knockback to enemy.
                    Vector2 direction = (collision.transform.position - transform.parent.transform.position).normalized;
                    target.GetComponent<EnemyAI>().SetKnockbackForceVector(direction * knockbackForce);
                }
                else
                {
                    target.GetComponent<PlayerInfo>().damage(damage);
                    
                    // Apply different knockback if this is an enemy attack.
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
