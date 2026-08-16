using UnityEngine;

public enum WeaponHandler
{
    Unarmed,
    OneHanded,
    TwoHanded
}

[CreateAssetMenu(
    fileName = "NewWeapon",
    menuName = "Soulveil/Weapons/Weapon Data"
)]
public class WeaponData : ScriptableObject
{
    [Header("Identity")]
    public string weaponName;
    public WeaponHandler weaponHandler;

    [Header("Visual")]
    public GameObject weaponPrefab;

    [Header("Combat")]
    public float lightDamage = 20f;
    public float heavyDamage = 35f;
    public float attackSpeed = 1f;

    [Header("Animation")]
    public AnimatorOverrideController animationOverride;
}