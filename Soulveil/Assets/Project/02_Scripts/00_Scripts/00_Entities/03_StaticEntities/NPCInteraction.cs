using UnityEngine;

public class NPCInteraction : MonoBehaviour, IInteractable
{
    [Header("Interaction")]
    [SerializeField] private Transform interactionPoint;

    [Header("NPC")]
    [SerializeField] private string interactionText = "Hablar";

    public string InteractionText => interactionText;
    public Transform InteractionPoint => interactionPoint != null ? interactionPoint : transform;

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
        Transform point = interactionPoint != null ? interactionPoint : transform;
        Gizmos.DrawWireCube(point.position, new Vector3(0.8f, 0.3f, 0.2f));
    }
}