using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerPound : MonoBehaviour
{
    [Header("Fall Attack")]
    [SerializeField] private float minimumFallingSpeed = 1.5f;
    [SerializeField] private float maximumFallingSpeed = 25f;
    [SerializeField] private AnimationCurve fallingSpeedCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    [SerializeField] private float fallingCurveDuration = 0.55f;
    [SerializeField] private float minimumFallDistance = 3f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Impact")]
    [SerializeField] private float impactRadius = 3f;
    [SerializeField] private float damageMultiplier = 1.5f;
    [SerializeField] private LayerMask targetLayer;

    private CharacterController characterController;
    private PlayerMovement playerMovement;
    private PlayerActionController playerActionController;
    private PlayerAnimationController playerAnimationController;
    private PlayerStats playerStats;
    private PlayerSkillController skillController;
    private EntityElement entityElement;

    private readonly HashSet<iDamageable> damagedTargets = new();

    private bool isFallingAttack;
    private bool hasImpacted;

    private float fallingAttackTimer;
    private float currentFallingSpeed;

    public bool IsFallingAttack => isFallingAttack;
    public bool HasImpacted => hasImpacted;
    public float CurrentFallingSpeed => currentFallingSpeed;

    private void Awake ( )
    {
        characterController = GetComponent<CharacterController>();
        playerMovement = GetComponent<PlayerMovement>();
        playerActionController = GetComponent<PlayerActionController>();
        playerAnimationController = GetComponent<PlayerAnimationController>();
        playerStats = GetComponent<PlayerStats>();
        skillController = GetComponent<PlayerSkillController>();
        entityElement = GetComponent<EntityElement>();
    }

    private void Update ( )
    {
        UpdateFallingAttack();
    }

    public void OnFallingAttack ( InputAction.CallbackContext context )
    {
        if (!context.performed) return;

        FallingAttack();
    }

    public void FallingAttack ( )
    {
        if (playerMovement == null) return;
        if (characterController == null) return;
        if (playerMovement.IsGrounded) return;
        if (isFallingAttack) return;
        if (!CanPerformFallingAttack()) return;
        if (playerActionController != null && !playerActionController.CanCombat) return;

        isFallingAttack = true;
        hasImpacted = false;

        fallingAttackTimer = 0f;
        currentFallingSpeed = minimumFallingSpeed;

        damagedTargets.Clear();

        if (playerActionController != null)
        {
            playerActionController.Block(this, PlayerActionBlock.All);

            Debug.Log($"[PlayerPound] Bloqueado. Combat permitido: {playerActionController.CanCombat}");
        }

        // Durante el Falling Attack la velocidad vertical es controlada
        // directamente por PlayerPound mediante la AnimationCurve.
        playerMovement.SetGravitySuspended(true);

        if (playerAnimationController != null) playerAnimationController.PlayFallingAttackIntro();

        Debug.Log("[PlayerPound] Falling Attack iniciado.");
    }

    private void UpdateFallingAttack ( )
    {
        if (!isFallingAttack) return;
        if (hasImpacted) return;

        fallingAttackTimer += Time.deltaTime;

        float normalizedTime = fallingCurveDuration > 0f ? fallingAttackTimer / fallingCurveDuration : 1f;
        normalizedTime = Mathf.Clamp01(normalizedTime);

        float curveValue = fallingSpeedCurve.Evaluate(normalizedTime);

        currentFallingSpeed = Mathf.LerpUnclamped(minimumFallingSpeed, maximumFallingSpeed, curveValue);

        CollisionFlags collisionFlags = characterController.Move(Vector3.down * currentFallingSpeed * Time.deltaTime);

        if ((collisionFlags & CollisionFlags.Below) != 0) Impact();
    }

    private void Impact ( )
    {
        if (hasImpacted) return;

        hasImpacted = true;

        Debug.Log($"[PlayerPound] Impacto | Velocidad de caída: {currentFallingSpeed:F2}");

        PerformImpactDamage();

        if (playerAnimationController != null)
        {
            playerAnimationController.PlayFallingAttackEnding();

            Debug.Log("[PlayerPound] FallingAttackEnd = TRUE.");
        }

        // Temporalmente termina inmediatamente al impactar.
        // Cuando exista animación de recuperación,
        // FinishFallingAttack será llamado por Animation Event.
        FinishFallingAttack();
    }

    private void PerformImpactDamage ( )
    {
        damagedTargets.Clear();

        Collider[] hits = Physics.OverlapSphere(transform.position, impactRadius, targetLayer);

        foreach (Collider hit in hits)
        {
            Hurtbox hurtbox = hit.GetComponent<Hurtbox>();

            if (hurtbox == null) continue;

            iDamageable damageable = hit.GetComponentInParent<iDamageable>();

            if (damageable == null) continue;
            if (damagedTargets.Contains(damageable)) continue;

            damagedTargets.Add(damageable);

            float damage = playerStats != null ? playerStats.AttackDamage * damageMultiplier : 10f;
            Vector3 hitPoint = hit.ClosestPoint(transform.position);
            Element element = entityElement != null ? entityElement.CurrentElement : null;

            DamageInfo damageInfo = new DamageInfo(damage, gameObject, hitPoint, hurtbox.HitZone, element, DamageType.FallingAttack);

            damageable.ReceiveDamage(damageInfo);

            if (skillController != null) skillController.GainEnergyFromHit(damageInfo.damage);

            Debug.Log($"[PlayerPound] Impacto | Objetivo: {hit.transform.root.name} | Daño: {damageInfo.damage} | Tipo: {damageInfo.damageType}");
        }
    }

    public void FinishFallingAttack ( )
    {
        if (!isFallingAttack) return;

        isFallingAttack = false;
        hasImpacted = false;

        fallingAttackTimer = 0f;
        currentFallingSpeed = 0f;

        damagedTargets.Clear();

        if (playerMovement != null) playerMovement.SetGravitySuspended(false);

        if (playerActionController != null)
        {
            playerActionController.Unblock(this);

            Debug.Log($"[PlayerPound] Unblock realizado. Combat permitido: {playerActionController.CanCombat}");
        }

        Debug.Log("[PlayerPound] Falling Attack finalizado.");
    }

    private bool CanPerformFallingAttack ( )
    {
        Ray ray = new Ray(transform.position, Vector3.down);
        bool groundDetected = Physics.Raycast(ray, out RaycastHit hit, minimumFallDistance, groundLayer);

        if (groundDetected)
        {
            Debug.Log($"[PlayerPound] Suelo detectado a {hit.distance:F2}m. Mínimo requerido: {minimumFallDistance}m.");
            return false;
        }

        Debug.Log($"[PlayerPound] No hay suelo dentro de {minimumFallDistance}m. Falling Attack permitido.");

        return true;
    }

    private void OnDisable ( )
    {
        if (playerMovement != null) playerMovement.SetGravitySuspended(false);
        if (playerActionController != null) playerActionController.Unblock(this);

        damagedTargets.Clear();

        isFallingAttack = false;
        hasImpacted = false;

        fallingAttackTimer = 0f;
        currentFallingSpeed = 0f;
    }

    private void OnDrawGizmosSelected ( )
    {
        Gizmos.color = Color.red + Color.yellow;
        Gizmos.DrawWireSphere(transform.position, impactRadius);

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * minimumFallDistance);
    }
}