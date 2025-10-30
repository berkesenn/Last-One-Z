using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [Header("Timer Settings")]
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI bestTimeText;
    private float gameTime = 0f;
    private bool isGameOver = false;
    private float bestTime = 0f;

    [Header("Menu Settings")]
    public Button backToMenuButton;
    
    [Header("UI Elements to Hide on Death")]
    public Canvas gameplayCanvas;
    
    [Header("Pause Settings")]
    public GameObject pausePanel;
    private bool isPaused = false;
    
    public static bool IsPaused { get; private set; } = false;

    void Start()
    {
        gameTime = 0f;
        bestTime = PlayerPrefs.GetFloat("BestTime", 0f);
        UpdateBestTimeDisplay();
        
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && !isGameOver)
        {
            TogglePause();
        }
        
        if (!isGameOver && !isPaused)
        {
            gameTime += Time.deltaTime;
            UpdateTimerDisplay();
        }
    }
    
    void TogglePause()
    {
        isPaused = !isPaused;
        IsPaused = isPaused; // Update static property
        
        if (isPaused)
        {
            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            
            if (pausePanel != null)
            {
                pausePanel.SetActive(true);
            }
        }
        else
        {
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            
            if (pausePanel != null)
            {
                pausePanel.SetActive(false);
            }
        }
    }
    
    public void ResumeGame()
    {
        if (isPaused)
        {
            TogglePause();
        }
    }
    
    public void QuitGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

    void UpdateTimerDisplay()
    {
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(gameTime / 60f);
            int seconds = Mathf.FloorToInt(gameTime % 60f);
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }

    public void BackToMenu()
    {
        ScreenFadeEffect fadeEffect = ScreenFadeEffect.GetInstance();
        if (fadeEffect != null)
        {
            fadeEffect.ResetFade();
            Destroy(fadeEffect.gameObject);
        }
        
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        SceneManager.LoadScene("menu");
    }

    public void SetGameOver()
    {
        isGameOver = true;
        
        if (gameTime > bestTime)
        {
            bestTime = gameTime;
            PlayerPrefs.SetFloat("BestTime", bestTime);
            PlayerPrefs.Save();
            UpdateBestTimeDisplay();
        }
        
        if (gameplayCanvas != null)
        {
            gameplayCanvas.gameObject.SetActive(false);
        }
        
        if (backToMenuButton != null)
        {
            backToMenuButton.interactable = false;
        }
    }
    
    void UpdateBestTimeDisplay()
    {
        if (bestTimeText != null)
        {
            int minutes = Mathf.FloorToInt(bestTime / 60f);
            int seconds = Mathf.FloorToInt(bestTime % 60f);
            bestTimeText.text = "Best: " + string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }

    public float GetGameTime()
    {
        return gameTime;
    }
    
    public float GetBestTime()
    {
        return bestTime;
    }
}
