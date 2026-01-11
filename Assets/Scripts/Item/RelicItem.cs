using UnityEngine;

public class RelicItem : MonoBehaviour, IHandUsable
{
    [Header("Relic Settings")]
    public RelicType relicType;
    public string relicName;
    [TextArea] public string description;

    [Header("Feedback")]
    public AudioSource audioSource;
    public GameObject useVfx;
    public AudioClip useSfx;

    public bool Use(HandGrab grabber, bool isLeft)
    {
        if (grabber == null) 
            return false;

        PlayerRelics manager = grabber.GetComponent<PlayerRelics>();
        if (manager == null)
        {
            Debug.LogWarning("Player does not have a PlayerRelics component!");
            return false;
        }

        manager.UnlockRelic(relicType);

        // Play Vfx and Sfx
        if (useVfx != null) Instantiate(useVfx, transform.position, Quaternion.identity);
        if (useSfx != null) AudioSource.PlayClipAtPoint(useSfx, transform.position);

        Destroy(gameObject);
        return true;
    }

}
