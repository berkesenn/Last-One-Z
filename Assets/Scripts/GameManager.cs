using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [Header("Timer Settings")]
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI bestTimeText; // En iyi süreyi göster (opsiyonel)
    private float gameTime = 0f;
    private bool isGameOver = false;
    private float bestTime = 0f;

    [Header("Menu Settings")]
    public Button backToMenuButton; // Butonu buraya bağla

    void Start()
    {
        gameTime = 0f;
        
        // En iyi süreyi yükle
        bestTime = PlayerPrefs.GetFloat("BestTime", 0f);
        UpdateBestTimeDisplay();
        
        // Butona tıklama olayını bağla
        if (backToMenuButton != null)
        {
            backToMenuButton.onClick.AddListener(BackToMenu);
        }
    }

    void Update()
    {
        if (!isGameOver)
        {
            // Oyun süresi artır
            gameTime += Time.deltaTime;

            // Timer'ı güncelle
            UpdateTimerDisplay();
        }
        
        
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
        // Menü sahnesine dön
        SceneManager.LoadScene("menu"); // Menü sahneni ismini kontrol et!
    }

    public void SetGameOver()
    {
        isGameOver = true;
        
        // Eğer mevcut süre rekordan uzunsa, yeni rekor kaydet
        if (gameTime > bestTime)
        {
            bestTime = gameTime;
            PlayerPrefs.SetFloat("BestTime", bestTime);
            PlayerPrefs.Save(); // Hemen kaydet
            UpdateBestTimeDisplay();
            Debug.Log("New Best Time: " + bestTime + " seconds!");
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
