using UnityEngine;

[CreateAssetMenu( menuName = "Soulveil/Elements/Fire CurrentElement")]
public class FireElement : Element
{
    public override void ReactWith ( Element otherElement, DamageInfo damageInfo )
    {
        if (otherElement is WaterElement)
        {
            Debug.Log("Fire + Water reaction");
        }

        if (otherElement is ElectroElement)
        {
            Debug.Log("Fire + Electro reaction");
        }
    }
}