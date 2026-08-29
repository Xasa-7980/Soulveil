using UnityEngine;

public class PlayerHealth : Health
{
    private PlayerStats playerStats;

    protected override void Awake ( )
    {
        playerStats = GetComponent<PlayerStats>();

        if (playerStats != null)
        {
            maxHealth = playerStats.MaxHealth;
        }

        base.Awake();
    }

    protected override void OnDamaged ( DamageInfo damageInfo )
    {
        base.OnDamaged(damageInfo);

        Debug.Log(
            $"Player golpeado en {damageInfo.hitZone} " +
            $"por {damageInfo.attacker.name}"
        );

        // Más adelante:
        // - Hit animation
        // - Sonido
        // - UI
        // - Camera feedback
    }

    protected override void Die ( )
    {
        if (isDead)
            return;

        base.Die();

        Debug.Log("Player muerto");

        // Más adelante:
        // PlayerCombat desactivado
        // PlayerMovement desactivado
        // Animación de muerte
        // GameManager.EndRun()
    }
}