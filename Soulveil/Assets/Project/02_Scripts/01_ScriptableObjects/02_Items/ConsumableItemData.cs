using UnityEngine;
public enum ConsumableType
{
    Potion,
    Bomb,
    Key,
    Map,
    ExtractionSeal,
    Buff,
    AntiDisadvantage
}

[CreateAssetMenu(fileName = "NewConsumable", menuName = "Soulveil/Items/Consumable")]
public class ConsumableItemData : ItemData
{
    [Header("Consumable")]
    [SerializeField] private ConsumableType consumableType;

    [Header("Stat Effects")]
    [SerializeField] private StatModifierData[] statModifiers;

    [Header("Duration")]
    [SerializeField] private float effectDuration;

    public ConsumableType ConsumableType => consumableType;
    public StatModifierData[] StatModifiers => statModifiers;
    public float EffectDuration => effectDuration;
}