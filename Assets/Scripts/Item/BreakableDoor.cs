using UnityEngine;

public class BreakableDoor : MonoBehaviour, IDamageable
{
    [Header("HP")]
    public float maxHP = 1f;
    public float invincibilityDuration = 1.0f;

    [Header("Feedback")]
    public AudioSource audioSource;
    public AudioClip hitSound;
    public AudioClip deathSound;

    public float currentHP;
    private float lastHit;
    public bool requireKey = false;
    void Awake()
    {
        currentHP = maxHP;
        lastHit = -999f;
    }
    public void TakeDamage(float damageAmount, Vector3 hitPoint, Vector3 hitForce, GameObject damageSource)
    {
        if (Time.time - lastHit < invincibilityDuration)
            return;

        if (requireKey == true)
        {
            bool isKey = false;

            if (damageSource != null)
            {
                GrabbableItem g = damageSource.GetComponent<GrabbableItem>();
                if (g != null && g.type == GrabbableType.Key)
                    isKey = true;
            }

            if (isKey == false)
                return;
        }

        lastHit = Time.time;
        currentHP -= damageAmount;

        if (audioSource != null && hitSound != null)
        {
            audioSource.PlayOneShot(hitSound);
        }

        // Death Logic
        if (currentHP <= 0f)
        {
            if (audioSource != null && deathSound != null)
            {
                audioSource.PlayOneShot(deathSound);
                Destroy(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}
