using UnityEngine;
public struct EntityInfo
{
    public GameObject entity;
    public Health health;
    public Stats stats;
    public EntityElement element;
    public EntityInfo ( GameObject entity, Health health, Stats stats, EntityElement element )
    {
        this.entity = entity;
        this.health = health;
        this.stats = stats;
        this.element = element;
    }
}
public abstract class Health : MonoBehaviour, iDamageable
{
    [Header("Health")]
    [SerializeField] protected float maxHealth = 100f;

    [Header("Hit Zone Multipliers")]
    [SerializeField] private float headDamageMultiplier = 1.5f;
    [SerializeField] private float bodyDamageMultiplier = 1f;
    [SerializeField] private float legsDamageMultiplier = 0.8f;

    protected float currentHealth;
    protected bool isDead;
    protected bool isInvincible;
    public event System.EventHandler<DamageInfo> OnDamagedEvent;
    public event System.EventHandler<ElementReactionType> OnElementReactionEvent;

    public float CurrentHealth => currentHealth;
    public float PercentHealth => maxHealth > 0f ? currentHealth / maxHealth : 0f;
    public float MaxHealth => maxHealth;
    public bool IsDead => isDead;
    public bool IsInvincible => isInvincible;
    private Stats stats;
    public EntityInfo EntityInfo => new EntityInfo(gameObject, this, stats, entityElement);
    private EntityElement entityElement;

    protected virtual void Awake ( )
    {
        currentHealth = maxHealth;
        stats = GetComponent<Stats>();
        entityElement = GetComponent<EntityElement>();
    }

    public virtual void ReceiveDamage ( DamageInfo damageInfo )
    {
        if (isDead) return;
        if (isInvincible) return;
        if (damageInfo.damage <= 0f) return;

        float finalDamage = CalculateHitZoneDamage( damageInfo.damage,  damageInfo.hitZone );

        // Actualizamos DamageInfo para que el resto de sistemas
        // reciban el daño final realmente aplicado.
        damageInfo.damage = finalDamage;

        currentHealth -= finalDamage;

        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);

        //Debug.Log( $"{gameObject.name} recibe {finalDamage} de daño " + $"en {damageInfo.hitZone}. " + $"Vida: {currentHealth}/{maxHealth}" + 
        //    $"Elemento personaje: {entityElement.CurrentElement}" + $"Elemento Enemigo: {damageInfo.element}");

        CheckElementReaction(damageInfo);
        OnDamaged(damageInfo);
        WorldTextManager.ShowDamage(damageInfo.damage, transform.position + Vector3.up * 2f);
        if (damageInfo.element != null)
        {
            CombatVFXManager.ShowHit( damageInfo.element, damageInfo.hitPoint);
        }

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    private float CalculateHitZoneDamage ( float damage,  HitZone hitZone )
    {
        switch (hitZone)
        {
            case HitZone.Head:
                return damage * headDamageMultiplier;

            case HitZone.Body:
                return damage * bodyDamageMultiplier;

            case HitZone.Legs:
                return damage * legsDamageMultiplier;

            default:
                return damage;
        }
    }

    protected virtual void OnDamaged ( DamageInfo damageInfo )
    {
        OnDamagedEvent?.Invoke(this, damageInfo);
    }

    public virtual void SetInvincible ( bool value )
    {
        isInvincible = value;
    }

    protected virtual void Die ( )
    {
        if (isDead)
            return;

        isDead = true;

        Debug.Log($"{gameObject.name} ha muerto.");
    }
    private void CheckElementReaction ( DamageInfo damageInfo )
    {
        if (entityElement == null) return;
        if (entityElement.CurrentElement == null) return;
        if (damageInfo.element == null) return;
        if (ElementReactionSystem.Instance == null) return;

        ElementReactionType reaction = ElementReactionSystem.Instance.TryReact(entityElement.CurrentElement, damageInfo.element, damageInfo, EntityInfo);

        if (reaction == ElementReactionType.None) return;

        OnElementReactionEvent?.Invoke(this,reaction);
    }
}
