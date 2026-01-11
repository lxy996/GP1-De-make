using UnityEngine;

public class BreakAfterCollision : MonoBehaviour
{
    [Header("References")]
    public GameObject breakVfx;
    public AudioClip breakSfx;
    public AudioSource audioSource;

    public ProjectileDamage damage;

    void Awake()
    {
        damage = GetComponent<ProjectileDamage>();

    }

    void OnCollisionEnter(Collision collision)
    {
        if (damage == null) return;
        if (damage.armed == false) return;

        if (breakVfx != null)
        {
            Instantiate(breakVfx, transform.position, Quaternion.identity);
        }
            
        if (audioSource != null && breakSfx != null )
        {
            audioSource.PlayOneShot(breakSfx);
        }

        EnemyHealth health = GetComponent<EnemyHealth>();
        if (health != null)
        {
            health.TakeDamage(9999f, transform.position, Vector3.zero, gameObject);
        }
        else
        {

            Destroy(gameObject);
        }

    }

}
