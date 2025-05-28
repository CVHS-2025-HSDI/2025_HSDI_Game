using UnityEngine;
using UnityEngine.SceneManagement;

public enum StairType
{
    Down, // Goes down one floor (or exits if on floor #1)
    Up    // Goes up one floor (locked until all keys are collected)
}

[RequireComponent(typeof(Collider2D))]
public class StairController : MonoBehaviour
{
    [Header("Stair Info")]
    public StairType stairType;
    public int currentFloor;    // Set by the generator
    public int totalFloors = 5; // Set by the generator

    [Header("Visuals")]
    public Sprite lockedSprite;
    public Sprite unlockedSprite;

    private MasterLevelManager _manager;
    public bool _unlocked;
    private SpriteRenderer _sr;
    private Collider2D _col;

    void Start()
    {
        _manager = FindFirstObjectByType<MasterLevelManager>();
        _sr = GetComponent<SpriteRenderer>();
        _col = GetComponent<Collider2D>();

        // For Up stairs, if the current floor has already been traversed (i.e. keys used),
        // then immediately unlock the stair.
        if (stairType == StairType.Up)
        {
            if (MasterLevelManager.Instance.IsFloorTraversable(currentFloor))
            {
                UnlockStair();
            }
            else
            {
                LockStair();
                // Subscribe to key event so that once all keys are collected, we unlock.
                if (KeyManager.Instance != null)
                    KeyManager.Instance.OnAllKeysCollected += UnlockStair;
            }
        }
        else
        {
            UnlockStair();
        }
    }

    private void LockStair()
    {
        _unlocked = false;
        if (_sr != null && lockedSprite != null)
            _sr.sprite = lockedSprite;
    }

    private void UnlockStair()
    {
        _unlocked = true;
        if (_sr != null && unlockedSprite != null)
            _sr.sprite = unlockedSprite;
        if (_col != null)
            _col.enabled = true;
        // Unsubscribe so that this only happens once per floor.
        if (KeyManager.Instance != null)
            KeyManager.Instance.OnAllKeysCollected -= UnlockStair;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        // For Up stairs, only trigger if unlocked.
        if (stairType == StairType.Up && !_unlocked)
            return;

        Debug.Log($"[StairController] Player stepped on {stairType} stair, floor {currentFloor}.");

        if (stairType == StairType.Down)
        {
            if (currentFloor == 1)
            {
                Debug.Log("Exiting the tower...");
                SingletonManager.Instance.player.gameObject.transform.position = new Vector3(-149.5f, 5f, 0);
            }
            else
            {
                int newFloor = currentFloor - 1;
                _manager.GenerateAndLoadFloor(newFloor, newFloor == 1);
            }
        }
        else // StairType.Up
        {
            if (currentFloor >= totalFloors)
            {
                Debug.Log("No more floors above!");
            }
            else
            {
                int newFloor = currentFloor + 1;
                // Only purge keys if this is a new highest floor.
                if (newFloor > MasterLevelManager.Instance.highestFloorReached)
                {
                    InventoryManager invManager = FindFirstObjectByType<InventoryManager>();
                    if (invManager != null)
                    {
                        invManager.PurgeKeys();
                    }
                    MasterLevelManager.Instance.MarkCurrentFloorTraversable();
                    MasterLevelManager.Instance.highestFloorReached = newFloor;
                    Debug.Log($"New highest floor reached: {newFloor}. Keys purged.");
                }
                else
                {
                    Debug.Log("Revisiting an existing floor; keys are preserved.");
                }
                _manager.GenerateAndLoadFloor(newFloor, newFloor == 1);
            }
        }
    }
}
