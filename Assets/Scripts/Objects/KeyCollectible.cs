using UnityEngine;

public class KeyCollectible : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the player enters the trigger
        if (other.CompareTag("Player"))
        {
            // Notify the KeyManager that a key has been collected.
            KeyManager.Instance.CollectKey();
            // Destroy this key
            Destroy(gameObject);
        }
    }
}