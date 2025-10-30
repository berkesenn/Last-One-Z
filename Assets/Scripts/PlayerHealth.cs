using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 100;
    public int currentHealth;
    
    [Header("Death Settings")]
    public bool restartOnDeath = true;
    public float restartDelay = 5f;
    
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
    }
    
    public void Die()
    {
        if (isDead)
            return;
        
        isDead = true;
        IsDead = true;
        deathTime = Time.time;

        AudioManager audioManager = AudioManager.GetInstance();
        if (audioManager != null)
        {
            audioManager.PlayGameOver();
            audioManager.StopBackgroundMusic();
            audioManager.LowerZombieVolume();
        }
        GameManager gameManager = FindFirstObjectByType<GameManager>();
        if (gameManager != null)
        {
            gameManager.SetGameOver();
        }
        
        ScreenFadeEffect fadeEffect = ScreenFadeEffect.GetInstance();
        if (fadeEffect != null)
        {
            fadeEffect.FadeToBlack(1f, 0.7f);
        }
        
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        PlayerController playerController = GetComponent<PlayerController>();
        if (playerController != null)
        {
            playerController.PlayDeathAnimation();
            playerController.enabled = false;            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = true;
                rb.useGravity = false;
            }
        }

        if (restartOnDeath)
        {
            Invoke("RestartGame", restartDelay);
        }
    }
    
    void RestartGame()
    {
        ScreenFadeEffect fadeEffect = ScreenFadeEffect.GetInstance();
        if (fadeEffect != null)
        {
            fadeEffect.ResetFade();
        }
        
        PlayerController playerController = GetComponent<PlayerController>();
        if (playerController != null)
        {
            playerController.ResetCamera();
        }
        
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    
    public float GetDeathTime()
    {
        return deathTime;
    }

}
