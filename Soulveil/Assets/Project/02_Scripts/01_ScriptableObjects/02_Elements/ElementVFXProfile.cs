using UnityEngine;

[CreateAssetMenu( fileName = "NewElementVFXProfile", menuName = "Soulveil/VFX/Element VFX Profile" )]
public class ElementVFXProfile : ScriptableObject
{
    [Header("Element")]
    [SerializeField] private Element element;

    [Header("Hit")]
    [SerializeField] private GameObject hitVFX;

    [Header("Weapon Auras")]
    [SerializeField] private GameObject commonWeaponAura;
    [SerializeField] private GameObject uncommonWeaponAura;
    [SerializeField] private GameObject rareWeaponAura;
    [SerializeField] private GameObject epicWeaponAura;
    [SerializeField] private GameObject legendaryWeaponAura;
    [SerializeField] private GameObject uniqueWeaponAura;

    [Header("Weapon Trails")]
    [SerializeField] private GameObject commonWeaponTrail;
    [SerializeField] private GameObject uncommonWeaponTrail;
    [SerializeField] private GameObject rareWeaponTrail;
    [SerializeField] private GameObject epicWeaponTrail;
    [SerializeField] private GameObject legendaryWeaponTrail;
    [SerializeField] private GameObject uniqueWeaponTrail;

    public Element Element => element;
    public GameObject HitVFX => hitVFX;

    public GameObject GetWeaponAura ( ItemRarity rarity )
    {
        return rarity switch
        {
            ItemRarity.Common => commonWeaponAura,
            ItemRarity.Uncommon => uncommonWeaponAura,
            ItemRarity.Rare => rareWeaponAura,
            ItemRarity.Epic => epicWeaponAura,
            ItemRarity.Legendary => legendaryWeaponAura,
            ItemRarity.Unique => uniqueWeaponAura,
            _ => null
        };
    }
    public GameObject GetWeaponTrail ( ItemRarity rarity )
    {
        return rarity switch
        {
            ItemRarity.Common => commonWeaponTrail,
            ItemRarity.Uncommon => uncommonWeaponTrail,
            ItemRarity.Rare => rareWeaponTrail,
            ItemRarity.Epic => epicWeaponTrail,
            ItemRarity.Legendary => legendaryWeaponTrail,
            ItemRarity.Unique => uniqueWeaponTrail,
            _ => null
        };
    }
}