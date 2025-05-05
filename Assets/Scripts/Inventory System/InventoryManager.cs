using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public InventorySlot[] inventorySlots;
    public GameObject inventoryItemPrefab;
    public Transform equippedWeaponSlot;
    public Vector3 weaponRotationOffset = Vector3.zero;

    public static InventoryManager Instance;

    public Item swordItem;
    public ItemPickup swordPickUpPrefab;

    private int selectedSlot = -1;
    public InventoryItem selectedItem;

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

    private void Start()
    {
        swordItem = Resources.Load<Item>("Items/Sword");
        if (swordItem != null)
            AddItem(swordItem);
        else
            Debug.LogError("Sword item not found in Resources!");

        ChangeSelectedSlot(0);
    }

    private void Update()
    {
        HandleSlotInput();

        if (Input.GetKeyDown(KeyCode.Q))
            DropSelectedItem();
    }

    #region Slot Handling

    private void HandleSlotInput()
    {
        if (Input.inputString != null && int.TryParse(Input.inputString, out int number) && number > 0 && number <= inventorySlots.Length)
            ChangeSelectedSlot(number - 1);
    }

    private void ChangeSelectedSlot(int newIndex)
    {
        if (selectedSlot >= 0)
            inventorySlots[selectedSlot].Deselect();

        inventorySlots[newIndex].Select();
        selectedSlot = newIndex;

        InventoryItem item = GetSelectedInventoryItem();
        selectedItem = item;

        if (item != null && item.item.type == Itemtype.Weapon)
            EquipWeapon(item.item);
        else
            UnequipWeapon();
    }

    private InventoryItem GetSelectedInventoryItem()
    {
        if (selectedSlot < 0 || selectedSlot >= inventorySlots.Length) return null;
        return inventorySlots[selectedSlot].GetComponentInChildren<InventoryItem>();
    }

    #endregion

    #region Item Management

    public bool AddItem(Item item, int damage = -1, float attackSpeed = -1, int durability = -1)
    {
        if (item == null) return false;

        foreach (var slot in inventorySlots)
        {
            var existing = slot.GetComponentInChildren<InventoryItem>();
            if (existing != null && existing.item == item && existing.item.stackable && existing.count < item.stackCount)
            {
                existing.count++;
                existing.RefreshCount();
                return true;
            }
        }

        foreach (var slot in inventorySlots)
        {
            if (slot.GetComponentInChildren<InventoryItem>() == null)
            {
                SpawnNewItem(item, slot, damage, attackSpeed, durability);
                return true;
            }
        }

        return false;
    }

    private void SpawnNewItem(Item item, InventorySlot slot, int damage = -1, float attackSpeed = -1, int durability = -1)
    {
        GameObject newItemObj = Instantiate(inventoryItemPrefab, slot.transform);
        InventoryItem inventoryItem = newItemObj.GetComponent<InventoryItem>();
        inventoryItem.InitialiseItem(item, damage, attackSpeed, durability);
    }


    public void RemoveSelectedItemFromInventory()
    {
        InventoryItem invItem = GetSelectedInventoryItem();
        if (invItem == null) return;

        Destroy(invItem.gameObject);
        UnequipWeapon();
        ChangeSelectedSlot((selectedSlot + 1) % inventorySlots.Length);
    }

    public void DropSelectedItem(float radius = 1.5f)
    {
        InventoryItem invItem = GetSelectedInventoryItem();
        if (invItem == null) return;

        Item itemData = invItem.item;
        GameObject prefab = itemData.worldDropPrefab != null ? itemData.worldDropPrefab : itemData.itemPrefab;

        if (prefab != null)
        {
            Transform player = GameObject.FindGameObjectWithTag("Player").transform;
            Vector3 dropPos = player.position + new Vector3(Random.Range(-radius, radius), Random.Range(-radius, radius), 0f);

            GameObject dropped = Instantiate(prefab, dropPos, Quaternion.identity);
            ItemPickup pickup = dropped.GetComponent<ItemPickup>();

            if (pickup){
                pickup.item = itemData;

                if (itemData.type == Itemtype.Weapon)
                {
                    pickup.savedDamage = invItem.damage;
                    pickup.savedAttackSpeed = invItem.attackSpeed;
                    pickup.savedDurability = invItem.durability;

                    SwordController sc = dropped.GetComponent<SwordController>();
                    if (sc != null)
                    {
                        sc.damage = invItem.damage;
                        sc.attackSpeed = invItem.attackSpeed;
                        sc.durability = invItem.durability;
                    }
                }
            }
        }



        if (invItem.count > 1)
        {
            invItem.count--;
            invItem.RefreshCount();
        }
        else
        {
            Destroy(invItem.gameObject);
            ChangeSelectedSlot((selectedSlot + 1) % inventorySlots.Length);
        }
    }

    #endregion

    #region Weapon Handling

    public void EquipWeapon(Item weaponItem)
    {
        if (equippedWeaponSlot.childCount > 0)
            Destroy(equippedWeaponSlot.GetChild(0).gameObject);

        InventoryItem invItem = GetSelectedInventoryItem();
        if (invItem == null) return;

        GameObject weapon = Instantiate(weaponItem.itemPrefab, equippedWeaponSlot);

        weapon.transform.localPosition = Vector3.zero;
        weapon.transform.localRotation = Quaternion.Euler(weaponRotationOffset);

        if (weapon.TryGetComponent(out SpriteRenderer sr))
            sr.sortingOrder = 1;

        if (weapon.TryGetComponent(out SwordController sword))
        {
            sword.inventoryItem = invItem;
            sword.damage = invItem.damage;
            sword.attackSpeed = invItem.attackSpeed;
            sword.durability = invItem.durability;
            weapon.transform.localScale = new Vector3(3f, 3f, 3f);
        }
        else if (weapon.TryGetComponent(out BowController bow))
        {
            bow.inventoryItem = invItem;
            bow.damage = invItem.damage;
            bow.attackSpeed = invItem.attackSpeed;
            bow.durability = invItem.durability;
        }
    }

    public void UnequipWeapon()
    {
        foreach (Transform child in equippedWeaponSlot)
            Destroy(child.gameObject);
    }

    public void CheckWeaponEquipped()
    {
        InventoryItem item = GetSelectedInventoryItem();
        if (item == null || item.item.type != Itemtype.Weapon)
            UnequipWeapon();
    }

    #endregion

    #region Utility Methods

