using UnityEngine;
using System.Collections;


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
    public GameObject deathEffect;

    private bool isDead;
    public float deathDelayTime = 2f;

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

        StartCoroutine(DeathRoutine());

    }
    IEnumerator DeathRoutine()
    {
        isDead = true;

        TriggerAnimation(deathTriggerName);
        if (deathEffect != null)
        {
            GameObject vfx = Instantiate(deathEffect, gameObject.transform.position, Quaternion.identity);

            Destroy(vfx, 5f);
        } 

        PlayOneShot(deathSound);

        yield return new WaitForSeconds(deathDelayTime);

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
