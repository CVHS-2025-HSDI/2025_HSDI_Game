using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InventorySlot : MonoBehaviour, IDropHandler
{
    public Image image;
    public Color selectedColor, notSelectedColor;
    
    private static InventoryItem selectedItem; // The currently selected slot

    void Update(){
        if (Input.GetKeyDown(KeyCode.E)){
            UseSelectedItem();
        }
    }

    public void Select()
    {
        image.color = selectedColor;
        selectedItem = GetComponentInChildren<InventoryItem>();
    }

    public void Deselect()
    {
        image.color = notSelectedColor;
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (transform.childCount == 0) // Ensure slot is empty before placing
        {
            InventoryItem inventoryItem = eventData.pointerDrag.GetComponent<InventoryItem>();
            inventoryItem.parentAfterDrag = transform;
        }
    }

    public static void UseSelectedItem()
{
    if (selectedItem != null && selectedItem.item.type == Itemtype.Potion)
    {
        PlayerInfo player = FindFirstObjectByType<PlayerInfo>(); // Find player health
        if (player != null)
        {
            player.Heal(selectedItem.item.healthAmount); // Heal player
            selectedItem.count--; 

            if (selectedItem.count <= 0)
            {
                Destroy(selectedItem.gameObject);
                selectedItem = null;
            }
            else
            {
                selectedItem.RefreshCount();
            }
        }
    }
}



    public void RemoveItem()
    {
        InventoryItem inventoryItem = GetComponentInChildren<InventoryItem>();
        if (inventoryItem != null)
        {
            if (inventoryItem.item.type == Itemtype.Weapon)
            {
                FindFirstObjectByType<InventoryManager>().UnequipWeapon(); // Unequip if it's a weapon
            }
            Destroy(inventoryItem.gameObject); // Remove the item
        }
    }
}
