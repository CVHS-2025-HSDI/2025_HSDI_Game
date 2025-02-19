using UnityEngine;
using System.Collections.Generic;

public class BossManager : MonoBehaviour
{
    public static BossManager Instance;
    
    [Header("Boss Prefabs")]
    public GameObject[] bossPrefabs; // Assign 8 boss prefabs in the Inspector

    private List<int> usedBossIndices = new List<int>();

    void Awake()
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

    public GameObject GetNextBossPrefab()
    {
        List<int> available = new List<int>();
        for (int i = 0; i < bossPrefabs.Length; i++)
        {
            if (!usedBossIndices.Contains(i))
                available.Add(i);
        }
        if (available.Count == 0)
        {
            // Reset used indices if all have been used.
            usedBossIndices.Clear();
            for (int i = 0; i < bossPrefabs.Length; i++)
            {
                available.Add(i);
            }
        }
        int randomIndex = available[Random.Range(0, available.Count)];
        usedBossIndices.Add(randomIndex);
        return bossPrefabs[randomIndex];
    }
}