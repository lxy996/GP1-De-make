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
