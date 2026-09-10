using UnityEngine;

public class PlayerEquipment : MonoBehaviour
{
    private PlayerStats playerStats;

    private ItemInstance equippedWeapon;
    private ItemInstance equippedArmor;
    private ItemInstance equippedArtifact;

    public ItemInstance EquippedWeapon => equippedWeapon;
    public ItemInstance EquippedArmor => equippedArmor;
    public ItemInstance EquippedArtifact => equippedArtifact;

    private void Awake ( )
    {
        playerStats = GetComponent<PlayerStats>();
    }
}