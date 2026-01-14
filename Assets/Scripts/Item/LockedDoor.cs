using UnityEngine;
using UnityEngine.Audio;

public class LockedDoor : EnemyHealth
{
    [Header("Locked Door Specifics")]
    public GameObject unlockVfx; 
    public AudioClip unlockSound;
    public AudioSource audioSource;


    public override void TakeDamage(float damageAmount, Vector3 hitPoint, Vector3 hitForce, GameObject damageSource)
    {

        bool isKey = false;
        if (damageSource != null)
        {
            GrabbableItem item = damageSource.GetComponent<GrabbableItem>();
            if (item != null && item.type == GrabbableType.Key)
            {
                isKey = true;
            }
        }

        if (isKey)
        {

            base.TakeDamage(damageAmount, hitPoint, hitForce, damageSource);


            if (unlockVfx != null) Instantiate(unlockVfx, transform.position, Quaternion.identity);

            AudioSource.PlayClipAtPoint(unlockSound, transform.position);
            
            Destroy(damageSource);
        }
       
    }

    // door is not calculated into the number of kills
    protected override void Die()
    {

        if (feedback != null) feedback.PlayDeath();
        Destroy(gameObject);
    }
}
