using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemSlotUI : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    [Header("References")]
    [SerializeField] private Image itemIcon;
    [SerializeField] private Image gradeFrame;
    [SerializeField] private TMP_Text quantityText;
    [SerializeField] private GameObject selectedFrame;

    private InventorySlot inventorySlot;
    private InventoryUI inventoryUI;
    private int slotIndex;

    public InventorySlot InventorySlot => inventorySlot;
    public ItemInstance ItemInstance => inventorySlot?.ItemInstance;
    public bool IsEmpty => inventorySlot == null || inventorySlot.IsEmpty;
    public int SlotIndex => slotIndex;

    public void Initialize ( InventoryUI newInventoryUI, int newSlotIndex )
    {
        inventoryUI = newInventoryUI;
        slotIndex = newSlotIndex;

        SetSelected(false);
    }

    public void SetSlot ( InventorySlot newInventorySlot )
    {
        inventorySlot = newInventorySlot;

        if (inventorySlot == null || inventorySlot.IsEmpty)
        {
            Clear();
            return;
        }

        ItemData itemData = inventorySlot.ItemInstance.ItemData;

        itemIcon.sprite = itemData.Icon;
        itemIcon.enabled = true;

        gradeFrame.color = GetRarityColor(itemData.Rarity);

        bool showQuantity = inventorySlot.Quantity > 1;
        quantityText.text = inventorySlot.Quantity.ToString();
        quantityText.gameObject.SetActive(showQuantity);
    }

    public void OnPointerClick ( PointerEventData eventData )
    {
        if (IsEmpty) return;
        if (eventData.button != PointerEventData.InputButton.Left) return;

        if (inventoryUI == null)
        {
            Debug.LogError($"ItemSlotUI {slotIndex}: InventoryUI es NULL.", this);
            return;
        }

        inventoryUI.SelectSlot(this);
    }

    public void OnBeginDrag ( PointerEventData eventData )
    {
        if (IsEmpty) return;

        if (inventoryUI == null)
        {
            Debug.LogError($"ItemSlotUI {slotIndex}: InventoryUI es NULL.", this);
            return;
        }

        inventoryUI.BeginDrag(this, eventData);
    }

    public void OnDrag ( PointerEventData eventData )
    {
        if (inventoryUI == null) return;

        inventoryUI.Drag(eventData);
    }

    public void OnEndDrag ( PointerEventData eventData )
    {
        if (inventoryUI == null) return;

        inventoryUI.EndDrag();
    }

    public void OnDrop ( PointerEventData eventData )
    {
        if (inventoryUI == null)
        {
            Debug.LogError($"ItemSlotUI {slotIndex}: InventoryUI es NULL.", this);
            return;
        }

        inventoryUI.DropOnSlot(this);
    }

    public void SetSelected ( bool selected )
    {
        if (selectedFrame != null) selectedFrame.SetActive(selected);
    }

    public void Clear ( )
    {
        inventorySlot = null;

        if (itemIcon != null)
        {
            itemIcon.sprite = null;
            itemIcon.enabled = false;
        }

        if (gradeFrame != null) gradeFrame.color = Color.white;

        if (quantityText != null)
        {
            quantityText.text = "";
            quantityText.gameObject.SetActive(false);
        }

        SetSelected(false);
    }

    private Color GetRarityColor ( ItemRarity rarity )
    {
        return rarity switch
        {
            ItemRarity.Common => Color.white, // Blanco
            ItemRarity.Uncommon => new Color32(66, 184, 131, 255),  // Verde
            ItemRarity.Rare => new Color32(68, 125, 255, 255),  // Azul
            ItemRarity.Epic => new Color32(165, 86, 255, 255),  // Morado
            ItemRarity.Legendary => new Color32(255, 170, 40, 255),  // Naranja
            ItemRarity.Unique => new Color32(255, 55, 95, 255),   // Carmesí
            _ => Color.white
        };
    }
}