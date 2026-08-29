using UnityEngine;

public class EnemyChase : MonoBehaviour
{
    private EnemyController controller;

    private void Awake ( )
    {
        controller = GetComponent<EnemyController>();
    }

    private void OnEnable ( )
    {
        if (controller.Agent != null && controller.Agent.isOnNavMesh)
        {
            controller.Agent.isStopped = false;
            controller.Agent.ResetPath();
        }
    }

    private void Update ( )
    {
        if (!controller.HasValidTarget())
        {
            controller.ReturnToStart();
            return;
        }

        if (!controller.IsTargetInsideChaseArea())
        {
            controller.ReturnToStart();
            return;
        }

        if (controller.DistanceToTarget() <= controller.AttackRange)
        {
            controller.ChangeState(controller.AttackState);
            return;
        }

        ChaseTarget();
    }

    private void ChaseTarget ( )
    {
        if (controller.Agent == null) return;
        if (!controller.Agent.isOnNavMesh) return;

        controller.Agent.SetDestination(controller.Target.position);
    }

    private void OnDisable ( )
    {
        if (controller.Agent != null && controller.Agent.isOnNavMesh) controller.Agent.ResetPath();
    }
}