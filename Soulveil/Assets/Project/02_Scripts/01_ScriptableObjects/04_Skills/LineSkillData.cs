using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(
    fileName = "NewLineSkill",
    menuName = "Soulveil/Skills/Line Skill"
)]
public class LineSkillData : DirectionalSkillData
{
    [Header("Line")]
    [SerializeField] private float range = 8f;
    [SerializeField] private float width = 1f;
    [SerializeField] private float height = 2f;

    [Header("Damage")]
    [SerializeField] private LayerMask targetLayer;

    public float Range => range;
    public float Width => width;
    public float Height => height;

    public override void Execute ( GameObject user, Vector3 castPosition, Quaternion castRotation )
    {
        if (user == null) return;

        if (CastVFX != null) Instantiate(CastVFX, castPosition, castRotation);

        Vector3 forward = castRotation * Vector3.forward;

        Vector3 boxCenter =
            castPosition +
            forward * (range * 0.5f) +
            Vector3.up * (height * 0.5f);

        Vector3 halfExtents = new Vector3(
            width * 0.5f,
            height * 0.5f,
            range * 0.5f
        );

        Collider[] hits = Physics.OverlapBox(
            boxCenter,
            halfExtents,
            castRotation,
            targetLayer
        );

        HashSet<Health> damagedTargets = new();

        foreach (Collider hit in hits)
        {
            Health health = hit.GetComponentInParent<Health>();

            if (health == null) continue;
            if (health.gameObject == user) continue;
            if (!damagedTargets.Add(health)) continue;

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

        Debug.Log(
            $"[Line Skill] {SkillName} | " +
            $"Targets: {damagedTargets.Count}"
        );
    }

    public override void DrawDebug ( SkillTargetContext context )
    {
        if (context == null) return;

        Vector3 forward = context.CastRotation * Vector3.forward;

        Vector3 boxCenter =
            context.CastPosition +
            forward * (range * 0.5f) +
            Vector3.up * (height * 0.5f);

        Vector3 size = new Vector3(
            width,
            height,
            range
        );

        Matrix4x4 previousMatrix = Gizmos.matrix;

        Gizmos.color = Color.cyan;
        Gizmos.matrix = Matrix4x4.TRS(
            boxCenter,
            context.CastRotation,
            Vector3.one
        );

        Gizmos.DrawWireCube(Vector3.zero, size);

        Gizmos.matrix = previousMatrix;
    }

    protected override void OnValidate ( )
    {
        base.OnValidate();

        range = Mathf.Max(0f, range);
        width = Mathf.Max(0f, width);
        height = Mathf.Max(0f, height);
    }
}