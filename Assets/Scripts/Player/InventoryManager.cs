using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public InventorySlot[] inventorySlots;

    public GameObject inventoryItemPrefab;

    int selectedSlot = -1;

    void Start (){
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

     void ChangeSelectedSlot(int newValue){
        if (selectedSlot >=0){
        inventorySlots[selectedSlot].Deselect();
        }
        inventorySlots[newValue].Select();
        selectedSlot = newValue;
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
}
