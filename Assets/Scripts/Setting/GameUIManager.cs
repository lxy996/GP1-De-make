using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using StarterAssets;
using System.Collections;

public class GameUIManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject hudPanel;        // The UI in gameplay like HP and map
    public GameObject mainMenuPanel; 
    public GameObject pauseMenuPanel;
    public GameObject optionsPanel;

    [Header("HUD Elements")]
    public Slider healthSlider;      
    public TextMeshProUGUI keyPromptText;
    public float tipsDuration = 20f;

    [Header("Options")]
    public TextMeshProUGUI sensitivityButtonText;
    public TextMeshProUGUI audioButtonText;
    private int currentSensLevel = 3;
    public int maxSensLevel;
    private bool isAudioOn = true;

    [Header("References")]
    public PlayerHealth playerHealth;
    public FirstPersonController playerController;
    public PlayerInteractor playerInteractor;
    public GameObject playerArms;
    public HandGrab handGrab;

    // Game Statue
    public bool isPaused = false;
    public bool isGameStarted = false;
    void Start()
    {
        playerArms.SetActive (false);
        ShowMainMenu();
        currentSensLevel = 3;
        isAudioOn = AudioListener.volume > 0.01f;
        UpdateOptionsUI();

    }
    void Update()
    {
        // Update player's current HP
        if (playerHealth != null && healthSlider != null)
        {
            healthSlider.value = playerHealth.currentHP / playerHealth.maxHP;
        }


        if (isGameStarted && Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused) ResumeGame();
            else PauseGame();
        }
    }

    IEnumerator ShowTips()
    {
        if (keyPromptText)
        {
            keyPromptText.gameObject.SetActive(true);
            yield return new WaitForSeconds(tipsDuration);
            keyPromptText.gameObject.SetActive(false);
            playerInteractor.tipHasShown = true;
        }
        else
        {
            playerInteractor.tipHasShown = true;
        }
    }

    // Switch between menus
    public void ShowMainMenu()
    {
        isGameStarted = false;
        Time.timeScale = 0f; // Pause time
        UnlockCursor();      // Unlock the mouse

        if (mainMenuPanel) mainMenuPanel.SetActive(true);
        if (hudPanel) hudPanel.SetActive(false);
        if (pauseMenuPanel) pauseMenuPanel.SetActive(false);
        if (optionsPanel) optionsPanel.SetActive(false);
    }

    // Play game
    public void OnPlayButton()
    {
        playerArms.SetActive(true);
        isGameStarted = true;
        isPaused = false;
        playerInteractor.tipHasShown = false;
        Time.timeScale = 1f;
        LockCursor();


        GameLevelManager.Instance.LoadNextLevel();
        if (mainMenuPanel) mainMenuPanel.SetActive(false);
        if (hudPanel) hudPanel.SetActive(true);

        StartCoroutine(ShowTips());
    }

    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f;  // Pause time
        UnlockCursor();       // Unlock the mouse

        if (pauseMenuPanel) pauseMenuPanel.SetActive(true);
        if (hudPanel) hudPanel.SetActive(false);
        if (handGrab != null) handGrab.enabled = false;
    }


    // Get back to game
    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;
        LockCursor();

        if (pauseMenuPanel) pauseMenuPanel.SetActive(false);
        if (optionsPanel) optionsPanel.SetActive(false); 
        if (hudPanel) hudPanel.SetActive(true);
        if (handGrab != null) handGrab.enabled = true;

        ApplySensitivityToPlayer();
    }


    public void OnBackToMenuButton()
    {
        
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        ShowMainMenu();


    }

    // Function of menu

    public void OnOptionsButton()
    {

        if (mainMenuPanel.activeSelf) mainMenuPanel.SetActive(false);
        if (pauseMenuPanel.activeSelf) pauseMenuPanel.SetActive(false);

        optionsPanel.SetActive(true);

        UpdateOptionsUI();

    }

    public void OnOptionsBack()
    {
        optionsPanel.SetActive(false);

        // If game is not start, go back to main menu
        if (!isGameStarted)
        {
            mainMenuPanel.SetActive(true);
        }
        else
        {
            pauseMenuPanel.SetActive(true);
        }
    }

    // The specific function

    public void OnSensitivityButtonClick()
    {
        currentSensLevel++;
        if (currentSensLevel > maxSensLevel)
        {
            currentSensLevel = 1;
        }

        UpdateOptionsUI();

        ApplySensitivityToPlayer();
    }

    public void OnAudioButtonClick()
    {
       
        isAudioOn = !isAudioOn;

        if (isAudioOn)
        {
            AudioListener.volume = 1f;
        }
        else
        {
            AudioListener.volume = 0f;
        }

        UpdateOptionsUI();
    }


    public void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        if (playerController != null)
        {
            playerController.enabled = true;
        }
    }

    public void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        if (playerController != null)
        {
            playerController.enabled = false;
        }
    }

    void UpdateOptionsUI()
    {
        if (sensitivityButtonText != null)
        {
            sensitivityButtonText.text = $"Sensitivity: {currentSensLevel}";
        }

        if (audioButtonText != null)
        {
            if (isAudioOn)
            {
                audioButtonText.text = "Audio: On";
            }
            else
            {
                audioButtonText.text = "Audio: Off";
            }

        }
    }

    void ApplySensitivityToPlayer()
    {
        if (playerController != null)
        {
            playerController.RotationSpeed = currentSensLevel * 0.5f;
        }
    }

}
