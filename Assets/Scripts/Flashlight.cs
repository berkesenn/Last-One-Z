using UnityEngine;

public class Flashlight : MonoBehaviour
{
    [Header("Flashlight Settings")]
    public Light flashlight;
    public KeyCode toggleKey = KeyCode.F;
    
    [Header("Light Settings")]
    public float intensity = 2f;
    public float range = 30f;
    public float spotAngle = 45f;
    public Color lightColor = Color.white;
    
    [Header("Audio")]
    public AudioClip toggleSound;
    private AudioSource audioSource;
    
    [Header("Battery (Optional)")]
    public bool useBattery = false;
    public float batteryLife = 300f; // 5 minutes
    public float batteryDrainRate = 1f; // Per second
    private float currentBattery;
    
    private bool isOn = false;
    
    void Start()
    {
        if (flashlight == null)
        {
            flashlight = GetComponentInChildren<Light>();
        }
        
        if (flashlight != null)
        {
            flashlight.enabled = false;
            flashlight.type = LightType.Spot;
            flashlight.intensity = intensity;
            flashlight.range = range;
            flashlight.spotAngle = spotAngle;
            flashlight.color = lightColor;
            flashlight.shadows = LightShadows.Soft;
        }
        
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0;
        
        if (useBattery)
        {
            currentBattery = batteryLife;
        }
    }
    
    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            ToggleFlashlight();
        }
        
        if (isOn && useBattery)
        {
            currentBattery -= batteryDrainRate * Time.deltaTime;
            
            if (currentBattery <= 0)
            {
                currentBattery = 0;
                TurnOff();
            }
            
            if (flashlight != null)
            {
                float batteryPercent = currentBattery / batteryLife;
                flashlight.intensity = intensity * batteryPercent;
                
                if (batteryPercent < 0.3f)
                {
                    flashlight.intensity += Random.Range(-0.2f, 0.2f) * (1f - batteryPercent);
                }
            }
        }
    }
    
    void ToggleFlashlight()
    {
        if (isOn)
        {
            TurnOff();
        }
        else
        {
            TurnOn();
        }
    }
    
    void TurnOn()
    {
        if (useBattery && currentBattery <= 0)
        {
            return;
        }
        
        if (flashlight != null)
        {
            flashlight.enabled = true;
            isOn = true;
            PlayToggleSound();
        }
    }
    
    void TurnOff()
    {
        if (flashlight != null)
        {
            flashlight.enabled = false;
            isOn = false;
            PlayToggleSound();
        }
    }
    
    void PlayToggleSound()
    {
        if (audioSource != null && toggleSound != null)
        {
            audioSource.PlayOneShot(toggleSound);
        }
    }
    
    public void RechargeBattery(float amount)
    {
        currentBattery = Mathf.Min(currentBattery + amount, batteryLife);
    }
    
    public bool IsOn()
    {
        return isOn;
    }
    
    public float GetBatteryPercent()
    {
        if (!useBattery) return 1f;
        return currentBattery / batteryLife;
    }
}
