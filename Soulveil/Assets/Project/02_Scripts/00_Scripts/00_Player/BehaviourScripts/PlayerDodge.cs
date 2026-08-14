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

    private CharacterController controller;
    private PlayerMovement playerMovement;

    private bool isDodging;
    private bool canDodge = true;

    public bool IsDodging => isDodging;

    private void Awake ( )
    {
        controller = GetComponent<CharacterController>();
        playerMovement = GetComponent<PlayerMovement>();
    }

    public void OnDodge ( InputAction.CallbackContext context )
    {
        if (!context.performed)
            return;

        if (!canDodge || isDodging)
            return;

        if (!controller.isGrounded)
            return;

        StartCoroutine(DodgeRoutine());
    }

    private IEnumerator DodgeRoutine ( )
    {
        isDodging = true;
        canDodge = false;

        // Si nos estamos moviendo, esquivamos en esa dirección.
        Vector3 dodgeDirection = playerMovement.MoveDirection;
        if (dodgeDirection.sqrMagnitude < 0.01f) dodgeDirection = transform.forward;

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