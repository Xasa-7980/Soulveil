using UnityEngine;

public class PortalInteraction : MonoBehaviour, IInteractable
{
    [Header("Interaction")]
    [SerializeField] private Transform interactionPoint;

    public string InteractionText => "Entrar al portal";
    public Transform InteractionPoint => interactionPoint != null ? interactionPoint : transform;

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
        Transform point = interactionPoint != null ? interactionPoint : transform;
        Gizmos.DrawWireCube(point.position, new Vector3(0.8f, 0.3f, 0.2f));
    }
}