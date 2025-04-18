using UnityEngine;

public class EnemySight : MonoBehaviour
{
    private float distance = 3f;
    public LayerMask mask;

    void Update()
    {

        RaycastHit2D hit = Physics2D.Raycast(transform.position, transform.TransformDirection(Vector2.up), distance, mask);

        Debug.DrawRay(transform.position, transform.TransformDirection(Vector2.up) * distance, Color.red);

        if (hit.collider != null)
        {
            if (hit.collider.CompareTag("Player"))
            {
                Debug.Log("Player spotted!");
            }
            else
            {
                Debug.Log("Hit wall or obstacle");
            }
        }
    }
}


