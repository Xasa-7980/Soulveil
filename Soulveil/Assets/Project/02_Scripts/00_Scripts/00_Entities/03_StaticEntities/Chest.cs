using UnityEngine;

public class Chest : MonoBehaviour, IInteractable
{
    [Header("Interaction")]
    [SerializeField] private Transform interactionPoint;

    private bool opened;

    public string InteractionText => opened ? "" : "Abrir cofre";
    public Transform InteractionPoint => interactionPoint != null ? interactionPoint : transform;

    public bool CanInteract ( GameObject interactor )
    {
        return !opened;
    }

    public void Interact ( GameObject interactor )
    {
        if (opened) return;

        opened = true;

        Debug.Log("Cofre abierto.");

        // Generar loot.
    }

    private void OnDrawGizmosSelected ( )
    {
        Transform point = interactionPoint != null ? interactionPoint : transform;
        Gizmos.DrawWireCube(point.position, new Vector3(0.8f, 0.3f, 0.2f));
    }
}