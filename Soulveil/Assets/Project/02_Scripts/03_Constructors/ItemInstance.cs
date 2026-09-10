using UnityEngine;

[System.Serializable]
public class ItemInstance
{
    [SerializeField] private ItemData itemData;
    [SerializeField] private int level = 1;
    [SerializeField] private ItemRarity rarity;

    public ItemData ItemData => itemData;
    public int Level => level;
    public ItemRarity Rarity => rarity;

    public ItemInstance ( ItemData itemData, int level, ItemRarity rarity )
    {
        this.itemData = itemData;
        this.level = level;
        this.rarity = rarity;
    }
}