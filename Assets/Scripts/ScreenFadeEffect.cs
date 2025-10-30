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
        fadeCanvas = gameObject.AddComponent<Canvas>();
        fadeCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        fadeCanvas.sortingOrder = 9998;
        
        // Add CanvasScaler for proper scaling
        CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        
        gameObject.AddComponent<GraphicRaycaster>();
        
        GameObject fadePanel = new GameObject("FadePanel");
        fadePanel.transform.SetParent(transform);
        
        fadeImage = fadePanel.AddComponent<Image>();
        fadeImage.color = new Color(0, 0, 0, 0);
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
    
    public void FadeToBlack(float delay, float duration)
    {
        StartCoroutine(FadeToBlackCoroutine(delay, duration));
    }
    
    public void FadeFromBlack(float duration)
    {
        StartCoroutine(FadeFromBlackCoroutine(duration));
    }
    
    private IEnumerator FadeToBlackCoroutine(float delay, float duration)
    {
        yield return new WaitForSeconds(delay);
        
        float elapsedTime = 0f;
        Color startColor = fadeImage.color;
        Color targetColor = new Color(0, 0, 0, 1);
        
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
        Color targetColor = new Color(0, 0, 0, 0);
        
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;
            fadeImage.color = Color.Lerp(startColor, targetColor, t);
            yield return null;
        }
        
        fadeImage.color = targetColor;
    }
    
    public void ResetFade()
    {
        if (fadeImage != null)
        {
            fadeImage.color = new Color(0, 0, 0, 0);
        }
    }
    
    public void BlinkEyes()
    {
        StartCoroutine(BlinkEyesCoroutine());
    }
    
    private IEnumerator BlinkEyesCoroutine()
    {
        fadeImage.color = new Color(0, 0, 0, 1);
        
        yield return new WaitForSeconds(0.3f);
        
        yield return FadeToAlpha(0.6f, 0.4f);
        yield return new WaitForSeconds(0.2f);
        
        yield return FadeToAlpha(1f, 0.3f);
        yield return new WaitForSeconds(0.4f);
        
        yield return FadeToAlpha(0.3f, 0.5f);
        yield return new WaitForSeconds(0.3f);
        
        yield return FadeToAlpha(0.4f, 0.15f);
        yield return new WaitForSeconds(0.15f);
        
        yield return FadeToAlpha(0f, 0.8f);
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
