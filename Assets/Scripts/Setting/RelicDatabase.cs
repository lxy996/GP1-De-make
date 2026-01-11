using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class RelicDatabase : MonoBehaviour
{
    public static RelicDatabase Instance;

    [Header("Relic List")]
    public List<GameObject> allRelics; // 在Inspector把所有遗物Prefab拖进去

    // Record the relics that player has obtained
    private HashSet<string> obtainedRelics = new HashSet<string>();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // Used for other script to spawn random relic from relic list
    public GameObject GetRandomRelic(System.Random rng)
    {  

        List<GameObject> validPool = new List<GameObject>();

        // Find the relics that player didn't obtain
        foreach (var prefab in allRelics)
        {
            RelicItem item = prefab.GetComponent<RelicItem>();

            if (item == null) 
                continue;

            if (item.relicType == RelicType.HealthBoost || item.relicType == RelicType.AttackBoost)
            {
                validPool.Add(prefab);
            }

            else if (!obtainedRelics.Contains(item.relicName))
            {
                validPool.Add(prefab);
            }
        }

        // If the pool is empty
        if (validPool.Count == 0)
        {
            
            foreach (var r in allRelics)
            {
                var item = r.GetComponent<RelicItem>();
                if (item.relicType == RelicType.HealthBoost || item.relicType == RelicType.AttackBoost)
                {
                    validPool.Add(r);
                }
            }
        }

        //  
        if (validPool.Count > 0)
        {
            int index = rng.Next(validPool.Count);
            return validPool[index];
        }

        return null;
    }

    // Mark which relic has been obtained
    public void MarkRelicAsObtained(string relicName)
    {
        obtainedRelics.Add(relicName);
    }
}
