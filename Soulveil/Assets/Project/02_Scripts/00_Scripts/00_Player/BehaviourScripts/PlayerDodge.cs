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

    private bool isDodging;
    private bool canDodge = true;

    public bool IsDodging => isDodging;

    private float dodgeStance;
    public float DodgeStance => dodgeStance;

    private void Awake ( )
    {
        controller = GetComponent<CharacterController>();
        playerMovement = GetComponent<PlayerMovement>();
    }
    private void Update ( )
    {
        if (dodgeBufferTimer > 0f)
        {
            dodgeBufferTimer -= Time.deltaTime;

            if (canDodge &&
                !isDodging &&
                playerMovement.IsGrounded)
            {
                dodgeBufferTimer = 0f;
                StartCoroutine(DodgeRoutine());
            }
        }
    }
    public void OnDodge ( InputAction.CallbackContext context )
    {
        if (!context.performed)
            return;


        if (!canDodge || isDodging)
            return;

        if (!playerMovement.IsGrounded)
            return;

        dodgeBufferTimer = dodgeBufferTime;
        StartCoroutine(DodgeRoutine());
    }

    private IEnumerator DodgeRoutine ( )
    {
        isDodging = true;
        canDodge = false;

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
            controller.Move(
                dodgeDirection *
                dodgeSpeed *
                Time.deltaTime
            );

            elapsed += Time.deltaTime;
            yield return null;
        }

        isDodging = false;

        yield return new WaitForSeconds(dodgeCooldown);

        canDodge = true;
    }
}