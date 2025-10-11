using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 5f;
    public float runSpeed = 8f;
    public float jumpForce = 5f;
    public float gravity = -9.81f;
    
    [Header("Camera Settings")]
    public float mouseSensitivity = 2f;
    public Transform cameraTransform;
    public float maxLookAngle = 80f;
    
    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;
    
    // Private variables
    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;
    private float xRotation = 0f;
    
    void Start()
    {
        controller = GetComponent<CharacterController>();
        
        // Lock and hide cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        // If camera is not assigned, find Main Camera
        if (cameraTransform == null)
        {
            cameraTransform = Camera.main.transform;
        }
    }
    
    void Update()
    {
        // Ground check
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
        
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }
        
        // Movement
        HandleMovement();
        
        // Camera movement
        HandleCamera();
        
        // Jump
        HandleJump();
        
        // Gravity
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
        
        // ESC to unlock cursor
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
    
    void HandleMovement()
    {
        // Get input
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        
        // Sprint check
        float currentSpeed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed;
        
        // Movement direction
        Vector3 move = transform.right * x + transform.forward * z;
        controller.Move(move * currentSpeed * Time.deltaTime);
    }
    
    void HandleCamera()
    {
        // Get mouse input
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
        
        // Vertical look (camera)
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -maxLookAngle, maxLookAngle);
        cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        
        // Horizontal rotation (character)
        transform.Rotate(Vector3.up * mouseX);
    }
    
    void HandleJump()
    {
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
        }
    }
}