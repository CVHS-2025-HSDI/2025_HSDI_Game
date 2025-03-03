using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InventorySlot : MonoBehaviour, IDropHandler
{

    public Image image;
    public Color selectedColor, notSelectedColor;

    private void Awake(){
        Deselect();
    }

    public void Select(){
        image.color = selectedColor;
    }

    public void Deselect(){
        image.color = notSelectedColor;
    }
    public void OnDrop(PointerEventData eventData){
        if(transform.childCount == 0){
            InventoryItem inventoryItem = eventData.pointerDrag.GetComponent<InventoryItem>();
            inventoryItem.parentAfterDrag = transform;
        }
    }

    public void RemoveItem() {
    InventoryItem inventoryItem = GetComponentInChildren<InventoryItem>();
    if (inventoryItem != null) {
        if (inventoryItem.item.type == Itemtype.Weapon) {
            FindObjectOfType<InventoryManager>().UnequipWeapon(); // Unequip the weapon if it was equipped
        }
        Destroy(inventoryItem.gameObject);
    }
}

}
