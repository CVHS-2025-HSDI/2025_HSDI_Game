using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    public Item item; // Assign the item in the Inspector
    private InventoryManager _inventory;
    private void Start()
    {
        // Find the InventoryManager in the scene (make sure it has the tag "InventoryManager")
        _inventory = InventoryManager.Instance;

        if (_inventory == null)
        {
            Debug.LogError("InventoryManager not found in the scene! Make sure it exists and has the correct tag.");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && _inventory != null) // Ensure only the player picks up items
        {
            bool added = _inventory.AddItem(item);
            if (added)
            {
                Destroy(gameObject); // Remove the item from the world
            }
        }
    }
}
