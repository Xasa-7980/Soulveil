using UnityEngine;

public struct DamageInfo
{
    public float damage;
    public GameObject attacker;
    public Vector3 hitPoint;
    public HitZone hitZone;
    public Element element;

    public DamageInfo (
        float damage,
        GameObject attacker,
        Vector3 hitPoint,
        HitZone hitZone,
        Element element)
    {
        this.damage = damage;
        this.attacker = attacker;
        this.hitPoint = hitPoint;
        this.hitZone = hitZone;
        this.element = element;
    }
}