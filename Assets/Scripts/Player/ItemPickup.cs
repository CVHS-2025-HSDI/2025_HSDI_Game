using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    public Item item; // Assign the item in the Inspector
    private InventoryManager inventory;

    private void Start()
    {
        // Find the InventoryManager in the scene (make sure it has the tag "InventoryManager")
        inventory = GameObject.FindGameObjectWithTag("InventoryManager")?.GetComponent<InventoryManager>();

        if (inventory == null)
        {
            Debug.LogError("InventoryManager not found in the scene! Make sure it exists and has the correct tag.");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && inventory != null) // Ensure only the player picks up items
        {
            bool added = inventory.AddItem(item);
            if (added)
            {
                Destroy(gameObject); // Remove the item from the world
            }
        }
    }
}
