using System;
using UnityEngine;

public class SwordScript : MonoBehaviour
{
    private GameObject target;
    public GameObject projectilePrefab; //drag in
    public GameObject AttackRadiusPrefab; //drag in
    private float attackRate;
    private float timer = 0;
    private Vector3 projPos;
    private Quaternion projRot;
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
            //instantitate radius if its an enemy
            Instantiate(AttackRadiusPrefab, transform.position, Quaternion.identity).transform.SetParent(transform);
        }
        timer = attackRate;

    }

    void Update()
    {
        if (target != null)
        {
            if (isPlayer)
            {
                //if m1 click and attack timer is reached: attack
                if (Input.GetMouseButtonDown(0) && timer >= attackRate)
                {
                    attack();
                    timer = 0;
                }
            }
            timer += Time.deltaTime;
        }
    }

    public void attack() //for player change to attack in facing direction rather than target - IN PROGRESS
    {
        //get the correct position and rotation of projectile based on target's position. hardcoded values may change when we get the official sprites
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
            else if(target.transform.position.y > transform.position.y)
            {
                projPos = new Vector3(transform.position.x, transform.position.y + 1.5f);
                projRot = Quaternion.Euler(new Vector3(0, 0, 315));
            }
        }

        //instantiate projectile, set parent, isPlayer
        SwordProjectileScript sps = (SwordProjectileScript)Instantiate(projectilePrefab, projPos, projRot).GetComponent("SwordProjectileScript");
        sps.gameObject.transform.SetParent(transform);
        sps.setIsPlayer(isPlayer);
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

    public float getAttackRate()
    {
        return attackRate;
    }

    public void setIsPlayer(bool isPlayer)
    {
        this.isPlayer = isPlayer;
    }
}
