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
    private float sightDistance = 6f;

    // New: Freeze flag
    public bool isFrozen = false;

    private SpriteRenderer sr;

    private float timer = 0f;
    public Vector2 knockbackForceVector;

    [Header("Loot")]
    public List<LootItem> loottable = new List<LootItem>();
    [Tooltip("Max distance from transform.position that items can spawn")]
    public float spawnRadius = 1f;

    // Pathfinding
    private TileGuide tileGuide;
    private Dictionary<Vector3, List<Vector3>> guide = null;
    private bool isPathFinding = false;
    private List<Vector3> path;
    private Vector2 nextPos;
    private int it = 0;

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

        // Get TileGuide
        GameObject tilemap = GameObject.FindWithTag("FloorTileMap");
        if (tilemap == null)
        {
            Debug.Log("Tilemap not found, apply tag");
        }
        else
        {
            tileGuide = (TileGuide)tilemap.GetComponentInParent<TileGuide>();
            if (tileGuide == null)
            {
                Debug.Log("TileGuide script not found, apply to floor grid");
            }
        }
    }

    void Update()
    {
        if (guide == null && tileGuide != null && tileGuide.GetGuide() != null) // Wait for guide to be generated
        {
            guide = tileGuide.GetGuide();
        }

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
            // Check if the player is within 4 units; if so, switch to Aggro
            if (playerTarget != null &&
                hasVision(sightDistance))
            {
                changeState("Aggro");
                Debug.Log("Changing to Aggro");
            }
        }
        if (state.Equals("Aggro"))
        {
            if (playerTarget != null &&
                    !hasVision(sightDistance + 1))
            {
                changeState("Passive");
            }
        }
    }

    void FixedUpdate()
    {

        if (isFrozen && !hasVision(sightDistance + 1))
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        if (state.Equals("Aggro") && playerTarget != null)
        {
            Vector2 targetPos = playerTarget.transform.position;
            Vector2 Pos = transform.position;
            Vector2 PosLeft = new Vector2(Pos.x - ((CircleCollider2D)gameObject.GetComponent("CircleCollider2D")).radius, Pos.y);
            Vector2 PosRight = new Vector2(Pos.x + ((CircleCollider2D)gameObject.GetComponent("CircleCollider2D")).radius, Pos.y);
            Vector2 PosUp = new Vector2(Pos.x, Pos.y + ((CircleCollider2D)gameObject.GetComponent("CircleCollider2D")).radius);
            Vector2 PosDown = new Vector2(Pos.x, Pos.y - ((CircleCollider2D)gameObject.GetComponent("CircleCollider2D")).radius);

            RaycastHit2D hitLeft = Physics2D.Raycast(PosLeft, targetPos - PosLeft);
            RaycastHit2D hitRight = Physics2D.Raycast(PosRight, targetPos - PosRight);
            RaycastHit2D hitUp = Physics2D.Raycast(PosUp, targetPos - PosUp);
            RaycastHit2D hitDown = Physics2D.Raycast(PosDown, targetPos - PosDown);

            bool TargetSight = (hitLeft.collider.tag.Equals("Player") && hitRight.collider.tag.Equals("Player") &&
            hitUp.collider.tag.Equals("Player") && hitDown.collider.tag.Equals("Player"));

            if (isPathFinding) // pathfinding aggro state
            {
                if (path == null)
                {
                    Debug.Log("Null path, exit pathfind");
                    isPathFinding = false;
                    return;
                }
                if ((it == path.Count - 1 && Mathf.Abs(nextPos.x - Pos.x) <= 0.1 && Mathf.Abs(nextPos.y - Pos.y) <= 0.1) || TargetSight) // if desitnation reached or target sighted, exit pathfind
                {
                    isPathFinding = false;
                }

                // move along best route to player
                if (it + 1 < path.Count && Mathf.Abs(nextPos.x - Pos.x) <= 0.1 && Mathf.Abs(nextPos.y - Pos.y) <= 0.1)
                {
                    it++;
                }
                nextPos = path[it];
                Vector2 newPos = Vector2.MoveTowards(rb.position, nextPos, moveSpeed * Time.fixedDeltaTime);
                rb.MovePosition(newPos);
            }
            else // normal aggro state
            {
                if (!TargetSight) // if lost sight of player, enter pathfinding
                {
                    rb.linearVelocity = Vector2.zero;
                    isPathFinding = true;
                    path = FindPathBFS(Pos, targetPos, guide);
                    it = 0;
                    if ((path[0].x > Pos.x && path[1].x < Pos.x) || (path[0].x < Pos.x && path[1].x > Pos.x) ||
                        (path[0].y > Pos.y && path[1].y < Pos.y) || (path[0].y < Pos.y && path[1].y > Pos.y)) // make sure we don't move backwards
                    {
                        it = 1;
                    }
                }

                Vector2 newPos = Vector2.MoveTowards(rb.position, targetPos, moveSpeed * Time.fixedDeltaTime);

                // Apply knockback force if any
                if (knockbackForceVector != Vector2.zero)
                {
                    newPos += knockbackForceVector * Time.fixedDeltaTime;
                    knockbackForceVector = Vector2.Lerp(knockbackForceVector, Vector2.zero, 0.5f); // Gradually reduce the knockback force
                }

                rb.MovePosition(newPos);
            }
        }
        if (state.Equals("Passive"))
        {
            timer += Time.fixedDeltaTime;
            if (isPaused)
            {
                if (timer >= Random.Range(1.5f, 2.5f))
                {
                    isPaused = false;
                    timer = 0f;
                    wander();
                }
            }
            else
            {
                if (timer >= wanderInterval)
                {
                    isPaused = true;
                    timer = 0f;
                    rb.linearVelocity = Vector2.zero;
                }
            }
        }
    }

    void wander()
    {
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

    private bool hasVision(float distance)
    {
        //If Out of Range, Return False.
        if (Vector2.Distance(playerTarget.transform.position, transform.position) > distance)
        {
            return false;
        }

        //Make an all-seeing raycast
        Vector2 direction = (playerTarget.transform.position - transform.position).normalized;
        RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, direction, distance);

        //For each hit, check if the hit object is a wall or not.
        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider.gameObject.CompareTag("Player"))
            {
                // If we hit the player, return true.
                return true;
            }
        }
        return true;
    }

    public List<Vector3> FindPathBFS(Vector3 start, Vector3 goal, Dictionary<Vector3, List<Vector3>> graph)
    {
        Queue<Vector3> queue = new Queue<Vector3>();
        Dictionary<Vector3, Vector3> cameFrom = new Dictionary<Vector3, Vector3>();
        HashSet<Vector3> visited = new HashSet<Vector3>();

        if (start.x >= 0)
        {
            start.x = 0.5f + (float)((int)start.x);
        }
        else
        {
            start.x = -0.5f + (float)((int)start.x);
        }
        if (start.y >= 0)
        {
            start.y = 0.5f + (float)((int)start.y);
        }
        else
        {
            start.y = -0.5f + (float)((int)start.y);
        }

        queue.Enqueue(start);
        visited.Add(start);

        if (goal.x >= 0)
        {
            goal.x = 0.5f + (float)((int)goal.x);
        }
        else
        {
            goal.x = -0.5f + (float)((int)goal.x);
        }
        if (goal.y >= 0)
        {
            goal.y = 0.5f + (float)((int)goal.y);
        }
        else
        {
            goal.y = -0.5f + (float)((int)goal.y);
        }

        while (queue.Count > 0)
        {
            Vector3 current = queue.Dequeue();

            if (current == goal) // If end found, create path from list
            {
                return ReconstructPath(cameFrom, start, goal);
            }

            foreach (Vector3 neighbor in graph[current])
            {
                if (!visited.Contains(neighbor))
                {
                    visited.Add(neighbor);
                    queue.Enqueue(neighbor);
                    cameFrom[neighbor] = current;
                }
            }
        }

        return null; // No path found
    }

    private List<Vector3> ReconstructPath(Dictionary<Vector3, Vector3> cameFrom, Vector3 start, Vector3 goal)
    {
        List<Vector3> path = new List<Vector3>();
        Vector3 current = goal;

        while (current != start)
        {
            path.Add(current);
            current = cameFrom[current];
        }

        path.Add(start);
        path.Reverse();

        return path;
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
