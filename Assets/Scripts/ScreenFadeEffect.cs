using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ScreenFadeEffect : MonoBehaviour
{
    private static ScreenFadeEffect instance;
    private Image fadeImage;
    private Canvas fadeCanvas;
    
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            CreateFadeUI();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void CreateFadeUI()
    {
        // Canvas oluştur
        fadeCanvas = gameObject.AddComponent<Canvas>();
        fadeCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        fadeCanvas.sortingOrder = 9998; // Death mesajlarının altında (onlar 9999'da)
        
        // Canvas Scaler ekle
        CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        
        // Graphic Raycaster ekle
        gameObject.AddComponent<GraphicRaycaster>();
        
        // Fade için Image oluştur
        GameObject fadePanel = new GameObject("FadePanel");
        fadePanel.transform.SetParent(transform);
        
        fadeImage = fadePanel.AddComponent<Image>();
        fadeImage.color = new Color(0, 0, 0, 0); // Başlangıçta şeffaf
        
        // Panel'i tam ekran yap
        RectTransform rectTransform = fadePanel.GetComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.sizeDelta = Vector2.zero;
        rectTransform.anchoredPosition = Vector2.zero;
    }
    
    public static ScreenFadeEffect GetInstance()
    {
        if (instance == null)
        {
            GameObject fadeObject = new GameObject("ScreenFadeEffect");
            instance = fadeObject.AddComponent<ScreenFadeEffect>();
        }
        return instance;
    }
    
    /// <summary>
    /// Ekranı kararttır (fade to black)
    /// </summary>
    /// <param name="delay">Karartma başlamadan önce beklenecek süre (saniye)</param>
    /// <param name="duration">Karartma animasyonunun süresi (saniye)</param>
    public void FadeToBlack(float delay, float duration)
    {
        StartCoroutine(FadeToBlackCoroutine(delay, duration));
    }
    
    /// <summary>
    /// Ekranı aydınlat (fade from black)
    /// </summary>
    /// <param name="duration">Aydınlatma animasyonunun süresi (saniye)</param>
    public void FadeFromBlack(float duration)
    {
        StartCoroutine(FadeFromBlackCoroutine(duration));
    }
    
    private IEnumerator FadeToBlackCoroutine(float delay, float duration)
    {
        // Belirtilen süre kadar bekle
        yield return new WaitForSeconds(delay);
        
        float elapsedTime = 0f;
        Color startColor = fadeImage.color;
        Color targetColor = new Color(0, 0, 0, 1); // Siyah, tam opak
        
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;
            fadeImage.color = Color.Lerp(startColor, targetColor, t);
            yield return null;
        }
        
        fadeImage.color = targetColor;
    }
    
    private IEnumerator FadeFromBlackCoroutine(float duration)
    {
        float elapsedTime = 0f;
        Color startColor = fadeImage.color;
        Color targetColor = new Color(0, 0, 0, 0); // Şeffaf
        
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;
            fadeImage.color = Color.Lerp(startColor, targetColor, t);
            yield return null;
        }
        
        fadeImage.color = targetColor;
    }
    
    /// <summary>
    /// Fade efektini hemen sıfırla
    /// </summary>
    public void ResetFade()
    {
        if (fadeImage != null)
        {
            fadeImage.color = new Color(0, 0, 0, 0);
        }
    }
    
    /// <summary>
    /// Göz kırpma efekti - 2 kez kırpar (gözler açılıyormuş gibi)
    /// </summary>
    public void BlinkEyes()
    {
        StartCoroutine(BlinkEyesCoroutine());
    }
    
    private IEnumerator BlinkEyesCoroutine()
    {
        // Başlangıçta tamamen karanlık (gözler kapalı)
        fadeImage.color = new Color(0, 0, 0, 1);
        
        // İlk bekleme - bilinç yavaşça geliyor
        yield return new WaitForSeconds(0.3f);
        
        // 1. Kırpma - Gözler ilk kez açılmaya çalışıyor
        yield return FadeToAlpha(0.6f, 0.4f); // Yavaşça aralık
        yield return new WaitForSeconds(0.2f);
        
        // Tekrar kapanır (ağır gözkapaklarını açamıyor)
        yield return FadeToAlpha(1f, 0.3f);
        yield return new WaitForSeconds(0.4f);
        
        // 2. Kırpma - Daha güçlü, daha fazla açılır
        yield return FadeToAlpha(0.3f, 0.5f); // Daha fazla açılır
        yield return new WaitForSeconds(0.3f);
        
        // Hafif titreme (odaklanmaya çalışıyor)
        yield return FadeToAlpha(0.4f, 0.15f);
        yield return new WaitForSeconds(0.15f);
        
        // Tamamen açılır - netleşir
        yield return FadeToAlpha(0f, 0.8f); // Yavaşça tamamen açık
    }
    
    private IEnumerator FadeToAlpha(float targetAlpha, float duration)
    {
        float elapsedTime = 0f;
        Color startColor = fadeImage.color;
        Color targetColor = new Color(0, 0, 0, targetAlpha);
        
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;
            fadeImage.color = Color.Lerp(startColor, targetColor, t);
            yield return null;
        }
        
        fadeImage.color = targetColor;
    }
}
