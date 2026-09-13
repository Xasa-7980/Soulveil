using UnityEngine;

public class NPCInteraction : MonoBehaviour, IInteractable
{
    [Header("Interaction")]
    [SerializeField] private Vector3 interactionOffset = new Vector3(0f, 1.5f, 0f);
    [SerializeField] private Vector3 interactionGizmoSize = new Vector3(0.8f, 0.3f, 0.2f);

    [Header("NPC")]
    [SerializeField] private string interactionText = "Hablar";

    public string InteractionText => interactionText;

    public Vector3 InteractionPosition => transform.position + interactionOffset;

    public bool CanInteract ( GameObject interactor )
    {
        return true;
    }

    public void Interact ( GameObject interactor )
    {
        Debug.Log("Iniciar conversación con NPC.");
    }

    private void OnDrawGizmosSelected ( )
    {
        Gizmos.DrawWireCube(
            InteractionPosition,
            interactionGizmoSize
        );
    }
}