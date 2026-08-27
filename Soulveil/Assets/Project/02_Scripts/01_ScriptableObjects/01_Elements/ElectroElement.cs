using UnityEngine;
[CreateAssetMenu(menuName = "Soulveil/Elements/Electro Element")]

public class ElectroElement : Element
{
    public override void ReactWith ( Element otherElement, DamageInfo damageInfo )
    {
        if (otherElement is WaterElement)
        {
            Debug.Log("Electro + Water reaction");
        }

        if (otherElement is FireElement)
        {
            Debug.Log("Electro + Fire reaction");
        }
    }
}
