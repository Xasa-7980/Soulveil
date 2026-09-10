using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombat : MonoBehaviour
{
    private enum AttackType
    {
        None,
        Light,
        Heavy
    }

    [Header("Combo")]
    [SerializeField, Range(0f, 1f)] private float nextAttackWindow = 0.75f;

    [Header("Combat State")]
    [SerializeField] private float inCombatCountdown = 4f;

    [Header("Attack Movement")]
    [SerializeField, Range(0f, 1f)] private float lightAttackMovementMultiplier = 0.45f;
    [SerializeField, Range(0f, 1f)] private float heavyAttackMovementMultiplier = 0.2f;

    [Header("Combat References")]
    [SerializeField] private WeaponHitBox weaponHitBox;
    [SerializeField] private PlayerStats playerStats;

    private EntityElement entityElement;
    private PlayerSkillController skillController;

    private readonly HashSet<iDamageable> hitTargets = new();

    private PlayerSpecialist playerSpecialist;
    private PlayerAnimationController animationController;
    private PlayerActionController playerActions;
    private PlayerMovement playerMovement;

    private int lightAttackIndex = 1;
    private int heavyAttackIndex = 1;

    private float combatTimer;

    private bool attackLocked;
    private bool attackStateEntered;
    private bool inCombat;

    private AttackType currentAttack;
    private AttackType bufferedAttack;

    public bool InCombat => inCombat;
    public bool IsAttacking => attackLocked;

    private void Awake ( )
    {
        playerSpecialist = GetComponent<PlayerSpecialist>();
        animationController = GetComponent<PlayerAnimationController>();
        entityElement = GetComponent<EntityElement>();
        skillController = GetComponent<PlayerSkillController>();
        playerActions = GetComponent<PlayerActionController>();
        playerMovement = GetComponent<PlayerMovement>();
    }

    private void Update ( )
    {
        UpdateAttack();
        UpdateCombatTimer();

        // Borrable / testeable para proto.
        UpdateHitDetection();
    }

    private void UpdateHitDetection ( )
    {
        if (!attackLocked) return;

        PerformHit();
    }

    public void OnLightAttack ( InputAction.CallbackContext context )
    {
        if (!context.performed) return;
        if (playerMovement != null && !playerMovement.IsGrounded) return;
        if (playerActions != null && !playerActions.CanCombat) return;

        if (animationController.GetCombatNormalizedTime() < nextAttackWindow) return;

        if (!attackLocked)
            StartAttack(AttackType.Light);
        else
            bufferedAttack = AttackType.Light;
    }

    public void OnHeavyAttack ( InputAction.CallbackContext context )
    {
        if (!context.performed) return;
        if (playerMovement != null && !playerMovement.IsGrounded) return;
        if (playerActions != null && !playerActions.CanCombat) return;

        if (animationController.GetCombatNormalizedTime() < nextAttackWindow) return;

        if (!attackLocked)
            StartAttack(AttackType.Heavy);
        else
            bufferedAttack = AttackType.Heavy;
    }

    private void StartAttack ( AttackType type )
    {
        if (playerActions != null && !playerActions.CanCombat) return;
        if (GetAttackLength(type) <= 0) return;

        hitTargets.Clear();

        bool startingNewChain = !attackLocked;

        attackLocked = true;

        if (startingNewChain)
        {
            attackStateEntered = false;
        }

        currentAttack = type;
        bufferedAttack = AttackType.None;

        EnterCombat();
        PlayAttack(type);
    }

    private void UpdateAttack ( )
    {
        if (!attackLocked || animationController.IsInAttackTransition()) return;

        bool inAttackState = animationController.IsInAttackState();

        if (!inAttackState && !attackStateEntered) return;

        if (!inAttackState && attackStateEntered)
        {
            FinishAttack();
            return;
        }

        if (!animationController.IsCurrentAttack(currentAttack == AttackType.Light, GetAttackIndex(currentAttack))) return;

        attackStateEntered = true;

        if (bufferedAttack == AttackType.None) return;

        AttackType nextAttack = bufferedAttack;

        AdvanceCombo(nextAttack);
        StartAttack(nextAttack);
    }

    private void PlayAttack ( AttackType type )
    {
        int index = GetAttackIndex(type);

        if (type == AttackType.Light)
        {
            animationController.PlayLightAttack(index);
        }
        else
        {
            animationController.PlayHeavyAttack(index);
        }
    }

    private void AdvanceCombo ( AttackType type )
    {
        if (type == AttackType.Light)
        {
            lightAttackIndex++;

            if (lightAttackIndex > playerSpecialist.lightAttackLength)
            {
                lightAttackIndex = 1;
            }

            heavyAttackIndex = 1;
        }
        else
        {
            heavyAttackIndex++;

            if (heavyAttackIndex > playerSpecialist.heavyAttackLength)
            {
                heavyAttackIndex = 1;
            }

            lightAttackIndex = 1;
        }
    }

    private int GetAttackIndex ( AttackType type )
    {
        return type == AttackType.Light ? lightAttackIndex : heavyAttackIndex;
    }

    private int GetAttackLength ( AttackType type )
    {
        return type == AttackType.Light ? playerSpecialist.lightAttackLength : playerSpecialist.heavyAttackLength;
    }

    private void FinishAttack ( )
    {
        attackLocked = false;
        attackStateEntered = false;

        lightAttackIndex = 1;
        heavyAttackIndex = 1;

        currentAttack = AttackType.None;
        bufferedAttack = AttackType.None;

        combatTimer = inCombatCountdown;
    }

    private void ResetCombo ( )
    {
        lightAttackIndex = 1;
        heavyAttackIndex = 1;

        currentAttack = AttackType.None;
        bufferedAttack = AttackType.None;
    }

    private void EnterCombat ( )
    {
        inCombat = true;
        combatTimer = inCombatCountdown;
    }

    private void UpdateCombatTimer ( )
    {
        if (!inCombat || attackLocked) return;

        combatTimer -= Time.deltaTime;

        if (combatTimer > 0f) return;

        inCombat = false;
        combatTimer = 0f;

        ResetCombo();
    }

    public void PerformHit ( )
    {
        if (weaponHitBox == null) return;

        Collider[] hits = weaponHitBox.CheckHitbox();

        foreach (Collider hit in hits)
        {
            Hurtbox hurtbox = hit.GetComponent<Hurtbox>();

            if (hurtbox == null) continue;

            iDamageable damageable = hit.GetComponentInParent<iDamageable>();

            if (damageable == null) continue;
            if (hitTargets.Contains(damageable)) continue;

            hitTargets.Add(damageable);

            float damage = GetCurrentAttackDamage();
            Vector3 hitPoint = hit.ClosestPoint(weaponHitBox.WorldPosition);

            DamageType damageType = currentAttack == AttackType.Light ? DamageType.LightAttack : DamageType.HeavyAttack;

            DamageInfo damageInfo = new DamageInfo(
                damage,
                gameObject,
                hitPoint,
                hurtbox.HitZone,
                entityElement != null ? entityElement.CurrentElement : null,
                damageType
            );

            damageable.ReceiveDamage(damageInfo);

            if (skillController != null)
            {
                skillController.GainEnergyFromHit(damageInfo.damage);
            }

            Debug.Log(
                $"Golpe detectado | " +
                $"Objetivo: {hit.transform.root.name} | " +
                $"Zona: {hurtbox.HitZone} | " +
                $"Elemento: {damageInfo.element} | " +
                $"Tipo: {damageInfo.damageType}"
            );
        }
    }

    private float GetCurrentAttackDamage ( )
    {
        float baseDamage = playerStats.AttackDamage;

        if (currentAttack == AttackType.Light) return baseDamage;
        if (currentAttack == AttackType.Heavy) return baseDamage * 1.5f;

        return 0f;
    }

    public float MovementMultiplier
    {
        get
        {
            if (!attackLocked) return 1f;

            return currentAttack switch
            {
                AttackType.Light => lightAttackMovementMultiplier,
                AttackType.Heavy => heavyAttackMovementMultiplier,
                _ => 1f
            };
        }
    }
}