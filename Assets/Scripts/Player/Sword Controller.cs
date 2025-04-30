using UnityEngine;
using UnityEngine.SceneManagement;

public class SwordController : MonoBehaviour
{
    public Transform pivotPoint; // The object that rotates
    public Transform swordTip;   // The point at the tip of the sword
    public GameObject swingEffectPrefab; // Prefab for the swing effect
    public float attackCooldown = 0.5f;
    private bool canAttack = true;
    
    void Awake() {
        if (SingletonManager.Instance.mainCamera == null)
            Debug.LogError("SingletonManager mainCamera is not assigned!");
        else
            Debug.Log("SingletonManager mainCamera is active: " + SingletonManager.Instance.mainCamera.gameObject.activeInHierarchy);
    }

    void Update()
    {
        // Return if the MainMenu scene is loaded or if the active scene is not PersistentManager.
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
        // Use the camera from SingletonManager if Camera.main is null.
        Camera cam = Camera.main;
        if (cam == null && SingletonManager.Instance != null)
        {
            SingletonManager.Instance.mainCamera.gameObject.SetActive(true);
            cam = SingletonManager.Instance.mainCamera;
            if (cam == null)
            {
                Debug.LogError("No camera available for RotateSword!");
                return;
            }
        }

        Vector3 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0f;

        if (pivotPoint == null)
        {
            Debug.LogError("PivotPoint is null in RotateSword!");
            return;
        }
    
        Vector3 direction = (mousePos - pivotPoint.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        angle -= 135;

        pivotPoint.rotation = Quaternion.Euler(0, 0, angle);
    }

    void Attack()
    {
        canAttack = false;
        Invoke(nameof(ResetAttack), attackCooldown);

        if (swingEffectPrefab && swordTip)
        {
            // Spawn swing effect as a child of pivotPoint so it follows movement
            Instantiate(swingEffectPrefab, swordTip.position, pivotPoint.rotation, pivotPoint);
        }
    }

    void ResetAttack()
    {
        canAttack = true;
    }
}