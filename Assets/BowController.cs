using UnityEngine;
using UnityEngine.SceneManagement;

public class BowController : MonoBehaviour {
    [Header("References")]
    public Transform pivotPoint;
    public Transform arrowSpawnPoint;
    public GameObject arrowPrefab;

    [Header("Bow Stats")]
    public float damage;
    public float attackSpeed;
    public int durability;

    [HideInInspector]
    public InventoryItem inventoryItem;

    private float attackCooldown => 1f / attackSpeed;
    private bool canShoot = true;

    private Camera mainCam;

    void Start() {
        mainCam = Camera.main ?? SingletonManager.Instance?.mainCamera;
        if (!mainCam) Debug.LogWarning("BowController: Main camera not assigned.");
    }

    void Update() {
        if (!IsScenePlayable()) return;

        RotateBow();

        if (Input.GetMouseButtonDown(0))
            Debug.Log("Mouse click detected.");

        if (Input.GetMouseButtonDown(0) && canShoot && durability > 0)
            ShootArrow();
    }

    void RotateBow() {
        if (!mainCam) return;

        Vector3 mousePos = mainCam.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;
        Vector3 direction = (mousePos - pivotPoint.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        pivotPoint.rotation = Quaternion.Euler(0, 0, angle);
    }

    void ShootArrow() {
    if (!InventoryManager.Instance.HasItem("Arrow")) {
        Debug.Log("No arrows left!");
        return;
    }

    canShoot = false;
    Invoke(nameof(ResetShoot), attackCooldown);

    float zAngle = pivotPoint.eulerAngles.z + 48f;
    Vector2 shootDirection = new Vector2(Mathf.Cos(zAngle * Mathf.Deg2Rad), Mathf.Sin(zAngle * Mathf.Deg2Rad)).normalized;

    GameObject arrow = Instantiate(
        arrowPrefab,
        arrowSpawnPoint.position,
        Quaternion.Euler(0, 0, zAngle)
    );

    if (arrow.TryGetComponent(out Arrow arrowScript)) {
        arrowScript.damage = damage;
        arrowScript.Fire(shootDirection);
    }

    durability--;
    InventoryManager.Instance.ReduceStackOrRemove("Arrow");

    if (inventoryItem != null) {
        inventoryItem.durability = durability;
        inventoryItem.UpdateDurabilitySlider();
    }

    if (durability <= 0) BreakBow();
}

    void ResetShoot() => canShoot = true;

    void BreakBow() {
        Debug.Log("The bow has broken!");
        InventoryManager.Instance.RemoveSelectedItemFromInventory();
        Destroy(gameObject);
    }

    bool IsScenePlayable() => !SceneManager.GetSceneByName("MainMenu").isLoaded && SceneManager.GetActiveScene().name == "PersistentManager";
}
