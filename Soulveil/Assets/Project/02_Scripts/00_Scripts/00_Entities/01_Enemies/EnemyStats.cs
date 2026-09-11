using UnityEngine;

[RequireComponent(typeof(EntityElement))]
public class EnemyStats : Stats
{
    [Header("Enemy Data")]
    [SerializeField] private EnemyData enemyData;

    private EntityElement entityElement;

    private void Awake ( )
    {
        entityElement = GetComponent<EntityElement>();

        if (enemyData != null && entityElement != null)
        {
            entityElement.SetElement(enemyData.Element);
        }
    }

    public EnemyData Data => enemyData;

    public float MaxHealth => enemyData != null ? CalculateStat(StatType.MaxHealth, enemyData.MaxHealth) : 100f;
    public override float AttackDamage => enemyData != null ? CalculateStat(StatType.AttackDamage, enemyData.AttackDamage) : 10f;
    public override float Defense => enemyData != null ? CalculateStat(StatType.Defense, enemyData.Defense) : 2f;

    public float MoveSpeed => enemyData != null ? enemyData.MoveSpeed : 3.5f;

    public float DetectionDistance => enemyData != null ? enemyData.DetectionDistance : 10f;
    public float DetectionAngle => enemyData != null ? enemyData.DetectionAngle : 90f;
    public float AttackRange => enemyData != null ? enemyData.AttackRange : 2f;
    public float MaxChaseDistance => enemyData != null ? enemyData.MaxChaseDistance : 20f;
}