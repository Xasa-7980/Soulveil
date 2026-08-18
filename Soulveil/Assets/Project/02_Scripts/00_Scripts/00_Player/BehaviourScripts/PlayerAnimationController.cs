using UnityEngine;

public class PlayerAnimationController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator animator;

    [Header("Layers")]
    [SerializeField] private float combatLayerBlendSpeed = 5f;

    private RuntimeAnimatorController baseAnimatorController;

    private PlayerMovement playerMovement;
    private PlayerDodge playerDodge;
    private PlayerSpeacialist playerSpecialist;
    private PlayerCombat playerCombat;

    private int baseLayerIndex;
    private int combatLayerIndex;
    private int faceLayerIndex;

    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int GroundedHash = Animator.StringToHash("Grounded");
    private static readonly int VerticalSpeedHash = Animator.StringToHash("VerticalSpeed");
    private static readonly int IsCrouchingHash = Animator.StringToHash("IsCrouching");

    private static readonly int DodgeHash = Animator.StringToHash("Dodge");
    private static readonly int DodgeStanceHash = Animator.StringToHash("DodgeStance");

    private static readonly int JumpHash = Animator.StringToHash("Jump");
    private static readonly int LandingIndexHash = Animator.StringToHash("LandingIndex");

    private static readonly int ComboIndexHash = Animator.StringToHash("ComboIndex");
    private static readonly int LightAttackHash = Animator.StringToHash("LightAttack");
    private static readonly int HeavyAttackHash = Animator.StringToHash("HeavyAttack");

    private bool wasDodging;

    private bool InCombat => playerCombat != null && playerCombat.InCombat;

    private void Awake ( )
    {
        playerMovement = GetComponent<PlayerMovement>();
        playerDodge = GetComponent<PlayerDodge>();
        playerSpecialist = GetComponent<PlayerSpeacialist>();
        playerCombat = GetComponent<PlayerCombat>();

        RuntimeAnimatorController currentController = animator.runtimeAnimatorController;

        if (currentController is AnimatorOverrideController currentOverride)
            baseAnimatorController = currentOverride.runtimeAnimatorController;
        else
            baseAnimatorController = currentController;

        CacheLayerIndexes();
        ResetLayerWeights();
    }

    private void OnEnable ( )
    {
        if (playerSpecialist != null)
            playerSpecialist.OnChangeSpecialist += OnChangeSpecialist;
    }

    private void OnDisable ( )
    {
        if (playerSpecialist != null)
            playerSpecialist.OnChangeSpecialist -= OnChangeSpecialist;
    }

    private void Update ( )
    {
        UpdateLayers();
        UpdateLocomotion();
        UpdateDodge();
        UpdateJump();
    }

    #region SPECIALIST

    private void OnChangeSpecialist ( object sender, PlayerSpeacialist.OnChangeSpecialistEventArgs e )
    {
        AnimatorOverrideController newOverride = e.specialistHandler != null
            ? e.specialistHandler.animatorOverrideController
            : null;

        SetAnimatorOverride(newOverride);
    }

    public void SetAnimatorOverride ( AnimatorOverrideController overrideController )
    {
        animator.runtimeAnimatorController = overrideController != null
            ? overrideController
            : baseAnimatorController;

        CacheLayerIndexes();
        ResetLayerWeights();
    }

    #endregion

    #region LAYERS

    private void CacheLayerIndexes ( )
    {
        baseLayerIndex = animator.GetLayerIndex("Base Layer");
        combatLayerIndex = animator.GetLayerIndex("Combat Layer");
        faceLayerIndex = animator.GetLayerIndex("Face Layer");
    }

    private void ResetLayerWeights ( )
    {
        if (baseLayerIndex >= 0)
            animator.SetLayerWeight(baseLayerIndex, 1f);

        if (combatLayerIndex >= 0)
            animator.SetLayerWeight(combatLayerIndex, InCombat ? 1f : 0f);

        if (faceLayerIndex >= 0)
            animator.SetLayerWeight(faceLayerIndex, 1f);
    }

    private void UpdateLayers ( )
    {
        if (combatLayerIndex < 0)
            return;

        float currentWeight = animator.GetLayerWeight(combatLayerIndex);
        float targetWeight = InCombat ? 1f : 0f;

        animator.SetLayerWeight(
            combatLayerIndex,
            Mathf.MoveTowards(
                currentWeight,
                targetWeight,
                combatLayerBlendSpeed * Time.deltaTime
            )
        );
    }

    #endregion

    #region LOCOMOTION

    private void UpdateLocomotion ( )
    {
        float speed = 0f;

        if (playerMovement.MoveDirection.sqrMagnitude > 0.01f)
            speed = playerMovement.IsSprinting ? 2f : 1f;

        animator.SetFloat(SpeedHash, speed, 0.1f, Time.deltaTime);
        animator.SetBool(IsCrouchingHash, playerMovement.IsCrouching);
        animator.SetBool(GroundedHash, playerMovement.IsGrounded);
        animator.SetFloat(VerticalSpeedHash, playerMovement.VerticalVelocity);
    }

    #endregion

    #region JUMP

    public void PlayJump ( )
    {
        animator.SetTrigger(JumpHash);
    }

    private void UpdateJump ( )
    {
        if (!playerMovement.JustLanded)
            return;

        float fallSpeed = Mathf.Abs(playerMovement.MaxFallSpeed);

        int landingIndex;

        if (fallSpeed < 15f)
            landingIndex = 0;
        else if (fallSpeed < 30f)
            landingIndex = 1;
        else
            landingIndex = 2;

        animator.SetFloat(LandingIndexHash, landingIndex);
    }

    #endregion

    #region DODGE

    private void UpdateDodge ( )
    {
        bool isDodging = playerDodge.IsDodging;

        if (isDodging && !wasDodging)
        {
            animator.SetFloat(DodgeStanceHash, playerDodge.DodgeStance);
            animator.SetTrigger(DodgeHash);
        }

        wasDodging = isDodging;
    }

    #endregion

    #region COMBAT

    public void PlayLightAttack ( int comboIndex )
    {
        animator.ResetTrigger(HeavyAttackHash);
        animator.SetInteger(ComboIndexHash, comboIndex);
        animator.SetTrigger(LightAttackHash);
    }

    public void PlayHeavyAttack ( int comboIndex )
    {
        animator.ResetTrigger(LightAttackHash);
        animator.SetInteger(ComboIndexHash, comboIndex);
        animator.SetTrigger(HeavyAttackHash);
    }

    public float GetCombatNormalizedTime ( )
    {
        if (combatLayerIndex < 0)
            return 0f;

        return animator.GetCurrentAnimatorStateInfo(combatLayerIndex).normalizedTime;
    }

    public bool IsInAttackTransition ( )
    {
        return combatLayerIndex >= 0 && animator.IsInTransition(combatLayerIndex);
    }

    public bool IsInAttackState ( )
    {
        if (combatLayerIndex < 0) return false;

        return IsAttackState( animator.GetCurrentAnimatorStateInfo(combatLayerIndex) );
    }

    public bool IsCurrentAttack ( bool isLight, int comboIndex )
    {
        if (combatLayerIndex < 0) return false;

        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(combatLayerIndex);

        string stateName = (isLight ? "LightAttack " : "HeavyAttack ") + comboIndex;

        return stateInfo.IsName(stateName);
    }

    private bool IsAttackState ( AnimatorStateInfo stateInfo )
    {
        return stateInfo.IsName("LightAttack 1") ||
               stateInfo.IsName("LightAttack 2") ||
               stateInfo.IsName("LightAttack 3") ||
               stateInfo.IsName("LightAttack 4") ||
               stateInfo.IsName("LightAttack 5") ||
               stateInfo.IsName("HeavyAttack 1") ||
               stateInfo.IsName("HeavyAttack 2") ||
               stateInfo.IsName("HeavyAttack 3") ||
               stateInfo.IsName("HeavyAttack 4") ||
               stateInfo.IsName("HeavyAttack 5");
    }

    #endregion
}