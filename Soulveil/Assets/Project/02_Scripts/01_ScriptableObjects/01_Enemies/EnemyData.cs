using UnityEngine;

[CreateAssetMenu(
    fileName = "NewEnemy",
    menuName = "Soulveil/Enemies/Enemy Data"
)]
public class EnemyData : ScriptableObject
{
    [Header("Identity")]
    [SerializeField] private string enemyName;

    [Header("Stats")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float attackDamage = 10f;
    [SerializeField] private float defense = 2f;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 3.5f;

    [Header("Detection")]
    [SerializeField] private float detectionDistance = 10f;
    [SerializeField, Range(0f, 360f)] private float detectionAngle = 90f;

    [Header("Combat")]
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float maxChaseDistance = 20f;

    [Header("Element")]
    [SerializeField] private Element element;

    public string EnemyName => enemyName;

    public float MaxHealth => maxHealth;
    public float AttackDamage => attackDamage;
    public float Defense => defense;

    public float MoveSpeed => moveSpeed;

    public float DetectionDistance => detectionDistance;
    public float DetectionAngle => detectionAngle;

    public float AttackRange => attackRange;
    public float MaxChaseDistance => maxChaseDistance;

    public Element Element => element;
}