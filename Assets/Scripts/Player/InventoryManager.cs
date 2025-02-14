using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public InventorySlot[] inventorySlots; 

    public void AddItem(Item item){
        foreach(InventorySlot slot in inventorySlots){
            InventoryItem itemInSlot = slot.GetComponentInChildren<InventoryItem>();
            if(itemInSlot == null){
                SpawnNewItem(item, slot);
                return;
            }
        }
    }

    void SpawnNewItem(Item item, InventorySlot slot){
        // GameObject newItemGo = Instantiate(inventoryItemPrefab, slot.transform);
        // InventoryItem inventoryItem = newItemGo.GetComponent<inventoryItem>();
        // inventoryItem.InitialiseItem(item);
    }
}
