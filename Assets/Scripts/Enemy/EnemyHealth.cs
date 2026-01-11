using UnityEngine;

public class EnemyHealth : MonoBehaviour, IDamageable
{

    public float maxHP = 3f;
    public float invincibilityDuration = 0.08f;

    public float currentHP;
    protected float lastHit;

    public int goldReward = 1;

    protected EnemyFeedback feedback;


    protected virtual void Awake()
    {
        currentHP = maxHP;
        feedback = GetComponent<EnemyFeedback>();
        lastHit = -999f;
    }

    public virtual void TakeDamage(float damageAmount, Vector3 hitPoint, Vector3 hitForce, GameObject damageSource)
    {
        if (Time.time - lastHit < invincibilityDuration) 
            return;

        lastHit = Time.time;
        currentHP -= damageAmount;

        if (feedback != null)
        {
            feedback.PlayHit();
        }

        OnDamageTaken(damageAmount);

        if (currentHP <= 0f)
        {
            Die();
        }
    }
    protected virtual void OnDamageTaken(float damage) { }

    // To count the number of kills
    protected virtual void Die()
    {
        if (feedback != null) feedback.PlayDeath();

        
        if (GameLevelManager.Instance)
        {
           
            GameLevelManager.Instance.AddGold(goldReward);
            GameLevelManager.Instance.RegisterKill();

        }
            

        Destroy(gameObject);
    }
}