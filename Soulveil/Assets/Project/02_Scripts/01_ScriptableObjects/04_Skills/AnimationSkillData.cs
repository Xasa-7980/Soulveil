using UnityEngine;

[CreateAssetMenu(
    fileName = "NewAnimationSkill",
    menuName = "Soulveil/Skills/Animation Skill"
)]
public class AnimationSkillData : SkillData
{
    [Header("Animation")]
    [SerializeField] private int animationIndex = 1;

    [Header("Damage")]
    [SerializeField] private float damageMultiplier = 1f;

    [Header("Action Blocking")]
    [SerializeField] private PlayerActionBlock blockedActions = PlayerActionBlock.All;

    public int AnimationIndex => animationIndex;
    public float DamageMultiplier => damageMultiplier;
    public PlayerActionBlock BlockedActions => blockedActions;

    public override void Execute (
        GameObject user,
        Vector3 castPosition,
        Quaternion castRotation
    )
    {
        // La ejecución runtime de esta habilidad la realiza
        // PlayerSkillController mediante Animator + Animation Events.
    }

    protected override void OnValidate ( )
    {
        base.OnValidate();

        animationIndex = Mathf.Max(1, animationIndex);
        damageMultiplier = Mathf.Max(0f, damageMultiplier);
    }
}