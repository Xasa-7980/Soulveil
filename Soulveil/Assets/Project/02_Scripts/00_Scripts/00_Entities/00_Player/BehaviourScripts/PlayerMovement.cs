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
    [SerializeField] private float airJumpHeight = 1.25f;
    [SerializeField, Range(0f, 1f)] private float airControl = 0.6f;
    [SerializeField] private float jumpBufferTime = 0.15f;
    [SerializeField] private float coyoteTime = 0.12f;
    [SerializeField, Min(0)] private int maxAirJumps = 1;

    [Header("Gravity")]
    [SerializeField] private float gravity = -20f;
    [SerializeField] private float groundedForce = -2f;

    [Header("References")]
    [SerializeField] private Transform cameraTransform;

    private PlayerCombat playerCombat;
    private CharacterController controller;
    private PlayerDodge playerDodge;
    private PlayerAnimationController playerAnimationController;
    private PlayerActionController playerActions;

    private Vector2 moveInput;
    private float verticalVelocity;

    private float coyoteTimer;
    private float jumpBufferTimer;

    private int airJumpsRemaining;

    private bool isSprinting;
    private bool isCrouching;

    private float maxFallSpeed;
    private bool wasGrounded;
    private bool gravitySuspended;
    public Vector2 MoveInput => moveInput;
    public bool IsGrounded { get; private set; }
    public bool JustLanded { get; private set; }

    public float VerticalVelocity => verticalVelocity;
    public bool IsSprinting => isSprinting;
    public bool IsCrouching => isCrouching;
    public Vector3 MoveDirection { get; private set; }

    public float MaxFallSpeed => maxFallSpeed;
    public int AirJumpsRemaining => airJumpsRemaining;
    public bool IsGravitySuspended => gravitySuspended;

    private void Awake ( )
    {
        controller = GetComponent<CharacterController>();
        playerDodge = GetComponent<PlayerDodge>();
        playerAnimationController = GetComponent<PlayerAnimationController>();
        playerCombat = GetComponent<PlayerCombat>();
        playerActions = GetComponent<PlayerActionController>();

        airJumpsRemaining = maxAirJumps;
    }

    public void OnMove ( InputAction.CallbackContext context )
    {
        // Guardamos el input incluso bloqueado.
        // Así, al recuperar control, responde inmediatamente
        // si el jugador sigue manteniendo el stick/tecla.
        moveInput = context.ReadValue<Vector2>();
    }

    public void OnSprint ( InputAction.CallbackContext context )
    {
        if (playerActions != null && !playerActions.CanMove)
        {
            isSprinting = false;
            return;
        }

        isCrouching = false;

        if (context.performed) isSprinting = true;
        if (context.canceled) isSprinting = false;
    }

    public void OnJump ( InputAction.CallbackContext context )
    {
        if (!context.performed) return;
        if (playerActions != null && !playerActions.CanMove) return;

        isCrouching = false;
        jumpBufferTimer = jumpBufferTime;
    }

    public void OnCrouch ( InputAction.CallbackContext context )
    {
        if (playerActions != null && !playerActions.CanMove) return;

        isCrouching = true;
    }

    private void Update ( )
    {
        HandleGravity();
        HandleMovement();
    }

    private void HandleMovement ( )
    {
        if (playerDodge != null && playerDodge.IsDodging) return;

        bool canMove = playerActions == null || playerActions.CanMove;
        Vector3 direction = Vector3.zero;

        if (canMove)
        {
            Vector3 forward = cameraTransform.forward;
            Vector3 right = cameraTransform.right;

            forward.y = 0f;
            right.y = 0f;

            forward.Normalize();
            right.Normalize();

            direction = forward * moveInput.y + right * moveInput.x;
            direction = Vector3.ClampMagnitude(direction, 1f);
        }

        MoveDirection = direction;

        float currentSpeed = moveSpeed;

        if (isSprinting && IsGrounded) currentSpeed = sprintSpeed;
        if (playerCombat != null && playerCombat.IsAttacking) currentSpeed = moveSpeed * playerCombat.MovementMultiplier;
        if (!IsGrounded) currentSpeed *= airControl;

        // X/Z.
        Vector3 velocity = direction * currentSpeed;

        // Y sigue funcionando aunque Movement esté bloqueado.
        velocity.y = verticalVelocity;

        controller.Move(velocity * Time.deltaTime);

        if (!canMove) return;
        if (direction.sqrMagnitude <= 0.001f) return;

        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    private void HandleGravity ( )
    {
        if (gravitySuspended)
        {
            verticalVelocity = 0f;
            JustLanded = false;
            return;
        }

        wasGrounded = IsGrounded;
        IsGrounded = controller.isGrounded;

        JustLanded = !wasGrounded && IsGrounded;

        if (!IsGrounded && verticalVelocity < maxFallSpeed)
        {
            maxFallSpeed = verticalVelocity;
        }

        if (IsGrounded)
        {
            coyoteTimer = coyoteTime;
            airJumpsRemaining = maxAirJumps;

            if (verticalVelocity < 0f) verticalVelocity = groundedForce;
        }
        else
        {
            coyoteTimer -= Time.deltaTime;
        }

        if (jumpBufferTimer > 0f) jumpBufferTimer -= Time.deltaTime;

        bool canMove = playerActions == null || playerActions.CanMove;

        if (canMove && jumpBufferTimer > 0f)
        {
            if (coyoteTimer > 0f)
            {
                PerformJump(jumpHeight);
            }
            else if (!IsGrounded && airJumpsRemaining > 0)
            {
                PerformAirJump();
            }
        }

        verticalVelocity += gravity * Time.deltaTime;
    }

    private void PerformJump ( float height )
    {
        verticalVelocity = Mathf.Sqrt(height * -2f * gravity);

        jumpBufferTimer = 0f;
        coyoteTimer = 0f;
        maxFallSpeed = 0f;

        if (playerAnimationController != null) playerAnimationController.PlayJump();
    }

    private void PerformAirJump ( )
    {
        airJumpsRemaining--;

        PerformJump(airJumpHeight);
        if (playerAnimationController != null) playerAnimationController.PlayJump(); //Por ahora la misma animacion
        Debug.Log($"[PlayerMovement] Air Jump | Restantes: {airJumpsRemaining}");
    }

    public void ApplyDownwardVelocity ( float multiplier, float minimumSpeed )
    {
        multiplier = Mathf.Max(0f, multiplier);
        minimumSpeed = Mathf.Max(0f, minimumSpeed);

        float currentDownwardSpeed = Mathf.Abs(verticalVelocity) * multiplier;
        verticalVelocity = -Mathf.Max(currentDownwardSpeed, minimumSpeed);
    }
    public void ApplyUpwardVelocity ( float force )
    {
        verticalVelocity = Mathf.Max(0f, force);
    }
    public void SetGravitySuspended ( bool suspended )
    {
        gravitySuspended = suspended;

        if (gravitySuspended) verticalVelocity = 0f;
    }
}