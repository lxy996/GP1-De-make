using UnityEngine;

public class HealthPotion : MonoBehaviour, IHandUsable
{
    public bool destroyOnUse = true;

    public bool Use(HandGrab grabber, bool isLeftHand)
    {
        if (grabber == null) 
            return false;

        PlayerHealth hp = grabber.GetComponentInParent<PlayerHealth>();
        if (hp == null) 
            return false;

        hp.BeHealth();

        if (destroyOnUse)
            Destroy(gameObject);

        return true;
    }
}
