using System.Collections.Generic;
using UnityEngine;

public abstract class Stats : MonoBehaviour
{

    protected readonly List<StatModifier> modifiers = new();

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
}