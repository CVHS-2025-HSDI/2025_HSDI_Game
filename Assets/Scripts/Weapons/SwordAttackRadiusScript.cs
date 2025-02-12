using UnityEngine;

public class SwordAttackRadiusScript : MonoBehaviour
{
    private SwordScript ss;
    private float timer;

    void Start()
    {
        ss = gameObject.GetComponentInParent<SwordScript>();
        timer = ss.getAttackRate();
    }

    void Update()
    {
        timer += Time.deltaTime;
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        //if the radius is entered by the player then call attack(cooldown) in SwordScript
        if (collision.gameObject.name.Equals("Player")) //causes transient artifact errors?
        {
            if (ss.getTarget() != null)
            {
                if (timer >= ss.getAttackRate())
                {
                    ss.attack();
                    timer = 0;
                }
            }
        }
    }
}
