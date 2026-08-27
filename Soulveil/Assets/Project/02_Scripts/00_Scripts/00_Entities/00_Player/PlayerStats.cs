using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("Base Stats")]
    [SerializeField] private float baseMaxHealth = 100f;
    [SerializeField] private float baseAttackDamage = 20f;
    [SerializeField] private float baseDefense = 5f;

    [Header("Combat")]
    [SerializeField] private float baseCriticalChance = 0.05f;
    [SerializeField] private float baseCriticalDamage = 1.5f;

    private readonly List<StatModifier> modifiers = new();

    public float MaxHealth => CalculateStat(StatType.MaxHealth, baseMaxHealth);

    public float AttackDamage => CalculateStat(StatType.AttackDamage, baseAttackDamage);

    public float Defense => CalculateStat(StatType.Defense, baseDefense);

    public float CriticalChance => CalculateStat(StatType.CriticalChance, baseCriticalChance);

    public float CriticalDamage => CalculateStat(StatType.CriticalDamage, baseCriticalDamage);

    public void AddModifier ( StatModifier modifier )
    {
        modifiers.Add(modifier);
    }

    public void RemoveModifier ( StatModifier modifier )
    {
        modifiers.Remove(modifier);
    }

    public void RemoveModifiersFromSource ( Object source )
    {
        modifiers.RemoveAll(modifier => modifier.Source == source);
    }

    private float CalculateStat ( StatType statType, float baseValue )
    {
        float flatBonus = 0f;
        float percentageBonus = 0f;

        foreach (StatModifier modifier in modifiers)
        {
            if (modifier.StatType != statType)
                continue;

            switch (modifier.ModifierType)
            {
                case StatModifierType.Flat:
                    flatBonus += modifier.Value;
                    break;

                case StatModifierType.Percentage:
                    percentageBonus += modifier.Value;
                    break;
            }
        }

        float finalValue = baseValue + flatBonus;

        finalValue *= 1f + percentageBonus;

        return finalValue;
    }
}