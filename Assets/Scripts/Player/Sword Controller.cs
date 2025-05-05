using UnityEngine;
using UnityEngine.SceneManagement;

public class SwordController : MonoBehaviour
{[Header("References")]
    public Transform pivotPoint;
    public Transform swordTip;
    public GameObject swingEffectPrefab;

    [Header("Sword Stats")]
    public float damage; 
    public float attackSpeed; // attacks per second
    public int durability; 
    private float attackCooldown => 1f / attackSpeed;
    private bool canAttack = true;

    public InventoryItem inventoryItem;


    void Update()
    {
        if (SceneManager.GetSceneByName("MainMenu").isLoaded || SceneManager.GetActiveScene().name != "PersistentManager")
            return;

        RotateSword();

        if (Input.GetMouseButtonDown(0) && canAttack)
        {
            Attack();
        }
    }

    void RotateSword()
    {
        Camera cam = Camera.main ?? SingletonManager.Instance?.mainCamera;
        if (!cam) return;

        Vector3 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0f;

        Vector3 direction = (mousePos - pivotPoint.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        pivotPoint.rotation = Quaternion.Euler(0, 0, angle - 135);
    }

  void Attack()
{

    canAttack = false;
    Invoke(nameof(ResetAttack), attackCooldown);

    if (swingEffectPrefab && swordTip)
    {
        GameObject swing = Instantiate(swingEffectPrefab, swordTip.position, pivotPoint.rotation, swordTip);
        Swordswing swingScript = swing.GetComponent<Swordswing>();
        if (swingScript){
        swingScript.baseDamage = damage;
        swingScript.swordOwner = this; 
        }
    }
}

public void DecreaseDurability(){
    durability--;
    inventoryItem.durability = durability;
    inventoryItem.UpdateDurabilitySlider();

    if (durability <= 0)
    {
        BreakSword();
    }
}

    void BreakSword()
{
    Debug.Log("The sword has broken!");
    InventoryManager.Instance.RemoveSelectedItemFromInventory();
    Destroy(gameObject); // Destroy the sword GameObject
}


    void ResetAttack()
    {
        canAttack = true;
    }
}