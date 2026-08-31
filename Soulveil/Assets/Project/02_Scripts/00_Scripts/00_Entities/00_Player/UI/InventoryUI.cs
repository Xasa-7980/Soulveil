using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform slotsContainer;
    [SerializeField] private ItemSlotUI itemSlotPrefab;

    [Header("Drag")]
    [SerializeField] private Image dragIcon;

    private PlayerInventory playerInventory;
    private readonly List<ItemSlotUI> slotUIs = new();

    private ItemSlotUI selectedSlot;
    private ItemSlotUI draggedSlot;

    public ItemSlotUI SelectedSlot => selectedSlot;
    public ItemInstance SelectedItem => selectedSlot?.ItemInstance;

    private void Awake ( )
    {
        playerInventory = GetComponentInParent<PlayerInventory>();

        CreateSlots();

        if (dragIcon != null)
        {
            dragIcon.gameObject.SetActive(false);
            dragIcon.raycastTarget = false;
        }
    }

    private void OnEnable ( )
    {
        if (playerInventory != null) playerInventory.OnInventoryChanged += RefreshInventory;

        RefreshInventory();
    }

    private void OnDisable ( )
    {
        if (playerInventory != null) playerInventory.OnInventoryChanged -= RefreshInventory;

        EndDrag();
    }

    private void CreateSlots ( )
    {
        if (playerInventory == null) return;
        if (slotsContainer == null) return;
        if (itemSlotPrefab == null) return;

        for (int i = 0; i < playerInventory.MaxSlots; i++)
        {
            ItemSlotUI slotUI = Instantiate(itemSlotPrefab, slotsContainer);

            slotUI.Initialize(this, i);
            slotUI.Clear();

            slotUIs.Add(slotUI);
        }
    }

    public void RefreshInventory ( )
    {
        if (playerInventory == null) return;

        for (int i = 0; i < slotUIs.Count; i++)
        {
            InventorySlot inventorySlot = playerInventory.Slots[i];

            if (inventorySlot == null || inventorySlot.IsEmpty)
            {
                slotUIs[i].Clear();
                continue;
            }

            slotUIs[i].SetSlot(inventorySlot);
        }
    }

    public void SelectSlot ( ItemSlotUI slot )
    {
        if (slot == null || slot.IsEmpty) return;

        if (selectedSlot == slot)
        {
            ClearSelection();
            return;
        }

        if (selectedSlot != null) selectedSlot.SetSelected(false);

        selectedSlot = slot;
        selectedSlot.SetSelected(true);
    }

    public void ClearSelection ( )
    {
        if (selectedSlot != null) selectedSlot.SetSelected(false);

        selectedSlot = null;
    }

    public void BeginDrag ( ItemSlotUI slot, PointerEventData eventData )
    {
        if (slot == null || slot.IsEmpty) return;
        if (dragIcon == null) return;

        draggedSlot = slot;

        dragIcon.sprite = slot.ItemInstance.ItemData.Icon;
        dragIcon.gameObject.SetActive(true);
        dragIcon.transform.position = eventData.position;
    }

    public void Drag ( PointerEventData eventData )
    {
        if (draggedSlot == null) return;
        if (dragIcon == null) return;

        dragIcon.transform.position = eventData.position;
    }

    public void DropOnSlot ( ItemSlotUI targetSlot )
    {
        if (draggedSlot == null) return;
        if (targetSlot == null) return;
        if (draggedSlot == targetSlot) return;
        if (playerInventory == null) return;

        int fromIndex = draggedSlot.SlotIndex;
        int toIndex = targetSlot.SlotIndex;
        int quantity = draggedSlot.InventorySlot.Quantity;

        playerInventory.MoveItem(fromIndex, toIndex, quantity);
    }

    public void EndDrag ( )
    {
        draggedSlot = null;

        if (dragIcon == null) return;

        dragIcon.sprite = null;
        dragIcon.gameObject.SetActive(false);
    }
}