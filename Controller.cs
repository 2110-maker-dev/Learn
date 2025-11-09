using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class FirstPersonController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float sprintSpeed = 8f;
    [SerializeField] private float jumpHeight = 1.5f;
    [SerializeField] private float gravity = -15f;
    
    [Header("Mouse Look Settings")]
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private float lookXLimit = 80f;
    
    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundDistance = 0.3f;
    [SerializeField] private LayerMask groundMask;
    
    // Private variables
    private CharacterController characterController;
    private Camera playerCamera;
    private Vector3 moveDirection = Vector3.zero;
    private Vector3 velocity;
    private float rotationX = 0f;
    private bool isGrounded;
    
    void Start()
    {
        // Get required components
        characterController = GetComponent<CharacterController>();
        playerCamera = GetComponentInChildren<Camera>();
        
        // Safety checks
        if (playerCamera == null)
        {
            Debug.LogError("No Camera found as child of Player! Please add a Camera as a child.");
        }
        
        if (groundCheck == null)
        {
            Debug.LogWarning("Ground Check not assigned! Ground detection may not work properly.");
        }
        
        // Position player at maze start
        MazeGenerator maze = FindObjectOfType<MazeGenerator>();
        if (maze != null)
        {
            transform.position = maze.GetStartPosition();
            Debug.Log("Player positioned at maze start");
        }
        else
        {
            Debug.LogWarning("MazeGenerator not found in scene!");
        }
        
        // Lock cursor for gameplay
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    
    void Update()
    {
        // Check if grounded
        CheckGroundStatus();
        
        // Handle all player inputs
        HandleMouseLook();
        HandleMovement();
        HandleJump();
        
        // Toggle cursor lock with Escape
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleCursorLock();
        }
    }
    
    private void CheckGroundStatus()
    {
        if (groundCheck != null)
        {
            isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
        }
        else
        {
            // Fallback if no ground check assigned
            isGrounded = characterController.isGrounded;
        }
        
        // Reset falling velocity when grounded
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; // Small negative value to keep grounded
        }
    }
    
    private void HandleMouseLook()
    {
        if (playerCamera == null) return;
        
        // Only look around when cursor is locked
        if (Cursor.lockState == CursorLockMode.Locked)
        {
            // Get mouse input
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
            
            // Rotate camera up/down (pitch)
            rotationX -= mouseY;
            rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
            playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0f, 0f);
            
            // Rotate player body left/right (yaw)
            transform.Rotate(Vector3.up * mouseX);
        }
    }
    
    private void HandleMovement()
    {
        // Get input from WASD or arrow keys
        float horizontal = Input.GetAxis("Horizontal"); // A/D or Left/Right
        float vertical = Input.GetAxis("Vertical");     // W/S or Up/Down
        
        // Calculate movement direction relative to player rotation
        Vector3 forward = transform.forward;
        Vector3 right = transform.right;
        
        // Remove vertical component to prevent flying
        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();
        
        moveDirection = (forward * vertical + right * horizontal).normalized;
        
        // Determine speed (sprint or walk)
        float currentSpeed = Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : walkSpeed;
        
        // Apply movement
        Vector3 move = moveDirection * currentSpeed;
        characterController.Move(move * Time.deltaTime);
        
        // Apply gravity
        velocity.y += gravity * Time.deltaTime;
        characterController.Move(velocity * Time.deltaTime);
    }
    
    private void HandleJump()
    {
        // Jump when Space is pressed and player is grounded
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            // Calculate jump velocity using physics formula: v = sqrt(2 * h * g)
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
    }
    
    private void ToggleCursorLock()
    {
        if (Cursor.lockState == CursorLockMode.Locked)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
    
    // Detect when player reaches the exit
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == "Exit_Gate")
        {
            Debug.Log("🎉 YOU WIN! You reached the exit!");
            OnReachExit();
        }
    }
    
    private void OnReachExit()
    {
        // Unlock cursor so player can click UI
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        // Optional: Add your win screen logic here
        // Example: FindObjectOfType<GameManager>().ShowWinScreen();
    }
    
    // Optional: Draw ground check sphere in editor for debugging
    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundDistance);
        }
    }
}