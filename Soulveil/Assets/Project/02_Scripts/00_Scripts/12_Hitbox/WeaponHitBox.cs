using UnityEngine;

public class WeaponHitBox : MonoBehaviour
{
    [Header("Hitbox Type")]
    [Tooltip("True = Sphere | False = Box")]
    [SerializeField] private bool useSphere = true;

    [Header("Transform Local")]
    [SerializeField] private Vector3 localPosition;
    [SerializeField] private Vector3 localRotation;

    [Header("Sphere")]
    [Min(0f)]
    [SerializeField] private float radius = 0.5f;

    [Header("Box")]
    [SerializeField] private Vector3 boxSize = Vector3.one;

    [Header("Detection")]
    [SerializeField] private LayerMask hitLayers;
    [SerializeField]
    /*
     
    Si lo colocamos en Collide colisionara con objetos triggers y no triggers
    useGlobal usara la configuracion por defecto
     
     */
    private QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Collide;

    [Header("Debug")]
    [SerializeField] private bool drawHitbox = true;

    /// <summary>
    /// Centro de la hitbox en World Space.
    /// localPosition funciona como una posición local respecto al arma.
    /// </summary>
    public Vector3 WorldPosition
    {
        get
        {
            return transform.TransformPoint(localPosition);
        }
    }

    /// <summary>
    /// Rotación de la hitbox relativa a la rotación del arma.
    /// </summary>
    public Quaternion WorldRotation
    {
        get
        {
            return transform.rotation * Quaternion.Euler(localRotation);
        }
    }

    /// <summary>
    /// Ejecuta el Overlap correspondiente y devuelve
    /// todos los Colliders encontrados.
    /// </summary>
    public Collider[] CheckHitbox ( )
    {
        if (useSphere)
        {
            return Physics.OverlapSphere(
                WorldPosition,
                radius,
                hitLayers,
                triggerInteraction
            );
        }

        return Physics.OverlapBox(
            WorldPosition,
            boxSize * 0.5f,
            WorldRotation,
            hitLayers,
            triggerInteraction
        );
    }

    /// <summary>
    /// Permite saber qué tipo de hitbox estamos usando.
    /// </summary>
    public bool IsSphere ( )
    {
        return useSphere;
    }

#if UNITY_EDITOR

    private void OnDrawGizmosSelected ( )
    {
        if (!drawHitbox)
            return;

        Vector3 position = WorldPosition;
        Quaternion rotation = WorldRotation;

        Gizmos.matrix = Matrix4x4.TRS(
            position,
            rotation,
            Vector3.one
        );

        if (useSphere)
        {
            Gizmos.DrawWireSphere(
                Vector3.zero,
                radius
            );
        }
        else
        {
            Gizmos.DrawWireCube(
                Vector3.zero,
                boxSize
            );
        }

        Gizmos.matrix = Matrix4x4.identity;
    }

#endif
}