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

[System.Serializable]
public struct StatModifierData
{
    [SerializeField] private StatType statType;
    [SerializeField] private StatModifierType modifierType;
    [SerializeField] private float value;

    public StatType StatType => statType;
    public StatModifierType ModifierType => modifierType;
    public float Value => value;

    public StatModifier CreateModifier ( Object source )
    {
        return new StatModifier(statType, modifierType, value, source);
    }
}