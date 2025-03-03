using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InventoryItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("UI")]
    public Image image;
    public Text countText;
    [HideInInspector]public Item item;
    [HideInInspector] public int count = 1;
    [HideInInspector] public Transform parentAfterDrag;


    void Start(){
        InitialiseItem(item);
    }


    public void InitialiseItem(Item newItem){
        item = newItem;
        image.sprite = newItem.image;
        RefreshCount();
    }

    public void RefreshCount(){
        countText.text = count.ToString();
        bool textActive = count > 1;
        countText.gameObject.SetActive(textActive);

    }

   public void OnBeginDrag(PointerEventData eventData){
    image.raycastTarget = false;
    parentAfterDrag = transform.parent;
    transform.SetParent(transform.root);

    InventoryManager inventoryManager = FindObjectOfType<InventoryManager>();
    if (inventoryManager != null && inventoryManager.equippedWeaponSlot.childCount > 0 && item.type == Itemtype.Weapon)
    {
        inventoryManager.UnequipWeapon();
    }
}


    
    public void OnDrag(PointerEventData eventData){
        transform.position = Input.mousePosition;
    }

public void OnEndDrag(PointerEventData eventData){
    image.raycastTarget = true;
    transform.SetParent(parentAfterDrag);

    InventoryManager inventoryManager = FindObjectOfType<InventoryManager>();

    if (inventoryManager != null)
    {
        inventoryManager.CheckWeaponEquipped();

        // Equip weapon immediately if dropped into selected slot
        int selectedSlotIndex = inventoryManager.GetSelectedSlotIndex();
        InventorySlot currentSlot = parentAfterDrag.GetComponent<InventorySlot>();

        if (currentSlot != null && inventoryManager.inventorySlots[selectedSlotIndex] == currentSlot && item.type == Itemtype.Weapon)
        {
            inventoryManager.EquipWeapon(item);
        }
    }
}






}