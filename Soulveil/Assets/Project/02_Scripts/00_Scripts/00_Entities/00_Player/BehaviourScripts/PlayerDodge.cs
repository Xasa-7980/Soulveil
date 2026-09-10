using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerDodge : MonoBehaviour
{
    [Header("Dodge")]
    [SerializeField] private float dodgeDistance = 4f;
    [SerializeField] private float dodgeDuration = 0.25f;
    [SerializeField] private float dodgeCooldown = 0.35f;
    [SerializeField] private float dodgeBufferTime = 0.15f;

    private float dodgeBufferTimer;

    private CharacterController controller;
    private PlayerMovement playerMovement;
    private PlayerActionController playerActions;

    private bool isDodging;
    private bool canDodge = true;

    private float dodgeStance;

    public bool IsDodging => isDodging;
    public float DodgeStance => dodgeStance;

    private void Awake ( )
    {
        controller = GetComponent<CharacterController>();
        playerMovement = GetComponent<PlayerMovement>();
        playerActions = GetComponent<PlayerActionController>();
    }

    private void Update ( )
    {
        if (playerActions != null && !playerActions.CanDodge)
        {
            dodgeBufferTimer = 0f;
            return;
        }

        if (dodgeBufferTimer <= 0f) return;

        dodgeBufferTimer -= Time.deltaTime;

        if (
            canDodge &&
            !isDodging &&
            playerMovement.IsGrounded
        )
        {
            dodgeBufferTimer = 0f;
            StartCoroutine(DodgeRoutine());
        }
    }

    public void OnDodge ( InputAction.CallbackContext context )
    {
        if (!context.performed) return;

        if (playerActions != null && !playerActions.CanDodge) return;
        if (!canDodge || isDodging) return;
        if (!playerMovement.IsGrounded) return;

        dodgeBufferTimer = dodgeBufferTime;

        StartCoroutine(DodgeRoutine());
    }

    private IEnumerator DodgeRoutine ( )
    {
        if (playerActions != null && !playerActions.CanDodge)
        {
            yield break;
        }

        isDodging = true;
        canDodge = false;
        dodgeBufferTimer = 0f;

        Vector2 input = playerMovement.MoveInput;

        Vector3 dodgeDirection;

        if (input.magnitude > 0.1f)
        {
            dodgeDirection = transform.forward;
            dodgeStance = 0f;
        }
        else
        {
            dodgeDirection = -transform.forward;
            dodgeStance = 1f;
        }

        dodgeDirection.y = 0f;
        dodgeDirection.Normalize();

        float dodgeSpeed = dodgeDistance / dodgeDuration;
        float elapsed = 0f;

        while (elapsed < dodgeDuration)
        {
            controller.Move(dodgeDirection * dodgeSpeed * Time.deltaTime);

            elapsed += Time.deltaTime;

            yield return null;
        }

        isDodging = false;

        yield return new WaitForSeconds(dodgeCooldown);

        canDodge = true;
    }
}