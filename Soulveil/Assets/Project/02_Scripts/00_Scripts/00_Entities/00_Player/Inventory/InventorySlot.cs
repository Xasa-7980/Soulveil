using UnityEngine;

[System.Serializable]
public class InventorySlot
{
    [SerializeField] private ItemInstance itemInstance;
    [SerializeField] private int quantity;

    public ItemInstance ItemInstance => itemInstance;
    public int Quantity => quantity;
    public bool IsEmpty => itemInstance == null || itemInstance.ItemData == null;

    public InventorySlot ( ItemInstance itemInstance, int quantity = 1 )
    {
        this.itemInstance = itemInstance;
        this.quantity = quantity;
    }

    public int AddQuantity ( int amount )
    {
        if (IsEmpty) return amount;

        int availableSpace = itemInstance.ItemData.MaxStack - quantity;
        int amountToAdd = Mathf.Min(amount, availableSpace);

        quantity += amountToAdd;

        return amount - amountToAdd;
    }
}