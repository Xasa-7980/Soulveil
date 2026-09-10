using UnityEngine;

public class PlayerStats : Stats
{
    [Header("Progression")]
    [SerializeField] private int level = 1;
    [SerializeField] private float currentExperience;
    [SerializeField] private float requiredExperience = 100f;

    [Header("Base Stats")]
    [SerializeField] private float baseMaxHealth = 100f;
    [SerializeField] private float baseAttackDamage = 20f;
    [SerializeField] private float baseDefense = 5f;

    [Header("Combat")]
    [SerializeField] private float baseCriticalChance = 0.05f;
    [SerializeField] private float baseCriticalDamage = 1.5f;

    public int Level => level;
    public float CurrentExperience => currentExperience;
    public float RequiredExperience => requiredExperience;
    public float MaxHealth => CalculateStat(StatType.MaxHealth, baseMaxHealth);
    public float AttackDamage => CalculateStat(StatType.AttackDamage, baseAttackDamage);
    public float Defense => CalculateStat(StatType.Defense, baseDefense);
    public float CriticalChance => CalculateStat(StatType.CriticalChance, baseCriticalChance);
    public float CriticalDamage => CalculateStat(StatType.CriticalDamage, baseCriticalDamage);
}