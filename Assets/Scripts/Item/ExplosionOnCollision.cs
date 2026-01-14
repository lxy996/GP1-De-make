using UnityEngine;
using System.Collections;

public class ExplosionOnCollision : MonoBehaviour
{
    [Header("References")]
    public GameObject vfx;
    public AudioClip sfx;
    public AudioSource audioSource;
    public ProjectileOwner projectileOwner;
    public ProjectileDamage projectileDamage;

    [Header("Settings")]
    public float delay = 10f;
    public float radius = 3f;
    public float damage = 2f;
    public LayerMask damageMask;

    private bool hasStartedCountdown = false;

    void Start()
    {
        if (projectileOwner == null)
            projectileOwner = GetComponent<ProjectileOwner>();
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (!projectileDamage.armed)
            return;

        if (!hasStartedCountdown)
        {
            hasStartedCountdown = true;
            StartCoroutine(ExplosionRoutine());
        }
    }
    private IEnumerator ExplosionRoutine()
    {
        yield return new WaitForSeconds(delay);

        if (vfx != null)
        {
            Instantiate(vfx, transform.position, Quaternion.identity);
        }
            
        if (sfx != null)
        {
            AudioSource.PlayClipAtPoint(sfx, transform.position);

        }
            

        Collider[] cols = Physics.OverlapSphere(transform.position, radius, damageMask, QueryTriggerInteraction.Ignore);
        foreach (Collider col in cols)
        {
            TeamTag targetTeam = col.GetComponentInParent<TeamTag>();
            if (projectileOwner != null && targetTeam != null)
            {
                // Don't hurt same team's member
                if (projectileOwner.owner == targetTeam.team)
                    continue;
            }

            IDamageable damageable = col.GetComponentInParent<IDamageable>();
            if (damageable == null) 
                continue;

            Vector3 hitPoint = col.ClosestPoint(transform.position);
            Vector3 hitForce = Vector3.zero;

            damageable.TakeDamage(damage, hitPoint, hitForce, gameObject);
        }

        Destroy(gameObject);
    }
    // Draw the area of explosion
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
