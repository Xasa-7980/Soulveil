using UnityEngine;

[System.Serializable]
public class InventorySlot
{
    [SerializeField] private ItemInstance itemInstance;
    [SerializeField] private int quantity;

    public ItemInstance ItemInstance => itemInstance;
    public int Quantity => quantity;
    public bool IsEmpty => itemInstance == null || itemInstance.ItemData == null;

    public InventorySlot ( )
    {
        Clear();
    }

    public InventorySlot ( ItemInstance itemInstance, int quantity = 1 )
    {
        SetItem(itemInstance, quantity);
    }

    public void SetItem ( ItemInstance newItemInstance, int newQuantity = 1 )
    {
        itemInstance = newItemInstance;
        quantity = itemInstance != null ? Mathf.Max(1, newQuantity) : 0;
    }

    public int AddQuantity ( int amount )
    {
        if (IsEmpty) return amount;

        int availableSpace = itemInstance.ItemData.MaxStack - quantity;
        int amountToAdd = Mathf.Min(amount, availableSpace);

        quantity += amountToAdd;

        return amount - amountToAdd;
    }

    public int RemoveQuantity ( int amount )
    {
        if (IsEmpty) return 0;

        int amountToRemove = Mathf.Min(amount, quantity);
        quantity -= amountToRemove;

        if (quantity <= 0) Clear();

        return amountToRemove;
    }

    public void Clear ( )
    {
        itemInstance = null;
        quantity = 0;
    }
}