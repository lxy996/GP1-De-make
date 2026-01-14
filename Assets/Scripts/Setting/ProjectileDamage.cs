using UnityEngine;

public class ProjectileDamage : MonoBehaviour
{
    [Header("Component References")]
    public ProjectileOwner projectileOwner; 
    private Rigidbody rb;
    public GameObject collisionEffect;
    public AudioClip collisionClip;
    public Animator animator;


    [Header("Damage")]
    public float damage = 1f;
    public float minSpeed = 0.5f;    // No damage will be caused at speeds below this.

    [Header("State")]
    public bool armed = false;      // Holding in hand = false, throwing/launching = true

    public string collisionTriggerName = "Collision";
    void Awake()
    {
        
        rb = GetComponent<Rigidbody>();
        if (projectileOwner == null)
            projectileOwner = GetComponent<ProjectileOwner>(); 
    }

    private void OnCollisionEnter(Collision other)
    {

        if (!armed) 
            return;

        // Low speed: usually rolling on the ground, stepping on, minor collision.
        if (rb.linearVelocity.magnitude < minSpeed)
        {
            armed = false;
            return;
        }
            

        ContactPoint contact = other.GetContact(0);
        Vector3 hitPosition = contact.point;
        Quaternion hitRotation = Quaternion.identity;

        if(animator != null) animator.SetTrigger(collisionTriggerName);
        if (collisionEffect != null)
        {
            GameObject vfx = Instantiate(collisionEffect, gameObject.transform.position, Quaternion.identity);
            Destroy(vfx, 1f);
        }
        if (collisionClip != null) AudioSource.PlayClipAtPoint(collisionClip, hitPosition);

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
