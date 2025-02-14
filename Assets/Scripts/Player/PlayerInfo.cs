using UnityEngine;

public class PlayerInfo : MonoBehaviour
{
    public GameObject weaponPrefab; //drag in
    private GameObject weapon;
    private float health = 25;

    void Start()
    {
        //instantiate weapon, set parent, set isPlayer
        weapon = Instantiate(weaponPrefab, transform.position, weaponPrefab.transform.rotation);
        weapon.transform.SetParent(transform);
        if (weapon.GetComponent("SwordScript") != null)
        {
            ((SwordScript)weapon.GetComponent("SwordScript")).setIsPlayer(true);
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
