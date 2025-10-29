using UnityEngine;
using TMPro;

public class DeathUI : MonoBehaviour
{
    [Header("Death UI")]
    public TextMeshProUGUI deathText; // "YOU DIED" yazısı
    public TextMeshProUGUI restartText; // "Restarting in..." yazısı
    
    private PlayerHealth playerHealth;
    private Canvas deathCanvas;
    
    void Start()
    {
        // PlayerHealth scriptini bul
        playerHealth = FindFirstObjectByType<PlayerHealth>();
        
        if (playerHealth == null)
        {
            Debug.LogError("PlayerHealth not found!");
        }
        
        // Canvas'ı al
        deathCanvas = GetComponent<Canvas>();
        if (deathCanvas != null)
        {
            deathCanvas.sortingOrder = 10000; // En üstte
        }
        
        // Başlangıçta gizle
        HideDeathUI();
    }
    
    void Update()
    {
        if (playerHealth == null)
            return;
        
        // Ölüm mesajlarını göster/gizle
        if (playerHealth.IsDead)
        {
            ShowDeathUI();
            
            if (restartText != null)
            {
                float timeRemaining = playerHealth.restartDelay - (Time.time - playerHealth.GetDeathTime());
                restartText.text = "Restarting in " + Mathf.Ceil(timeRemaining) + "...";
            }
        }
        else
        {
            HideDeathUI();
        }
    }
    
    void ShowDeathUI()
    {
        if (deathText != null)
            deathText.gameObject.SetActive(true);
        if (restartText != null)
            restartText.gameObject.SetActive(true);
    }
    
    void HideDeathUI()
    {
        if (deathText != null)
            deathText.gameObject.SetActive(false);
        if (restartText != null)
            restartText.gameObject.SetActive(false);
    }
}
