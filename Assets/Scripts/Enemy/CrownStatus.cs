using UnityEngine;

public class CrownStatus : MonoBehaviour
{
    [Header("Settings")]
    [Range(0f, 1f)] public float eliteChance = 0.1f;
    public GameObject crownVisual;

    [Header("State")]
    public bool isElite = false;  // Used for spawn fixed elite

    [Header("Feedback")]
    public AudioSource audioSource;
    public GameObject breakVfx;
    public AudioClip breakSfx;

    private EnemyHealth healthScript;
    private float normalMaxHP;
    private bool crownBroken = false;
    void Start()
    {
        healthScript = GetComponent<EnemyHealth>();

        
        if (!isElite && Random.value < eliteChance)
        {
            isElite = true;
        }

        if (isElite)
        {
            ActivateEliteStatus();
        }
        else
        {
            if (crownVisual != null) crownVisual.SetActive(false);
        }
    }

    void Update()
    {
        
        if (isElite && !crownBroken && healthScript != null)
        {
            if (healthScript.currentHP <= normalMaxHP)
            {
                BreakCrown();
            }
        }
    }

    void ActivateEliteStatus()
    {
        
        if (crownVisual != null) crownVisual.SetActive(true);

       
        if (healthScript != null)
        {
            normalMaxHP = healthScript.maxHP; 
            healthScript.maxHP *= 2;
            healthScript.currentHP = healthScript.maxHP; 
        }
    }

    void BreakCrown()
    {
        crownBroken = true;

        if (crownVisual != null) crownVisual.SetActive(false);

        if (breakVfx != null) Instantiate(breakVfx, transform.position, Quaternion.identity);
        if (breakSfx != null) AudioSource.PlayClipAtPoint(breakSfx, transform.position);
    }

    // Used for fixed elite
    public void ForceElite()
    {
        isElite = true;
        
        if (healthScript != null) ActivateEliteStatus();
    }

}
