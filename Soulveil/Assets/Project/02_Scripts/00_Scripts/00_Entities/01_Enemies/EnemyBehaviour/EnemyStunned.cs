using UnityEngine;

public class EnemyStunned : MonoBehaviour
{
    [Header("Stun")]
    [SerializeField] private float stunDuration = 1.5f;

    private EnemyController controller;

    private float stunTimer;
    private float nextStunDuration = -1f;

    private void Awake ( )
    {
        controller = GetComponent<EnemyController>();
    }

    private void OnEnable ( )
    {
        stunTimer = nextStunDuration >= 0f ? nextStunDuration : stunDuration;
        nextStunDuration = -1f;

        if (controller.Agent != null && controller.Agent.isOnNavMesh)
        {
            controller.Agent.isStopped = true;
            controller.Agent.ResetPath();
        }
    }

    private void Update ( )
    {
        stunTimer -= Time.deltaTime;

        if (stunTimer > 0f) return;

        EndStun();
    }

    private void EndStun ( )
    {
        if (controller.HasValidTarget() && controller.IsTargetInsideChaseArea())
        {
            controller.ChangeState(controller.ChaseState);
            return;
        }

        if (controller.TryDetectTarget()) return;

        controller.ReturnToStart();
    }

    private void OnDisable ( )
    {
        stunTimer = 0f;
    }

    public void SetStunDuration ( float duration )
    {
        nextStunDuration = Mathf.Max(0f, duration);
    }
}