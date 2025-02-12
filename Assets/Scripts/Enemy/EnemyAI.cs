using UnityEngine;
using System;

public class EnemyAI : MonoBehaviour
{
    private GameObject playerTarget;
    public GameObject weaponPrefab; //drag in
    private GameObject weapon;
    private float moveSpeed = 1.5f;
    private string state = ("Passive");
    private float health = 25;

    void Start()
    {
        //Will find the player when they are in the same scene
        playerTarget = GameObject.FindWithTag("Player");
        
        //instantiate weapon, set parent, isPlayer
        weapon = Instantiate(weaponPrefab, transform.position, weaponPrefab.transform.rotation);
        weapon.transform.SetParent(transform);
        if(weapon.GetComponent("SwordScript") != null)
        {
            ((SwordScript)weapon.GetComponent("SwordScript")).setIsPlayer(false);
        }//else if for other weapon scripts
    }

    void Update()
    {
        if (state.Equals("Passive"))
        {
            //move around and change direction, patrol - IN PROGRESS

            //if target is within 3 units, change state to aggro (change to a line of sight) - IN PROGRESS
            if (Math.Abs(playerTarget.transform.position.x - transform.position.x) <= 3 && Math.Abs(playerTarget.transform.position.y - transform.position.y) <= 3)
            {
                changeState("Aggro");
            }
        }
        else if (state.Equals("Aggro") && playerTarget != null) //smarter ai, make choices based on players actions - IN PROGRESS
        {
            //Move towards target at moveSpeed
            transform.position = Vector3.MoveTowards(transform.position, playerTarget.transform.position, moveSpeed * Time.deltaTime);
        }
        else if (state.Equals("Stagger"))
        {
            //not sure what goes here yet
        }
    }

    public void changeState(String state)
    {
        if (weapon.GetComponent("SwordScript") != null)
        {
            SwordScript ws = (SwordScript)weapon.GetComponent("SwordScript");
            if (state.Equals("Passive"))
            {
                this.state = state;
                ws.setTarget(null);
            }
            else if (state.Equals("Aggro"))
            {
                this.state = state;
                ws.setTarget(playerTarget);
            }
            else if (state.Equals("Stagger"))
            {
                this.state = state;
                ws.setTarget(null);
            }
        }//else if for other weapon scripts
    }

    public void damage(float damage)
    {
        health -= damage;
        Debug.Log("enemy hit for " + damage);
        if (health <= 0)
        {
            //death related stuff goes here. Possible drops(gold & weapon), death animation
            Destroy(gameObject);
        }
    }
}
