using UnityEngine;

public class PortalInteraction : MonoBehaviour, IInteractable
{
    [Header("Interaction")]
    [SerializeField] private Vector3 interactionOffset = new Vector3(0f, 0.5f, 0f);
    [SerializeField] private Vector3 interactionGizmoSize = new Vector3(0.8f, 0.3f, 0.2f);

    public string InteractionText => "Entrar al portal";

    public Vector3 InteractionPosition => transform.position + interactionOffset;

    public bool CanInteract ( GameObject interactor )
    {
        return true;
    }

    public void Interact ( GameObject interactor )
    {
        Debug.Log("Abrir selección de run.");
    }

    private void OnDrawGizmosSelected ( )
    {
        Gizmos.DrawWireCube(
            InteractionPosition,
            interactionGizmoSize
        );
    }
}