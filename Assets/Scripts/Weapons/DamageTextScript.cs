using UnityEngine;

public class DamageTextScript : MonoBehaviour
{
    private float timer = 0;
    private Transform target;
    private float d = 0.4f;

    void Update()
    {
        if(timer > 0.3)
        {
            Destroy(gameObject);
        }

        if (target != null)
        {
            //Track target's position while moving up by 0.4
            transform.position = new Vector2(target.position.x, target.position.y + 0.8f);
            transform.position = new Vector2(transform.position.x, transform.position.y + d * Time.deltaTime);
            d += 0.4f;
        }
        else
        {
            //Keep going up even if target is gone
            transform.position = new Vector2(transform.position.x, transform.position.y + 0.4f * Time.deltaTime);
        }

        timer += Time.deltaTime;
    }

    public void SetTarget(Transform t)
    {
        target = t;
    }
}
