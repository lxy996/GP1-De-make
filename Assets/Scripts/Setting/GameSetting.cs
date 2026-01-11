using System;
using UnityEngine;
public enum Team
{
    Player,
    Enemy,
    Neutral
}
public enum RelicType
{
    HealthBoost,        // Increase maximum health
    AttackBoost,        // Increase attack power
    AutoFireball,       // Generate fireballs on right hand regularly
    ConvertToBomb,        // Convert ball to bomb
    ConvertToPotion,      // Convert ball to health potion
    FurnitureGrabber,   // Allow grabbing furniture
    RelicLuckUp         // Increase the chance of turning into a bomb/health potion

}
public interface IDamageable
{
    void TakeDamage(float amount, Vector3 hitPoint, Vector3 hitForce, GameObject source);

}

public interface IHandUsable
{
    // Return true means that the item has been consumed, and HandGrab should clear the holding data
    bool Use(HandGrab grabber, bool isLeftHand);
}

public interface IInteractable
{
    string GetInteractPrompt(); // Tell player the effect of interaction 
    void OnInteract(GameObject interactor); 
}


[Serializable]
public class SpawnEntry
{
    public GameObject prefab;
    public float weight = 1f;
    public float yOffset = 0f;
    public float blockRadius = 0.5f;
}

[System.Serializable]
public class GameStats
{
    public float startTime;
    public float endTime;
    public int throwCount;
    public int killCount;

    public float Duration => endTime - startTime;

    public void Reset()
    {
        startTime = UnityEngine.Time.time;
        throwCount = 0;
        killCount = 0;
    }
}