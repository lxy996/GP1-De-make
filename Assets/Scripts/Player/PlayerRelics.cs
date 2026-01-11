using UnityEngine;

public class PlayerRelics : MonoBehaviour
{
    [Header("Abilities")]
    public bool canPickFurniture = false;
    public bool hasFireballRelic = false;

    [Header("Convert Chance")]
    [Range(0f, 1f)] public float convertToBombChance = 0f;   // Convert normal ball to bomb
    [Range(0f, 1f)] public float convertToPotionChance = 0f; // Convert normal ball to health potion
    public float basicBombChance = 0.2f;
    public float basicPotionChance = 0.1f;
    public float luckUpEffect = 0.2f;
    public Rigidbody bombPrefab;
    public Rigidbody potionPrefab;

    [Header("Damage")]
    public float attackMultiplier = 1f;
    public float attackBonus = 0f;

    [Header("Fireball Relic Settings")]
    public Rigidbody fireballPrefab;
    public float fireballInterval = 5.0f;
    private float fireballTimer;

    private HandGrab handGrab;
    private PlayerHealth playerHealth;

    void Awake()
    {
        handGrab = GetComponent<HandGrab>();
        playerHealth = GetComponent<PlayerHealth>();
    }
    void Update()
    {
        // Auto fireball
        if (hasFireballRelic)
        {
            AutoFireball();
        }
    }

    public void UnlockRelic(RelicType type)
    {
        switch (type)
        {
            case RelicType.HealthBoost:
                if (playerHealth != null)
                {
                    playerHealth.maxHP += 2; 
                    playerHealth.BeHealth(); 
                }
                break;

            case RelicType.AttackBoost:
                attackMultiplier += 0.5f; 
                break;

            case RelicType.FurnitureGrabber:
                canPickFurniture = true;
                break;

            case RelicType.AutoFireball:
                hasFireballRelic = true;
                fireballTimer = fireballInterval; 
                break;

            case RelicType.ConvertToBomb:
                
                if (convertToBombChance <= 0) convertToBombChance = basicBombChance;
                else convertToBombChance += 0.1f; 
                break;

            case RelicType.ConvertToPotion:
               
                if (convertToPotionChance <= 0) convertToPotionChance = basicPotionChance;
                else convertToPotionChance += 0.05f;
                break;

            case RelicType.RelicLuckUp:
                
                if (convertToBombChance > 0) convertToBombChance += luckUpEffect;
                if (convertToPotionChance > 0) convertToPotionChance += luckUpEffect;
                break;
        }

        Debug.Log($"Unlocked Relic: {type}");
    }

    private void AutoFireball()
    {
        fireballTimer -= Time.deltaTime;

        if (fireballTimer <= 0f)
        {
           
            if (handGrab != null && fireballPrefab != null)
            {
                // Check whether right hand is empty
                if (handGrab.TrySpawnItemInRightHand(fireballPrefab))
                {
                    fireballTimer = fireballInterval; 
                }
            }
        }
    }
}
