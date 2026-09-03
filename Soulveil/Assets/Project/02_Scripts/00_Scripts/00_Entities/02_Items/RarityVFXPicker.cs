using UnityEngine;
using System.Linq;

public class RarityVFXPicker : MonoBehaviour
{
    [System.Serializable]
    private class RarityVFXSelector
    {
        public ItemRarity itemRarity;
        public GameObject itemVFX;
        public GameObject itemVFXTrail;
    }

    [SerializeField] private RarityVFXSelector[] rarityVFXSelectors;

    private RarityVFXSelector currentRaritySelected;
    private LootItem lootItem;

    private void Awake ( )
    {
        lootItem = GetComponentInParent<LootItem>();

        if (lootItem == null) return;

        lootItem.onPicked.AddListener(OnPicked);
    }

    private void Start ( )
    {
        UpdateRarityVFX();
    }

    private void Update ( )
    {
        UpdateState();
    }

    private void UpdateRarityVFX ( )
    {
        if (lootItem == null) return;
        if (lootItem.ItemInstance == null) return;

        currentRaritySelected = rarityVFXSelectors.FirstOrDefault(rarity => rarity.itemRarity == lootItem.ItemInstance.Rarity);

        if (currentRaritySelected == null) return;

        if (currentRaritySelected.itemVFX != null) currentRaritySelected.itemVFX.SetActive(true);
        if (currentRaritySelected.itemVFXTrail != null) currentRaritySelected.itemVFXTrail.SetActive(true);
    }

    private void UpdateState ( )
    {
        if (lootItem == null) return;
        if (currentRaritySelected == null) return;
        if (lootItem.IsDropping) return;

        if (currentRaritySelected.itemVFXTrail != null) currentRaritySelected.itemVFXTrail.SetActive(false);
    }

    private void OnPicked ( )
    {
        if (currentRaritySelected == null) return;

        if (currentRaritySelected.itemVFX != null) currentRaritySelected.itemVFX.SetActive(false);
        if (currentRaritySelected.itemVFXTrail != null) currentRaritySelected.itemVFXTrail.SetActive(false);
    }
}