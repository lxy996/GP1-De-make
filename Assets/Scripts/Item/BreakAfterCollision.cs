using UnityEngine;

public class BreakAfterCollision : MonoBehaviour
{
    [Header("References")]
    public GameObject breakVfx;
    public AudioClip breakSfx;
    public AudioSource audioSource;



    void OnCollisionEnter(Collision collision)
    {

        if (breakVfx != null)
        {
            Instantiate(breakVfx, transform.position, Quaternion.identity);
        }
            
        if (audioSource != null && breakSfx != null )
        {
            audioSource.PlayOneShot(breakSfx);
        }

        Destroy(gameObject);
    
    }

}
