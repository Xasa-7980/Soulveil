using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerAnimationController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator animator;
    private PlayerMovement playerMovement;
    private PlayerDodge playerDodge;
    private CharacterController characterController;

    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int GroundedHash = Animator.StringToHash("Grounded");
    private static readonly int VerticalSpeedHash = Animator.StringToHash("VerticalSpeed");
    private static readonly int DodgeHash = Animator.StringToHash("Dodge");
    private static readonly int DodgeStanceHash = Animator.StringToHash("DodgeStance");

    private bool wasDodging;
    private void Awake ( )
    {
        playerMovement = GetComponent<PlayerMovement>();
        playerDodge = GetComponent<PlayerDodge>();
        characterController = GetComponent<CharacterController>();
    }

    private void Update ( )
    {
        UpdateLocomotion();
        UpdateDodge();
    }

    private void UpdateLocomotion ( )
    {
        float speed = 0f;

        if (playerMovement.MoveDirection.sqrMagnitude > 0.01f)
        {
            speed = playerMovement.IsSprinting ? 1f : 0.5f;
        }

        animator.SetFloat(
            SpeedHash,
            speed,
            0.1f,
            Time.deltaTime
        );

        animator.SetBool(
            GroundedHash,
            characterController.isGrounded
        );

        animator.SetFloat(
            VerticalSpeedHash,
            playerMovement.VerticalVelocity
        );
    }
    private void UpdateDodge ( )
    {
        bool isDodging = playerDodge.IsDodging;

        if (isDodging && !wasDodging)
        {
            animator.SetFloat(
                DodgeStanceHash,
                playerDodge.DodgeStance
            );

            animator.SetTrigger(DodgeHash);
        }

        wasDodging = isDodging;
    }
}