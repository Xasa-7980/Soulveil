using UnityEngine;
public abstract class EquipmentItemData : ItemData
{
    [Header("Stats")]
    [SerializeField] private StatModifierData[] statModifiers;

    public StatModifierData[] StatModifiers => statModifiers;
}