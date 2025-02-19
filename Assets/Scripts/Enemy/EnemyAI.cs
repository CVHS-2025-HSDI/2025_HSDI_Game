using UnityEngine;
using System.Collections;

public class EnemyAI : MonoBehaviour
{
    private Rigidbody2D rb;
    private GameObject playerTarget;
    public GameObject weaponPrefab; // assign in Inspector
    private GameObject weapon;
    public float moveSpeed = 1.5f;
    private string state = "Passive";
    private float health = 25f;
    
    // New: Freeze flag
    public bool isFrozen = false;
    
    private SpriteRenderer sr;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();

        // Find the player in the scene
        playerTarget = GameObject.FindWithTag("Player");
        
        // Instantiate weapon and set parent
        weapon = Instantiate(weaponPrefab, transform.position, weaponPrefab.transform.rotation);
        weapon.transform.SetParent(transform);
        if (weapon.GetComponent("SwordScript") != null)
        {
            ((SwordScript)weapon.GetComponent("SwordScript")).setIsPlayer(false);
        }
    }

    void Update()
    {
        if (isFrozen)
            return; // If frozen, skip any behavior

        if (state.Equals("Passive"))
        {
            // Example patrol logic can be added here
            // Check if the player is within 3 units; if so, switch to Aggro
            if (playerTarget != null &&
                Mathf.Abs(playerTarget.transform.position.x - transform.position.x) <= 3 &&
                Mathf.Abs(playerTarget.transform.position.y - transform.position.y) <= 3)
            {
                changeState("Aggro");
            }
        }
    }

    void FixedUpdate()
    {
        if (isFrozen)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        if (state.Equals("Aggro") && playerTarget != null)
        {
            Vector2 targetPos = playerTarget.transform.position;
            Vector2 newPos = Vector2.MoveTowards(rb.position, targetPos, moveSpeed * Time.fixedDeltaTime);
            rb.MovePosition(newPos);
        }
    }

    public void changeState(string newState)
    {
        if (weapon.GetComponent("SwordScript") != null)
        {
            SwordScript ws = (SwordScript)weapon.GetComponent("SwordScript");
            if (newState.Equals("Passive"))
            {
                state = newState;
                ws.setTarget(null);
            }
            else if (newState.Equals("Aggro"))
            {
                state = newState;
                ws.setTarget(playerTarget);
            }
            else if (newState.Equals("Stagger"))
            {
                state = newState;
                ws.setTarget(null);
            }
        }
    }

    public void damage(float dmg)
    {
        health -= dmg;
        Debug.Log("Enemy hit for " + dmg);
        if (health <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        // Freeze enemy and fade out its sprite before destroying
        StartCoroutine(FreezeAndFadeOut());
    }

    private IEnumerator FreezeAndFadeOut()
    {
        isFrozen = true;
        float fadeDuration = 2f;
        float timer = 0f;
        Color initialColor = sr.color;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float alpha = Mathf.Lerp(initialColor.a, 0f, timer / fadeDuration);
            sr.color = new Color(initialColor.r, initialColor.g, initialColor.b, alpha);
            yield return null;
        }
        Destroy(gameObject);
    }
}
