using UnityEngine;

public class PlayerHealth : Health
{
    private PlayerStats playerStats;
    private PlayerSkillController skillController;
    private PlayerActionController playerActions;

    protected override void Awake ( )
    {
        playerStats = GetComponent<PlayerStats>();
        skillController = GetComponent<PlayerSkillController>();
        playerActions = GetComponent<PlayerActionController>();

        if (playerStats != null)
        {
            maxHealth = playerStats.MaxHealth;
        }

        base.Awake();
    }

    protected override void OnDamaged ( DamageInfo damageInfo )
    {
        base.OnDamaged(damageInfo);

        if (skillController != null)
        {
            skillController.GainEnergyFromDamageTaken(damageInfo.damage);
        }

        // Más adelante:
        // - Hit animation
        // - Sonido
        // - UI
        // - Camera feedback
    }

    protected override void Die ( )
    {
        if (isDead) return;

        base.Die();

        if (playerActions != null)
        {
            playerActions.Block(this, PlayerActionBlock.All);
        }

        Debug.Log("Player muerto");

        // Más adelante:
        // - Animación de muerte
        // - UI de derrota
        // - GameManager.EndRun()
        // - Respawn / Revive
    }

    public void Revive ( float healthPercent = 1f )
    {
        if (!isDead) return;

        isDead = false;

        healthPercent = Mathf.Clamp01(healthPercent);

        currentHealth = maxHealth * healthPercent;

        if (currentHealth <= 0f)
        {
            currentHealth = 1f;
        }

        if (playerActions != null)
        {
            playerActions.Unblock(this);
        }

        Debug.Log(
            $"Player revivido | " +
            $"Vida: {currentHealth}/{maxHealth}"
        );
    }
}