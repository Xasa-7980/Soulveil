using UnityEngine;
using UnityEngine.SceneManagement;

public class ExtractionPortal : MonoBehaviour, IInteractable
{
    [Header("Extraction")]
    [SerializeField] private string lobbySceneName = "Lobby";

    [Header("Interaction")]
    [SerializeField] private Vector3 interactionOffset = new Vector3(0f, 0.5f, 0f);
    [SerializeField] private Vector3 interactionGizmoSize = new Vector3(0.8f, 0.3f, 0.2f);

    public string InteractionText => "Extraer";

    public Vector3 InteractionPosition => transform.position + interactionOffset;

    public bool CanInteract ( GameObject interactor )
    {
        return interactor != null;
    }

    public void Interact ( GameObject interactor )
    {
        if (interactor == null)
            return;

        if (string.IsNullOrEmpty(lobbySceneName))
            return;

        Debug.Log($"{interactor.name} ha extraído correctamente.");

        SceneManager.LoadScene(lobbySceneName);
    }

    private void OnDrawGizmosSelected ( )
    {
        Gizmos.DrawWireCube(
            InteractionPosition,
            interactionGizmoSize
        );
    }
}