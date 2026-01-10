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

public enum EnemyState
{
    ChasePlayer,
    Attack,
    SeekBall,
    CarryBall,
    Dead
}

[Serializable]
public class SpawnEntry
{
    public GameObject prefab;
    public float weight = 1f;
    public float yOffset = 0f;
    public float blockRadius = 0.5f;
}
