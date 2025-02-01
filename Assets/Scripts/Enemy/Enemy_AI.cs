using UnityEngine;

public class Enemy_AI : MonoBehaviour
{
    public GameObject target;
    public float moveSpeed = 1.5f;

    void Start()
    {
        //Will find the player when they are in the same scene
        target = GameObject.Find("Player");
    }

    void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, target.transform.position, moveSpeed * Time.deltaTime);
    }
}
