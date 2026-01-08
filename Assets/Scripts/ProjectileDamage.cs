using UnityEngine;

public class ProjectileDamage : MonoBehaviour
{
    [Header("Component References")]
    public ProjectileOwner projectileOwner; 
    private Rigidbody rb;

    [Header("Damage")]
    public float damage = 1f;
    public float minSpeed = 4f;    // No damage will be caused at speeds below this.

    [Header("State")]
    public bool armed = true;      // Holding in hand = false, throwing/launching = true

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (projectileOwner == null)
            projectileOwner = GetComponent<ProjectileOwner>();
    }

    private void OnCollisionEnter(Collision other)
    {
        if (!armed) return;


        // Low speed: usually rolling on the ground, stepping on, minor collision.
        if (rb.linearVelocity.magnitude < minSpeed)
            return;

        // Do not attack members of the same team
        TeamTag targetTeam = other.collider.GetComponentInParent<TeamTag>();
        if (projectileOwner != null && targetTeam != null)
        {
            if (projectileOwner.owner == targetTeam.team)
                return;
        }

        IDamageable damageable = other.collider.GetComponentInParent<IDamageable>();
        if (damageable == null) 
            return;

        Vector3 hitPoint = other.GetContact(0).point;
        Vector3 hitForce = Vector3.zero; 

        damageable.TakeDamage(damage, hitPoint, hitForce, gameObject);
    }
}
