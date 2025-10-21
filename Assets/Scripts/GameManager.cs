using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    [Header("Timer Settings")]
    public TextMeshProUGUI timerText;
    private float gameTime = 0f;
    private bool isGameOver = false;

    void Start()
    {
        gameTime = 0f;
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

    public void SetGameOver()
    {
        isGameOver = true;
    }

    public float GetGameTime()
    {
        return gameTime;
    }
}
