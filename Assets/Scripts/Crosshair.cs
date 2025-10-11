using UnityEngine;

public class Crosshair : MonoBehaviour
{
    [Header("Crosshair Settings")]
    public Color crosshairColor = Color.white;
    public float crosshairSize = 10f;
    public float crosshairThickness = 2f;
    public float crosshairGap = 5f;
    public bool showDot = true;
    public float dotSize = 2f;
    
    [Header("Dynamic Crosshair")]
    public bool isDynamic = false;
    public float maxSpread = 20f;
    private float currentSpread = 0f;
    
    void OnGUI()
    {
        float screenCenterX = Screen.width / 2f;
        float screenCenterY = Screen.height / 2f;
        
        // Calculate spread
        float spread = isDynamic ? currentSpread : 0f;
        float gap = crosshairGap + spread;
        
        // Create texture for crosshair
        Texture2D crosshairTexture = new Texture2D(1, 1);
        crosshairTexture.SetPixel(0, 0, crosshairColor);
        crosshairTexture.Apply();
        
        // Top line
        GUI.DrawTexture(new Rect(screenCenterX - crosshairThickness / 2f, 
                                 screenCenterY - gap - crosshairSize, 
                                 crosshairThickness, 
                                 crosshairSize), 
                       crosshairTexture);
        
        // Bottom line
        GUI.DrawTexture(new Rect(screenCenterX - crosshairThickness / 2f, 
                                 screenCenterY + gap, 
                                 crosshairThickness, 
                                 crosshairSize), 
                       crosshairTexture);
        
        // Left line
        GUI.DrawTexture(new Rect(screenCenterX - gap - crosshairSize, 
                                 screenCenterY - crosshairThickness / 2f, 
                                 crosshairSize, 
                                 crosshairThickness), 
                       crosshairTexture);
        
        // Right line
        GUI.DrawTexture(new Rect(screenCenterX + gap, 
                                 screenCenterY - crosshairThickness / 2f, 
                                 crosshairSize, 
                                 crosshairThickness), 
                       crosshairTexture);
        
        // Center dot
        if (showDot)
        {
            GUI.DrawTexture(new Rect(screenCenterX - dotSize / 2f, 
                                     screenCenterY - dotSize / 2f, 
                                     dotSize, 
                                     dotSize), 
                           crosshairTexture);
        }
        
        // Cleanup
        Destroy(crosshairTexture);
    }
    
    // Call this from weapon script when shooting
    public void AddSpread(float amount)
    {
        if (isDynamic)
        {
            currentSpread = Mathf.Min(currentSpread + amount, maxSpread);
        }
    }
    
    void Update()
    {
        // Reduce spread over time
        if (isDynamic && currentSpread > 0)
        {
            currentSpread = Mathf.Lerp(currentSpread, 0f, Time.deltaTime * 5f);
        }
    }
}