using UnityEngine;

/// <summary>
/// Marks a location where an enemy spawner will be placed.
/// Holds no runtime behaviour - it only describes the anchor and draws gizmos so
/// the point is visible and selectable in the Scene view.
///
/// The radii mirror the enemy system so the anchor can be validated by eye:
///   EnemyPatrol  roams patrolRadius around the position the enemy spawns at
///   EnemyData    detects the player within detectionDistance (default 10 m)
///   EnemyReturn  walks back to the enemy's Awake position
/// </summary>
public class SpawnPointMarker : MonoBehaviour
{
    [Header("Area")]
    [SerializeField] private string areaName = "Unassigned";

    [Header("Radii")]
    [Tooltip("Patrol radius. EnemyPatrol patrols within 6 m of the spawn position.")]
    [SerializeField] private float patrolRadius = 6f;

    [Tooltip("Detection distance, from EnemyData (default 10 m). Draws the alert ring.")]
    [SerializeField] private float alertRadius = 10f;

    [Header("Gizmo")]
    [SerializeField] private Color gizmoColor = new Color(1f, 0.35f, 0.1f, 0.85f);

    public string AreaName => areaName;
    public float PatrolRadius => patrolRadius;
    public float AlertRadius => alertRadius;

    private void OnDrawGizmos()
    {
        Gizmos.color = gizmoColor;

        // patrol area the enemy will wander inside
        Gizmos.DrawWireSphere(transform.position, patrolRadius);

        // detection ring (EnemyData.detectionDistance)
        Gizmos.DrawWireSphere(transform.position, alertRadius);

        // stand-in for the enemy, plus the facing its vision cone will use
        Gizmos.DrawLine(transform.position, transform.position + Vector3.up * 2.5f);
        Gizmos.DrawRay(transform.position + Vector3.up * 1.5f, transform.forward * 3f);
    }
}
