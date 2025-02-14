using UnityEngine;
using UnityEngine.EventSystems;

public class InventorySlot : MonoBehaviour, IDropHandler
{
    public void onDrop(PointerEvent Data eventData){
        if(transform.childCount == 0){
            InventoryItem inventoryItem = eventData.pointerDrag.GetComponent<inventoryItem>();
            inventoryItem.parentAfterDrag = transform;
        }
    }
}
