using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class GameLevelManager : MonoBehaviour
{
    public static GameLevelManager Instance;

    public GameUIManager gameUIManager;
    public LevelSpawner levelSpawner;

    [Header("Level Settings")]
    public List<GameObject> levelPrefabs; 
    public Transform player;              
    public Transform levelOrigin;         

    [Header("UI References")]
    public GameObject summaryPanel;
    public TextMeshProUGUI title;
    public TextMeshProUGUI time;
    public TextMeshProUGUI thrown;
    public TextMeshProUGUI kill;
    public TextMeshProUGUI timeAmount;
    public TextMeshProUGUI thrownAmount;
    public TextMeshProUGUI killAmount;
    public Button summaryButton;

    [Header("Economy")]
    public int currentGold = 0;
    public TextMeshProUGUI goldText;

    [Header("Runtime Data")]
    public int currentLevelIndex = 0;
    public GameStats currentStats = new GameStats();
    public GameStats totalStats = new GameStats();

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
        //LoadNextLevel();
    }

    public void LoadNextLevel()
    {
        if (currentLevelIndex >= 3)
        {
            GameFinish(1f, totalStats);
            return;
        }
        gameUIManager.LockCursor();
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

        if (levelSpawner != null)
        {
            levelSpawner.InitLevel(currentLevelInstance.transform);
        }

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
    public bool CanSpawnEnemy(string enemyName)
    {
        // First: Don't spawn Cannon, FireMan, Crystal
        if (currentLevelIndex == 1)
        {
            if (enemyName.Contains("Cannon") || enemyName.Contains("FireMan") || enemyName.Contains("Crystal"))
                return false;
        }
        // Second: Don't spawn Cannon
        else if (currentLevelIndex == 2)
        {
            if (enemyName.Contains("Cannon"))
                return false;
        }
        

        return true;
    }

    // After player interact with portal
    public void FinishLevel()
    {
        currentStats.endTime = Time.time;

        totalStats.throwCount += currentStats.throwCount;
        totalStats.killCount += currentStats.killCount;

        ShowSummary();
        gameUIManager.UnlockCursor();
    }

    void ShowSummary()
    {
        if (summaryPanel != null)
        {
            summaryPanel.SetActive(true);
            title.text = $"Level {currentLevelIndex} cleared!";
            time.text = $"Time  taken:";
            timeAmount.text = $"{currentStats.Duration:F2}s";
            thrown.text = $"Items  thrown:";
            thrownAmount.text = $"{currentStats.throwCount}";
            kill.text = $"Monsters  killed:";
            killAmount.text = $"{currentStats.killCount}";


        }
        else
        {

            LoadNextLevel();
        }
    }

    public void GameFinish(float type, GameStats totalStats)
    {
        totalStats.throwCount += currentStats.throwCount;
        totalStats.killCount += currentStats.killCount;

        currentStats.endTime = Time.time;
        gameUIManager.isGameStarted = false;
        Time.timeScale = 0f;

        gameUIManager.UnlockCursor();
        gameUIManager.hudPanel.SetActive(false);

        if (type <= 0) // Death
        {
            summaryPanel.SetActive(true);
            title.text = "You Died";
            time.text = $"Time  taken:";
            timeAmount.text = $"{currentStats.Duration:F2}s";
            thrown.text = $"Items  thrown:";
            thrownAmount.text = $"{totalStats.throwCount}";
            kill.text = $"Monsters  killed:";
            killAmount.text = $"{totalStats.killCount}";

            summaryButton.onClick.RemoveAllListeners();
            summaryButton.onClick.AddListener(gameUIManager.OnBackToMenuButton);
        }
        else // Victory
        {
            summaryPanel.SetActive(true);
            title.text = "Victory!";
            time.text = $"Time  taken:";
            timeAmount.text = $"{currentStats.Duration:F2}s";
            thrown.text = $"Items  thrown:";
            thrownAmount.text = $"{totalStats.throwCount}";
            kill.text = $"Monsters  killed:";
            killAmount.text = $"{totalStats.killCount}";

            summaryButton.onClick.RemoveAllListeners();
            summaryButton.onClick.AddListener(gameUIManager.OnBackToMenuButton);
        }
            
    }
    void UpdateGoldUI()
    {
        if (goldText) goldText.text = $"{currentGold}";
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
