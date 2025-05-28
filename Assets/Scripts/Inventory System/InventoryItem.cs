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
    public Slider durabilitySlider; 

    public int damage;
    public float attackSpeed;
    public int durability;


    public void InitialiseItem(Item newItem, int dmg = -1, float atkSpeed = -1, int dura = -1)
    {
        item = newItem;
        image.sprite = newItem.image;
        count = 1;
        RefreshCount();

        if (item.type == Itemtype.Weapon)
        {
            damage = (dmg != -1) ? dmg : Random.Range(5, 16);
            attackSpeed = (atkSpeed != -1) ? atkSpeed : Random.Range(1f, 1.5f);
            durability = (dura != -1) ? dura : Random.Range(10, 20);

            durabilitySlider.gameObject.SetActive(true);
            UpdateDurabilitySlider();
        }
        else
        {
            if (durabilitySlider != null)
                durabilitySlider.gameObject.SetActive(false);
        }
    }



   public void UpdateDurabilitySlider()
{
    durabilitySlider.value = durability / 25f;
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

    InventoryManager inventoryManager = FindFirstObjectByType<InventoryManager>();
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

    InventoryManager inventoryManager = FindFirstObjectByType<InventoryManager>();

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