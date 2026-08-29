using UnityEngine;

[CreateAssetMenu(menuName = "Soulveil/Elements/Water CurrentElement")]
public class WaterElement : Element
{
    public override void ReactWith ( Element otherElement, DamageInfo damageInfo )
    {
        if (otherElement is FireElement)
        {
            Debug.Log("Water + Fire  reaction");
        }

        if (otherElement is ElectroElement)
        {
            Debug.Log("Water + Electro reaction");
        }
    }
}
