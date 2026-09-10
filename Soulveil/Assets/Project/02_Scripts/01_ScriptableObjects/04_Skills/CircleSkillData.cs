using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(
    fileName = "NewCircleSkill",
    menuName = "Soulveil/Skills/Circle Skill"
)]
public class CircleSkillData : SkillData, ISkillTargeting, ISkillPreview, ISkillDebug
{
    [Header("Circle")]
    [SerializeField] private float radius = 3f;

    [Header("Targeting")]
    [SerializeField] private bool movable = true;
    [SerializeField] private float maxCastDistance = 8f;
    [SerializeField] private bool requiresHold = true;

    [Header("Preview")]
    [SerializeField] private GameObject previewVFX;

    [Header("Damage")]
    [SerializeField] private LayerMask targetLayer;

    public float Radius => radius;
    public bool Movable => movable;
    public float MaxCastDistance => maxCastDistance;

    public bool RequiresHold => requiresHold;
    public GameObject PreviewVFX => previewVFX;

    public void BeginTargeting ( SkillTargetContext context )
    {
        if (context == null || context.User == null) return;

        if (!movable)
        {
            context.CastPosition = context.User.transform.position;
            context.CastPosition = SkillTargetingUtility.SnapToGround(context, context.CastPosition);
            context.CastRotation = Quaternion.identity;
            return;
        }

        Vector3 forward = context.Camera != null
            ? SkillTargetingUtility.GetCameraForwardDirection(context)
            : context.User.transform.forward;

        context.CastPosition = context.User.transform.position + forward * (maxCastDistance * 0.5f);
        context.CastPosition = SkillTargetingUtility.SnapToGround(context, context.CastPosition);
        context.CastRotation = Quaternion.identity;
    }

    public void UpdateTargeting ( SkillTargetContext context )
    {
        if (context == null || context.User == null) return;

        if (!movable)
        {
            context.CastPosition = context.User.transform.position;
            context.CastPosition = SkillTargetingUtility.SnapToGround(context, context.CastPosition);
            context.CastRotation = Quaternion.identity;
            return;
        }

        if (SkillTargetingUtility.TryGetCameraGroundPoint(context, maxCastDistance, out Vector3 point))
        {
            context.CastPosition = point;
        }

        context.CastRotation = Quaternion.identity;
    }

    public void EndTargeting ( SkillTargetContext context )
    {
    }

    public override void Execute ( GameObject user, Vector3 castPosition, Quaternion castRotation )
    {
        if (user == null) return;

        if (CastVFX != null) Instantiate(CastVFX, castPosition, castRotation);

        Collider[] hits = Physics.OverlapSphere(castPosition, radius, targetLayer);
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
            $"[Circle Skill] {SkillName} | " +
            $"Posición: {castPosition} | " +
            $"Targets: {damagedTargets.Count}"
        );
    }

    public void DrawDebug ( SkillTargetContext context )
    {
#if UNITY_EDITOR
        if (context == null) return;

        Handles.color = Color.green;
        Handles.DrawWireDisc(context.CastPosition, Vector3.up, radius);

        if (movable && context.User != null)
        {
            Handles.color = Color.yellow;
            Handles.DrawWireDisc(context.User.transform.position, Vector3.up, maxCastDistance);
            Handles.DrawLine(context.User.transform.position, context.CastPosition);
        }
#endif
    }
    protected override void OnValidate ( )
    {
        base.OnValidate();

        radius = Mathf.Max(0f, radius);
        maxCastDistance = Mathf.Max(0f, maxCastDistance);
    }
}