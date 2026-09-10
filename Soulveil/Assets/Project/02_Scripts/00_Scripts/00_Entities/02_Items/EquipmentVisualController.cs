using UnityEngine;

public class EquipmentVisualController : MonoBehaviour
{
    [Header("Weapon")]
    [SerializeField] private Transform weaponParent;

    private GameObject currentWeaponModel;

    public GameObject CurrentWeaponModel => currentWeaponModel;

    public void SetWeaponVisual ( WeaponItemData weaponData )
    {
        ClearWeaponVisual();

        if (weaponData == null) return;
        if (weaponData.WeaponModel == null) return;

        currentWeaponModel = Instantiate(
            weaponData.WeaponModel,
            weaponParent
        );

        currentWeaponModel.transform.localPosition = Vector3.zero;
        currentWeaponModel.transform.localRotation = Quaternion.identity;
    }

    public void ClearWeaponVisual ( )
    {
        if (currentWeaponModel == null) return;

        Destroy(currentWeaponModel);
        currentWeaponModel = null;
    }
}