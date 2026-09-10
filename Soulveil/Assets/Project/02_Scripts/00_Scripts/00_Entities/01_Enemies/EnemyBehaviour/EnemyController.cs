using UnityEngine;
using UnityEngine.AI;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class EnemyController : MonoBehaviour
{
    [Header("States")]
    [SerializeField] private EnemyIdle idleState;
    [SerializeField] private EnemyPatrol patrolState;
    [SerializeField] private EnemyChase chaseState;
    [SerializeField] private EnemyAttack attackState;
    [SerializeField] private EnemyStunned stunnedState;
    [SerializeField] private EnemyReturn returnState;
    [SerializeField] private EnemyDeath deadState;

    [Header("Detection")]
    [SerializeField] private float detectionDistance = 10f;
    [SerializeField, Range(0f, 360f)] private float detectionAngle = 90f;
    [SerializeField] private float eyeHeight = 1.5f;
    [SerializeField] private LayerMask targetLayer;
    [SerializeField] private LayerMask obstacleLayer;

    [Header("Combat")]
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float maxChaseDistance = 20f;

    [Header("Debug")]
    [SerializeField] private Color visionColor = new Color(1f, 0.8f, 0f, 0.2f);
    [SerializeField] private Color attackRangeColor = new Color(1f, 0f, 0f, 0.8f);
    [SerializeField] private Color chaseRangeColor = new Color(0f, 0.5f, 1f, 0.5f);

    private NavMeshAgent agent;
    private Transform target;
    private MonoBehaviour currentState;
    private Vector3 startPosition;

    public NavMeshAgent Agent => agent;
    public Transform Target => target;
    public MonoBehaviour CurrentState => currentState;
    public Vector3 StartPosition => startPosition;

    public EnemyIdle IdleState => idleState;
    public EnemyPatrol PatrolState => patrolState;
    public EnemyChase ChaseState => chaseState;
    public EnemyAttack AttackState => attackState;
    public EnemyStunned StunnedState => stunnedState;
    public EnemyReturn ReturnState => returnState;
    public EnemyDeath DeadState => deadState;

    public float DetectionDistance => detectionDistance;
    public float DetectionAngle => detectionAngle;
    public float AttackRange => attackRange;
    public float MaxChaseDistance => maxChaseDistance;

    private void Awake ( )
    {
        agent = GetComponent<NavMeshAgent>();
        startPosition = transform.position;
        currentState = idleState;
    }

    private void Update ( )
    {
        if (Input.GetKeyDown(KeyCode.T)) ChangeState(stunnedState);
    }

    public void ChangeState ( MonoBehaviour newState )
    {
        if (currentState == deadState) return;
        if (currentState == newState) return;

        if (currentState != null) currentState.enabled = false;

        currentState = newState;

        if (currentState != null) currentState.enabled = true;
    }

    public void SetTarget ( Transform newTarget )
    {
        target = newTarget;
    }

    public void FocusAttacker ( GameObject attacker )
    {
        if (attacker == null) return;
        if (currentState == deadState) return;

        PlayerStats playerStats = attacker.GetComponentInParent<PlayerStats>();

        if (playerStats == null) return;

        target = playerStats.transform;

        if (!IsTargetInsideChaseArea()) return;

        if (currentState == stunnedState) return;

        ChangeState(chaseState);
    }

    public void ClearTarget ( )
    {
        target = null;
    }

    public bool TryDetectTarget ( )
    {
        Transform detectedTarget = FindTarget();

        if (detectedTarget == null) return false;

        target = detectedTarget;
        ChangeState(chaseState);

        return true;
    }

    public bool HasValidTarget ( )
    {
        if (target == null) return false;

        PlayerHealth playerHealth = target.GetComponent<PlayerHealth>();

        if (playerHealth != null && playerHealth.IsDead) return false;

        return true;
    }

    public bool IsTargetInsideChaseArea ( )
    {
        if (!HasValidTarget()) return false;

        return Vector3.Distance(startPosition, target.position) <= maxChaseDistance;
    }

    public float TargetDistanceFromStart ( )
    {
        if (target == null) return Mathf.Infinity;

        return Vector3.Distance(startPosition, target.position);
    }

    public void ReturnToStart ( )
    {
        ChangeState(returnState);
    }

    public Transform FindTarget ( )
    {
        Collider[] targets = Physics.OverlapSphere(transform.position, detectionDistance, targetLayer);

        foreach (Collider targetCollider in targets)
        {
            PlayerStats playerStats = targetCollider.GetComponentInParent<PlayerStats>();

            if (playerStats == null) continue;

            PlayerHealth playerHealth = playerStats.GetComponent<PlayerHealth>();

            if (playerHealth != null && playerHealth.IsDead) continue;

            Transform possibleTarget = playerStats.transform;

            if (Vector3.Distance(startPosition, possibleTarget.position) > maxChaseDistance) continue;

            if (CanDetectTarget(possibleTarget)) return possibleTarget;
        }

        return null;
    }

    public bool CanDetectTarget ( Transform possibleTarget )
    {
        if (possibleTarget == null) return false;

        Vector3 origin = transform.position + Vector3.up * eyeHeight;
        Vector3 targetPosition = possibleTarget.position;
        Vector3 direction = targetPosition - origin;

        if (direction.magnitude > detectionDistance) return false;

        float angle = Vector3.Angle(transform.forward, direction);

        if (angle > detectionAngle * 0.5f) return false;

        if (Physics.Raycast(origin, direction.normalized, direction.magnitude, obstacleLayer)) return false;

        return true;
    }

    public float DistanceToTarget ( )
    {
        if (target == null) return Mathf.Infinity;

        return Vector3.Distance(transform.position, target.position);
    }

    public float DistanceFromStart ( )
    {
        return Vector3.Distance(transform.position, startPosition);
    }

    private void OnDrawGizmosSelected ( )
    {
        Vector3 origin = transform.position + Vector3.up * eyeHeight;

        DrawVisionGizmo(origin);

        Gizmos.color = attackRangeColor;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = chaseRangeColor;
        Gizmos.DrawWireSphere(Application.isPlaying ? startPosition : transform.position, maxChaseDistance);
    }

    private void DrawVisionGizmo ( Vector3 origin )
    {
        Vector3 leftDirection = Quaternion.Euler(0f, -detectionAngle * 0.5f, 0f) * transform.forward;
        Vector3 rightDirection = Quaternion.Euler(0f, detectionAngle * 0.5f, 0f) * transform.forward;

        Gizmos.color = visionColor;
        Gizmos.DrawLine(origin, origin + leftDirection * detectionDistance);
        Gizmos.DrawLine(origin, origin + rightDirection * detectionDistance);

#if UNITY_EDITOR
        Color fillColor = visionColor;
        fillColor.a = 0.15f;

        Handles.color = fillColor;
        Handles.DrawSolidArc(origin, Vector3.up, leftDirection, detectionAngle, detectionDistance);

        Color outlineColor = visionColor;
        outlineColor.a = 1f;

        Handles.color = outlineColor;
        Handles.DrawWireArc(origin, Vector3.up, leftDirection, detectionAngle, detectionDistance);
#endif
    }
}