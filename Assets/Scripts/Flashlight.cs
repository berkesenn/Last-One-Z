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
    public float batteryLife = 300f; // 5 dakika
    public float batteryDrainRate = 1f; // Saniyede azalma
    private float currentBattery;
    
    private bool isOn = false;
    
    void Start()
    {
        // Eğer flashlight referansı yoksa, bu objede bul
        if (flashlight == null)
        {
            flashlight = GetComponentInChildren<Light>();
        }
        
        // Başlangıçta kapalı
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
        
        // Audio source ekle
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0; // 2D ses
        
        // Batarya başlat
        if (useBattery)
        {
            currentBattery = batteryLife;
        }
    }
    
    void Update()
    {
        // F tuşu ile aç/kapat
        if (Input.GetKeyDown(toggleKey))
        {
            ToggleFlashlight();
        }
        
        // Batarya sistemi
        if (isOn && useBattery)
        {
            currentBattery -= batteryDrainRate * Time.deltaTime;
            
            // Batarya bitti
            if (currentBattery <= 0)
            {
                currentBattery = 0;
                TurnOff();
            }
            
            // Batarya azaldıkça ışık azalır
            if (flashlight != null)
            {
                float batteryPercent = currentBattery / batteryLife;
                flashlight.intensity = intensity * batteryPercent;
                
                // Titreme efekti (batarya azaldıkça artar)
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
        // Batarya kontrolü
        if (useBattery && currentBattery <= 0)
        {
            Debug.Log("Flashlight battery is dead!");
            return;
        }
        
        if (flashlight != null)
        {
            flashlight.enabled = true;
            isOn = true;
            PlayToggleSound();
            Debug.Log("Flashlight ON");
        }
    }
    
    void TurnOff()
    {
        if (flashlight != null)
        {
            flashlight.enabled = false;
            isOn = false;
            PlayToggleSound();
            Debug.Log("Flashlight OFF");
        }
    }
    
    void PlayToggleSound()
    {
        if (audioSource != null && toggleSound != null)
        {
            audioSource.PlayOneShot(toggleSound);
        }
    }
    
    // Batarya yenile (medkit gibi)
    public void RechargeBattery(float amount)
    {
        currentBattery = Mathf.Min(currentBattery + amount, batteryLife);
        Debug.Log("Battery recharged: " + currentBattery + "/" + batteryLife);
    }
    
    // Dışarıdan kontrol için
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
