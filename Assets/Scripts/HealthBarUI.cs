using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HealthBarUI : MonoBehaviour
{
    [Header("UI References")]
    public Image healthBarFill; // Can barının içi (fill)
    public TextMeshProUGUI healthText; // Can yazısı (100 / 100)
    
    [Header("Colors")]
    public Color fullHealthColor = Color.red; // Parlak kırmızı
    public Color lowHealthColor = new Color(0.5f, 0f, 0f); // Koyu kırmızı
    
    private PlayerHealth playerHealth;
    
    void Start()
    {
        // PlayerHealth scriptini bul
        playerHealth = FindFirstObjectByType<PlayerHealth>();
        
        if (playerHealth == null)
        {
            Debug.LogError("PlayerHealth not found!");
        }
    }
    
    void Update()
    {
        if (playerHealth == null)
            return;
        
        // Can yüzdesini hesapla
        float healthPercent = (float)playerHealth.currentHealth / playerHealth.maxHealth;
        
        // Can barını güncelle
        if (healthBarFill != null)
        {
            healthBarFill.fillAmount = healthPercent;
            healthBarFill.color = Color.Lerp(lowHealthColor, fullHealthColor, healthPercent);
        }
        
        // Can yazısını güncelle
        if (healthText != null)
        {
            healthText.text = playerHealth.currentHealth + " / " + playerHealth.maxHealth;
        }
    }
}
