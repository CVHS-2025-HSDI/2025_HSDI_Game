using UnityEngine;
using System.Collections;
using TMPro;
using System.Collections.Generic;
using UnityEngine.Tilemaps;

public class EnemyAI : MonoBehaviour
{
    private Rigidbody2D rb;
    public Sprite[] enemySprites;
    private GameObject playerTarget;
    public GameObject weaponPrefab; // assign in Inspector
    public GameObject DamageTextPrefab;
    private GameObject weapon;
    public float moveSpeed = 1.8f;
    private string state = "Passive";
    private float health = 25f;
    public float wanderSpeed = 1f;
    public float wanderInterval = 6f;
    public float pauseDuration = 1f;
    private bool isPaused = false;
    private Vector2 wanderDirection;

    // New: Freeze flag
    public bool isFrozen = false;

    private SpriteRenderer sr;

    private float timer = 0f;
    public Vector2 knockbackForceVector;

    // For Vision
    private Tilemap tilemap;
    private string wallTileName = "WallSprite";
	private float sightDistance = 6f;


    [Header("Loot")]
    public List<LootItem> loottable = new List<LootItem>();
    [Tooltip("Max distance from transform.position that items can spawn")]
    public float spawnRadius = 1f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        
        if (enemySprites != null && enemySprites.Length > 0)
        {
            int randomIndex = Random.Range(0, enemySprites.Length);
            sr.sprite = enemySprites[randomIndex];
        }
        
        playerTarget = GameObject.FindWithTag("Player");
        weapon = Instantiate(weaponPrefab, transform.position, weaponPrefab.transform.rotation);
        weapon.transform.SetParent(transform);
    
        if (weapon.GetComponent("SwordScript") != null)
        {
            ((SwordScript)weapon.GetComponent("SwordScript")).SetIsPlayer(false);
        }
    
        int currentLevel = 1;
        if (MasterLevelManager.Instance != null)
        {
            currentLevel = MasterLevelManager.Instance.highestFloorReached;
        }
    
        float enemyScalingFactor = 1f + 0.2f * (currentLevel - 1);
        health *= enemyScalingFactor;

