using UnityEngine;

public class Chest : MonoBehaviour, IInteractable
{
    [Header("Interaction")]
    [SerializeField] private Vector3 interactionOffset = new Vector3(0f, 0.5f, 0f);
    [SerializeField] private Vector3 interactionGizmoSize = new Vector3(0.8f, 0.3f, 0.2f);

    private bool opened;

    public string InteractionText =>
        opened ? "" : "Abrir cofre";

    public Vector3 InteractionPosition => transform.position + interactionOffset;

    public bool CanInteract ( GameObject interactor )
    {
        return !opened;
    }

    public void Interact ( GameObject interactor )
    {
        if (opened)
            return;

        opened = true;

        Debug.Log("Cofre abierto.");

        // Generar loot.
    }

    private void OnDrawGizmosSelected ( )
    {
        Gizmos.DrawWireCube(
            InteractionPosition,
            interactionGizmoSize
        );
    }
}