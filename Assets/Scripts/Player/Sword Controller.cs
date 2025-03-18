using UnityEngine;
using UnityEngine.SceneManagement;

public class SwordController : MonoBehaviour
{
    public Transform pivotPoint; // The object that rotates
    public Transform swordTip;   // New: The point at the tip of the sword
    public GameObject swingEffectPrefab; // Prefab for the swing effect
    public float attackCooldown = 0.5f;
    private bool canAttack = true;

    void Update(){
    // Only run in the game scene
    if (SceneManager.GetActiveScene().name != "PersistentManager") return;

    RotateSword();

    if (Input.GetMouseButtonDown(0) && canAttack)
    {
        Attack();
    }
}


   void RotateSword(){

    Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
    mousePos.z = 0f;

    Vector3 direction = (mousePos - pivotPoint.position).normalized;
    float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
    pivotPoint.rotation = Quaternion.Euler(0, 0, angle);
}


    void Attack()
    {
        canAttack = false;
        Invoke(nameof(ResetAttack), attackCooldown);

        if (swingEffectPrefab && swordTip)
        {
           Quaternion effectRotation = pivotPoint.rotation * Quaternion.Euler(0, 0, 0);
           Instantiate(swingEffectPrefab, swordTip.position, effectRotation);

        }
    }

    void ResetAttack()
    {
        canAttack = true;
    }
}
