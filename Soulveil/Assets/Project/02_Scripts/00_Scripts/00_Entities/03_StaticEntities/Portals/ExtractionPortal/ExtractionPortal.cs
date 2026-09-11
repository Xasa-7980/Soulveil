using UnityEngine;
using UnityEngine.SceneManagement;

public class ExtractionPortal : MonoBehaviour, IInteractable
{
    [Header("Extraction")]
    [SerializeField] private string lobbySceneName = "Lobby";

    public string InteractionText => "Extraer";

    public bool CanInteract ( GameObject interactor )
    {
        return interactor != null;
    }

    public void Interact ( GameObject interactor )
    {
        if (interactor == null) return;
        if (string.IsNullOrEmpty(lobbySceneName)) return;

        Debug.Log($"{interactor.name} ha extraído correctamente.");

        SceneManager.LoadScene(lobbySceneName);
    }
}