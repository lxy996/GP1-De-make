using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class GameLevelManager : MonoBehaviour
{
    public static GameLevelManager Instance;

    [Header("Level Settings")]
    public List<GameObject> levelPrefabs; 
    public Transform player;              
    public Transform levelOrigin;         

    [Header("UI References")]
    public GameObject summaryPanel;       
    public UnityEngine.UI.Text summaryText;

    [Header("Economy")]
    public int currentGold = 0;
    public TextMeshProUGUI goldText;

    [Header("Runtime Data")]
    public int currentLevelIndex = 0;
    public GameStats currentStats = new GameStats();

    private GameObject currentLevelInstance;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (summaryPanel != null) summaryPanel.SetActive(false);
        UpdateGoldUI();
    }

    void Start()
    {
        LoadNextLevel();
    }

    public void LoadNextLevel()
    {
        StartCoroutine(LevelTransitionRoutine());
    }

    IEnumerator LevelTransitionRoutine()
    {
        // Clear the old level
        if (currentLevelInstance != null)
        {
            Destroy(currentLevelInstance);
            yield return null; 
        }

        // Choose a level prefab
        if (levelPrefabs.Count == 0)
        {
            yield break;
        }

        int index = Random.Range(0, levelPrefabs.Count);
        GameObject prefabToSpawn = levelPrefabs[index];

        // Spawn new level
        currentLevelInstance = Instantiate(prefabToSpawn, levelOrigin.position, Quaternion.identity);
        currentLevelIndex++;

        // Reloate player's location
        Transform startPoint = currentLevelInstance.transform.Find("PlayerStart");
        if (startPoint != null)
        {
         
            CharacterController cc = player.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;

            player.position = startPoint.position;
            player.rotation = startPoint.rotation;

            if (cc != null) cc.enabled = true;
        }
        else
        {
            // If didn't find the player start point
            player.position = new Vector3(0, 2, 0);
        }

        // Reset data
        currentStats.Reset();
        if (summaryPanel != null) summaryPanel.SetActive(false);

    }

    // After player interact with portal
    public void FinishLevel()
    {
        currentStats.endTime = Time.time;
        ShowSummary();
    }

    void ShowSummary()
    {
        if (summaryPanel != null && summaryText != null)
        {
            summaryPanel.SetActive(true);
            summaryText.text = $"LEVEL {currentLevelIndex} COMPLETE!\n\n" +
                               $"Time taken: {currentStats.Duration:F2} s\n" +
                               $"Items thrown: {currentStats.throwCount}\n" +
                               $"Monsters killed: {currentStats.killCount}";

            
            Invoke(nameof(LoadNextLevel), 3.0f);
        }
        else
        {

            LoadNextLevel();
        }
    }
    void UpdateGoldUI()
    {
        if (goldText) goldText.text = $"GOLD: {currentGold}";
    }

    // Used for other script
    public void RegisterKill() => currentStats.killCount++;
    public void RegisterThrow() => currentStats.throwCount++;
    public void AddGold(int amount)
    {
        currentGold += amount;
        UpdateGoldUI();
    }
    public bool TrySpendGold(int amount)
    {
        if (currentGold >= amount)
        {
            currentGold -= amount;
            UpdateGoldUI();
            return true;
        }
        return false;
    }
    
}
