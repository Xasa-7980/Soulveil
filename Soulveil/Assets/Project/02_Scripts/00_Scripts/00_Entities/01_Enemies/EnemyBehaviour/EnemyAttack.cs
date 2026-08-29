using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    [Header("Attack")]
    [SerializeField] private WeaponHitBox weaponHitBox;
    [SerializeField] private float attackCooldown = 2f;
    [SerializeField] private float damageDelay = 0.2f;
    [SerializeField] private float rotationSpeed = 8f;

    private EnemyController controller;
    private EnemyStats stats;
    private EntityElement entityElement;

    private readonly HashSet<iDamageable> damagedTargets = new();

    private float attackTimer;
    private Coroutine attackCoroutine;

    private void Awake ( )
    {
        controller = GetComponent<EnemyController>();
        stats = GetComponent<EnemyStats>();
        entityElement = GetComponent<EntityElement>();
    }

    private void OnEnable ( )
    {
        attackTimer = 0f;
        damagedTargets.Clear();

        if (controller.Agent != null && controller.Agent.isOnNavMesh)
        {
            controller.Agent.isStopped = true;
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

        if (controller.DistanceToTarget() > controller.AttackRange)
        {
            controller.ChangeState(controller.ChaseState);
            return;
        }

        LookAtTarget();

        attackTimer -= Time.deltaTime;

        if (attackTimer <= 0f && attackCoroutine == null)
        {
            attackCoroutine = StartCoroutine(Attack());
            attackTimer = attackCooldown;
        }
    }

    private IEnumerator Attack ( )
    {
        damagedTargets.Clear();

        Debug.Log($"{gameObject.name} realiza un ataque.");

        // Aquí después llamaremos a la animación de ataque.

        yield return new WaitForSeconds(damageDelay);

        if (controller.HasValidTarget() && controller.DistanceToTarget() <= controller.AttackRange) CheckHitbox();

        attackCoroutine = null;
    }

    private void CheckHitbox ( )
    {
        if (weaponHitBox == null)
        {
            Debug.LogWarning($"{gameObject.name} no tiene un WeaponHitBox asignado.");
            return;
        }

        Collider[] hits = weaponHitBox.CheckHitbox();

        foreach (Collider hit in hits)
        {
            Hurtbox hurtbox = hit.GetComponent<Hurtbox>();

            if (hurtbox == null) continue;

            PlayerHealth playerHealth = hit.GetComponentInParent<PlayerHealth>();

            if (playerHealth == null) continue;
            if (playerHealth.IsDead) continue;

            iDamageable damageable = playerHealth;

            if (damagedTargets.Contains(damageable)) continue;

            damagedTargets.Add(damageable);

            Vector3 hitPoint = hit.ClosestPoint(weaponHitBox.WorldPosition);
            float damage = stats != null ? stats.AttackDamage : 10f;
            Element element = entityElement != null ? entityElement.CurrentElement : null;

            DamageInfo damageInfo = new DamageInfo(damage, gameObject, hitPoint, hurtbox.HitZone, element);

            damageable.ReceiveDamage(damageInfo);
        }
    }

    private void LookAtTarget ( )
    {
        if (controller.Target == null) return;

        Vector3 direction = controller.Target.position - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.01f) return;

        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    private void OnDisable ( )
    {
        if (attackCoroutine != null)
        {
            StopCoroutine(attackCoroutine);
            attackCoroutine = null;
        }

        attackTimer = 0f;
        damagedTargets.Clear();
    }
}