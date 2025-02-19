using UnityEngine;

public class SwordAttackRadiusScript : MonoBehaviour
{
    private SwordScript ss;
    private float timer;

    void Start()
    {
        ss = GetComponentInParent<SwordScript>();
        if (ss != null)
        {
            timer = ss.GetAttackRate();
        }
    }

    void Update()
    {
        if (ss != null && timer < ss.GetAttackRate())
        {
            timer += Time.deltaTime;
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        // Use CompareTag to check for the Player rather than checking name
        if (collision.CompareTag("Player"))
        {
            if (ss != null && ss.GetTarget() != null)
            {
                if (timer >= ss.GetAttackRate())
                {
                    ss.Attack();
                    timer = 0f;
                }
            }
        }
    }
}