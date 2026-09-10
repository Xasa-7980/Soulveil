using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Interaction")]
    [SerializeField] private float interactionRadius = 2f;
    [SerializeField] private LayerMask interactionLayer;
    [SerializeField] private float maxInteractionAngle = 70f;

    private IInteractable currentInteractable;
    private PlayerActionController playerActions;

    public IInteractable CurrentInteractable => currentInteractable;
    public bool HasInteractable => currentInteractable != null;

    public string InteractionText =>
        currentInteractable != null
            ? currentInteractable.InteractionText
            : "";

    private void Awake ( )
    {
        playerActions = GetComponent<PlayerActionController>();
    }

    private void Update ( )
    {
        if (playerActions != null && !playerActions.CanInteract)
        {
            currentInteractable = null;
            return;
        }

        FindInteractable();
    }

    public void OnInteract ( InputAction.CallbackContext context )
    {
        if (!context.performed) return;

        TryInteract();
    }

    public void TryInteract ( )
    {
        if (playerActions != null && !playerActions.CanInteract) return;
        if (currentInteractable == null) return;
        if (!currentInteractable.CanInteract(gameObject)) return;

        currentInteractable.Interact(gameObject);
    }

    private void FindInteractable ( )
    {
        Collider[] hits = Physics.OverlapSphere(
            transform.position,
            interactionRadius,
            interactionLayer
        );

        IInteractable bestInteractable = null;
        float bestScore = Mathf.Infinity;

        foreach (Collider hit in hits)
        {
            IInteractable interactable = hit.GetComponentInParent<IInteractable>();

            if (interactable == null) continue;
            if (!interactable.CanInteract(gameObject)) continue;

            Vector3 direction = hit.transform.position - transform.position;
            direction.y = 0f;

            float distance = direction.magnitude;

            if (distance <= 0.01f) continue;

            direction.Normalize();

            float angle = Vector3.Angle(transform.forward, direction);

            if (angle > maxInteractionAngle) continue;

            float score = distance + angle * 0.02f;

            if (score >= bestScore) continue;

            bestScore = score;
            bestInteractable = interactable;
        }

        currentInteractable = bestInteractable;
    }

    private void OnDrawGizmosSelected ( )
    {
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
    }
}