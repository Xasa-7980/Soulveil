using UnityEngine;
using UnityEngine.Events;

public class LootItem : MonoBehaviour, IInteractable
{
    [Header("Item")]
    [SerializeField] private ItemInstance itemInstance;

    [Header("Interaction")]
    [SerializeField] private Vector3 interactionOffset = new Vector3(0f, 0.5f, 0f);
    [SerializeField] private Vector3 interactionGizmoSize = new Vector3(0.8f, 0.3f, 0.2f);

    [Header("Drop")]
    [SerializeField] private float launchForce = 4f;
    [SerializeField] private float horizontalForce = 2f;
    [SerializeField] private float rotationSpeed = 180f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform visual;

    private Rigidbody rb;
    private bool isDropping;

    public UnityEvent onPicked;

    public bool IsDropping => isDropping;
    public Vector3 InteractionPosition => transform.position + interactionOffset;
    public ItemInstance ItemInstance => itemInstance;

    //Texto del objeto en el suelo interactuable
    public string InteractionText => itemInstance != null && itemInstance.ItemData != null ? $"Recoger {itemInstance.ItemData.ItemName}" : "Recoger";

    private void Awake ( )
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Update ( )
    {
        RotateVisual();
    }

    public void Initialize ( ItemInstance newItemInstance )
    {
        itemInstance = newItemInstance;

        Launch();
    }

    private void Launch ( )
    {
        if (rb == null) return;

        Vector2 randomDirection = Random.insideUnitCircle.normalized;

        Vector3 horizontalVelocity = new Vector3(randomDirection.x, 0f, randomDirection.y) * horizontalForce;
        Vector3 verticalVelocity = Vector3.up * launchForce;

        isDropping = true;

        rb.isKinematic = false;
        rb.linearVelocity = horizontalVelocity + verticalVelocity;
    }

    private void RotateVisual ( )
    {
        if (visual == null) return;

        visual.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);
    }

    private void OnCollisionEnter ( Collision collision )
    {
        TryStopDrop(collision);
    }

    private void OnCollisionStay ( Collision collision )
    {
        TryStopDrop(collision);
    }

    private void TryStopDrop ( Collision collision )
    {
        if (!isDropping) return;
        if (!IsGroundCollision(collision)) return;

        StopDrop();
    }

    private bool IsGroundCollision ( Collision collision )
    {
        if ((groundLayer.value & (1 << collision.gameObject.layer)) == 0) return false;

        for (int i = 0; i < collision.contactCount; i++)
        {
            ContactPoint contact = collision.GetContact(i);

            // Solo consideramos suelo una superficie que esté debajo del objeto.
            if (contact.normal.y > 0.5f) return true;
        }

        return false;
    }

    private void StopDrop ( )
    {
        isDropping = false;

        if (rb == null) return;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = true;

        Debug.Log($"{gameObject.name} terminó de caer y ya puede recogerse.");
    }

    public bool CanInteract ( GameObject interactor )
    {
        if (isDropping) return false;
        if (itemInstance == null || itemInstance.ItemData == null) return false;

        return interactor.GetComponent<PlayerInventory>() != null;
    }

    public void Interact ( GameObject interactor )
    {
        PlayerInventory inventory = interactor.GetComponent<PlayerInventory>();

        if (inventory == null) return;
        if (!inventory.AddItem(itemInstance)) return;

        Debug.Log($"Recogido: {itemInstance.ItemData.ItemName}");

        onPicked?.Invoke();

        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected ( )
    {
        Gizmos.DrawWireCube(InteractionPosition, interactionGizmoSize);
    }
}