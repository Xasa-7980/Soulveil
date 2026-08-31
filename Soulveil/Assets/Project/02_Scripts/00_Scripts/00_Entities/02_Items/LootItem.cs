using UnityEngine;

public class LootItem : MonoBehaviour, IInteractable
{
    [Header("Item")]
    [SerializeField] private ItemInstance itemInstance;

    [Header("Interaction")]
    [SerializeField] private Transform interactionPoint;
    [SerializeField] private Vector3 interactionGizmoSize = new Vector3(0.8f, 0.3f, 0.2f);

    [Header("Drop")]
    [SerializeField] private float launchForce = 4f;
    [SerializeField] private float horizontalForce = 2f;
    [SerializeField] private float rotationSpeed = 180f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform visual;

    private Rigidbody rb;
    private bool isDropping;

    public Transform InteractionPoint => interactionPoint != null ? interactionPoint : transform;
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

        Vector3 horizontalVelocity = new Vector3(
            randomDirection.x,
            0f,
            randomDirection.y
        ) * horizontalForce;

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
        if (!isDropping) return;
        if ((groundLayer.value & (1 << collision.gameObject.layer)) == 0) return;

        if (rb.linearVelocity.y > 0f) return;

        StopDrop();
    }
    private void StopDrop ( )
    {
        isDropping = false;

        if (rb == null) return;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = true;
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

        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected ( )
    {
        Transform point = interactionPoint != null ? interactionPoint : transform;

        Gizmos.matrix = Matrix4x4.TRS(point.position, point.rotation, Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, interactionGizmoSize);
    }
}