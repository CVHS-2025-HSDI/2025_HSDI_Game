using UnityEngine;
using System;
using System.Collections.Generic;   // NEW

public class KeyManager : MonoBehaviour
{
    public static KeyManager Instance;

    [Header("Key settings")]
    public int keysNeeded = 4;

    // how many unique keys the player has so far
    public int KeysCollected => _collectedKeyIds.Count;

    // event when target reached
    public event Action OnAllKeysCollected;
    
    /// <summary>IDs of every key that has ever been picked up on this floor.</summary>
    private readonly HashSet<int> _collectedKeyIds = new HashSet<int>();
    // ----------------------------------------------------------------

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Called by KeyCollectible whenever the player collides with a key.
    /// </summary>
    public void CollectKey(int keyId)
    {
        // Ignore repeats of the *same* key
        if (!_collectedKeyIds.Add(keyId)) return;

        Debug.Log($"Key {keyId} collected   ({KeysCollected}/{keysNeeded})");

        if (KeysCollected >= keysNeeded)
            OnAllKeysCollected?.Invoke();
    }

    /// <summary>Call when a new floor is generated.</summary>
    public void ResetKeys()
    {
        _collectedKeyIds.Clear();
    }
}