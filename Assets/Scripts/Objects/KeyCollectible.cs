using UnityEngine;

/// <summary>Attached to every key prefab dropped in the world.</summary>
public class KeyCollectible : MonoBehaviour
{
    [HideInInspector] public int keyId;     // assigned by FloorGenerator

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        KeyManager.Instance.CollectKey(keyId);
        Destroy(gameObject);                // remove key from scene – it’s now in inventory
    }
}