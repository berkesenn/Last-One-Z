using UnityEngine;

public class WeaponSystem : MonoBehaviour
{
    [Header("Weapon Stats")]
    public int damage = 25;
    public float range = 100f;
    public float fireRate = 0.1f;
    public int magazineSize = 12;
    public float reloadTime = 2f;
    
    [Header("Weapon Behavior")]
    public bool isAutomatic = false;
    public float recoilAmount = 2f;
    public float recoilSpeed = 10f;
    
    [Header("References")]
    public Camera fpsCam;
    public PlayerController playerController;
    public PlayerHealth playerHealth;
    public ParticleSystem muzzleFlash;
    public GameObject impactEffect;
    public Transform firePoint;
    
    [Header("Trigger Finger")]
    public Transform rightIndexFinger;
    public float triggerPullRotation = 15f;
    public float triggerSpeed = 20f;
    
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
        
        if (rightIndexFinger != null)
        {
            originalFingerRotation = rightIndexFinger.localEulerAngles;
        }
        
        if (muzzleFlash != null)
        {
            var main = muzzleFlash.main;
            main.playOnAwake = false;
            main.loop = false;
            main.stopAction = ParticleSystemStopAction.None;
            muzzleFlash.Stop();
            
            ParticleSystem[] allParticles = muzzleFlash.GetComponentsInChildren<ParticleSystem>(true);
            foreach (ParticleSystem ps in allParticles)
            {
                var psMain = ps.main;
                psMain.playOnAwake = false;
                psMain.loop = false;
                psMain.stopAction = ParticleSystemStopAction.None;
            }
            
            MonoBehaviour[] scripts = muzzleFlash.GetComponentsInChildren<MonoBehaviour>();
            foreach (MonoBehaviour script in scripts)
            {
                if (script != null && script.GetType().Name.Contains("WFX"))
                {
                    script.enabled = false;
                }
            }
            
            Light[] lights = muzzleFlash.GetComponentsInChildren<Light>(true);
            foreach (Light light in lights)
            {
                light.enabled = false;
            }
        }
    }
    
    void Update()
    {
        // Don't process input when game is paused
        if (GameManager.IsPaused)
        {
            return;
        }
        
        if (playerHealth != null && playerHealth.IsDead)
        {
            return;
        }
        
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
        
        if (muzzleFlash != null)
        {
            muzzleFlash.gameObject.SetActive(true);
            
            muzzleFlash.Stop();
            muzzleFlash.Clear();
            muzzleFlash.Play();
            
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
            // Check for headshot first
            EnemyHeadshot headshot = hit.transform.GetComponent<EnemyHeadshot>();
            if (headshot != null)
            {
                headshot.TakeDamage(damage);
            }
            else
            {
                // Check for body shot
                Enemy enemy = hit.transform.GetComponent<Enemy>();
                if (enemy != null)
                {
                    enemy.TakeDamage(damage, false);
                }
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
            Light[] lights = muzzleFlash.GetComponentsInChildren<Light>(true);
            foreach (Light light in lights)
            {
                light.enabled = true;
            }
            
            yield return new WaitForSeconds(0.05f);
            
            foreach (Light light in lights)
            {
                light.enabled = false;
            }
        }
    }
}