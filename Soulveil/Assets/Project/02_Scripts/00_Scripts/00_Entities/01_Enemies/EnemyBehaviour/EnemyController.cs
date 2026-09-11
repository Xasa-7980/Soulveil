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
    [SerializeField] private float eyeHeight = 1.5f;
    [SerializeField] private LayerMask targetLayer;
    [SerializeField] private LayerMask obstacleLayer;

    [Header("Debug")]
    [SerializeField] private Color visionColor = new Color(1f, 0.8f, 0f, 0.2f);
    [SerializeField] private Color attackRangeColor = new Color(1f, 0f, 0f, 0.8f);
    [SerializeField] private Color chaseRangeColor = new Color(0f, 0.5f, 1f, 0.5f);

    [Header("Runtime Debug")]
    [SerializeField] private string currentStateDebug;
    [SerializeField] private string targetDebug;
    [SerializeField] private float distanceToTargetDebug;
    [SerializeField] private float targetDistanceFromStartDebug;
    [SerializeField] private float distanceFromStartDebug;
    [SerializeField] private bool hasValidTargetDebug;
    [SerializeField] private bool targetInsideChaseAreaDebug;
    [SerializeField] private bool canDetectTargetDebug;
    [SerializeField] private bool agentOnNavMeshDebug;
    [SerializeField] private bool agentStoppedDebug;
    [SerializeField] private float agentSpeedDebug;
    [SerializeField] private float agentRemainingDistanceDebug;

    private NavMeshAgent agent;
    private EnemyStats stats;

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

    public float DetectionDistance => stats != null ? stats.DetectionDistance : 10f;
    public float DetectionAngle => stats != null ? stats.DetectionAngle : 90f;
    public float AttackRange => stats != null ? stats.AttackRange : 2f;
    public float MaxChaseDistance => stats != null ? stats.MaxChaseDistance : 20f;

    private void Awake ( )
    {
        agent = GetComponent<NavMeshAgent>();
        stats = GetComponent<EnemyStats>();

        startPosition = transform.position;
        currentState = idleState;

        if (agent != null && stats != null) agent.speed = stats.MoveSpeed;

        InitializeStates();
    }

    private void Update ( )
    {
#if UNITY_EDITOR
        UpdateRuntimeDebug();

        // Debug temporal para probar Stunned.
        if (Input.GetKeyDown(KeyCode.T)) ChangeState(stunnedState);
#endif
    }

    private void InitializeStates ( )
    {
        if (idleState != null) idleState.enabled = true;
        if (patrolState != null) patrolState.enabled = false;
        if (chaseState != null) chaseState.enabled = false;
        if (attackState != null) attackState.enabled = false;
        if (stunnedState != null) stunnedState.enabled = false;
        if (returnState != null) returnState.enabled = false;
        if (deadState != null) deadState.enabled = false;

        currentState = idleState;
    }

    private void UpdateRuntimeDebug ( )
    {
        currentStateDebug = currentState != null ? currentState.GetType().Name : "None";
        targetDebug = target != null ? target.name : "None";

        hasValidTargetDebug = HasValidTarget();
        targetInsideChaseAreaDebug = IsTargetInsideChaseArea();

        distanceToTargetDebug = DistanceToTarget();
        targetDistanceFromStartDebug = TargetDistanceFromStart();
        distanceFromStartDebug = DistanceFromStart();

        if (target != null)
            canDetectTargetDebug = CanDetectTarget(target);
        else
            canDetectTargetDebug = false;

        if (agent != null)
        {
            agentOnNavMeshDebug = agent.isOnNavMesh;
            agentSpeedDebug = agent.speed;

            if (agent.isOnNavMesh)
            {
                agentStoppedDebug = agent.isStopped;
                agentRemainingDistanceDebug = agent.pathPending ? -1f : agent.remainingDistance;
            }
            else
            {
                agentStoppedDebug = true;
                agentRemainingDistanceDebug = -1f;
            }
        }
        else
        {
            agentOnNavMeshDebug = false;
            agentStoppedDebug = true;
            agentSpeedDebug = 0f;
            agentRemainingDistanceDebug = -1f;
        }
    }

    public void ChangeState ( MonoBehaviour newState )
    {
        if (currentState == deadState) return;
        if (currentState == newState) return;
        if (newState == null) return;

        if (currentState != null) currentState.enabled = false;

        currentState = newState;
        currentState.enabled = true;
    }

    public void SetTarget ( Transform newTarget )
    {
        target = newTarget;
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

        return Vector3.Distance(startPosition, target.position) <= MaxChaseDistance;
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
        Collider[] targets = Physics.OverlapSphere(transform.position, DetectionDistance, targetLayer);

        foreach (Collider targetCollider in targets)
        {
            PlayerStats playerStats = targetCollider.GetComponentInParent<PlayerStats>();

            if (playerStats == null) continue;

            PlayerHealth playerHealth = playerStats.GetComponent<PlayerHealth>();

            if (playerHealth != null && playerHealth.IsDead) continue;

            Transform possibleTarget = playerStats.transform;

            if (Vector3.Distance(startPosition, possibleTarget.position) > MaxChaseDistance) continue;

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

        if (direction.magnitude > DetectionDistance) return false;

        float angle = Vector3.Angle(transform.forward, direction);

        if (angle > DetectionAngle * 0.5f) return false;

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
        Gizmos.DrawWireSphere(transform.position, AttackRange);

        Gizmos.color = chaseRangeColor;
        Gizmos.DrawWireSphere(Application.isPlaying ? startPosition : transform.position, MaxChaseDistance);
    }

    private void DrawVisionGizmo ( Vector3 origin )
    {
        Vector3 leftDirection = Quaternion.Euler(0f, -DetectionAngle * 0.5f, 0f) * transform.forward;
        Vector3 rightDirection = Quaternion.Euler(0f, DetectionAngle * 0.5f, 0f) * transform.forward;

        Gizmos.color = visionColor;
        Gizmos.DrawLine(origin, origin + leftDirection * DetectionDistance);
        Gizmos.DrawLine(origin, origin + rightDirection * DetectionDistance);

#if UNITY_EDITOR
        Color fillColor = visionColor;
        fillColor.a = 0.15f;

        Handles.color = fillColor;
        Handles.DrawSolidArc(origin, Vector3.up, leftDirection, DetectionAngle, DetectionDistance);

        Color outlineColor = visionColor;
        outlineColor.a = 1f;

        Handles.color = outlineColor;
        Handles.DrawWireArc(origin, Vector3.up, leftDirection, DetectionAngle, DetectionDistance);
#endif
    }
}