using UnityEngine;

public class PlayerInfo_placeholder : MonoBehaviour
{
    public GameObject weaponPrefab; //drag in
    private GameObject weapon;
    private GameObject cursor; //remove later
    private float health = 25;

    void Start()
    {
        //Find cursor when in same scene
        cursor = GameObject.Find("Cursor"); //remove later

        //instantiate weapon, set parent, set isPlayer
        weapon = Instantiate(weaponPrefab, transform.position, weaponPrefab.transform.rotation);
        weapon.transform.SetParent(transform);
        if (weapon.GetComponent("SwordScript") != null)
        {
            ((SwordScript)weapon.GetComponent("SwordScript")).setIsPlayer(true);
            ((SwordScript)weapon.GetComponent("SwordScript")).setTarget(cursor);
        }//else if for other weapon scripts
    }

    public void damage(float damage)
    {
        health -= damage;
        Debug.Log("player hit for " + damage);
        if(health <= 0)
        {
            //death related stuff goes here
            Destroy(gameObject);
        }
    }
}
