using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerAirDash : MonoBehaviour
{
    [Header("Air Dash")]
    [SerializeField] private float dashDistance = 5f;
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField, Min(1)] private int maxAirDashes = 1;

    [Header("References")]
    [SerializeField] private Transform cameraTransform;

    private CharacterController controller;
    private PlayerMovement playerMovement;
    private PlayerActionController playerActions;
    private PlayerAnimationController playerAnimationController;

    private int airDashesRemaining;
    private bool isAirDashing;

    public bool IsAirDashing => isAirDashing;
    public int AirDashesRemaining => airDashesRemaining;

    private void Awake ( )
    {
        controller = GetComponent<CharacterController>();
        playerMovement = GetComponent<PlayerMovement>();
        playerActions = GetComponent<PlayerActionController>();
        playerAnimationController = GetComponent<PlayerAnimationController>();

        airDashesRemaining = maxAirDashes;
    }

    private void Update ( )
    {
        if (playerMovement != null && playerMovement.JustLanded)
        {
            airDashesRemaining = maxAirDashes;
        }
    }

    public void OnAirDash ( InputAction.CallbackContext context )
    {
        if (!context.performed) return;

        TryAirDash();
    }

    public void TryAirDash ( )
    {
        if (playerMovement == null) return;
        if (playerMovement.IsGrounded) return;
        if (isAirDashing) return;
        if (airDashesRemaining <= 0) return;
        if (playerActions != null && !playerActions.CanDodge) return;

        StartCoroutine(AirDashRoutine());
    }

    private IEnumerator AirDashRoutine ( )
    {
        isAirDashing = true;
        airDashesRemaining--;

        if (playerActions != null)
        {
            PlayerActionBlock blockedActions = PlayerActionBlock.Movement | PlayerActionBlock.Combat | PlayerActionBlock.Dodge | PlayerActionBlock.Skills | PlayerActionBlock.Interaction;
            playerActions.Block(this, blockedActions);
        }

        playerMovement.SetGravitySuspended(true);

        Vector3 dashDirection = GetDashDirection();
        float dashSpeed = dashDistance / dashDuration;
        float elapsed = 0f;

        while (elapsed < dashDuration)
        {
            controller.Move(dashDirection * dashSpeed * Time.deltaTime);

            elapsed += Time.deltaTime;
            yield return null;
        }

        FinishAirDash();
    }

    private Vector3 GetDashDirection ( )
    {
        Vector2 input = playerMovement.MoveInput;

        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;

        forward.y = 0f;
        right.y = 0f;

        forward.Normalize();
        right.Normalize();

        Vector3 direction = forward * input.y + right * input.x;

        if (direction.sqrMagnitude <= 0.01f) direction = transform.forward;

        direction.y = 0f;

        return direction.normalized;
    }

    private void FinishAirDash ( )
    {
        if (!isAirDashing) return;

        isAirDashing = false;

        if (playerMovement != null)
        {
            playerMovement.SetGravitySuspended(false);
        }

        if (playerActions != null)
        {
            playerActions.Unblock(this);
        }
    }

    private void OnDisable ( )
    {
        StopAllCoroutines();

        if (playerMovement != null)
        {
            playerMovement.SetGravitySuspended(false);
        }

        if (playerActions != null)
        {
            playerActions.Unblock(this);
        }

        isAirDashing = false;
    }
}