using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HealthBarUI : MonoBehaviour
{
    [Header("UI References")]
    public Image healthBarFill;
    public TextMeshProUGUI healthText;
    
    [Header("Colors")]
    public Color fullHealthColor = Color.red;
    public Color lowHealthColor = new Color(0.5f, 0f, 0f);
    
    private PlayerHealth playerHealth;
    
    void Start()
    {
        playerHealth = FindFirstObjectByType<PlayerHealth>();
    }
    
    void Update()
    {
        if (playerHealth == null)
            return;
        
        float healthPercent = (float)playerHealth.currentHealth / playerHealth.maxHealth;
        
        if (healthBarFill != null)
        {
            healthBarFill.fillAmount = healthPercent;
            healthBarFill.color = Color.Lerp(lowHealthColor, fullHealthColor, healthPercent);
        }
        
        if (healthText != null)
        {
            healthText.text = playerHealth.currentHealth + " / " + playerHealth.maxHealth;
        }
    }
}
