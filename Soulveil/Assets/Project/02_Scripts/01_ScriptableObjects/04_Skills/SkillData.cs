using UnityEngine;

public abstract class SkillData : ScriptableObject
{
    [Header("Info")]
    [SerializeField] private string skillName;
    [SerializeField] private Sprite icon;

    [Header("Combat")]
    [SerializeField] private Element element;
    [SerializeField] private float baseDamage;

    [Header("Cost")]
    [SerializeField] private float manaCost;
    [SerializeField] private float cooldown = 5f;

    [Header("VFX")]
    [SerializeField] private GameObject castVFX;
    [SerializeField] private GameObject hitVFX;

    public string SkillName => skillName;
    public Sprite Icon => icon;

    public Element Element => element;
    public float BaseDamage => baseDamage;

    public float ManaCost => manaCost;
    public float Cooldown => cooldown;

    public GameObject CastVFX => castVFX;
    public GameObject HitVFX => hitVFX;

    public abstract void Execute ( GameObject user, Vector3 castPosition, Quaternion castRotation );

    protected virtual void OnValidate ( )
    {
        baseDamage = Mathf.Max(0f, baseDamage);
        manaCost = Mathf.Max(0f, manaCost);
        cooldown = Mathf.Max(0f, cooldown);
    }
}