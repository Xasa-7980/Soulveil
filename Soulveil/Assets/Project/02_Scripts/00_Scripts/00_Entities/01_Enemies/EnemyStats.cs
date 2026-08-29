using UnityEngine;

public class EnemyStats : Stats
{
    [Header("Base Stats")]
    [SerializeField] private float baseMaxHealth = 100f;
    [SerializeField] private float baseAttackDamage = 10f;
    [SerializeField] private float baseDefense = 2f;

    public float MaxHealth => CalculateStat(StatType.MaxHealth, baseMaxHealth);
    public float AttackDamage => CalculateStat(StatType.AttackDamage, baseAttackDamage);
    public float Defense => CalculateStat(StatType.Defense, baseDefense);
}   