using UnityEngine;

public class EnemyHurtReaction : MonoBehaviour
{
    [Header("Hurt Reaction")]
    [SerializeField] protected GameObject hurtVFX;

    protected EnemyHealth enemyHealth;

    protected virtual void Awake ( )
    {
        enemyHealth = GetComponent<EnemyHealth>();

        if (enemyHealth == null) return;

        enemyHealth.OnDamagedEvent += HandleOnDamaged;
        enemyHealth.OnElementReactionEvent += HandleElementReaction;
    }

    protected virtual void HandleOnDamaged ( object sender, DamageInfo damageInfo )
    {
        PlayHurtVFX(damageInfo);

        Debug.Log($"{gameObject.name} ha recibido daño. Salud actual: {enemyHealth.CurrentHealth}");
    }

    protected virtual void HandleElementReaction ( object sender, ElementReactionType reaction )
    {
        Debug.Log($"{gameObject.name} recibió reacción elemental: {reaction}");
    }

    protected virtual void PlayHurtVFX ( DamageInfo damageInfo )
    {
        if (hurtVFX == null) return;

        Instantiate(hurtVFX, damageInfo.hitPoint, Quaternion.identity);
    }

    protected virtual void OnDestroy ( )
    {
        if (enemyHealth == null) return;

        enemyHealth.OnDamagedEvent -= HandleOnDamaged;
        enemyHealth.OnElementReactionEvent -= HandleElementReaction;
    }
}