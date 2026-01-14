using UnityEngine;

public class BreakableDoor : EnemyHealth
{
    [Header("Breakable Door Settings")]
    public GameObject fracturedPrefab;
    public float explosionForce = 10f;

    [Header("Feedback")]
    public AudioSource audioSource;
    public AudioClip hitSound;
    public AudioClip deathSound;
    public GameObject breakVfx;

    protected override void Die()
    {
        
        if (fracturedPrefab != null)
        {
            GameObject debris = Instantiate(fracturedPrefab, transform.position, transform.rotation);

            Rigidbody[] rbs = debris.GetComponentsInChildren<Rigidbody>();
            foreach (var rb in rbs)
            {
                
                rb.AddExplosionForce(explosionForce * 10f, transform.position, 2f);
            }

            Destroy(debris, 5f);
        }

        if (feedback != null) feedback.PlayDeath();

        AudioSource.PlayClipAtPoint(deathSound, transform.position);

        Destroy(gameObject);
    }
}
