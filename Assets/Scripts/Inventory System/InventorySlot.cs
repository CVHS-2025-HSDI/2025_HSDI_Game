using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InventorySlot : MonoBehaviour, IDropHandler
{
    public Image image;
    public Color selectedColor, notSelectedColor;
    private InventoryItem selectedItem; // The currently selected item in this slot

    void Update(){
    if (InventoryManager.Instance.selectedItem == selectedItem && Input.GetKeyDown(KeyCode.E)){
        
        // selectedItem.count--;
        // selectedItem.RefreshCount(); 
        UseSelectedItem(); 
    }
}


    public void Select(){
        image.color = selectedColor;
        selectedItem = GetComponentInChildren<InventoryItem>(); // Reference the item in the current slot
        if (selectedItem != null){
            // Update selected item in InventoryManager to the current slot's item
            InventoryManager.Instance.selectedItem = selectedItem;
        }
    }

    public void Deselect(){
        image.color = notSelectedColor;
    }

    public void OnDrop(PointerEventData eventData){
        if (transform.childCount == 0) // Ensure slot is empty before placing
        {
            InventoryItem inventoryItem = eventData.pointerDrag.GetComponent<InventoryItem>();
            inventoryItem.parentAfterDrag = transform; // Reassign the parent slot of the item
        }
    }

   public void UseSelectedItem(){
    if (InventoryManager.Instance.selectedItem != null){
        InventoryItem selectedItem = InventoryManager.Instance.selectedItem;
        if (selectedItem != null && selectedItem.item.type == Itemtype.Potion){
            PlayerInfo player = FindFirstObjectByType<PlayerInfo>();
            if (player != null){
                if (selectedItem.item.itemName == "Teleport Potion")
                        {
                            player.Teleport(); 
                        }
                if (player.currentHealth < player.maxHealth)
                    {
                        player.Heal(selectedItem.item.healthAmount); // Heal the player

                        // Decrease item count only if used
                        selectedItem.count--;
                        selectedItem.RefreshCount();

                        if (selectedItem.count <= 0)
                        {
                            Destroy(selectedItem.gameObject);
                            InventoryManager.Instance.selectedItem = null;
                        }
                    }
                    else
                    {
                        Debug.Log("Player already has max health. Potion not used.");
                    }
            }
        }
    }
}

public void RemoveItem()
{
    InventoryItem inventoryItem = GetComponentInChildren<InventoryItem>();
    if (inventoryItem == null) return;

    if (inventoryItem.item.stackable)
    {
        inventoryItem.count--;
        inventoryItem.RefreshCount();

        if (inventoryItem.count <= 0)
        {
            Destroy(inventoryItem.gameObject);
            InventoryManager.Instance.selectedItem = null;
        }
    }
    else
    {
        if (inventoryItem.item.type == Itemtype.Weapon)
        {
            InventoryManager.Instance.UnequipWeapon(); // Unequip if it's a weapon
        }

        Destroy(inventoryItem.gameObject);
        InventoryManager.Instance.selectedItem = null;
    }
}

}
