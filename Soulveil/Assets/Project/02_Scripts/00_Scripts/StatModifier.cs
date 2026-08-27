using UnityEngine;

public enum StatModifierType
{
    Flat,
    Percentage
}
public enum StatType
{
    MaxHealth,
    AttackDamage,
    Defense,
    CriticalChance,
    CriticalDamage
}
public class StatModifier
{
    public StatType StatType { get; }
    public StatModifierType ModifierType { get; }
    public float Value { get; }
    public Object Source { get; }

    public StatModifier ( StatType statType, StatModifierType modifierType, float value, Object source )
    {
        StatType = statType;
        ModifierType = modifierType;
        Value = value;
        Source = source;
    }
}