using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombat : MonoBehaviour
{
    [Header("Combo")]
    [SerializeField, Range(0f, 1f)] private float nextAttackNormalizedTime = 0.95f;
    [SerializeField] private float comboResetDelay = 1f;

    [Header("Combat State")]
    [SerializeField] private float inCombatCountdown = 4f;

    private PlayerSpeacialist playerSpecialist;
    private PlayerAnimationController animationController;

    private int lightAttackIndex = 1;
    private int heavyAttackIndex = 1;

    private float comboResetTimer;
    private float combatTimer;

    private bool attackLocked;
    private bool inCombat;
    private bool lightAttackBuffered;
    private bool heavyAttackBuffered;

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
        if (!context.performed)
            return;

        if (!attackLocked)
        {
            StartLightAttack();
            return;
        }

        lightAttackBuffered = true;
        heavyAttackBuffered = false;
    }

    public void OnHeavyAttack ( InputAction.CallbackContext context )
    {
        if (!context.performed)
            return;

        if (!attackLocked)
        {
            StartHeavyAttack();
            return;
        }

        lightAttackBuffered = false;
        heavyAttackBuffered = true;
    }

    private void StartLightAttack ( )
    {
        if (playerSpecialist.lightAttackLength <= 0)
            return;

        attackLocked = true;
        lightAttackBuffered = false;
        heavyAttackBuffered = false;
        comboResetTimer = 0f;

        EnterCombat();
        animationController.PlayLightAttack(lightAttackIndex);
    }

    private void StartHeavyAttack ( )
    {
        if (playerSpecialist.heavyAttackLength <= 0)
            return;

        attackLocked = true;
        lightAttackBuffered = false;
        heavyAttackBuffered = false;
        comboResetTimer = 0f;

        EnterCombat();
        animationController.PlayHeavyAttack(heavyAttackIndex);
    }

    private void UpdateAttack ( )
    {
        if (!attackLocked)
            return;

        if (!animationController.IsInAttackState())
            return;

        if (animationController.GetCombatNormalizedTime() < nextAttackNormalizedTime)
            return;

        if (lightAttackBuffered)
        {
            AdvanceLightCombo();
            attackLocked = false;
            StartLightAttack();
            return;
        }

        if (heavyAttackBuffered)
        {
            AdvanceHeavyCombo();
            attackLocked = false;
            StartHeavyAttack();
            return;
        }

        FinishAttack();
    }

    private void AdvanceLightCombo ( )
    {
        lightAttackIndex++;

        if (lightAttackIndex > playerSpecialist.lightAttackLength)
            lightAttackIndex = 1;

        heavyAttackIndex = 1;
    }

    private void AdvanceHeavyCombo ( )
    {
        heavyAttackIndex++;

        if (heavyAttackIndex > playerSpecialist.heavyAttackLength)
            heavyAttackIndex = 1;

        lightAttackIndex = 1;
    }

    private void FinishAttack ( )
    {
        attackLocked = false;
        lightAttackBuffered = false;
        heavyAttackBuffered = false;
        comboResetTimer = comboResetDelay;
    }

    private void UpdateComboReset ( )
    {
        if (attackLocked || comboResetTimer <= 0f)
            return;

        comboResetTimer -= Time.deltaTime;

        if (comboResetTimer <= 0f)
            ResetCombo();
    }

    private void ResetCombo ( )
    {
        lightAttackIndex = 1;
        heavyAttackIndex = 1;
        lightAttackBuffered = false;
        heavyAttackBuffered = false;
        comboResetTimer = 0f;
    }

    private void EnterCombat ( )
    {
        inCombat = true;
        combatTimer = inCombatCountdown;
    }

    private void UpdateCombatTimer ( )
    {
        if (!inCombat)
            return;

        combatTimer -= Time.deltaTime;

        if (combatTimer <= 0f)
            ExitCombat();
    }

    private void ExitCombat ( )
    {
        inCombat = false;
        combatTimer = 0f;
        ResetCombo();
    }
}