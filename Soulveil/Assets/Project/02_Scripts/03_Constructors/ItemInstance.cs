using UnityEngine;

[System.Serializable]
public class ItemInstance
{
    [SerializeField] private ItemData itemData;
    [SerializeField] private int level = 1;

    public ItemData ItemData => itemData;
    public int Level => level;

    public ItemInstance ( ItemData itemData, int level )
    {
        this.itemData = itemData;
        this.level = level;
    }
}