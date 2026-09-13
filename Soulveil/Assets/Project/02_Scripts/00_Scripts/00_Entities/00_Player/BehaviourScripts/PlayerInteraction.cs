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
    private PlayerInput playerInput;
    private InputAction interactAction;

    public IInteractable CurrentInteractable => currentInteractable;
    public bool HasInteractable => currentInteractable != null;

    public string InteractionText
    {
        get
        {
            if (currentInteractable == null) return "";

            string binding = GetInteractionBinding();

            if (string.IsNullOrEmpty(binding))
            {
                return currentInteractable.InteractionText;
            }

            return $"[{binding}] {currentInteractable.InteractionText}";
        }
    }

    private void Awake ( )
    {
        playerActions = GetComponent<PlayerActionController>();
        playerInput = GetComponent<PlayerInput>();

        if (playerInput != null)
        {
            interactAction = playerInput.actions["Interaction"];
        }
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

    public Vector3 GetInteractionUIPosition ( )
    {
        if (currentInteractable == null) return Vector3.zero;

        return currentInteractable.InteractionPosition;
    }

    private string GetInteractionBinding ( )
    {
        if (interactAction == null) return "";
        if (playerInput == null) return "";

        string controlScheme = playerInput.currentControlScheme;

        if (string.IsNullOrEmpty(controlScheme)) return "";

        for (int i = 0; i < interactAction.bindings.Count; i++)
        {
            InputBinding binding = interactAction.bindings[i];

            if (string.IsNullOrEmpty(binding.groups)) continue;
            if (!binding.groups.Contains(controlScheme)) continue;

            return interactAction.GetBindingDisplayString(i);
        }

        return "";
    }

    private void FindInteractable ( )
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, interactionRadius, interactionLayer);

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