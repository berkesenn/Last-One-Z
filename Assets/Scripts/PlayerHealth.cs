using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 100;
    public int currentHealth;
    
    [Header("UI Settings")]
    public bool showHealthUI = true;
    public Color healthBarColor = Color.green;
    public Color lowHealthColor = Color.red;
    
    [Header("Death Settings")]
    public bool restartOnDeath = true;
    public float restartDelay = 5f; // 3'ten 5'e çıkardım (2 saniye daha)
    
    // Private variables
    [SerializeField]private bool isDead = false;
    private float deathTime = 0f;

    public bool IsDead { get; set; }
    
    void Start()
    {
        currentHealth = maxHealth;
    }
    
    public void TakeDamage(int damage)
    {
        if (isDead)
            return;
        
        AudioManager audioManager = AudioManager.GetInstance();
        if (audioManager != null)
        {
            audioManager.PlayPlayerHurt();
        }
        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth);
        
        Debug.Log("Player took " + damage + " damage. Health: " + currentHealth + "/" + maxHealth);
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    public void Heal(int amount)
    {
        if (isDead)
            return;
        
        currentHealth += amount;
        currentHealth = Mathf.Min(maxHealth, currentHealth);
        
        Debug.Log("Player healed " + amount + ". Health: " + currentHealth + "/" + maxHealth);
    }
    
    public void Die()
    {
        if (isDead)
            return;
        
        isDead = true;
        IsDead = true; // Property'yi de set et
        deathTime = Time.time;
        Debug.Log("Player died!");

        AudioManager audioManager = AudioManager.GetInstance();
        if (audioManager != null)
        {
            audioManager.PlayGameOver();
            audioManager.StopBackgroundMusic(); // Background music'i durdur
            audioManager.LowerZombieVolume(); // Zombi seslerini kıs
        }

        // Timer'ı durdur
        GameManager gameManager = FindFirstObjectByType<GameManager>();
        if (gameManager != null)
        {
            gameManager.SetGameOver();
        }
        
        // Lock cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        // Disable controls
        PlayerController playerController = GetComponent<PlayerController>();
        if (playerController != null)
        {
            playerController.enabled = false;
            Rigidbody rb = playerController.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = true;
                rb.useGravity = false;
            }
        }


        // Restart after delay
        if (restartOnDeath)
        {
            Invoke("RestartGame", restartDelay);
        }
    }
    
    void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    
    void OnGUI()
    {
        if (!showHealthUI)
            return;
        
        // Health bar background
        float barWidth = 200f;
        float barHeight = 30f;
        float barX = 20f;
        float barY = 20f;
        
        // Background (black)
        GUI.color = Color.black;
        GUI.DrawTexture(new Rect(barX - 2, barY - 2, barWidth + 4, barHeight + 4), Texture2D.whiteTexture);
        
        // Health bar color (red to green based on health)
        float healthPercent = (float)currentHealth / maxHealth;
        GUI.color = Color.Lerp(lowHealthColor, healthBarColor, healthPercent);
        GUI.DrawTexture(new Rect(barX, barY, barWidth * healthPercent, barHeight), Texture2D.whiteTexture);
        
        // Health text
        GUI.color = Color.white;
        GUI.skin.label.fontSize = 18;
        GUI.skin.label.alignment = TextAnchor.MiddleCenter;
        GUI.Label(new Rect(barX, barY, barWidth, barHeight), currentHealth + " / " + maxHealth);
        
        // Death message
        if (isDead)
        {
            GUI.skin.label.fontSize = 48;
            GUI.skin.label.alignment = TextAnchor.MiddleCenter;
            GUI.color = Color.red;
            GUI.Label(new Rect(0, Screen.height / 2 - 50, Screen.width, 100), "YOU DIED");
            
            GUI.skin.label.fontSize = 24;
            GUI.color = Color.white;
            float timeRemaining = restartDelay - (Time.time - deathTime);
            GUI.Label(new Rect(0, Screen.height / 2 + 20, Screen.width, 50), "Restarting in " + Mathf.Ceil(timeRemaining) + "...");
        }
    }
}