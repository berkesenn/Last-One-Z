using UnityEngine;

public class LightFlicker : MonoBehaviour
{
    private Light lightComponent;
    
    [Header("Flicker Settings")]
    public float minIntensity = 0.8f;
    public float maxIntensity = 1.2f;
    public float flickerSpeed = 10f;
    
    private float baseIntensity;
    
    void Start()
    {
        lightComponent = GetComponent<Light>();
        if (lightComponent != null)
        {
            baseIntensity = lightComponent.intensity;
        }
    }
    
    void Update()
    {
        if (lightComponent != null)
        {
            float noise = Mathf.PerlinNoise(Time.time * flickerSpeed, 0f);
            float intensity = Mathf.Lerp(minIntensity, maxIntensity, noise);
            lightComponent.intensity = baseIntensity * intensity;
        }
    }
}
