using Unity.VisualScripting;
using UnityEngine;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 5f;
    public float runSpeed = 8f;
    public float jumpForce = 8f;
    public float gravity = -20f;
    
    [Header("Camera Settings")]
    public float mouseSensitivity = 2f;
    public Transform cameraTransform;
    public float maxLookAngle = 80f;
    
    [Header("Camera Recoil Settings")]
    public float cameraRecoilAmount = 1.5f;
    public float cameraRecoilSpeed = 10f;
    public float cameraRecoilSideways = 0.5f;
    
    [Header("Animation Settings")]
    public Animator animator;
    
    [Header("Weapon Settings")]
    public GameObject weaponObject; // Silah objesi (Inspector'dan atanacak)
    
    [Header("First Person View Settings")]
    public bool hideBodyInFirstPerson = true; // Sadece sol kolu göster
    public List<string> visibleMeshNames = new List<string> { "Man_Arms_Mesh" }; // Görünür olacak mesh'ler
    public bool castShadowsOnly = true; // Gizli mesh'ler sadece gölge mi atsın
    
    [Header("Movement Boundaries")]
    public bool useBoundaries = true; // Sınırları aktif et
    public float boundaryMargin = 10f; // Terrain kenarından ne kadar uzakta dur (metre)
    
    // Private variables
    private CharacterController controller;
    private Vector3 velocity;
    private float xRotation = 0f;
    private Vector3 currentCameraRecoil;
    private Vector3 targetCameraRecoil;
    private bool initialized = false;
    private Terrain terrain;
    private Vector3 terrainSize;
    private Vector3 terrainPosition;
    private Transform originalCameraParent; // Camera'nın orijinal parent'ı
    private Transform originalClavicleL_Parent; // clavicle_l'nin orijinal parent'ı
    private Transform originalClavicleR_Parent; // clavicle_r'nin orijinal parent'ı
    private Transform originalNeck_Parent; // neck_01'in orijinal parent'ı
    private Transform originalWeapon_Parent; // M1911'in orijinal parent'ı

    // Footstep variables
    private Vector3 lastPosition;
    private float footstepDistance = 0f;
    public float walkStepDistance = 2.2f;
    public float runStepDistance = 2f;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        AudioManager audioManager = AudioManager.GetInstance();
        if (audioManager != null)
        {
            audioManager.backgroundMusicPlay();
        }
        
        // Get Animator component if not assigned
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
        
        // Hide body meshes except visible ones (first person view)
        if (hideBodyInFirstPerson)
        {
            HideBodyMeshes();
        }
        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (cameraTransform == null)
        {
            cameraTransform = Camera.main.transform;
        }
        
        // Camera'nın orijinal parent'ını kaydet
        if (cameraTransform != null)
        {
            originalCameraParent = cameraTransform.parent;
        }
        
        // Terrain bilgilerini al
        terrain = Terrain.activeTerrain;
        if (terrain != null)
        {
            terrainSize = terrain.terrainData.size;
            terrainPosition = terrain.transform.position;
            
            float terrainHeight = terrain.SampleHeight(transform.position);
            Vector3 startPosition = transform.position;
            startPosition.y = terrainHeight + controller.height / 2f;
            transform.position = startPosition;
        }

        lastPosition = transform.position;
    }
    
    void Update()
    {
        bool isGrounded = controller.isGrounded;

        if (initialized)
        {
            if (isGrounded && velocity.y < 0)
            {
                velocity.y = -2f;
                
                // Reset jump trigger when grounded
                if (animator != null && animator.runtimeAnimatorController != null)
                {
                    animator.ResetTrigger("Jump");
                }
            }

            HandleMovement();
            HandleCamera();

            if (Input.GetButtonDown("Jump") && isGrounded)
            {
                velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
                
                // Trigger jump animation
                if (animator != null && animator.runtimeAnimatorController != null)
                {
                    animator.SetTrigger("Jump");
                }
            }

            velocity.y += gravity * Time.deltaTime;
            controller.Move(velocity * Time.deltaTime);
        }
        else
        {
            initialized = true;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        PlayerHealth playerHealth = GetComponent<PlayerHealth>();
        if (Input.GetKeyDown(KeyCode.F1))
        {
            playerHealth.IsDead = true;
            playerHealth.Die();
        }
    }
    
    void HandleMovement()
    {
        AudioManager audioManager = AudioManager.GetInstance();

        // Get input
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        // Sprint check
        bool isRunning = Input.GetKey(KeyCode.LeftShift);
        float currentSpeed = isRunning ? runSpeed : walkSpeed;

        // Movement direction
        Vector3 move = transform.right * x + transform.forward * z;
        Vector3 movement = move * currentSpeed * Time.deltaTime;
        
        // Hareket öncesi pozisyonu kaydet
        Vector3 nextPosition = transform.position + movement;
        
        // Sınır kontrolü
        if (useBoundaries && terrain != null)
        {
            nextPosition = ClampPositionToBoundaries(nextPosition);
        }
        
        // Hareketi uygula
        controller.Move(nextPosition - transform.position);
        
        // Update animations
        if (animator != null && animator.runtimeAnimatorController != null)
        {
            bool isMoving = move.magnitude > 0.1f;
            bool isGrounded = controller.isGrounded;
            
            // Set animation parameters
            animator.SetBool("IsMoving", isMoving);
            animator.SetBool("IsRunning", isRunning && isMoving);
            animator.SetBool("IsGrounded", isGrounded);
            animator.SetFloat("Speed", move.magnitude * currentSpeed);
            animator.SetFloat("VelocityY", velocity.y);
        }

        // Footstep
        float stepDistance = isRunning ? runStepDistance : walkStepDistance;
        footstepDistance += Vector3.Distance(transform.position, lastPosition);
        if (footstepDistance >= stepDistance && move.magnitude > 0.1f)
        {
            if (audioManager != null)
                audioManager.PlayFootstep();

            footstepDistance = 0f;
        }
        lastPosition = transform.position;
    }
    
    void HandleCamera()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
        
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -maxLookAngle, maxLookAngle);
        
        // Tepme efektini uygula
        ApplyCameraRecoil();
        
        // Kamera rotasyonunu ayarla (tepme dahil)
        cameraTransform.localRotation = Quaternion.Euler(xRotation + currentCameraRecoil.x, currentCameraRecoil.y, currentCameraRecoil.z);
        
        transform.Rotate(Vector3.up * mouseX);
    }
    
    void HideBodyMeshes()
    {
        // Karakterdeki tüm SkinnedMeshRenderer'ları bul
        SkinnedMeshRenderer[] meshRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();
        
        Debug.Log($"Toplam {meshRenderers.Length} mesh bulundu.");
        
        foreach (SkinnedMeshRenderer meshRenderer in meshRenderers)
        {
            // Mesh'in ismini kontrol et
            bool shouldBeVisible = false;
            
            foreach (string visibleName in visibleMeshNames)
            {
                if (meshRenderer.gameObject.name.Contains(visibleName))
                {
                    shouldBeVisible = true;
                    break;
                }
            }
            
            if (shouldBeVisible)
            {
                // Görünür mesh'ler normal şekilde render edilsin
                meshRenderer.enabled = true;
                meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
                Debug.Log($"{meshRenderer.gameObject.name} - Görünür (Gölge: On)");
            }
            else
            {
                // Görünmez mesh'ler - hepsi gölge atsın (kafa dahil)
                if (castShadowsOnly)
                {
                    // Sadece gölge at, mesh görünmesin
                    meshRenderer.enabled = true;
                    meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
                    Debug.Log($"{meshRenderer.gameObject.name} - Sadece Gölge");
                }
                else
                {
                    // Tamamen gizle (gölge de yok)
                    meshRenderer.enabled = false;
                    Debug.Log($"{meshRenderer.gameObject.name} - Tamamen Gizli");
                }
            }
        }
    }
    
    Vector3 ClampPositionToBoundaries(Vector3 position)
    {
        // Terrain sınırlarını hesapla (margin dahil)
        float minX = terrainPosition.x + boundaryMargin;
        float maxX = terrainPosition.x + terrainSize.x - boundaryMargin;
        float minZ = terrainPosition.z + boundaryMargin;
        float maxZ = terrainPosition.z + terrainSize.z - boundaryMargin;
        
        // Pozisyonu sınırla
        position.x = Mathf.Clamp(position.x, minX, maxX);
        position.z = Mathf.Clamp(position.z, minZ, maxZ);
        
        return position;
    }
    
    void ApplyCameraRecoil()
    {
        // Hedef tepmeye doğru hızlıca git
        currentCameraRecoil = Vector3.Lerp(currentCameraRecoil, targetCameraRecoil, cameraRecoilSpeed * Time.deltaTime);
        
        // Hedef tepmeyi yavaşça sıfıra çek (yumuşak geri dönüş)
        targetCameraRecoil = Vector3.Lerp(targetCameraRecoil, Vector3.zero, cameraRecoilSpeed * Time.deltaTime);
    }

    // WeaponSystem'dan çağrılacak - public olmalı
    public void AddCameraRecoil()
    {
        // Yukarı doğru tepme + ufak yanlara sallanma
        float sideways = Random.Range(-cameraRecoilSideways, cameraRecoilSideways);
        targetCameraRecoil += new Vector3(-cameraRecoilAmount, sideways, 0);
    }

    // PlayerHealth'ten çağrılacak - ölüm animasyonu
    public void PlayDeathAnimation()
    {
        if (animator != null && animator.runtimeAnimatorController != null)
        {
            Debug.Log("Death animation started");
            
            // Character Controller'ı kapat (animasyonun pozisyonu kontrol etmesi için)
            CharacterController cc = GetComponent<CharacterController>();
            if (cc != null)
            {
                cc.enabled = false;
            }
            
            // spine_03'ü bul
            Transform spine03 = FindChildRecursive(transform, "spine_03");
            
            if (spine03 != null)
            {
                // ÖNCELİKLE objeleri taşı (Animator bone cache'i güncellensin)
                Transform clavicleL = FindChildRecursive(transform, "clavicle_l");
                Transform clavicleR = FindChildRecursive(transform, "clavicle_r");
                Transform neck01 = FindChildRecursive(transform, "neck_01");
                
                if (clavicleL != null)
                {
                    originalClavicleL_Parent = clavicleL.parent;
                    clavicleL.SetParent(spine03, true); // worldPositionStays = true
                    Debug.Log("clavicle_l moved to spine_03");
                }
                
                if (clavicleR != null)
                {
                    originalClavicleR_Parent = clavicleR.parent;
                    clavicleR.SetParent(spine03, true);
                    Debug.Log("clavicle_r moved to spine_03");
                    
                    // clavicle_r taşındıktan sonra M1911'i bul ve bağımsız yap
                    Transform handR = FindChildRecursive(clavicleR, "hand_r");
                    Debug.Log("hand_r bulundu mu? " + (handR != null ? "EVET" : "HAYIR"));
                    Debug.Log("weaponObject null mu? " + (weaponObject == null ? "EVET" : "HAYIR"));
                    
                    if (handR != null)
                    {
                        // M1911'i doğrudan hand_r altında ara
                        Transform m1911 = null;
                        foreach (Transform child in handR)
                        {
                            if (child.name.Contains("M1911") || child.name.Contains("1911"))
                            {
                                m1911 = child;
                                Debug.Log("M1911 bulundu: " + child.name);
                                break;
                            }
                        }
                        
                        // Eğer Inspector'dan weaponObject atanmışsa onu kullan
                        if (weaponObject != null)
                        {
                            m1911 = weaponObject.transform;
                            Debug.Log("weaponObject kullanılıyor: " + weaponObject.name);
                        }
                        
                        if (m1911 != null)
                        {
                            // M1911'in world pozisyon ve rotasyonunu kaydet
                            Vector3 weaponWorldPos = m1911.position;
                            Quaternion weaponWorldRot = m1911.rotation;
                            
                            // Orijinal parent'ı kaydet
                            originalWeapon_Parent = m1911.parent;
                            Debug.Log("M1911 parent'tan ayrılıyor. Önceki parent: " + originalWeapon_Parent.name);
                            
                            // Parent'tan ayır (bağımsız parent obje yap)
                            m1911.SetParent(null, true);
                            Debug.Log("M1911 parent'ı: " + (m1911.parent == null ? "NULL (Bağımsız)" : m1911.parent.name));
                            
                            // World pozisyonunu geri yükle
                            m1911.position = weaponWorldPos;
                            m1911.rotation = weaponWorldRot;
                            
                            // Rigidbody ekle (eğer yoksa)
                            Rigidbody weaponRb = m1911.GetComponent<Rigidbody>();
                            if (weaponRb == null)
                            {
                                weaponRb = m1911.gameObject.AddComponent<Rigidbody>();
                                Debug.Log("Rigidbody eklendi");
                            }
                            
                            // Physics ayarları
                            weaponRb.useGravity = true;
                            weaponRb.isKinematic = false;
                            weaponRb.collisionDetectionMode = CollisionDetectionMode.Continuous;
                            
                            // Rotasyonu sabitle (dönmesin)
                            weaponRb.freezeRotation = true;
                            
                            // Collider ekle (eğer yoksa)
                            if (m1911.GetComponent<Collider>() == null)
                            {
                                BoxCollider col = m1911.gameObject.AddComponent<BoxCollider>();
                                col.size = new Vector3(0.05f, 0.15f, 0.2f);
                                Debug.Log("BoxCollider eklendi");
                            }
                            
                            // Kuvvet ekleme - KALDIRILDI (sadece gravity ile düşsün)
                            
                            // WeaponSystem scriptini devre dışı bırak
                            WeaponSystem ws = m1911.GetComponent<WeaponSystem>();
                            if (ws != null)
                            {
                                ws.enabled = false;
                                Debug.Log("WeaponSystem disabled");
                            }
                            
                            Debug.Log("Weapon dropped from hand_r - BAŞARILI!");
                        }
                        else
                        {
                            Debug.LogError("M1911 bulunamadı! hand_r altındaki child objeleri kontrol et.");
                        }
                    }
                }
                
                if (neck01 != null)
                {
                    originalNeck_Parent = neck01.parent;
                    neck01.SetParent(spine03, true);
                    Debug.Log("neck_01 moved to spine_03");
                }
                
                // Camera'yı spine_03'e taşı
                if (cameraTransform != null)
                {
                    cameraTransform.SetParent(spine03, true);
                    Debug.Log("Camera moved to spine_03");
                }
            }
            else
            {
                Debug.LogWarning("spine_03 not found!");
            }
            
            // SONRA Apply Root Motion'u aç ve animasyonu başlat
            animator.applyRootMotion = true;
            
            // Animator'ı rebind et (bone hierarchy değiştiği için)
            animator.Rebind();
            
            // IsDead parametresini ayarla
            animator.SetBool("IsDead", true);
        }
        else
        {
            Debug.LogError("Animator not found or not configured!");
        }
    }
    
    // Oyun resetlendiğinde camera ve clavicle'ları eski yerine döndür
    public void ResetCamera()
    {
        if (cameraTransform != null && originalCameraParent != null)
        {
            cameraTransform.SetParent(originalCameraParent);
            Debug.Log("Camera restored to original parent");
        }
        
        // clavicle'ları ve neck'i geri döndür
        Transform clavicleL = FindChildRecursive(transform, "clavicle_l");
        Transform clavicleR = FindChildRecursive(transform, "clavicle_r");
        Transform neck01 = FindChildRecursive(transform, "neck_01");
        
        if (clavicleL != null && originalClavicleL_Parent != null)
        {
            clavicleL.SetParent(originalClavicleL_Parent);
            Debug.Log("clavicle_l restored to original parent");
        }
        
        if (clavicleR != null && originalClavicleR_Parent != null)
        {
            clavicleR.SetParent(originalClavicleR_Parent);
            Debug.Log("clavicle_r restored to original parent");
        }
        
        if (neck01 != null && originalNeck_Parent != null)
        {
            neck01.SetParent(originalNeck_Parent);
            Debug.Log("neck_01 restored to original parent");
        }
    }
    
    // Recursive child bulma fonksiyonu
    private Transform FindChildRecursive(Transform parent, string childName)
    {
        foreach (Transform child in parent)
        {
            if (child.name == childName)
                return child;
            
            Transform result = FindChildRecursive(child, childName);
            if (result != null)
                return result;
        }
        return null;
    }
}

