using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public Inventoryslot[] inventorySlots; 

    public void AddItem(Item item){
        for(int i = 0; i < inventorySlots.Length; i++){
            InventorySlots slot - inventorySlots[i];
            InventoryItem itemInSlot = slot.GetComponentInChildren<InventoryItem>();
            if(itemInSlot == null){
                SpawnNewItem(item, slot);
                return;
            }
        }
    }

    void SpawnNewItem(Item item, InventorySlot slot){
        GameObject newItemGo = Instantiate(inventoryItemPrefab, slot.transform);
        InventoryItem inventoryItem = newItemGo.GetComponent<inventoryItem>();
        inventoryItem.InitialiseItem(item);
    }
}
