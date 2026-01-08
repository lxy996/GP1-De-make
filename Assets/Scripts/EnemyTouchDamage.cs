using UnityEngine;
public class EnemyTouchDamage : MonoBehaviour
{

    public TeamTag teamTag;

    public float damage = 1f;
    //public float damageInterval = 0.5f; // Enemy collision damage cooldown

    //private float nextDamageTime;       // player calculates invincibility frames, the enemy don't need a cooldown.
    private void Awake()
    {
        if (teamTag == null)
            teamTag = GetComponent<TeamTag>();
    }

    private void OnTriggerEnter(Collider other)
    {
        TryDealDamage(other);
    }
    private void OnTriggerStay(Collider other)
    {
        TryDealDamage(other);
    }

    private void TryDealDamage(Collider other)
    {
        //if (Time.time < nextDamageTime) 
        //    return;

        if (other.transform.IsChildOf(transform)) // Avoid causing damage to itself or its child objects
            return;

        TeamTag otherTeam = other.GetComponentInParent<TeamTag>();
        if (teamTag != null && otherTeam != null && otherTeam.team == teamTag.team) 
            return;
        if (otherTeam == null || otherTeam.team != Team.Player) // Only cause damage to the player
            return;

        IDamageable damageable = other.GetComponentInParent<IDamageable>();
        if (damageable == null)
            return;

        //nextDamageTime = Time.time + damageInterval;  // Calculate damage cooldown

        Vector3 hitPoint = other.ClosestPoint(transform.position); 
        Vector3 hitForce = Vector3.zero; // Players do not need to be knocked back

        damageable.TakeDamage(damage, hitPoint, hitForce, gameObject);
    }
}
