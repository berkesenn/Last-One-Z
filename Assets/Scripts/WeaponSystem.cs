using UnityEngine;

public class WeaponSystem : MonoBehaviour
{
    [Header("Weapon Stats")]
    public int damage = 25;
    public float range = 100f;
    public float fireRate = 0.1f;
    public int magazineSize = 12; // Pistol için 12 mermi
    public float reloadTime = 2f;
    
    [Header("Weapon Behavior")]
    public bool isAutomatic = false; // Pistol tekli ateş
    public float recoilAmount = 2f;
    public float recoilSpeed = 10f;
    
    [Header("References")]
    public Camera fpsCam;
    public PlayerController playerController; // Kamera tepme efekti için
    public ParticleSystem muzzleFlash;
    public GameObject impactEffect;
    public Transform firePoint;
    
    [Header("Trigger Finger")]
    public Transform rightIndexFinger; // Sağ işaret parmağı kemiği (index_01_r)
    public float triggerPullRotation = 15f; // Tetik çekerken parmağın dönüş açısı
    public float triggerSpeed = 20f; // Tetik animasyon hızı
    
    [Header("UI")]
    public bool showAmmoUI = true;
    
    // Private variables
    private int currentAmmo;
    private float nextTimeToFire = 0f;
    private bool isReloading = false;
    private Vector3 originalRotation;
    private Vector3 originalFingerRotation;
    private float currentFingerRotation = 0f;
    private Vector3 currentRecoil;
    
    void Start()
    {
        currentAmmo = magazineSize;
        
        if (fpsCam == null)
        {
            fpsCam = Camera.main;
        }
        
        originalRotation = transform.localEulerAngles;
        
        // Parmağın orijinal rotasyonunu kaydet
        if (rightIndexFinger != null)
        {
            originalFingerRotation = rightIndexFinger.localEulerAngles;
        }
        
        // Muzzle flash ayarları
        if (muzzleFlash != null)
        {
            // Particle System ayarları
            var main = muzzleFlash.main;
            main.playOnAwake = false;
            main.loop = false;
            main.stopAction = ParticleSystemStopAction.None; // Objeyi kapatma!
            muzzleFlash.Stop();
            
            // Child Particle System'leri de ayarla
            ParticleSystem[] allParticles = muzzleFlash.GetComponentsInChildren<ParticleSystem>(true);
            foreach (ParticleSystem ps in allParticles)
            {
                var psMain = ps.main;
                psMain.playOnAwake = false;
                psMain.loop = false;
                psMain.stopAction = ParticleSystemStopAction.None; // Objeyi kapatma!
            }
            
            // WFX Light Flicker script'lerini devre dışı bırak
            MonoBehaviour[] scripts = muzzleFlash.GetComponentsInChildren<MonoBehaviour>();
            foreach (MonoBehaviour script in scripts)
            {
                if (script != null && script.GetType().Name.Contains("WFX"))
                {
                    script.enabled = false;
                }
            }
            
            // Tüm Light component'lerini kapat
            Light[] lights = muzzleFlash.GetComponentsInChildren<Light>(true);
            foreach (Light light in lights)
            {
                light.enabled = false;
            }
        }
    }
    
    void Update()
    {
        if (isReloading)
            return;
        
        // Auto reload when empty
        if (currentAmmo <= 0)
        {
            StartReload();
            return;
        }
        
        // Shooting input
        if (isAutomatic)
        {
            if (Input.GetButton("Fire1") && Time.time >= nextTimeToFire)
            {
                nextTimeToFire = Time.time + fireRate;
                Shoot();
            }
        }
        else
        {
            if (Input.GetButtonDown("Fire1") && Time.time >= nextTimeToFire)
            {
                nextTimeToFire = Time.time + fireRate;
                Shoot();
            }
        }
        
        // Manual reload
        if (Input.GetKeyDown(KeyCode.R) && currentAmmo < magazineSize)
        {
            StartReload();
        }
        
        // Apply recoil
        ApplyRecoil();
        
        // Apply trigger finger animation
        ApplyTriggerFingerAnimation();
    }
    
