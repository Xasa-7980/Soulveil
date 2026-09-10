using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerPound : MonoBehaviour
{
    [Header("Fall Attack")]
    [SerializeField, Range(0f, 3f)] private float fallingVelocityMultiplier = 2f;
    [SerializeField] private AnimationCurve fallingSpeedCurve;
    [SerializeField] private float minimumFallingSpeed = 15f;
    [SerializeField] private float minimumFallDistance = 15f;
    [SerializeField] private LayerMask groundLayer;
    [Header("Impact")]
    [SerializeField] private float impactRadius = 3f;
    [SerializeField] private float damageMultiplier = 1.5f;
    [SerializeField] private LayerMask targetLayer;

    private PlayerMovement playerMovement;
    private PlayerActionController playerActionController;
    private PlayerAnimationController playerAnimationController;
    private PlayerStats playerStats;
    private PlayerSkillController skillController;
    private EntityElement entityElement;

    private readonly HashSet<iDamageable> damagedTargets = new();

    private bool isFallingAttack;
    private bool hasImpacted;

    public bool IsFallingAttack => isFallingAttack;
    public bool HasImpacted => hasImpacted;

    private void Awake ( )
    {
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
        if (playerMovement.IsGrounded) return;
        if (isFallingAttack) return;
        if (!CanPerformFallingAttack()) return;
        if (playerActionController != null && !playerActionController.CanCombat) return;

        isFallingAttack = true;
        hasImpacted = false;

        damagedTargets.Clear();

        if (playerActionController != null) playerActionController.Block(this, PlayerActionBlock.All);

        playerMovement.ApplyDownwardVelocity(fallingVelocityMultiplier, minimumFallingSpeed);

        if (playerAnimationController != null) playerAnimationController.PlayFallingAttackIntro();

        Debug.Log("[PlayerPound] Falling Attack iniciado.");
    }

    private void UpdateFallingAttack ( )
    {
        if (!isFallingAttack) return;
        if (hasImpacted) return;
        if (!playerMovement.IsGrounded) return;

        Impact();
    }

    private void Impact ( )
    {
        if (hasImpacted) return;

        hasImpacted = true;

        float impactSpeed = Mathf.Abs(playerMovement.MaxFallSpeed);

        Debug.Log($"[PlayerPound] Impacto | Velocidad de caída: {impactSpeed:F2}");

        PerformImpactDamage();

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

        damagedTargets.Clear();

        if (playerActionController != null) playerActionController.Unblock(this);

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
        if (playerActionController != null) playerActionController.Unblock(this);

        damagedTargets.Clear();

        isFallingAttack = false;
        hasImpacted = false;
    }

    private void OnDrawGizmosSelected ( )
    {
        Gizmos.color = Color.red + Color.yellow;
        Gizmos.DrawWireSphere(transform.position, impactRadius);

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * minimumFallDistance);
    }
}