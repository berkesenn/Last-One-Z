using UnityEngine;

public class WeaponSystem : MonoBehaviour
{
    [Header("Weapon Stats")]
    public int damage = 25;
    public float range = 100f;
    public float fireRate = 0.1f;
    public int magazineSize = 30;
    public float reloadTime = 2f;
    
    [Header("Weapon Behavior")]
    public bool isAutomatic = true;
    public float recoilAmount = 2f;
    public float recoilSpeed = 10f;
    
    [Header("References")]
    public Camera fpsCam;
    public ParticleSystem muzzleFlash;
    public GameObject impactEffect;
    public Transform firePoint;
    
    [Header("UI")]
    public bool showAmmoUI = true;
    
    // Private variables
    private int currentAmmo;
    private float nextTimeToFire = 0f;
    private bool isReloading = false;
    private Vector3 originalRotation;
    private Vector3 currentRecoil;
    
    void Start()
    {
        currentAmmo = magazineSize;
        
        if (fpsCam == null)
        {
            fpsCam = Camera.main;
        }
        
        originalRotation = transform.localEulerAngles;
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
        
        // Muzzle flash
        if (muzzleFlash != null)
        {
            muzzleFlash.Play();
        }
        
        // Add recoil
        currentRecoil += new Vector3(-recoilAmount, Random.Range(-recoilAmount * 0.5f, recoilAmount * 0.5f), 0);
        
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
}