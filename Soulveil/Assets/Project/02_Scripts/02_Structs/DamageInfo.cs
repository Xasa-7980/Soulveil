using UnityEngine;

[System.Flags]
public enum DamageType
{
    None = 0,

    LightAttack = 1 << 0,
    HeavyAttack = 1 << 1,
    FallingAttack = 1 << 2,
    Skill = 1 << 3,
    Projectile = 1 << 4,
    Explosion = 1 << 5
}

public struct DamageInfo
{
    public float damage;
    public GameObject attacker;
    public Vector3 hitPoint;
    public HitZone hitZone;
    public Element element;
    public DamageType damageType;

    public DamageInfo ( float damage, GameObject attacker, Vector3 hitPoint, HitZone hitZone, Element element, DamageType damageType = DamageType.None )
    {
        this.damage = damage;
        this.attacker = attacker;
        this.hitPoint = hitPoint;
        this.hitZone = hitZone;
        this.element = element;
        this.damageType = damageType;
    }

    public bool HasAnyDamageType ( DamageType type )
    {
        return (damageType & type) != 0;
    }

    public bool HasAllDamageTypes ( DamageType types )
    {
        return (damageType & types) == types;
    }

}