using UnityEngine;

[CreateAssetMenu( fileName = "ElementVFXDatabase", menuName = "Soulveil/VFX/Element VFX Database" ) ]
public class ElementVFXDatabase : ScriptableObject
{
    [SerializeField] private ElementVFXProfile[] profiles;

    public ElementVFXProfile GetProfile ( Element element )
    {
        if (element == null) return null;

        foreach (ElementVFXProfile profile in profiles)
        {
            if (profile == null) continue;
            if (profile.Element == element) return profile;
        }

        return null;
    }
}