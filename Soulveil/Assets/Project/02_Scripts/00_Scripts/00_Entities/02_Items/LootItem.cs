using UnityEngine;
using UnityEngine.Events;

public class LootItem : MonoBehaviour, IInteractable
{
    [SerializeField] private ItemInstance itemInstance;

    [Header("Drop")]
    [SerializeField] private float launchForce = 4f;
    [SerializeField] private float horizontalForce = 2f;
    [SerializeField] private float rotationSpeed = 180f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform visual;

    private Rigidbody rb;
    private bool isDropping;

    public ItemInstance ItemInstance => itemInstance;
    public bool IsDropping => isDropping;
    public string InteractionText => itemInstance != null && itemInstance.ItemData != null ? $"Recoger {itemInstance.ItemData.ItemName}" : "Recoger";
    public UnityEvent onPicked;
    private void Awake ( )
    {
        rb = GetComponent<Rigidbody>();
        onPicked.AddListener(( ) => Destroy(gameObject));
    }

    private void Update ( )
    {
        if (!isDropping) return;
        if (visual == null) return;

        visual.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);
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

        Vector3 force = new Vector3(
            randomDirection.x * horizontalForce,
            launchForce,
            randomDirection.y * horizontalForce
        );

        isDropping = true;

        rb.isKinematic = false;
        rb.AddForce(force, ForceMode.Impulse);
    }

    private void OnCollisionEnter ( Collision collision )
    {
        if (!isDropping) return;
        if ((groundLayer.value & (1 << collision.gameObject.layer)) == 0) return;

        isDropping = false;

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

        onPicked?.Invoke();
    }
}