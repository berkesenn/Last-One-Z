using Unity.VisualScripting;
using UnityEngine;

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
    
    // Private variables
    private CharacterController controller;
    private Vector3 velocity;
    private float xRotation = 0f;
    private bool initialized = false;

    // Footstep variables
    private Vector3 lastPosition;
    private float footstepDistance = 0f;
    public float walkStepDistance = 2.2f;
    public float runStepDistance = 2f;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        
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
                velocity.y = -2f;

            HandleMovement();
            HandleCamera();

            if (Input.GetButtonDown("Jump") && isGrounded)
                velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);

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
        cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        
        transform.Rotate(Vector3.up * mouseX);
    }
}
