using UnityEngine;
using UnityEngine.AI;

public class EnemyPatrol : MonoBehaviour
{
    [Header("Patrol")]
    [SerializeField] private float patrolRadius = 6f;
    [SerializeField] private float arrivalDistance = 0.5f;
    [SerializeField] private int maxPointAttempts = 10;

    private EnemyController controller;
    private Vector3 patrolPoint;
    private bool hasPatrolPoint;

    private void Awake ( )
    {
        controller = GetComponent<EnemyController>();
    }

    private void OnEnable ( )
    {
        hasPatrolPoint = false;

        if (controller.Agent != null && controller.Agent.isOnNavMesh)
        {
            controller.Agent.isStopped = false;
            controller.Agent.ResetPath();
        }

        FindPatrolPoint();
    }

    private void Update ( )
    {
        if (controller.TryDetectTarget()) return;

        if (!hasPatrolPoint)
        {
            controller.ChangeState(controller.IdleState);
            return;
        }

        UpdateMovement();
    }

    private void UpdateMovement ( )
    {
        if (controller.Agent == null) return;
        if (!controller.Agent.isOnNavMesh) return;
        if (controller.Agent.pathPending) return;

        if (controller.Agent.remainingDistance > arrivalDistance) return;

        controller.Agent.ResetPath();
        controller.ChangeState(controller.IdleState);
    }

    private void FindPatrolPoint ( )
    {
        for (int i = 0; i < maxPointAttempts; i++)
        {
            Vector2 randomCircle = Random.insideUnitCircle * patrolRadius;

            Vector3 randomPosition = controller.StartPosition + new Vector3(randomCircle.x, 0f, randomCircle.y);

            if (!NavMesh.SamplePosition(randomPosition, out NavMeshHit hit, patrolRadius, NavMesh.AllAreas)) continue;

            patrolPoint = hit.position;
            hasPatrolPoint = true;

            if (controller.Agent != null && controller.Agent.isOnNavMesh)
            {
                controller.Agent.SetDestination(patrolPoint);
            }

            return;
        }
    }

    private void OnDisable ( )
    {
        hasPatrolPoint = false;
    }

    private void OnDrawGizmosSelected ( )
    {
        Gizmos.DrawWireSphere(transform.position, patrolRadius);
    }
}