using UnityEngine;
using UnityEngine.UI;

public class ShopItem : MonoBehaviour
{
    public Item item;
    public int price;
    public int maxPurchaseAmount = 5;
    private int timesPurchased = 0;

    public float resetTime = 120f; // Cooldown in seconds
    private float resetTimer;

    public Image itemImage;
    public Text priceText;
    public Button buyButton;
    public Text purchaseLimitText;

    void Start()
    {
        itemImage.sprite = item.image;
        
        UpdateLimitDisplay();

        resetTimer = resetTime;
        buyButton.onClick.AddListener(BuyItem);
    }

    void Update()
    {

        priceText.text = "Price: " + GetDiscountedPrice().ToString()+ " gold";
        resetTimer -= Time.deltaTime;
        if (resetTimer <= 0)
        {
            ResetPurchaseLimit();
            resetTimer = resetTime;
        }
    }

    void BuyItem(){

    int finalPrice = Mathf.Max(1, Mathf.RoundToInt(price - (price * PlayerStats.GetTradingDiscountMultiplier())));

    if (timesPurchased >= maxPurchaseAmount)
    {
        Debug.Log("Purchase limit reached for " + item.itemName);
        return;
    }

    
    int playerGold = InventoryManager.Instance.GetTotalGold();
    if (playerGold < finalPrice)
    {
        Debug.Log("Not enough gold to buy " + item.itemName);
        return;
    }

    
    InventoryManager.Instance.SpendGold(finalPrice);
    InventoryManager.Instance.AddItem(item);

    
    timesPurchased++;
    UpdateLimitDisplay();

   
    if (timesPurchased >= maxPurchaseAmount)
    {
        buyButton.interactable = false;
    }

    Debug.Log($"Bought {item.itemName} for {finalPrice} gold (Trading Level: {PlayerStats.Trading})");
}


    float GetDiscountedPrice(){
        float discount = price * PlayerStats.GetTradingDiscountMultiplier();
        int finalPrice = Mathf.Max(1, Mathf.RoundToInt(price - discount)); // Never 0 or negative
        return finalPrice;
    }

    void UpdateLimitDisplay()
    {
        if (purchaseLimitText != null)
        {
            purchaseLimitText.text = $"Purchased: {timesPurchased}/{maxPurchaseAmount}";
        }
    }

    void ResetPurchaseLimit()
    {
        timesPurchased = 0;
        UpdateLimitDisplay();
        buyButton.interactable = true;

        Debug.Log($"Reset purchase limit for {item.itemName}");
    }
}
