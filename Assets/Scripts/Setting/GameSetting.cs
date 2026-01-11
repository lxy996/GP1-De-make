using System;
using UnityEngine;
public enum Team
{
    Player,
    Enemy,
    Neutral
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


[Serializable]
public class SpawnEntry
{
    public GameObject prefab;
    public float weight = 1f;
    public float yOffset = 0f;
    public float blockRadius = 0.5f;
}
