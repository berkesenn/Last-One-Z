using UnityEngine;
using TMPro;

public class DeathUI : MonoBehaviour
{
    [Header("Death UI")]
    public TextMeshProUGUI deathText;
    public TextMeshProUGUI restartText;
    
    private PlayerHealth playerHealth;
    private Canvas deathCanvas;
    
    void Start()
    {
        playerHealth = FindFirstObjectByType<PlayerHealth>();
        
        deathCanvas = GetComponent<Canvas>();
        if (deathCanvas != null)
        {
            deathCanvas.sortingOrder = 9998;
        }
        HideDeathUI();
    }
    
    void Update()
    {
        if (playerHealth == null)
            return;
        
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
        if (deathCanvas != null)
        {
            deathCanvas.sortingOrder = 10000;
        }
        
        if (deathText != null)
            deathText.gameObject.SetActive(true);
        if (restartText != null)
            restartText.gameObject.SetActive(true);
    }
    
    void HideDeathUI()
    {
        if (deathCanvas != null)
        {
            deathCanvas.sortingOrder = 9998;
        }
        
        if (deathText != null)
            deathText.gameObject.SetActive(false);
        if (restartText != null)
            restartText.gameObject.SetActive(false);
    }
}