    void Shoot()
    {
        currentAmmo--;
        
        // Play gunshot sound
        AudioManager audioManager = AudioManager.GetInstance();
        if (audioManager != null)
        {
            audioManager.PlayGunshot();
        }
        
        // Muzzle flash - hem particle hem light
        if (muzzleFlash != null)
        {
            // Objeyi aktif et (eğer devre dışıysa)
            muzzleFlash.gameObject.SetActive(true);
            
            // Particle System'i tetikle
            muzzleFlash.Stop(); // Önce durdur
            muzzleFlash.Clear(); // Temizle
            muzzleFlash.Play(); // Başlat
            
            // Light'ı kısa süreliğine aç
            StartCoroutine(FlashLight());
        }
        
        // Add recoil
        currentRecoil += new Vector3(-recoilAmount, Random.Range(-recoilAmount * 0.5f, recoilAmount * 0.5f), 0);
        
        // Camera recoil
        if (playerController != null)
        {
            playerController.AddCameraRecoil();
        }
        
        // Trigger finger pull
        currentFingerRotation = triggerPullRotation;
        
        // Raycast
        RaycastHit hit;
        Vector3 shootDirection = fpsCam.transform.forward;
        
        if (Physics.Raycast(fpsCam.transform.position, shootDirection, out hit, range))
        {
            Debug.Log("Hit: " + hit.transform.name);
            
            // Damage
            Enemy enemy = hit.transform.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
            }
            
            // Impact effect
            if (impactEffect != null)
            {
                GameObject impact = Instantiate(impactEffect, hit.point, Quaternion.LookRotation(hit.normal));
                Destroy(impact, 2f);
            }
        }
    }
    
    void ApplyRecoil()
    {
        // Smoothly return to original position
        currentRecoil = Vector3.Lerp(currentRecoil, Vector3.zero, recoilSpeed * Time.deltaTime);
        transform.localEulerAngles = originalRotation + currentRecoil;
    }
    
    void ApplyTriggerFingerAnimation()
    {
        if (rightIndexFinger == null)
            return;
        
        // Smoothly return finger to original position
        currentFingerRotation = Mathf.Lerp(currentFingerRotation, 0f, triggerSpeed * Time.deltaTime);
        
        // Apply rotation (Z axis for finger curl)
        Vector3 newRotation = originalFingerRotation;
        newRotation.z += currentFingerRotation;
        rightIndexFinger.localEulerAngles = newRotation;
    }
    
    void StartReload()
    {
        isReloading = true;
        Invoke("FinishReload", reloadTime);
        AudioManager audioManager = AudioManager.GetInstance();
        if (audioManager != null)
        {
            audioManager.PlayReload();
        }
    }
    
    void FinishReload()
{
    currentAmmo = magazineSize;
    isReloading = false;
}
    
    void OnGUI()
    {
        if (!showAmmoUI)
            return;
        
        // Simple ammo counter
        int fontSize = 24;
        GUI.skin.label.fontSize = fontSize;
        GUI.skin.label.normal.textColor = Color.white;
        
        string ammoText = currentAmmo + " / ∞";
        if (isReloading)
        {
            ammoText = "RELOADING...";
        }
        
        GUI.Label(new Rect(Screen.width - 200, Screen.height - 50, 200, 50), ammoText);
    }
    
    System.Collections.IEnumerator FlashLight()
    {
        if (muzzleFlash != null)
        {
            // Tüm child Light'ları bul ve aç
            Light[] lights = muzzleFlash.GetComponentsInChildren<Light>(true);
            foreach (Light light in lights)
            {
                light.enabled = true;
            }
            
            // 0.05 saniye bekle
            yield return new WaitForSeconds(0.05f);
            
            // Light'ları kapat
            foreach (Light light in lights)
            {
                light.enabled = false;
            }
        }
    }
}