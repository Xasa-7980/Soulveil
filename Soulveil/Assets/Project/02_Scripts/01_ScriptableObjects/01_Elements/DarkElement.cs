using UnityEngine;
[CreateAssetMenu(menuName = "Soulveil/Elements/Dark CurrentElement")]

public class DarkElement : Element
{
    public override void ReactWith ( Element otherElement, DamageInfo damageInfo )
    {
        if (otherElement is LightElement)
        {
            Debug.Log("Dark + Light reaction");
        }
    }
}
