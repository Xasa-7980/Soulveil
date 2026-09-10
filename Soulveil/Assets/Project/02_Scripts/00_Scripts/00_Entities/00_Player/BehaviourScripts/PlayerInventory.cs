using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    [Header("Inventory")]
    [SerializeField] private int maxSlots = 20;
    [SerializeField] private List<InventorySlot> slots = new();

    public IReadOnlyList<InventorySlot> Slots => slots;
    public int MaxSlots => maxSlots;

    public event Action OnInventoryChanged;

    private void Awake ( )
    {
        InitializeSlots();
    }

    private void InitializeSlots ( )
    {
        if (slots.Count > maxSlots) slots.RemoveRange(maxSlots, slots.Count - maxSlots);

        while (slots.Count < maxSlots)
        {
            slots.Add(new InventorySlot());
        }
    }

    public bool AddItem ( ItemInstance itemInstance )
    {
        if (itemInstance == null || itemInstance.ItemData == null) return false;

        ItemData itemData = itemInstance.ItemData;

        if (itemData.Stackable)
        {
            for (int i = 0; i < slots.Count; i++)
            {
                if (!CanStack(slots[i], itemInstance)) continue;

                int remaining = slots[i].AddQuantity(1);

                if (remaining == 0)
                {
                    OnInventoryChanged?.Invoke();
                    return true;
                }
            }
        }

        int emptyIndex = FindFirstEmptySlot();

        if (emptyIndex == -1) return false;

        slots[emptyIndex].SetItem(itemInstance, 1);

        OnInventoryChanged?.Invoke();

        return true;
    }

    public bool MoveItem ( int fromIndex, int toIndex, int quantity )
    {
        if (!IsValidIndex(fromIndex) || !IsValidIndex(toIndex)) return false;
        if (fromIndex == toIndex) return false;

        InventorySlot fromSlot = slots[fromIndex];
        InventorySlot toSlot = slots[toIndex];

        if (fromSlot.IsEmpty) return false;

        quantity = Mathf.Clamp(quantity, 1, fromSlot.Quantity);

        if (toSlot.IsEmpty)
        {
            MoveToEmptySlot(fromSlot, toSlot, quantity);

            OnInventoryChanged?.Invoke();
            return true;
        }

        if (CanStack(toSlot, fromSlot.ItemInstance))
        {
            StackItems(fromSlot, toSlot, quantity);

            OnInventoryChanged?.Invoke();
            return true;
        }

        if (quantity == fromSlot.Quantity)
        {
            SwapSlots(fromSlot, toSlot);

            OnInventoryChanged?.Invoke();
            return true;
        }

        return false;
    }

    private void MoveToEmptySlot ( InventorySlot fromSlot, InventorySlot toSlot, int quantity )
    {
        ItemInstance itemInstance = fromSlot.ItemInstance;

        if (quantity >= fromSlot.Quantity)
        {
            toSlot.SetItem(itemInstance, fromSlot.Quantity);
            fromSlot.Clear();
            return;
        }

        fromSlot.RemoveQuantity(quantity);
        toSlot.SetItem(itemInstance, quantity);
    }

    private void StackItems ( InventorySlot fromSlot, InventorySlot toSlot, int quantity )
    {
        int amountToMove = Mathf.Min(quantity, fromSlot.Quantity);
        int remaining = toSlot.AddQuantity(amountToMove);
        int movedAmount = amountToMove - remaining;

        fromSlot.RemoveQuantity(movedAmount);
    }
    private void GetFirstEmptySlot ( ItemInstance itemInstance, int quantity )
    {
        int emptyIndex = FindFirstEmptySlot();

        if (emptyIndex == -1) return;

        slots[emptyIndex].SetItem(itemInstance, quantity);
    }

    private void SwapSlots ( InventorySlot firstSlot, InventorySlot secondSlot )
    {
        ItemInstance firstItem = firstSlot.ItemInstance;
        int firstQuantity = firstSlot.Quantity;

        ItemInstance secondItem = secondSlot.ItemInstance;
        int secondQuantity = secondSlot.Quantity;

        firstSlot.SetItem(secondItem, secondQuantity);
        secondSlot.SetItem(firstItem, firstQuantity);
    }

    private int FindFirstEmptySlot ( )
    {
        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i].IsEmpty) return i;
        }

        return -1;
    }

    private bool CanStack ( InventorySlot slot, ItemInstance itemInstance )
    {
        if (slot.IsEmpty) return false;
        if (slot.ItemInstance.ItemData != itemInstance.ItemData) return false;
        if (!itemInstance.ItemData.Stackable) return false;
        if (slot.Quantity >= itemInstance.ItemData.MaxStack) return false;

        return true;
    }

    private bool IsValidIndex ( int index )
    {
        return index >= 0 && index < slots.Count;
    }
}