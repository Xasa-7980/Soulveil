using UnityEngine;
[CreateAssetMenu(menuName = "Soulveil/Elements/Geo Element")]

public class GeoElement : Element
{
    public override void ReactWith ( Element otherElement, DamageInfo damageInfo )
    {
        if (otherElement is ElectroElement)
        {
            Debug.Log("Geo + Electro reaction");
        }
    }
}
