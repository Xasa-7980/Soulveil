using UnityEngine;

public class CombatVFXManager : MonoBehaviour
{
    [SerializeField] private ElementVFXDatabase elementVFXDatabase;

    private static CombatVFXManager instance;

    private void Awake ( )
    {
        instance = this;
    }

    public static void ShowHit ( Element element, Vector3 position )
    {
        if (instance == null) return;
        if (element == null) return;

        ElementVFXProfile profile = instance.elementVFXDatabase.GetProfile(element);

        if (profile == null) return;
        if (profile.HitVFX == null) return;

        Instantiate(profile.HitVFX, position, Quaternion.identity);
    }
}