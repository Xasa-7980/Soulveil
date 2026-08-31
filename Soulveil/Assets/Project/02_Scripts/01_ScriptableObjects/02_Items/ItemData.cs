using UnityEngine;
public enum ItemRarity
{
    Common,
    Uncommon,
    Rare,
    Epic,
    Legendary,
    Mythic
}

public abstract class ItemData : ScriptableObject
{
    [Header("Info")]
    [SerializeField] private string itemName;

    [TextArea]
    [SerializeField] private string description;

    [SerializeField] private Sprite icon;
    [SerializeField] private ItemRarity rarity;

    [Header("Inventory")]
    [SerializeField] private bool stackable;
    [Min(1)]
    [SerializeField] private int maxStack = 1;

    [Header("World")]
    [SerializeField] private GameObject worldPrefab;

    [Header("Economy")]
    [SerializeField] private int value;

    public string ItemName => itemName;
    public string Description => description;
    public Sprite Icon => icon;
    public ItemRarity Rarity => rarity;

    public bool Stackable => stackable;
    public int MaxStack => stackable ? maxStack : 1;

    public GameObject WorldPrefab => worldPrefab;
    public int Value => value;
}