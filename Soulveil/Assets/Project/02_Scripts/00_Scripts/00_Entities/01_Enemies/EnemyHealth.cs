using UnityEngine;

public class EnemyHealth : Health
{
    private EnemyStats enemyStats;
    private EnemyController controller;

    protected override void Awake ( )
    {
        enemyStats = GetComponent<EnemyStats>();
        if (enemyStats != null) maxHealth = enemyStats.MaxHealth;
        base.Awake();
        controller = GetComponent<EnemyController>();
    }

    protected override void OnDamaged ( DamageInfo damageInfo )
    {
        Debug.Log($"{gameObject.name} golpeado en {damageInfo.hitZone} por {damageInfo.attacker.name}");
        if (controller != null) controller.FocusAttacker(damageInfo.attacker);
    }

    protected override void Die ( )
    {
        if (isDead) return;

        base.Die();

        if (controller != null) controller.ChangeState(controller.DeadState);
        Destroy(gameObject, 2f);
    }
}