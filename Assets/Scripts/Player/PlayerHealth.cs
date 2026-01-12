using UnityEngine;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    [Header("HP")]
    public float maxHP = 6f;
    public float invincibilityDuration = 1.0f;

    [Header("Feedback")]
    public AudioSource audioSource;
    public AudioClip hitSound;
    public AudioClip deathSound;
    public AudioClip healthSound;
    public Animator UIAnimator;

    public float currentHP;
    private float lastHit;

    void Awake()
    {
        currentHP = maxHP;
        lastHit = -999f;
    }

    public void TakeDamage(float damageAmount, Vector3 hitPoint, Vector3 hitForce, GameObject damageSource)
    {
        if (Time.time - lastHit < invincibilityDuration)
            return;

        lastHit = Time.time;
        currentHP -= damageAmount;

        if (audioSource != null && hitSound != null)
        {
            audioSource.PlayOneShot(hitSound);
        }

        UIAnimator.SetTrigger("Hurt");
        // Death Logic
        if (currentHP <= 0f)
        {
            if (audioSource != null && deathSound != null)
            {
                audioSource.PlayOneShot(deathSound);
                
                GameLevelManager.Instance.GameFinish(currentHP, GameLevelManager.Instance.totalStats);

                GetComponent<CharacterController>().enabled = false;
            }
            else
            {
                GameLevelManager.Instance.GameFinish(currentHP, GameLevelManager.Instance.totalStats);

                GetComponent<CharacterController>().enabled = false;
            }
        }
    }

    public void BeHealth()
    {
        currentHP = maxHP;

        UIAnimator.SetTrigger("Cure");
        if (audioSource != null && healthSound != null)
        {
            audioSource.PlayOneShot(healthSound);
        }

    }
}
