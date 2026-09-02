using UnityEngine;

public class WorldTextManager : MonoBehaviour
{
    [Header("World Text")]
    [SerializeField] private DamageNumbersPro.DamageNumber healthText;
    [SerializeField] private DamageNumbersPro.DamageNumber experienceText;
    [SerializeField] private DamageNumbersPro.DamageNumber jobExperienceText;
    [SerializeField] private DamageNumbersPro.DamageNumber damageText;
    [SerializeField] private DamageNumbersPro.DamageNumber recoverHealthText;
    [SerializeField] private DamageNumbersPro.DamageNumber recoverManaText;

    private static WorldTextManager instance;

    private void Awake ( )
    {
        instance = this;
    }

    public static void ShowDamage ( float amount, Vector3 position )
    {
        if (instance == null || instance.damageText == null) return;

        instance.damageText.Spawn(position, Mathf.RoundToInt(amount));
    }

    public static void ShowCritical ( float amount, Vector3 position )
    {
        if (instance == null || instance.damageText == null) return;

        DamageNumbersPro.DamageNumber number = instance.damageText.Spawn(position, Mathf.RoundToInt(amount));

    }

    public static void ShowHealth ( float amount, Vector3 position )
    {
        if (instance == null || instance.healthText == null) return;

        instance.healthText.Spawn(position, Mathf.RoundToInt(amount));
    }

    public static void ShowRecoverHealth ( float amount, Vector3 position )
    {
        if (instance == null || instance.recoverHealthText == null) return;

        instance.recoverHealthText.Spawn(position, Mathf.RoundToInt(amount));
    }

    public static void ShowRecoverMana ( float amount, Vector3 position )
    {
        if (instance == null || instance.recoverManaText == null) return;

        instance.recoverManaText.Spawn(position, Mathf.RoundToInt(amount));
    }

    public static void ShowExperience ( int amount, Vector3 position )
    {
        if (instance == null || instance.experienceText == null) return;

        instance.experienceText.Spawn(position, amount);
    }

    public static void ShowJobExperience ( int amount, Vector3 position )
    {
        if (instance == null || instance.jobExperienceText == null) return;

        instance.jobExperienceText.Spawn(position, amount);
    }
}