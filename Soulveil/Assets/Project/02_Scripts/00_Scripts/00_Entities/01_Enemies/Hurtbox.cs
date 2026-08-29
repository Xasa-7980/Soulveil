using UnityEngine;

public enum HitZone
{
    Head,
    Body,
    Legs
}

public class Hurtbox : MonoBehaviour //Se colocara en cada hurtbox del enemigo
{
    [SerializeField] private HitZone hitZone;
    public HitZone HitZone => hitZone;
}