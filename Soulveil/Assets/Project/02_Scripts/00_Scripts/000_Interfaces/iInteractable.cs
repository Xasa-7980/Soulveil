using UnityEngine;

public interface IInteractable
{
    string InteractionText { get; }
    Vector3 InteractionPosition { get; }
    bool CanInteract ( GameObject interactor );
    void Interact ( GameObject interactor );
}