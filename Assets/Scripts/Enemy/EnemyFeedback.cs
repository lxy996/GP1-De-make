using UnityEngine;

public class EnemyFeedback : MonoBehaviour
{
    [Header("Component References")]
    public Animator animator;
    public Rigidbody rigidBody;
    public AudioSource audioSource;

    [Header("Animation Triggers")]
    public string hitTriggerName = "Hit";
    public string deathTriggerName = "Die";

    [Header("Audio Clips")]
    public AudioClip hitSound;
    public AudioClip deathSound;

    private bool isDead;

    public void PlayHit()
    {
        if (isDead) 
            return;

        TriggerAnimation(hitTriggerName);
        PlayOneShot(hitSound);
    }

    public void PlayDeath()
    {
        if (isDead) 
            return;
        isDead = true;

        TriggerAnimation(deathTriggerName);
        PlayOneShot(deathSound);
        Destroy(gameObject);

    }

    private void TriggerAnimation(string triggerName)
    {
        if (animator == null) 
            return;
        if (string.IsNullOrEmpty(triggerName)) 
            return;

        animator.SetTrigger(triggerName);
    }
    private void PlayOneShot(AudioClip clip)
    {
        if (audioSource == null) 
            return;
        if (clip == null) 
            return;

        audioSource.PlayOneShot(clip);
    }
}
