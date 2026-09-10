using UnityEngine;
[CreateAssetMenu(menuName = "Soulveil/Elements/Light CurrentElement")]

public class LightElement : Element
{
    public override void ReactWith ( Element otherElement, DamageInfo damageInfo )
    {
        if (otherElement is DarkElement)
        {
            Debug.Log("Light + Dark reaction");
        }
    }
}
