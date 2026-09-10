using UnityEngine;

public interface IInteractable
{
    string InteractionText { get; }
    Transform InteractionPoint { get; }

    bool CanInteract ( GameObject interactor );
    void Interact ( GameObject interactor );
}