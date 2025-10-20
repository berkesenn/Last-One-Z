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
    
    [Header("First Person View Settings")]
    public bool hideBodyInFirstPerson = true; // Sadece sol kolu göster
    public List<string> visibleMeshNames = new List<string> { "Man_Arms_Mesh" }; // Görünür olacak mesh'ler
    public bool castShadowsOnly = true; // Gizli mesh'ler sadece gölge mi atsın
    
    // Private variables
    private CharacterController controller;
    private Vector3 velocity;
    private float xRotation = 0f;
    private Vector3 currentCameraRecoil;
    private Vector3 targetCameraRecoil;
    private bool initialized = false;

    // Footstep variables
    private Vector3 lastPosition;
    private float footstepDistance = 0f;
    public float walkStepDistance = 2.2f;
    public float runStepDistance = 2f;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        
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
        
        if (Terrain.activeTerrain != null)
        {
            float terrainHeight = Terrain.activeTerrain.SampleHeight(transform.position);
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
        controller.Move(move * currentSpeed * Time.deltaTime);
        
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
}
