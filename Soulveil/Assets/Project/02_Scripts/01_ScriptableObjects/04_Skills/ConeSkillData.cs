using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(
    fileName = "NewConeSkill",
    menuName = "Soulveil/Skills/Cone Skill"
)]
public class ConeSkillData : DirectionalSkillData
{
    [Header("Cone")]
    [SerializeField] private float range = 5f;
    [SerializeField, Range(0f, 180f)] private float angle = 60f;

    [Header("Damage")]
    [SerializeField] private LayerMask targetLayer;

    public float Range => range;
    public float Angle => angle;

    public override void Execute ( GameObject user, Vector3 castPosition, Quaternion castRotation )
    {
        if (user == null) return;

        if (CastVFX != null) Instantiate(CastVFX, castPosition, castRotation);

        Collider[] hits = Physics.OverlapSphere(castPosition, range, targetLayer);
        HashSet<Health> damagedTargets = new();

        Vector3 forward = castRotation * Vector3.forward;

        foreach (Collider hit in hits)
        {
            Health health = hit.GetComponentInParent<Health>();

            if (health == null) continue;
            if (health.gameObject == user) continue;
            if (damagedTargets.Contains(health)) continue;

            Vector3 direction = health.transform.position - castPosition;
            direction.y = 0f;

            if (direction.sqrMagnitude <= 0.01f) continue;

            float targetAngle = Vector3.Angle(forward, direction.normalized);

            if (targetAngle > angle * 0.5f) continue;

            damagedTargets.Add(health);

            Vector3 hitPoint = hit.ClosestPoint(castPosition);

            DamageInfo damageInfo = new DamageInfo
            {
                damage = BaseDamage,
                attacker = user,
                element = Element,
                hitPoint = hitPoint,
                hitZone = HitZone.Body
            };

            health.ReceiveDamage(damageInfo);
        }

        Debug.Log($"[Skill] {SkillName} ejecutada. Targets: {damagedTargets.Count}");
    }

    public override void DrawDebug ( SkillTargetContext context )
    {
#if UNITY_EDITOR
        if (context == null) return;

        Vector3 forward = context.CastRotation * Vector3.forward;
        Vector3 left = Quaternion.AngleAxis(-angle * 0.5f, Vector3.up) * forward;

        Handles.color = Color.red;
        Handles.DrawSolidArc(context.CastPosition, Vector3.up, left, angle, range);

        Handles.color = Color.white;
        Handles.DrawWireArc(context.CastPosition, Vector3.up, left, angle, range);
#endif
    }

    protected override void OnValidate ( )
    {
        base.OnValidate();

        range = Mathf.Max(0f, range);
        angle = Mathf.Clamp(angle, 0f, 180f);
    }
}