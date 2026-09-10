using UnityEngine;

public abstract class DirectionalSkillData : SkillData, ISkillTargeting, ISkillPreview, ISkillDebug
{
    [Header("Directional Targeting")]
    [SerializeField] private bool requiresHold = true;
    [SerializeField] private float rotationSpeed = 15f;

    [Header("Preview")]
    [SerializeField] private GameObject previewVFX;

    public bool RequiresHold => requiresHold;
    public GameObject PreviewVFX => previewVFX;

    public void BeginTargeting ( SkillTargetContext context )
    {
        if (context == null || context.User == null) return;

        context.CastPosition = context.User.transform.position;
        context.CastRotation = context.User.transform.rotation;
    }

    public void UpdateTargeting ( SkillTargetContext context )
    {
        if (context == null || context.User == null) return;

        Vector3 direction = GetAimDirection(context);

        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.01f) return;

        direction.Normalize();

        Quaternion targetRotation = Quaternion.LookRotation(direction);

        if (requiresHold)
        {
            context.User.transform.rotation = Quaternion.Slerp(
                context.User.transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
        }
        else
        {
            context.User.transform.rotation = targetRotation;
        }

        context.CastPosition = context.User.transform.position;
        context.CastRotation = context.User.transform.rotation;
    }

    public void EndTargeting ( SkillTargetContext context )
    {
    }

    private Vector3 GetAimDirection ( SkillTargetContext context )
    {
        Vector3 direction = SkillTargetingUtility.GetCameraForwardDirection(context);

        if (direction.sqrMagnitude <= 0.01f)
        {
            return context.User.transform.forward;
        }

        return direction;
    }

    public abstract void DrawDebug ( SkillTargetContext context );
}