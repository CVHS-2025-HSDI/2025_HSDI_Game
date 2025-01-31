using UnityEngine;

public class movement_placeholder : MonoBehaviour
{
    public float speed = 5f; // Speed of movement

    void Update()
    {
        float horizontalInput = Input.GetAxis("Horizontal"); // Get horizontal input (A and D keys)
        float verticalInput = Input.GetAxis("Vertical"); // Get vertical input (W and S keys)

        // Calculate the movement vector
        Vector3 movement = new Vector3(horizontalInput, verticalInput, 0) * speed * Time.deltaTime;

        // Move the GameObject
        transform.Translate(movement);
    }
}