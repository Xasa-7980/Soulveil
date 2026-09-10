using UnityEngine;

public class SkillContext
{
    public GameObject User { get; }
    public Camera Camera { get; }

    public Vector2 AimInput;
    public Vector3 CastPosition;
    public Quaternion CastRotation;

    public SkillContext ( GameObject user, Camera camera )
    {
        User = user;
        Camera = camera;

        CastPosition = user.transform.position;
        CastRotation = user.transform.rotation;
    }
}