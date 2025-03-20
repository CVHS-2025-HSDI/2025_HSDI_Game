using UnityEngine;
using UnityEngine.SceneManagement;

public class SwordController : MonoBehaviour
{
    public Transform pivotPoint; // The object that rotates
    public Transform swordTip;   // The point at the tip of the sword
    public GameObject swingEffectPrefab; // Prefab for the swing effect
    public float attackCooldown = 0.5f;
    private bool canAttack = true;

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
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0f;

        Vector3 direction = (mousePos - pivotPoint.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // If your sprite points 'up' in its default orientation, you might subtract 90
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