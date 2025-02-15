using UnityEngine;
using System;

public class KeyManager : MonoBehaviour
{
    public static KeyManager Instance;

    public int keysNeeded = 4; // Number of keys per floor
    public int keysCollected = 0;

    // Event fired when all keys are collected
    public event Action OnAllKeysCollected;

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

    public void CollectKey()
    {
        keysCollected++;
        Debug.Log("Key collected: " + keysCollected);
        if (keysCollected >= keysNeeded)
        {
            OnAllKeysCollected?.Invoke();
        }
    }

    // Call this when a new floor is generated
    public void ResetKeys()
    {
        keysCollected = 0;
    }
}