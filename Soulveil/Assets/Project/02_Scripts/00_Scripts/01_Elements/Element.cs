using UnityEngine;
public abstract class Element : ScriptableObject
{
    public abstract void ReactWith ( Element otherElement, DamageInfo damageInfo );
}
