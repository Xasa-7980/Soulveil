using UnityEngine;
using UnityEngine.Events;
public abstract class Element : ScriptableObject
{
    public UnityEvent OnReactsWith;
    public abstract void ReactWith ( Element otherElement, DamageInfo damageInfo );
}
