using UnityEngine;

public class Chest : MonoBehaviour, IInteractable
{
    private bool opened;

    public string InteractionText => opened ? "" : "Abrir cofre";

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
}