using System.Collections.Generic;
using UnityEngine;

public abstract class Stats : MonoBehaviour
{
    protected readonly List<StatModifier> modifiers = new();

    public abstract float AttackDamage { get; }
    public abstract float Defense { get; }

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

    protected float CalculateStat ( StatType statType, float baseValue )
    {
        float flatBonus = 0f;
        float percentageBonus = 0f;

        foreach (StatModifier modifier in modifiers)
        {
            if (modifier.StatType != statType) continue;

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

    public float CalculateReceivedDamage ( float attack )
    {
        attack = Mathf.Max(0f, attack);
        float defense = Mathf.Max(0f, Defense);

        if (attack <= 0f) return 0f;

        const float fortificationScale = 100f;

        float defenseRatio = defense / attack;
        float effectiveDefenseRatio = defenseRatio * (1f + defenseRatio / fortificationScale);

        return attack / (1f + effectiveDefenseRatio);
    }
}