using UnityEngine;
public enum WeaponType
{
    Sword,
    GreatSword,
    Dagger,
    Bow,
    Staff,
    Wand
}
[CreateAssetMenu(fileName = "NewWeapon", menuName = "Soulveil/Items/Equipment/Weapon")]
public class WeaponItemData : EquipmentItemData
{
    [Header("Weapon")]
    [SerializeField] private WeaponType weaponType;
    [SerializeField] private float baseDamage;
    [SerializeField] private GameObject weaponModel;

    public WeaponType WeaponType => weaponType;
    public float BaseDamage => baseDamage;
    public GameObject WeaponModel => weaponModel;

}