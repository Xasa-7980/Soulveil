using UnityEngine;

public class WeaponVFXController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ElementVFXDatabase elementVFXDatabase;
    [SerializeField] private Transform auraParent;

    private GameObject currentAura;
    private Element currentElement;
    private ItemRarity currentRarity;

    public Element CurrentElement => currentElement;
    public ItemRarity CurrentRarity => currentRarity;

    public void SetAura ( Element element, ItemRarity rarity )
    {
        ClearAura();

        currentElement = element;
        currentRarity = rarity;

        if (elementVFXDatabase == null) return;
        if (currentElement == null) return;

        ElementVFXProfile profile = elementVFXDatabase.GetProfile(currentElement);

        if (profile == null) return;

        GameObject auraPrefab = profile.GetWeaponAura(currentRarity);

        if (auraPrefab == null) return;

        Transform parent = auraParent != null ? auraParent : transform;

        currentAura = Instantiate(
            auraPrefab,
            parent.position,
            parent.rotation,
            parent
        );

        currentAura.transform.localPosition = Vector3.zero;
        currentAura.transform.localRotation = Quaternion.identity;
    }

    public void ClearAura ( )
    {
        if (currentAura != null)
        {
            Destroy(currentAura);
            currentAura = null;
        }

        currentElement = null;
    }
}