        if(FloorGenerator.Instance!= null){
            tilemap = FloorGenerator.Instance.floorTilemap;
        } else {
            Debug.Log("FloorGenerator instance not found. Tilemap will not be used for vision.");
        }

    }

    void Update()
    {
        if (isFrozen)
        {
            if (weapon.activeSelf)
            {
                weapon.SetActive(false);
            }
            return; // If frozen, skip any behavior
        }

        if (state.Equals("Passive"))
        {
            // Example patrol logic can be added here
            // Check if the player is within 4 units; if so, switch to Aggro
            if (playerTarget != null &&
                hasVision(sightDistance))
            {
                changeState("Aggro");
                Debug.Log("Changing to Aggro");
            }
        }
        if(state.Equals("Aggro"))
        {
            if (playerTarget != null &&
                    !hasVision(sightDistance+1))
                {
                    changeState("Passive");
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

                // Apply knockback force if any
                if (knockbackForceVector != Vector2.zero)
                {
                    newPos += knockbackForceVector * Time.fixedDeltaTime;
                    knockbackForceVector = Vector2.Lerp(knockbackForceVector, Vector2.zero, 0.5f);
                }

                rb.MovePosition(newPos);
            }
            if(state.Equals("Passive"))
            {
                timer += Time.fixedDeltaTime;
                if(isPaused)
                {
                    if(timer>=Random.Range(1.5f,2.5f))
                    {
                        isPaused = false;
                        timer = 0f;
                        wander();
                    }
                }
                else
                {
                    if(timer>=wanderInterval)
                    {
                        isPaused = true;
                        timer = 0f;
                        rb.linearVelocity = Vector2.zero;
                    }
                }
            }
        }

        void wander(){
            Vector2 randomDirection = Random.insideUnitCircle.normalized;
            rb.linearVelocity = randomDirection * wanderSpeed;
        }

        public void changeState(string newState)
        {
            if (weapon.GetComponent("SwordScript") != null)
            {
                SwordScript ws = (SwordScript)weapon.GetComponent("SwordScript");
                if (newState.Equals("Passive"))
                {
                    state = newState;
                    ws.SetTarget(null);
                }
                else if (newState.Equals("Aggro"))
                {
                    state = newState;
                    ws.SetTarget(playerTarget);
                }
                else if (newState.Equals("Stagger"))
            {
                state = newState;
                ws.SetTarget(null);
            }
        }
    }

    private bool hasVision(float distance){
        //If Out of Range, Return False.
        if (Vector2.Distance(playerTarget.transform.position, transform.position) > distance){
            return false;
        }
        
        //Make an all-seeing raycast
        Vector2 direction = (playerTarget.transform.position - transform.position).normalized;
        RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, direction, distance);

        //For each hit, check if the hit object is a wall or not.
        foreach (RaycastHit2D hit in hits){

            Vector3 biasedHitPoint = hit.point + (Vector2)(direction * 0.01f);
            Vector3Int tilePosition = tilemap.WorldToCell(biasedHitPoint);
            TileBase hitTile = tilemap.GetTile(tilePosition);
            if (hitTile != null && hitTile.name == wallTileName)
            {
                return false;
            } 
            else if (hit.collider.gameObject.CompareTag("Player"))
            {
                // If we hit the player, return true.
                return true;
            }
        }
        return true;
    }
    public void damage(float dmg)
    {
        health -= dmg;

        //Display damage taken
        GameObject text = Instantiate(DamageTextPrefab, transform.position, Quaternion.identity);
        text.GetComponent<DamageTextScript>().SetTarget(transform);
        TMP_Text textComp = text.GetComponent<TMP_Text>();
        textComp.text = "" + dmg;
        RectTransform textTransform = text.GetComponent<RectTransform>();
        textTransform.position = new Vector2(transform.position.x, transform.position.y + 0.8f);

        Debug.Log("Enemy hit for " + dmg);
        if (health <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        // Award XP to the player.
        int baseXPReward = 50; // Define a base XP reward for this enemy.
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            PlayerXP xpComponent = player.GetComponent<PlayerXP>();
            if (xpComponent != null)
            {
                Debug.Log("Awarding XP to player...");
                xpComponent.AddXP(baseXPReward);
            }
            else
            {
                Debug.LogWarning("PlayerXP component not found on player!");
            }
        
            // Instantiate floating XP text.
            GameObject xpText = Instantiate(SingletonManager.Instance.XPTextPrefab, transform.position, Quaternion.identity);
            XPTextScript xpTextScript = xpText.GetComponent<XPTextScript>();
            if (xpTextScript != null)
            {
                xpTextScript.Setup(baseXPReward);
            }
        }
    
        // Loot drop: always drop the first item and one extra based on chance
        if (loottable.Count > 0)
        {
            SpawnLootItem(loottable[0]);
            for (int i = 1; i < loottable.Count; i++)
            {
                LootItem item = loottable[i];
                float finalDropChance = item.dropChance;
                if (playerTarget != null)
                    finalDropChance *= PlayerStats.GetDropMultiplier();

                if (Random.Range(0f, 100f) <= finalDropChance)
                {
                    SpawnLootItem(item);
                    break;
                }
            }
        }

        // Fade and destroy enemy.
        gameObject.GetComponent<CircleCollider2D>().enabled = false;
        StartCoroutine(FreezeAndFadeOut());
    }

    private void SpawnLootItem(LootItem item)
    {
        Vector2 offset2D = Random.insideUnitCircle * spawnRadius;
        Vector3 spawnPos = transform.position + (Vector3)offset2D;
        Instantiate(item.itemPrefab, spawnPos, Quaternion.identity);
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

    public void SetKnockbackForceVector(Vector2 v)
    {
        knockbackForceVector = v;
    }
}
