using UnityEngine;

public class ShopManager : MonoBehaviour
{
    public GameObject shopItemPrefab;
    public Transform shopContent;
    public ShopItemData[] itemsForSale;

    void Start()
    {
        foreach (var shopItemData in itemsForSale)
        {
            GameObject obj = Instantiate(shopItemPrefab, shopContent);
            ShopItem shopItem = obj.GetComponent<ShopItem>();
            shopItem.item = shopItemData.item;
            shopItem.price = shopItemData.price;
        }
    }
}

[System.Serializable]
public class ShopItemData
{
    public Item item;
    public int price;
}
