using UnityEngine;
using System.Collections.Generic;

public class Chest : MonoBehaviour
{
   public List<LootItem> loottable = new List<LootItem>();
   public float spawnRadius = 1f;
   public GameObject playerTarget;
   
   void OnTriggerEnter2D(Collider2D collider){


    if(collider.CompareTag("Player")){
        for (int i = 0; i < loottable.Count; i++)
            {
                LootItem item = loottable[i];
                float finalDropChance = item.dropChance;
                if (playerTarget != null)
                    finalDropChance *= PlayerStats.GetDropMultiplier();

                if (Random.Range(0f, 100f) <= finalDropChance)
                {
                    SpawnLootItem(item);
                }
            }

        Destroy(gameObject);
    }

   }

   private void SpawnLootItem(LootItem item)
    {
        Vector2 offset2D = Random.insideUnitCircle * spawnRadius;
        Vector3 spawnPos = transform.position + (Vector3)offset2D;
        Instantiate(item.itemPrefab, spawnPos, Quaternion.identity);
    }
   
    // Update is called once per frame
    void Update()
    {
        
    }
}
