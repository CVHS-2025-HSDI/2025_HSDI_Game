using System.Collections;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public InventorySlot[] inventorySlots;
    public GameObject inventoryItemPrefab;
    int selectedSlot = -1;
    public Transform equippedWeaponSlot; 
    public Vector3 weaponRotationOffset = new Vector3(0, 0, 0);

    // NEW: Store a reference to the Sword item.
    public Item swordItem;
    
    public static InventoryManager Instance;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start() {
        // Load and store the Sword item.
        swordItem = Resources.Load<Item>("Items/Sword"); // Adjust path if needed
        if (swordItem != null) {
            AddItem(swordItem);
        } else {
            Debug.LogError("Sword item not found in Resources!");
        }
        ChangeSelectedSlot(0);
    }

    void Update () {
        if(Input.inputString != null){
            bool isNum = int.TryParse(Input.inputString, out int number);
            if(isNum && number > 0 && number < 10){
                ChangeSelectedSlot(number - 1);
            }
        }
    }

    void ChangeSelectedSlot(int newValue)
    {
        if (selectedSlot >= 0) {
            inventorySlots[selectedSlot].Deselect();
        }
        inventorySlots[newValue].Select();
        selectedSlot = newValue;
        InventoryItem selectedItem = inventorySlots[selectedSlot].GetComponentInChildren<InventoryItem>();
        if (selectedItem != null && selectedItem.item.type == Itemtype.Weapon) {
            EquipWeapon(selectedItem.item);
        } else {
            UnequipWeapon();
        }
    }

    public bool AddItem(Item item){
        foreach(InventorySlot slot in inventorySlots){
            InventoryItem itemInSlot = slot.GetComponentInChildren<InventoryItem>();
            if(itemInSlot != null && itemInSlot.item == item && itemInSlot.count < 4 && itemInSlot.item.stackable == true){
                itemInSlot.count++;
                itemInSlot.RefreshCount();
                return true;
            }
        }
        foreach(InventorySlot slot in inventorySlots){
            InventoryItem itemInSlot = slot.GetComponentInChildren<InventoryItem>();
            if(itemInSlot == null){
                SpawnNewItem(item, slot);
                return true;
            }
        }
        return false;
    }

    void SpawnNewItem(Item item, InventorySlot slot){
        GameObject newItemGo = Instantiate(inventoryItemPrefab, slot.transform);
        InventoryItem inventoryItem = newItemGo.GetComponent<InventoryItem>();
        inventoryItem.InitialiseItem(item);
    }

    public void EquipWeapon(Item weaponItem) {
        if (equippedWeaponSlot.childCount > 0) {
            Destroy(equippedWeaponSlot.GetChild(0).gameObject);
        }
        GameObject newWeapon = Instantiate(weaponItem.itemPrefab, equippedWeaponSlot);
        newWeapon.transform.localPosition = new Vector3(0, 0, 0);
        newWeapon.transform.localRotation = Quaternion.Euler(weaponRotationOffset);
        newWeapon.transform.localScale = new Vector3(5f, 5f, 5f);
        SpriteRenderer sr = newWeapon.GetComponent<SpriteRenderer>();
        if (sr != null) {
            sr.sortingOrder = 1;
        }
    }

    public void UnequipWeapon() {
        foreach (Transform child in equippedWeaponSlot) {
            Destroy(child.gameObject);
        }
    }

    public void CheckWeaponEquipped(){
        InventoryItem selectedItem = inventorySlots[selectedSlot].GetComponentInChildren<InventoryItem>();
        if (selectedItem == null || selectedItem.item.type != Itemtype.Weapon)
        {
            UnequipWeapon();
        }
    }

    public int GetSelectedSlotIndex(){
        return selectedSlot;
    }

    public void PurgeKeys()
    {
        foreach (InventorySlot slot in inventorySlots)
        {
            InventoryItem invItem = slot.GetComponentInChildren<InventoryItem>();
            if (invItem != null && invItem.item.type == Itemtype.Key)
            {
                Destroy(invItem.gameObject);
            }
        }
        Debug.Log("Key items purged from inventory.");
    }

    // NEW: Clear all inventory items except for the Sword.
    public void ClearInventoryExceptSword()
    {
        foreach (InventorySlot slot in inventorySlots)
        {
            InventoryItem invItem = slot.GetComponentInChildren<InventoryItem>();
            if (invItem != null)
            {
                // If this item is not the Sword item, remove it.
                if (swordItem == null || invItem.item != swordItem)
                {
                    Destroy(invItem.gameObject);
                }
            }
        }
        Debug.Log("Inventory cleared, preserving the Sword.");
    }
}
