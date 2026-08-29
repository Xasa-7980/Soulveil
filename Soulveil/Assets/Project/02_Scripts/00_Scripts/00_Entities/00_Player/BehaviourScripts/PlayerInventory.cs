using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    [Header("Inventory")]
    [SerializeField] private int maxSlots = 20;
    [SerializeField] private List<InventorySlot> slots = new();

    public IReadOnlyList<InventorySlot> Slots => slots;
    public int MaxSlots => maxSlots;

    public bool AddItem ( ItemInstance itemInstance )
    {
        if (itemInstance == null || itemInstance.ItemData == null) return false;

        ItemData itemData = itemInstance.ItemData;

        if (itemData.Stackable)
        {
            foreach (InventorySlot slot in slots)
            {
                if (!CanStack(slot, itemInstance)) continue;

                int remaining = slot.AddQuantity(1);

                if (remaining == 0)
                {
                    Debug.Log($"Añadido al inventario: {itemData.ItemName} x1");
                    return true;
                }
            }
        }

        if (slots.Count >= maxSlots) return false;

        slots.Add(new InventorySlot(itemInstance));

        Debug.Log($"Añadido al inventario: {itemData.ItemName} x1");

        return true;
    }

    private bool CanStack ( InventorySlot slot, ItemInstance itemInstance )
    {
        if (slot.IsEmpty) return false;
        if (slot.ItemInstance.ItemData != itemInstance.ItemData) return false;
        if (slot.Quantity >= itemInstance.ItemData.MaxStack) return false;

        return true;
    }
}