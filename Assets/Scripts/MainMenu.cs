using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    /// <summary>
    /// Oyunu başlatır - oyun sahnesine geçer
    /// </summary>
    public void StartGame()
    {
        // Eğer oyun sahnesi farklı bir isimde ise burayı değiştir
        // Örnek: "GameScene", "Level1", "SampleScene" vb.
        SceneManager.LoadScene("main"); 
        
        // Alternatif: Sıradaki sahneyi yükle
        // SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        
        Debug.Log("Game Started!");
    }
    
    /// <summary>
    /// Oyundan çıkar
    /// </summary>
    public void QuitGame()
    {
        Debug.Log("Quitting Game...");
        
        #if UNITY_EDITOR
            // Unity Editor'de çalışıyorsa oyunu durdur
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            // Build edilmiş oyunda uygulamayı kapat
            Application.Quit();
        #endif
    }
    
    /// <summary>
    /// Ayarlar menüsünü aç (isteğe bağlı)
    /// </summary>
    public void OpenSettings()
    {
        Debug.Log("Settings opened");
        // Buraya ayarlar menüsü açma kodu ekleyebilirsin
    }
    
    /// <summary>
    /// Belirli bir sahneyi yükle
    /// </summary>
    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}
