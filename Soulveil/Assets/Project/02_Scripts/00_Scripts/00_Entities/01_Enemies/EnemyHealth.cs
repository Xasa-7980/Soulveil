using UnityEngine;

public class EnemyHealth : Health
{
    protected override void OnDamaged ( DamageInfo damageInfo )
    {
        base.OnDamaged(damageInfo);

        Debug.Log(
            $"{gameObject.name} golpeado en {damageInfo.hitZone} " +
            $"por {damageInfo.attacker.name}"
        );

        Debug.Log(
            $"Punto de impacto: {damageInfo.hitPoint}"
        );

        // Más adelante:
        // - Hit reaction
        // - Aggro
        // - Stagger
        // - Damage numbers
        // - VFX en damageInfo.hitPoint
    }

    protected override void Die ( )
    {
        if (isDead)
            return;

        base.Die();

        Debug.Log("Enemigo muerto");

        // Temporal para el prototipo.
        Destroy(gameObject, 2f);
    }
}