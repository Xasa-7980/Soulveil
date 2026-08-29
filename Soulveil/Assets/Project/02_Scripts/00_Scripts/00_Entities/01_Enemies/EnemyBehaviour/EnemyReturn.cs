using UnityEngine;

public class EnemyReturn : MonoBehaviour
{
    [Header("Return")]
    [SerializeField] private float arrivalDistance = 0.5f;
    [SerializeField] private float returnWaitTime = 2f;

    private EnemyController controller;
    private float waitTimer;
    private bool isReturning;

    private void Awake ( )
    {
        controller = GetComponent<EnemyController>();
    }

    private void OnEnable ( )
    {
        waitTimer = returnWaitTime;
        isReturning = false;

        if (controller.Agent != null && controller.Agent.isOnNavMesh)
        {
            controller.Agent.isStopped = true;
            controller.Agent.ResetPath();
        }
    }

    private void Update ( )
    {
        if (controller.HasValidTarget() && controller.IsTargetInsideChaseArea())
        {
            controller.ChangeState(controller.ChaseState);
            return;
        }

        if (!isReturning)
        {
            waitTimer -= Time.deltaTime;

            if (waitTimer > 0f) return;

            StartReturn();
        }

        UpdateReturn();
    }

    private void StartReturn ( )
    {
        isReturning = true;

        if (controller.Agent == null) return;
        if (!controller.Agent.isOnNavMesh) return;

        controller.Agent.isStopped = false;
        controller.Agent.ResetPath();
        controller.Agent.SetDestination(controller.StartPosition);
    }

    private void UpdateReturn ( )
    {
        if (controller.Agent == null) return;
        if (!controller.Agent.isOnNavMesh) return;
        if (controller.Agent.pathPending) return;
        if (controller.Agent.remainingDistance > arrivalDistance) return;

        controller.Agent.ResetPath();
        controller.ClearTarget();
        controller.ChangeState(controller.IdleState);
    }

    private void OnDisable ( )
    {
        waitTimer = 0f;
        isReturning = false;

        if (controller.Agent != null && controller.Agent.isOnNavMesh) controller.Agent.ResetPath();
    }
}