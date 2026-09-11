using UnityEngine;

public class PortalInteraction : MonoBehaviour, IInteractable
{
    public string InteractionText => "Entrar al portal";

    public bool CanInteract ( GameObject interactor )
    {
        return true;
    }

    public void Interact ( GameObject interactor )
    {
        Debug.Log("Abrir selección de run.");
    }
}