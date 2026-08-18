using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombat : MonoBehaviour
{
    private enum AttackType { None, Light, Heavy }

    [Header("Combo")]
    [SerializeField] private float comboResetDelay = 1f;
    [SerializeField, Range(0f, 1f)] private float normTime = 0.75f;

    [Header("Combat State")]
    [SerializeField] private float inCombatCountdown = 4f;

    private PlayerSpeacialist playerSpecialist;
    private PlayerAnimationController animationController;

    private int lightAttackIndex = 1;
    private int heavyAttackIndex = 1;

    private float comboResetTimer;
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
        playerSpecialist = GetComponent<PlayerSpeacialist>();
        animationController = GetComponent<PlayerAnimationController>();
    }

    private void Update ( )
    {
        UpdateAttack();
        UpdateComboReset();
        UpdateCombatTimer();
    }

    public void OnLightAttack ( InputAction.CallbackContext context )
    {
        if (!context.performed) return;

        if (!attackLocked)
            StartAttack(AttackType.Light);
        else
            bufferedAttack = AttackType.Light;
    }

    public void OnHeavyAttack ( InputAction.CallbackContext context )
    {
        if (!context.performed) return;

        if (!attackLocked)
            StartAttack(AttackType.Heavy);
        else
            bufferedAttack = AttackType.Heavy;
    }

    private void StartAttack ( AttackType type )
    {
        if (GetAttackLength(type) <= 0) return;

        bool startingNewChain = !attackLocked;

        attackLocked = true;

        if (startingNewChain)
            attackStateEntered = false;

        currentAttack = type;
        bufferedAttack = AttackType.None;
        comboResetTimer = 0f;

        EnterCombat();
        PlayAttack(type);
    }

    private void UpdateAttack ( )
    {
        if (!attackLocked || animationController.IsInAttackTransition()) return;

        bool inAttackState = animationController.IsInAttackState();

        if (!inAttackState && !attackStateEntered)
            return;

        if (!inAttackState && attackStateEntered)
        {
            FinishAttack();
            return;
        }

        if (!animationController.IsCurrentAttack( currentAttack == AttackType.Light, GetAttackIndex(currentAttack)))
            return;

        attackStateEntered = true;

        if (animationController.GetCombatNormalizedTime() < normTime) return;
        if (bufferedAttack == AttackType.None) return;

        AttackType nextAttack = bufferedAttack;

        AdvanceCombo(nextAttack);
        StartAttack(nextAttack);
    }

    private void PlayAttack ( AttackType type )
    {
        int index = GetAttackIndex(type);

        if (type == AttackType.Light)
            animationController.PlayLightAttack(index);
        else
            animationController.PlayHeavyAttack(index);
    }

    private void AdvanceCombo ( AttackType type )
    {
        if (type == AttackType.Light)
        {
            lightAttackIndex++;

            if (lightAttackIndex > playerSpecialist.lightAttackLength)
                lightAttackIndex = 1;

            heavyAttackIndex = 1;
        }
        else
        {
            heavyAttackIndex++;

            if (heavyAttackIndex > playerSpecialist.heavyAttackLength)
                heavyAttackIndex = 1;

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

        currentAttack = AttackType.None;
        bufferedAttack = AttackType.None;

        comboResetTimer = comboResetDelay;
        combatTimer = inCombatCountdown;
    }

    private void UpdateComboReset ( )
    {
        if (attackLocked || comboResetTimer <= 0f) return;

        comboResetTimer -= Time.deltaTime;

        if (comboResetTimer <= 0f)
            ResetCombo();
    }

    private void ResetCombo ( )
    {
        lightAttackIndex = 1;
        heavyAttackIndex = 1;

        currentAttack = AttackType.None;
        bufferedAttack = AttackType.None;

        comboResetTimer = 0f;
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
}