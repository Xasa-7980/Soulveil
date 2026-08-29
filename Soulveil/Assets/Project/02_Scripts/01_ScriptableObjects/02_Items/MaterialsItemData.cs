using UnityEngine;

public enum MaterialsType
{
    Fang,
    Bone,
    Hide,
    MagicStone,
    RarityDust,
    CorruptedEssence
}

[CreateAssetMenu(fileName = "NewMaterial", menuName = "Soulveil/Items/Material")]
public class MaterialsItemData : ItemData
{
    [SerializeField] private MaterialsType resourceType;

    public MaterialsType ResourceType => resourceType;
}