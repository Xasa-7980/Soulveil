using UnityEngine;

public class EnemyStats : Stats
{
    [Header("Enemy Data")]
    [SerializeField] private EnemyData enemyData;

    public EnemyData Data => enemyData;

    public float MaxHealth => enemyData != null ? CalculateStat(StatType.MaxHealth, enemyData.MaxHealth) : 100f;
    public float AttackDamage => enemyData != null ? CalculateStat(StatType.AttackDamage, enemyData.AttackDamage) : 10f;
    public float Defense => enemyData != null ? CalculateStat(StatType.Defense, enemyData.Defense) : 2f;

    public float MoveSpeed => enemyData != null ? enemyData.MoveSpeed : 3.5f;

    public float DetectionDistance => enemyData != null ? enemyData.DetectionDistance : 10f;
    public float DetectionAngle => enemyData != null ? enemyData.DetectionAngle : 90f;
    public float AttackRange => enemyData != null ? enemyData.AttackRange : 2f;
    public float MaxChaseDistance => enemyData != null ? enemyData.MaxChaseDistance : 20f;

    public Element Element => enemyData != null ? enemyData.Element : null;
}