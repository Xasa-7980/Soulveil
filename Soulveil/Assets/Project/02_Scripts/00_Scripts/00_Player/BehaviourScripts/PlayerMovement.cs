using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float sprintSpeed = 8f;
    [SerializeField] private float rotationSpeed = 12f;

    [Header("Jump")]
    [SerializeField] private float jumpHeight = 1.5f;
    [SerializeField, Range(0f, 1f)] private float airControl = 0.6f;
    [SerializeField] private float jumpBufferTime = 0.15f;
    [SerializeField] private float coyoteTime = 0.12f;

    [Header("Gravity")]
    [SerializeField] private float gravity = -20f;
    [SerializeField] private float groundedForce = -2f;

    [Header("References")]
    [SerializeField] private Transform cameraTransform;

    private CharacterController controller; 
    private PlayerDodge playerDodge;
    private Vector2 moveInput;
    private float verticalVelocity;
    private bool isSprinting;
    private bool jumpRequested;
    private float coyoteTimer;
    private float jumpBufferTimer;
    public Vector3 MoveDirection { get; private set; }

    private void Awake ( )
    {
        controller = GetComponent<CharacterController>();
        playerDodge = GetComponent<PlayerDodge>();
    }

    public void OnMove ( InputAction.CallbackContext context )
    {
        moveInput = context.ReadValue<Vector2>();
    }
    public void OnSprint ( InputAction.CallbackContext context )
    {
        if (context.performed)
            isSprinting = true;

        if (context.canceled)
            isSprinting = false;
    }
    public void OnJump ( InputAction.CallbackContext context )
    {
        if (context.performed)
        {
            jumpBufferTimer = jumpBufferTime;
        }
    }
    private void Update ( )
    {
        HandleGravity();
        HandleMovement();
    }

    private void HandleMovement ( )
    {
        if (playerDodge != null && playerDodge.IsDodging) return;

        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;

        forward.y = 0f;
        right.y = 0f;

        forward.Normalize();
        right.Normalize();

        Vector3 direction =
            forward * moveInput.y +
            right * moveInput.x;

        direction = Vector3.ClampMagnitude(direction, 1f);

        MoveDirection = direction;

        float currentSpeed = moveSpeed;

        if (isSprinting && controller.isGrounded) currentSpeed = sprintSpeed; 
        if (!controller.isGrounded) currentSpeed *= airControl; 

        controller.Move(
            direction *
            currentSpeed *
            Time.deltaTime
        );

        if (direction.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
        }
    }

    private void HandleGravity ( )
    {
        bool grounded = controller.isGrounded;

        // Ground / Coyote Time
        if (grounded)
        {
            coyoteTimer = coyoteTime;

            if (verticalVelocity < 0f)
                verticalVelocity = groundedForce;
        }
        else
        {
            coyoteTimer -= Time.deltaTime;
        }

        // Jump Buffer
        if (jumpBufferTimer > 0f)
            jumpBufferTimer -= Time.deltaTime;

        // SALTO
        if (jumpBufferTimer > 0f && coyoteTimer > 0f)
        {
            verticalVelocity =
                Mathf.Sqrt(jumpHeight * -2f * gravity);

            jumpBufferTimer = 0f;
            coyoteTimer = 0f;
        }

        // Gravedad
        verticalVelocity += gravity * Time.deltaTime;

        controller.Move(
            Vector3.up *
            verticalVelocity *
            Time.deltaTime
        );
    }
}