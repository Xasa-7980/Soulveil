using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerSkillController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private WeaponHitBox weaponHitBox;

    [Header("Targeting")]
    [SerializeField] private LayerMask groundLayer;

    [Header("Skill 2 Energy")]
    [SerializeField] private float maxSkill2Energy = 100f;

    [Header("Debug")]
    [SerializeField] private bool showSkillDebug = true;
    [SerializeField] private bool showConfiguredSkills = true;
    [SerializeField] private float skill2DebugOffset = 0.25f;

    private PlayerSpecialist playerSpecialist;
    private PlayerAnimationController animationController;
    private PlayerActionController playerActions;

    private float skill1CooldownTimer;
    private float skill2Energy;

    private SkillData castingSkill;
    private int castingSkillIndex;

    private SkillTargetContext targetContext;
    private GameObject currentPreview;

    // Animation Skill.
    private AnimationSkillData currentAnimationSkill;
    private readonly HashSet<iDamageable> skillHitTargets = new();

    private bool animationSkillHitActive;

    public SkillData Skill1 => playerSpecialist != null && playerSpecialist.CurrentSpecialist != null ? playerSpecialist.CurrentSpecialist.skill1 : null;
    public SkillData Skill2 => playerSpecialist != null && playerSpecialist.CurrentSpecialist != null ? playerSpecialist.CurrentSpecialist.skill2 : null;
    public float Skill1CooldownRemaining => skill1CooldownTimer;
    public float Skill1CooldownPercent => Skill1 != null && Skill1.Cooldown > 0f ? Mathf.Clamp01(skill1CooldownTimer / Skill1.Cooldown) : 0f;
    public float Skill2Energy => skill2Energy;
    public float MaxSkill2Energy => maxSkill2Energy;
    public float Skill2EnergyPercent => maxSkill2Energy > 0f ? Mathf.Clamp01(skill2Energy / maxSkill2Energy) : 0f;
    public bool CanUseSkill1 => Skill1 != null && skill1CooldownTimer <= 0f;

    // Temporalmente Skill 2 puede usarse sin energía hay que quitar la condicion "&& skill2Energy >= maxSkill2Energy"
    public bool CanUseSkill2 => Skill2 != null && skill2Energy >= maxSkill2Energy;

    public bool IsCasting => castingSkill != null;
    public bool IsUsingAnimationSkill => currentAnimationSkill != null;

    private void Awake ( )
    {
        playerSpecialist = GetComponent<PlayerSpecialist>();
        animationController = GetComponent<PlayerAnimationController>();
        playerActions = GetComponent<PlayerActionController>();

        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }
    }

    private void OnDisable ( )
    {
        ClearSkillState();
    }

    private void Update ( )
    {
        UpdateCooldowns();
        UpdateCasting();
    }

    public void OnSkill1 ( InputAction.CallbackContext context )
    {
        if (context.started)
        {
            HandleSkillPressed(Skill1, 1);
        }

        if (context.canceled)
        {
            HandleSkillReleased(1);
        }
    }

    public void OnSkill2 ( InputAction.CallbackContext context )
    {
        if (context.started)
        {
            HandleSkillPressed(Skill2, 2);
        }

        if (context.canceled)
        {
            HandleSkillReleased(2);
        }
    }

    private void HandleSkillPressed ( SkillData skill, int skillIndex )
    {
        if (skill == null) return;
        if (playerActions != null && !playerActions.CanUseSkills) return;
        if (!CanUseSkill(skillIndex)) return;
        if (castingSkill != null) return;
        if (currentAnimationSkill != null) return;

        if (skill is not ISkillTargeting targetingSkill)
        {
            ExecuteSkill(
                skill,
                skillIndex,
                transform.position,
                transform.rotation
            );

            return;
        }

        targetContext = new SkillTargetContext(
            gameObject,
            playerCamera,
            groundLayer
        );

        targetingSkill.BeginTargeting(targetContext);
        targetingSkill.UpdateTargeting(targetContext);

        if (!targetingSkill.RequiresHold)
        {
            Vector3 castPosition = targetContext.CastPosition;
            Quaternion castRotation = targetContext.CastRotation;

            targetingSkill.EndTargeting(targetContext);

            targetContext = null;

            ExecuteSkill(
                skill,
                skillIndex,
                castPosition,
                castRotation
            );

            return;
        }

        castingSkill = skill;
        castingSkillIndex = skillIndex;

        CreatePreview(skill);
    }

    private void HandleSkillReleased ( int skillIndex )
    {
        if (castingSkill == null) return;
        if (castingSkillIndex != skillIndex) return;
        if (targetContext == null) return;

        SkillData skill = castingSkill;

        Vector3 castPosition = targetContext.CastPosition;
        Quaternion castRotation = targetContext.CastRotation;

        if (skill is ISkillTargeting targetingSkill)
        {
            targetingSkill.EndTargeting(targetContext);
        }

        ClearPreview();

        castingSkill = null;
        castingSkillIndex = 0;
        targetContext = null;

        ExecuteSkill(
            skill,
            skillIndex,
            castPosition,
            castRotation
        );
    }

    private void UpdateCasting ( )
    {
        if (castingSkill == null) return;
        if (targetContext == null) return;
        if (castingSkill is not ISkillTargeting targetingSkill) return;

        targetingSkill.UpdateTargeting(targetContext);

        UpdatePreview();
    }

    private void ExecuteSkill (
        SkillData skill,
        int skillIndex,
        Vector3 castPosition,
        Quaternion castRotation
    )
    {
        if (skill == null) return;

        if (skill is AnimationSkillData animationSkill)
        {
            StartAnimationSkill(animationSkill);
        }
        else
        {
            skill.Execute(gameObject, castPosition, castRotation);
        }

        if (skillIndex == 1)
        {
            skill1CooldownTimer = skill.Cooldown;

            Debug.Log(
                $"Skill 1: {skill.SkillName} | " +
                $"Cooldown: {skill.Cooldown}"
            );
        }

        if (skillIndex == 2)
        {
            skill2Energy = 0f;

            Debug.Log(
                $"Skill 2: {skill.SkillName} | " +
                $"Energía consumida."
            );
        }
    }

    private void StartAnimationSkill ( AnimationSkillData skill )
    {
        if (skill == null) return;
        if (animationController == null) return;

        currentAnimationSkill = skill;

        animationSkillHitActive = false;
        skillHitTargets.Clear();

        if (playerActions != null)
        {
            playerActions.Block(this, skill.BlockedActions);
        }

        if (skill.CastVFX != null)
        {
            Instantiate(skill.CastVFX, transform.position, transform.rotation);
        }

        animationController.PlaySkill(skill.AnimationIndex);
    }

    #region ANIMATION SKILL EVENTS

    // Animation Event.
    public void BeginSkillHit ( )
    {
        if (currentAnimationSkill == null) return;

        animationSkillHitActive = true;
        skillHitTargets.Clear();
    }

    // Animation Event.
    public void PerformSkillHit ( )
    {
        if (!animationSkillHitActive) return;
        if (currentAnimationSkill == null) return;
        if (weaponHitBox == null) return;

        Collider[] hits = weaponHitBox.CheckHitbox();

        foreach (Collider hit in hits)
        {
            Hurtbox hurtbox = hit.GetComponent<Hurtbox>();

            if (hurtbox == null) continue;

            iDamageable damageable = hit.GetComponentInParent<iDamageable>();

            if (damageable == null) continue;
            if (skillHitTargets.Contains(damageable)) continue;

            skillHitTargets.Add(damageable);

            float damage =
                currentAnimationSkill.BaseDamage *
                currentAnimationSkill.DamageMultiplier;

            Vector3 hitPoint = hit.ClosestPoint(weaponHitBox.WorldPosition);

            DamageInfo damageInfo = new DamageInfo(
                damage,
                gameObject,
                hitPoint,
                hurtbox.HitZone,
                currentAnimationSkill.Element,
                DamageType.Skill
            );

            damageable.ReceiveDamage(damageInfo);

            if (currentAnimationSkill.HitVFX != null)
            {
                Instantiate(
                    currentAnimationSkill.HitVFX,
                    hitPoint,
                    Quaternion.identity
                );
            }

            GainEnergyFromHit(damageInfo.damage);
        }
    }

    // Animation Event.
    public void EndSkillHit ( )
    {
        animationSkillHitActive = false;
    }

    // Animation Event.
    public void EndAnimationSkill ( )
    {
        animationSkillHitActive = false;
        currentAnimationSkill = null;

        skillHitTargets.Clear();

        if (playerActions != null)
        {
            playerActions.Unblock(this);
        }
    }

    #endregion

    private void CreatePreview ( SkillData skill )
    {
        if (skill is not ISkillPreview previewSkill) return;
        if (previewSkill.PreviewVFX == null) return;
        if (targetContext == null) return;

        currentPreview = Instantiate(
            previewSkill.PreviewVFX,
            targetContext.CastPosition,
            targetContext.CastRotation
        );
    }

    private void UpdatePreview ( )
    {
        if (currentPreview == null) return;
        if (targetContext == null) return;

        currentPreview.transform.SetPositionAndRotation(
            targetContext.CastPosition,
            targetContext.CastRotation
        );
    }

    private void ClearPreview ( )
    {
        if (currentPreview == null) return;

        Destroy(currentPreview);

        currentPreview = null;
    }

    private bool CanUseSkill ( int skillIndex )
    {
        return skillIndex switch
        {
            1 => CanUseSkill1,
            2 => CanUseSkill2,
            _ => false
        };
    }

    private void UpdateCooldowns ( )
    {
        if (skill1CooldownTimer <= 0f) return;

        skill1CooldownTimer -= Time.deltaTime;

        if (skill1CooldownTimer < 0f)
        {
            skill1CooldownTimer = 0f;
        }
    }

    public void AddSkill2Energy ( float amount )
    {
        if (amount <= 0f) return;

        skill2Energy = Mathf.Clamp( skill2Energy + amount, 0f, maxSkill2Energy );
    }

    public void SetSkill2Energy ( float amount )
    {
        skill2Energy = Mathf.Clamp(amount, 0f, maxSkill2Energy);
    }

    public void GainEnergyFromHit ( float damageDealt )
    {
        // Valores temporales ajustables.
        float amount = Mathf.Max(1f, damageDealt * 0.1f);

        AddSkill2Energy(amount);
    }

    public void GainEnergyFromDamageTaken ( float damageTaken )
    {
        // Valores temporales ajustables.
        float amount = Mathf.Max(1f, damageTaken * 0.05f);

        AddSkill2Energy(amount);
    }

    public void CancelCast ( )
    {
        if (
            castingSkill is ISkillTargeting targetingSkill &&
            targetContext != null
        )
        {
            targetingSkill.EndTargeting(targetContext);
        }

        ClearPreview();

        castingSkill = null;
        castingSkillIndex = 0;
        targetContext = null;
    }

    public void ResetSkills ( )
    {
        CancelCast();

        skill1CooldownTimer = 0f;
        skill2Energy = 0f;

        animationSkillHitActive = false;
        currentAnimationSkill = null;

        skillHitTargets.Clear();

        if (playerActions != null)
        {
            playerActions.Unblock(this);
        }
    }

    private void ClearSkillState ( )
    {
        CancelCast();

        animationSkillHitActive = false;
        currentAnimationSkill = null;

        skillHitTargets.Clear();

        if (playerActions != null)
        {
            playerActions.Unblock(this);
        }
    }
    #region  Draw Skill Debug
    private void OnDrawGizmos ( )
    {
        if (!showSkillDebug) return;

        if (
            Application.isPlaying &&
            castingSkill != null &&
            targetContext != null
        )
        {
            DrawRuntimeSkillDebug();
            return;
        }

        if (!showConfiguredSkills) return;

        DrawConfiguredSkillsDebug();
    }

    private void DrawRuntimeSkillDebug ( )
    {
        if (castingSkill is not ISkillDebug debugSkill) return;

        debugSkill.DrawDebug(targetContext);
    }

    private void DrawConfiguredSkillsDebug ( )
    {
        PlayerSpecialist specialist = playerSpecialist;

        if (specialist == null)
        {
            specialist = GetComponent<PlayerSpecialist>();
        }

        if (specialist == null) return;
        if (specialist.CurrentSpecialist == null) return;

        DrawConfiguredSkill(
            specialist.CurrentSpecialist.skill1,
            0f
        );

        DrawConfiguredSkill(
            specialist.CurrentSpecialist.skill2,
            skill2DebugOffset
        );
    }

    private void DrawConfiguredSkill (
        SkillData skill,
        float verticalOffset
    )
    {
        if (skill == null) return;
        if (skill is not ISkillDebug debugSkill) return;

        Camera debugCamera =
            playerCamera != null
                ? playerCamera
                : Camera.main;

        SkillTargetContext debugContext = new SkillTargetContext(
            gameObject,
            debugCamera,
            groundLayer
        );

        debugContext.CastPosition =
            transform.position +
            Vector3.up * verticalOffset;

        debugContext.CastRotation = transform.rotation;

        if (skill is ISkillTargeting targetingSkill)
        {
            targetingSkill.BeginTargeting(debugContext);
        }

        debugContext.CastPosition += Vector3.up * verticalOffset;

        debugSkill.DrawDebug(debugContext);
    }
    #endregion
}