public bool HasItem(string itemName)
{
    foreach (InventorySlot slot in inventorySlots)
    {
        InventoryItem item = slot.GetComponentInChildren<InventoryItem>();
        if (item != null)
        {
            Debug.Log($"Found item in slot: {item.item.itemName}, count: {item.count}");
            if (item.item.itemName == itemName)
            {
                Debug.Log("Arrow match found!");
                return true;
            }
        }
    }
    Debug.Log("No matching item found in inventory for " + itemName);
    return false;
}


public InventoryItem item => GetComponentInChildren<InventoryItem>();

public void ReduceStackOrRemove(string itemName)
{
    foreach (InventorySlot slot in inventorySlots)
    {
        InventoryItem item = slot.GetComponentInChildren<InventoryItem>();
        if (item != null && item.item.itemName == itemName)
        {
            Debug.Log($"Reducing count of {item.item.itemName} from {item.count}");

            item.count--;
            item.RefreshCount();

            if (item.count <= 0)
            {
                Destroy(item.gameObject);
                if (InventoryManager.Instance.selectedItem == item)
                {
                    InventoryManager.Instance.selectedItem = null;
                }
            }
            return;
        }
    }

    Debug.LogWarning($"Arrow item '{itemName}' not found in inventory when trying to reduce count.");
}





    public int GetTotalGold()
    {
        int total = 0;
        foreach (var slot in inventorySlots)
        {
            InventoryItem item = slot.GetComponentInChildren<InventoryItem>();
            if (item != null && item.item.type == Itemtype.Gold)
                total += item.count;
        }
        return total;
    }

    public bool TrySpendGold(int amount)
    {
        if (GetTotalGold() < amount) return false;
        SpendGold(amount);
        return true;
    }

    public void SpendGold(int amount)
    {
        foreach (var slot in inventorySlots)
        {
            InventoryItem goldItem = slot.GetComponentInChildren<InventoryItem>();
            if (goldItem != null && goldItem.item.type == Itemtype.Gold)
            {
                int take = Mathf.Min(amount, goldItem.count);
                goldItem.count -= take;
                amount -= take;
                goldItem.RefreshCount();

                if (goldItem.count <= 0)
                    Destroy(goldItem.gameObject);

                if (amount <= 0) return;
            }
        }
    }

    public void PurgeKeys()
    {
        foreach (var slot in inventorySlots)
        {
            InventoryItem item = slot.GetComponentInChildren<InventoryItem>();
            if (item != null && item.item.type == Itemtype.Key)
                Destroy(item.gameObject);
        }
    }

    public void ClearInventoryExceptSword()
    {
        foreach (var slot in inventorySlots)
        {
            InventoryItem item = slot.GetComponentInChildren<InventoryItem>();
            if (item != null && item.item != swordItem)
                Destroy(item.gameObject);
        }
    }

    public int GetSelectedSlotIndex() => selectedSlot;

    #endregion
}