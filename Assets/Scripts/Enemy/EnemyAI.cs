using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    public GameObject playerTarget;
    public float moveSpeed = 1.5f;

    void Start()
    {
        //Will find the player when they are in the same scene
        playerTarget = GameObject.FindWithTag("Player");
    }

    void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, playerTarget.transform.position, moveSpeed * Time.deltaTime);
    }
}
