using UnityEngine;

public static class SkillTargetingUtility
{
    public static bool TryGetCameraGroundPoint ( SkillTargetContext context, float maxDistance, out Vector3 point )
    {
        point = Vector3.zero;

        if (context == null) return false;
        if (context.Camera == null) return false;
        if (context.User == null) return false;

        Vector3 screenCenter = new Vector3(
            context.Camera.pixelWidth * 0.5f,
            context.Camera.pixelHeight * 0.5f,
            0f
        );

        Ray cameraRay = context.Camera.ScreenPointToRay(screenCenter);

        Plane targetingPlane = new Plane(Vector3.up, context.User.transform.position);

        if (targetingPlane.Raycast(cameraRay, out float enter))
        {
            point = cameraRay.GetPoint(enter);
        }
        else
        {
            Vector3 forward = context.Camera.transform.forward;
            forward.y = 0f;

            if (forward.sqrMagnitude <= 0.01f) forward = context.User.transform.forward;

            forward.Normalize();

            point = context.User.transform.position + forward * maxDistance;
        }

        point = ClampDistance(context.User.transform.position, point, maxDistance);
        point = SnapToGround(context, point);

        return true;
    }

    public static Vector3 GetCameraForwardDirection ( SkillTargetContext context )
    {
        if (context == null) return Vector3.zero;
        if (context.Camera == null) return Vector3.zero;

        Vector3 direction = context.Camera.transform.forward;
        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.01f) return Vector3.zero;

        return direction.normalized;
    }

    public static Vector3 ClampDistance ( Vector3 origin, Vector3 target, float maxDistance )
    {
        Vector3 offset = target - origin;
        offset.y = 0f;

        if (offset.magnitude <= maxDistance) return target;

        offset = offset.normalized * maxDistance;

        Vector3 result = origin + offset;
        result.y = target.y;

        return result;
    }

    public static Vector3 SnapToGround ( SkillTargetContext context, Vector3 position )
    {
        if (context == null) return position;

        Vector3 rayOrigin = position + Vector3.up * 20f;

        if (!Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, 40f, context.GroundLayer))
        {
            return position;
        }

        return hit.point;
    }
}