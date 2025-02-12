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

    void Start()
    {
        damage = (int)((UnityEngine.Random.value * 4) + 1);
    }

    void Update()
    {
        //if visible for timeVisible seconds, destory
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

            //if projectile collided with a valid target, remove 'damage' health from target
            if ((isPlayer && target.GetComponent<EnemyAI>() != null) || (!isPlayer && target.GetComponent<PlayerInfo_placeholder>() != null))
            {
                if (isPlayer)
                {
                    target.GetComponent<EnemyAI>().damage(damage);
                }
                else
                {
                    target.GetComponent<PlayerInfo_placeholder>().damage(damage); //may change
                }

                //knock back target by 'knockbackForce' in appropiate direction - IN PROGRESS
            }
            attack = false;
        }
    }

    public void setIsPlayer(bool isPlayer)
    {
        this.isPlayer = isPlayer;
    }
}
