using UnityEngine;

public class PlayerRelics : MonoBehaviour
{
    [Header("Abilities")]
    public bool canPickFurniture = false;

    [Header("Convert Chance")]
    [Range(0f, 1f)] public float convertToBombChance = 0f;   // Convert normal ball to bomb
    [Range(0f, 1f)] public float convertToPotionChance = 0f; // Convert normal ball to health potion
    public Rigidbody bombPrefab;
    public Rigidbody potionPrefab;

    [Header("Damage")]
    public float projectileDamageMultiplier = 1f;
    public float projectileDamageBonus = 0f;


}
