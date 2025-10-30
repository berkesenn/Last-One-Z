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
    public float groundCheckDistance = 0.3f;
    public LayerMask groundLayer;
    
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
    public GameObject weaponObject;
    
    [Header("First Person View Settings")]
    public bool hideBodyInFirstPerson = true;
    public List<string> visibleMeshNames = new List<string> { "Man_Arms_Mesh" };
    public bool castShadowsOnly = true;
    
    [Header("Movement Boundaries")]
    public bool useBoundaries = true;
    public float boundaryMargin = 10f;
    
    private CharacterController controller;
    private Vector3 velocity;
    private float xRotation = 0f;
    private Vector3 currentCameraRecoil;
    private Vector3 targetCameraRecoil;
    private bool initialized = false;
    private Terrain terrain;
    private Vector3 terrainSize;
    private Vector3 terrainPosition;
    private Transform originalCameraParent;
    private Transform originalClavicleL_Parent;
    private Transform originalClavicleR_Parent;
    private Transform originalNeck_Parent;
    private Transform originalWeapon_Parent;
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
        
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
        
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
        
        if (cameraTransform != null)
        {
            originalCameraParent = cameraTransform.parent;
        }
        
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
        // Don't process input when game is paused
        if (GameManager.IsPaused)
        {
            return;
        }
        
        // Better ground check using raycast
        bool isGrounded = IsGrounded();

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
        
        // Debug kill command
        if (Input.GetKeyDown(KeyCode.F1))
        {
            PlayerHealth playerHealth = GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.IsDead = true;
                playerHealth.Die();
            }
        }
    }
    
    bool IsGrounded()
    {
        if (controller.isGrounded)
            return true;
        
        Vector3 rayStart = transform.position + Vector3.up * 0.1f;
        return Physics.Raycast(rayStart, Vector3.down, groundCheckDistance, groundLayer);
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
        
        Vector3 nextPosition = transform.position + movement;
        
        if (useBoundaries && terrain != null)
        {
            nextPosition = ClampPositionToBoundaries(nextPosition);
        }
        
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
        
        ApplyCameraRecoil();
        
        // Adjust camera rotation including recoil
        cameraTransform.localRotation = Quaternion.Euler(xRotation + currentCameraRecoil.x, currentCameraRecoil.y, currentCameraRecoil.z);
        
        transform.Rotate(Vector3.up * mouseX);
    }
    
    void HideBodyMeshes()
    {
        SkinnedMeshRenderer[] meshRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();
        
        foreach (SkinnedMeshRenderer meshRenderer in meshRenderers)
        {
            // Check if the mesh name is in the visible list
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
                meshRenderer.enabled = true;
                meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            }
            else
            {
                if (castShadowsOnly)
                {
                    meshRenderer.enabled = true;
                    meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
                }
                else
                {
                    meshRenderer.enabled = false;
                }
            }
        }
    }
    
    Vector3 ClampPositionToBoundaries(Vector3 position)
    {
        float minX = terrainPosition.x + boundaryMargin;
        float maxX = terrainPosition.x + terrainSize.x - boundaryMargin;
        float minZ = terrainPosition.z + boundaryMargin;
        float maxZ = terrainPosition.z + terrainSize.z - boundaryMargin;
        
        position.x = Mathf.Clamp(position.x, minX, maxX);
        position.z = Mathf.Clamp(position.z, minZ, maxZ);
        
        return position;
    }
    
    void ApplyCameraRecoil()
    {
        currentCameraRecoil = Vector3.Lerp(currentCameraRecoil, targetCameraRecoil, cameraRecoilSpeed * Time.deltaTime);
        targetCameraRecoil = Vector3.Lerp(targetCameraRecoil, Vector3.zero, cameraRecoilSpeed * Time.deltaTime);
    }

    public void AddCameraRecoil()
    {
        float sideways = Random.Range(-cameraRecoilSideways, cameraRecoilSideways);
        targetCameraRecoil += new Vector3(-cameraRecoilAmount, sideways, 0);
    }
    public void PlayDeathAnimation()
    {
        if (animator != null && animator.runtimeAnimatorController != null)
        {
            CharacterController cc = GetComponent<CharacterController>();
            if (cc != null)
            {
                cc.enabled = false;
            }
            
            Transform spine03 = FindChildRecursive(transform, "spine_03");
            
            if (spine03 != null)
            {
                Transform clavicleL = FindChildRecursive(transform, "clavicle_l");
                Transform clavicleR = FindChildRecursive(transform, "clavicle_r");
                Transform neck01 = FindChildRecursive(transform, "neck_01");
                
                if (clavicleL != null)
                {
                    originalClavicleL_Parent = clavicleL.parent;
                    clavicleL.SetParent(spine03, true);
                }
                
                if (clavicleR != null)
                {
                    originalClavicleR_Parent = clavicleR.parent;
                    clavicleR.SetParent(spine03, true);
                    
                    Transform handR = FindChildRecursive(clavicleR, "hand_r");
                    
                    if (handR != null)
                    {
                        Transform m1911 = null;
                        foreach (Transform child in handR)
                        {
                            if (child.name.Contains("M1911") || child.name.Contains("1911"))
                            {
                                m1911 = child;
                                break;
                            }
                        }
                        
                        if (weaponObject != null)
                        {
                            m1911 = weaponObject.transform;
                        }
                        
                        if (m1911 != null)
                        {
                            Vector3 weaponWorldPos = m1911.position;
                            Quaternion weaponWorldRot = m1911.rotation;
                            
                            originalWeapon_Parent = m1911.parent;
                            
                            m1911.SetParent(null, true);
                            
                            m1911.position = weaponWorldPos;
                            m1911.rotation = weaponWorldRot;
                            
                            Rigidbody weaponRb = m1911.GetComponent<Rigidbody>();
                            if (weaponRb == null)
                            {
                                weaponRb = m1911.gameObject.AddComponent<Rigidbody>();
                            }
                            
                            weaponRb.useGravity = true;
                            weaponRb.isKinematic = false;
                            weaponRb.collisionDetectionMode = CollisionDetectionMode.Continuous;
                            weaponRb.freezeRotation = true;
                            
                            if (m1911.GetComponent<Collider>() == null)
                            {
                                BoxCollider col = m1911.gameObject.AddComponent<BoxCollider>();
                                col.size = new Vector3(0.05f, 0.15f, 0.2f);
                            }
                            
                            WeaponSystem ws = m1911.GetComponent<WeaponSystem>();
                            if (ws != null)
                            {
                                ws.enabled = false;
                            }
                        }
                    }
                }
                
                if (neck01 != null)
                {
                    originalNeck_Parent = neck01.parent;
                    neck01.SetParent(spine03, true);
                }
                
                if (cameraTransform != null)
                {
                    cameraTransform.SetParent(spine03, true);
                }
            }
            
            animator.applyRootMotion = true;
            animator.Rebind();
            animator.SetBool("IsDead", true);
        }
    }
    
    public void ResetCamera()
    {
        if (cameraTransform != null && originalCameraParent != null)
        {
            cameraTransform.SetParent(originalCameraParent);
        }
        
        Transform clavicleL = FindChildRecursive(transform, "clavicle_l");
        Transform clavicleR = FindChildRecursive(transform, "clavicle_r");
        Transform neck01 = FindChildRecursive(transform, "neck_01");
        
        if (clavicleL != null && originalClavicleL_Parent != null)
        {
            clavicleL.SetParent(originalClavicleL_Parent);
        }
        
        if (clavicleR != null && originalClavicleR_Parent != null)
        {
            clavicleR.SetParent(originalClavicleR_Parent);
        }
        
        if (neck01 != null && originalNeck_Parent != null)
        {
            neck01.SetParent(originalNeck_Parent);
        }
    }
    
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

