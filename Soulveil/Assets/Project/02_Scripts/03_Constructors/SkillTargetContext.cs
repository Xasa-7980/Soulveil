using UnityEngine;

public class SkillTargetContext
{
    public GameObject User { get; }
    public Camera Camera { get; }
    public LayerMask GroundLayer { get; }

    public Vector2 AimInput;
    public bool IsUsingGamepad;

    public Vector3 CastPosition;
    public Quaternion CastRotation;

    public SkillTargetContext ( GameObject user, Camera playerCamera, LayerMask groundLayer )
    {
        User = user;
        Camera = playerCamera;
        GroundLayer = groundLayer;

        CastPosition = user.transform.position;
        CastRotation = user.transform.rotation;
    }
}