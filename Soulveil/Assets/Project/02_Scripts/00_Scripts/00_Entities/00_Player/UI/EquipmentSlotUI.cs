using UnityEngine;
using UnityEngine.UI;

public class EquipmentSlotUI : MonoBehaviour
{
    public enum EquipmentType
    {
        Weapon,
        Armor,
        Artifact
    }

    [Header("Slot")]
    [SerializeField] private EquipmentType equipmentType;
    [SerializeField] private Image itemIcon;

    private ItemInstance itemInstance;

    public EquipmentType Type => equipmentType;
    public ItemInstance ItemInstance => itemInstance;
    public bool IsEmpty => itemInstance == null || itemInstance.ItemData == null;

    public void SetItem ( ItemInstance newItemInstance )
    {
        if (newItemInstance == null || newItemInstance.ItemData == null)
        {
            Clear();
            return;
        }

        if (!CanEquip(newItemInstance)) return;

        itemInstance = newItemInstance;
        itemIcon.sprite = itemInstance.ItemData.Icon;
        itemIcon.enabled = true;
    }

    public void Clear ( )
    {
        itemInstance = null;
        itemIcon.sprite = null;
        itemIcon.enabled = false;
    }

    public bool CanEquip ( ItemInstance itemInstance )
    {
        if (itemInstance == null || itemInstance.ItemData == null) return false;

        return equipmentType switch
        {
            EquipmentType.Weapon => itemInstance.ItemData is WeaponItemData,
            EquipmentType.Armor => itemInstance.ItemData is ArmorItemData,
            EquipmentType.Artifact => itemInstance.ItemData is ArtifactItemData,
            _ => false
        };
    }
}