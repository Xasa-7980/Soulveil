using UnityEngine;

public class EnemyIdle : MonoBehaviour
{
    [Header("Patrol")]
    [SerializeField] private float minPatrolTime = 5f;
    [SerializeField] private float maxPatrolTime = 10f;
    [SerializeField, Range(0f, 1f)] private float patrolChance = 0.4f;

    [Header("Idle Animations")]
    [SerializeField] private float minAnimationTime = 3f;
    [SerializeField] private float maxAnimationTime = 7f;
    [SerializeField, Range(0f, 1f)] private float animationChance = 0.5f;

    private EnemyController controller;

    private float patrolTimer;
    private float animationTimer;

    private void Awake ( )
    {
        controller = GetComponent<EnemyController>();
    }

    private void OnEnable ( )
    {
        ResetPatrolTimer();
        ResetAnimationTimer();
    }

    private void Update ( )
    {
        if (controller.TryDetectTarget()) return;

        UpdatePatrol();
        UpdateIdleAnimation();
    }
    private void UpdatePatrol ( )
    {
        patrolTimer -= Time.deltaTime;

        if (patrolTimer > 0f) return;

        if (Random.value <= patrolChance)
        {
            controller.ChangeState(controller.PatrolState);
            return;
        }

        ResetPatrolTimer();
    }

    private void UpdateIdleAnimation ( )
    {
        animationTimer -= Time.deltaTime;

        if (animationTimer > 0f) return;

        if (Random.value <= animationChance) PlayRandomIdleAnimation();

        ResetAnimationTimer();
    }

    private void PlayRandomIdleAnimation ( )
    {
        Debug.Log($"{gameObject.name} reproduce una animación Idle aleatoria.");
    }

    private void ResetPatrolTimer ( )
    {
        patrolTimer = Random.Range(minPatrolTime, maxPatrolTime);
    }

    private void ResetAnimationTimer ( )
    {
        animationTimer = Random.Range(minAnimationTime, maxAnimationTime);
    }
}