using UnityEngine;

public class EnemyHealth : MonoBehaviour, IDamageable
{
    public float maxHP = 3f;
    public float invincibilityDuration = 0.08f;

    public float currentHP;
    private float lastHit;

    private EnemyFeedback feedback;

    void Awake()
    {
        currentHP = maxHP;
        feedback = GetComponent<EnemyFeedback>();
        lastHit = -999f;
    }

    public void TakeDamage(float damageAmount, Vector3 hitPoint, Vector3 hitForce, GameObject damageSource)
    {
        if (Time.time - lastHit < invincibilityDuration) 
            return;

        lastHit = Time.time;
        currentHP -= damageAmount;

        if (feedback != null)
        {
            feedback.PlayHit();
        }

        if (currentHP <= 0f)
        {
            if (feedback != null)
            {
                feedback.PlayDeath();
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}
