using UnityEngine;

public class TreasureRoom : MonoBehaviour
{
    [Header("Spawn Locations")]
    public Transform leftPoint;
    public Transform rightPoint;

    [Header("UI Reference")]
    public GameObject floatingUiPrefab; 

    private GameObject leftObj;
    private GameObject rightObj;
    private bool choiceMade = false;

    void Start()
    {
        SpawnChoices();
    }

    void SpawnChoices()
    {
        if (RelicDatabase.Instance == null) 
            return;
        var rng = new System.Random();

        GameObject prefab1 = RelicDatabase.Instance.GetRandomRelic(rng);
        GameObject prefab2 = RelicDatabase.Instance.GetRandomRelic(rng);

        // Remove the same choices' condition
        int safeguard = 0;
        while (prefab1 == prefab2 && safeguard < 10)
        {
            prefab2 = RelicDatabase.Instance.GetRandomRelic(rng);
            safeguard++;
        }

        
        if (prefab1 != null) leftObj = SpawnItemWithUI(prefab1, leftPoint);
        if (prefab2 != null) rightObj = SpawnItemWithUI(prefab2, rightPoint);
        Debug.Log($"TreasureRoom pool count = {RelicDatabase.Instance.allRelics.Count}");
        Debug.Log($"prefab1={(prefab1 ? prefab1.name : "NULL")} prefab2={(prefab2 ? prefab2.name : "NULL")}");
    }

    GameObject SpawnItemWithUI(GameObject prefab, Transform pos)
    {
        // Spawn relic
        GameObject obj = Instantiate(prefab, pos.position, Quaternion.identity);

        // Spawn UI
        if (floatingUiPrefab)
        {
            GameObject ui = Instantiate(floatingUiPrefab, obj.transform.position + Vector3.up * 0.8f, Quaternion.identity);
            ui.transform.SetParent(obj.transform); 

            FloatingInfo info = ui.GetComponent<FloatingInfo>();
            RelicItem data = prefab.GetComponent<RelicItem>();

            if (info != null && data != null)
            {
                // This is treasure room, don't need price
                info.Setup(data.relicName, data.description, -1);
            }
        }
        return obj;
    }

    void Update()
    {
        if (choiceMade) 
            return;

        // Witness whether player pick a relic, if player picked, hide another one
        if (IsPickedUp(leftObj))
        {
            ConfirmChoice(leftObj, rightObj);
        }
        else if (IsPickedUp(rightObj))
        {
            ConfirmChoice(rightObj, leftObj);
        }
    }

    bool IsPickedUp(GameObject obj)
    {
        if (obj == null) 
            return false;
        
        var owner = obj.GetComponent<ProjectileOwner>();
        
        return owner != null && owner.isHeld;
    }

    void ConfirmChoice(GameObject picked, GameObject other)
    {
        choiceMade = true;

        // Destroy another one
        if (other != null)
        {
            
            Destroy(other);
        }

        // Mark this relic as obtained
        RelicItem data = picked.GetComponent<RelicItem>();
        if (data)
        {
            RelicDatabase.Instance.MarkRelicAsObtained(data.relicName);
        }

        var ui = picked.GetComponentInChildren<FloatingInfo>();
        if (ui != null) Destroy(ui.gameObject);
    }
}
