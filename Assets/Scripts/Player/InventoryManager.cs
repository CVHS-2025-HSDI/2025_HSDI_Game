using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public InventorySlot[] inventorySlots;

    public GameObject inventoryItemPrefab;

    int selectedSlot = -1;

    public Transform equippedWeaponSlot; 

    public Vector3 weaponRotationOffset = new Vector3(0, 0, 0);

    void Start() {
    // Find the sword item from resources (or assign in the inspector)
    Item swordItem = Resources.Load<Item>("Items/Sword"); // Adjust path if needed

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
        UnequipWeapon(); // Unequip if no weapon is in the slot
    }
}


    public bool AddItem(Item item){

        foreach(InventorySlot slot in inventorySlots){
            InventoryItem itemInSlot = slot.GetComponentInChildren<InventoryItem>();
            if(itemInSlot != null && itemInSlot.item == item && itemInSlot.count < 4  && itemInSlot.item.stackable == true){
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
    // Remove previously equipped weapon
        if (equippedWeaponSlot.childCount > 0) {
            Destroy(equippedWeaponSlot.GetChild(0).gameObject);
        }

    // Spawn and equip the new weapon
        GameObject newWeapon = Instantiate(weaponItem.itemPrefab, equippedWeaponSlot);
        newWeapon.transform.localPosition = new Vector3(0.05f, 0.06f, 0); // Ensure correct position
        newWeapon.transform.localRotation = Quaternion.Euler(weaponRotationOffset);
        newWeapon.transform.localScale = new Vector3(0.35f, 0.35f, 0.35f); // Ensure correct size

        SpriteRenderer sr = newWeapon.GetComponent<SpriteRenderer>();

        sr.sortingOrder=1;
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





}
