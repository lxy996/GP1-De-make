using UnityEngine;

public class HealthPotion : MonoBehaviour, IHandUsable
{
    [Header("Feedback")]
    public AudioSource audioSource;
    public GameObject cureVfx;
    public AudioClip cureSfx;

    public bool destroyOnUse = true;

    public bool Use(HandGrab grabber, bool isLeftHand)
    {
        if (grabber == null) 
            return false;

        PlayerHealth hp = grabber.GetComponentInParent<PlayerHealth>();
        if (hp == null) 
            return false;

        hp.BeHealth();

        if (cureVfx != null) Instantiate(cureVfx, transform.position, Quaternion.identity);
        if (cureSfx != null) AudioSource.PlayClipAtPoint(cureSfx, transform.position);


        if (destroyOnUse)
            Destroy(gameObject);

        return true;
    }
}
