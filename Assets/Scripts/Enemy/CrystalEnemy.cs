using UnityEngine;
using static UnityEditor.PlayerSettings;

public class CrystalEnemy : EnemyHealth
{
    [Header("Components")]
    public GameObject[] crystalParts; 
    public GameObject breakEffect;  
    public GameObject spawnEffect;
    public AudioSource audioSource;
    public AudioClip breakSound;

    [Header("Spawning")]
    public GameObject spawnPointPrefab; 
    public int spawnsPerHit = 4;        
    public float spawnRadius = 5f;

    [Header("Visual Feedback")]
    public GameObject fragmentPrefab;


    protected override void OnDamageTaken(float damage)
    {
        
        int partIndex = Mathf.CeilToInt(currentHP);

        if (partIndex >= 0 && partIndex < crystalParts.Length)
        {
            if (crystalParts[partIndex] != null && crystalParts[partIndex].activeSelf)
            {
                crystalParts[partIndex].SetActive(false);

                GameObject vfx = Instantiate(breakEffect, crystalParts[partIndex].transform.position, Quaternion.identity);

                Destroy(vfx, 5f);

                SpawnFragment(crystalParts[partIndex].transform.position);

                SpawnItems(); 
            }
        }
        else
        {

            SpawnItems();
        }
    }
    void SpawnFragment(Vector3 pos)
    {
        if (fragmentPrefab != null)
        {
            GameObject fragment = Instantiate(fragmentPrefab, pos, Quaternion.identity);

            Rigidbody rb = fragment.GetComponent<Rigidbody>();
            if (rb != null)
            {
                Vector3 randomDir = Random.insideUnitSphere;
                randomDir.y = Mathf.Abs(randomDir.y);  // avoid fragment get under the floor
                
                rb.AddForce(randomDir * 5f, ForceMode.Impulse);
                rb.AddTorque(Random.insideUnitSphere * 10f, ForceMode.Impulse);
            }

            Destroy(fragment, 5f);
        }

        if (audioSource && breakSound)
        {
            audioSource.PlayOneShot(breakSound);
        }
    }

    void SpawnItems()
    {
        for (int i = 0; i < spawnsPerHit; i++)
        {
            Vector2 randomCircle = Random.insideUnitCircle * 5f;
            Vector3 spawnPos = new Vector3(transform.position.x + randomCircle.x, 0, transform.position.z + randomCircle.y);

            // let spawn point to spawn items
            GameObject spObj = Instantiate(spawnPointPrefab, spawnPos, Quaternion.identity);
            var sp = spObj.GetComponent<SpawnPoint>();
            if (sp) sp.Spawn(new System.Random());
        }
    }
}
