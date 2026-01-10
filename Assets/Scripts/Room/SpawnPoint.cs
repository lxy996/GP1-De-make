using System.Collections.Generic;
using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    [Header("Position")]
    public float baseY = 0f;

    [Header("Spawn List")]
    public List<SpawnEntry> entries = new List<SpawnEntry>();  // The List of items that can be instantiated

    [Header("Overlap Check")]
    public LayerMask blockingMask;
    public int maxPrefabRerollAttempts = 5;   // Number of times that can reroll after discovering overlap

    private readonly Collider[] overlapCache = new Collider[16]; //A temporary storage for physics overlap checks.

    // Execute the Spawn task based on the Spawn weight
    public GameObject Spawn(System.Random rng)
    {
        if (entries == null || entries.Count == 0) 
            return null;

        for (int attempt = 0; attempt < maxPrefabRerollAttempts; attempt++)
        {

            // Execute the Spawn task based on the Spawn weight
            SpawnEntry chosen = PickEntryByWeight(rng);
            if (chosen == null || chosen.prefab == null) 
                return null;

            Vector3 spawnPos = new Vector3(
                transform.position.x,
                baseY + chosen.yOffset, // Calculate the position.y based on the type of object to be spawned.
                transform.position.z
            );

            if (IsBlocked(spawnPos, chosen.blockRadius))
            {
                // If there is overlap problem, try to reroll
                continue;
            }
            
            return Instantiate(chosen.prefab, spawnPos, transform.rotation);

        }


        return null;
    }

    private bool IsBlocked(Vector3 position, float radius)
    {
        // Check physics overlap and write into temporary storage
        int hitCount = Physics.OverlapSphereNonAlloc(
            position,
            radius,
            overlapCache,
            blockingMask,
            QueryTriggerInteraction.Ignore  // Ignore the triggers
        );

        return hitCount > 0;
    }

    // Pick the items based on the Spawn weight
    private SpawnEntry PickEntryByWeight(System.Random rng)
    {
        float total = 0f;
        // calculate the total weight
        for (int i = 0; i < entries.Count; i++)
        {
            SpawnEntry e = entries[i];
            if (e == null || e.prefab == null) 
                continue;
            if (e.weight <= 0f) 
                continue;
            total += e.weight;
        }
        if (total <= 0f) 
            return null;

        // Roll the dice to get a random number
        double roll = rng.NextDouble() * total;
        float accum = 0f;

        for (int i = 0; i < entries.Count; i++)
        {
            SpawnEntry e = entries[i];
            if (e == null || e.prefab == null || e.weight <= 0f) 
                continue;

            // Check if the random roll falls within the weight range of the prefab
            accum += e.weight;
            if (roll <= accum) 
                return e;
        }

        return null;
    }
}
