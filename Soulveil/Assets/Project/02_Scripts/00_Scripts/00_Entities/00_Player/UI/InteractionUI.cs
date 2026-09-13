using TMPro;
using UnityEngine;

public class InteractionUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject interactionPanel;
    [SerializeField] private TMP_Text interactionText;

    private PlayerInteraction playerInteraction;

    private void Awake ( )
    {
        playerInteraction = GetComponentInParent<PlayerInteraction>();
    }

    private void Update ( )
    {
        RefreshInteraction();
    }

    private void RefreshInteraction ( )
    {
        if (playerInteraction == null) return;

        bool showInteraction = playerInteraction.HasInteractable;

        if (interactionPanel != null) interactionPanel.SetActive(showInteraction);

        if (!showInteraction) return;

        if (interactionText != null)
        {
            interactionText.text = playerInteraction.InteractionText;
            interactionText.gameObject.transform.position = playerInteraction.GetInteractionUIPosition();
        }
    }
}