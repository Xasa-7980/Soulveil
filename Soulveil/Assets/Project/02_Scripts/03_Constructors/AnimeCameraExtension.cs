using Unity.Cinemachine;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class AnimeCameraExtension : CinemachineExtension
{
    private Vector3 positionOffset;
    private Vector3 rotationOffset;

    private bool hasPoseTarget;
    private Vector3 poseTargetPosition;
    private Quaternion poseTargetRotation = Quaternion.identity;
    private float poseWeight;

    private bool hasCinematicLookAt;
    private Vector3 cinematicLookAtPosition;
    private float cinematicLookAtWeight;

    private bool hasLockOn;
    private Vector3 lockOnPosition;
    private float lockOnWeight;
    private Vector3 lockOnShoulderOffset;

    public void SetOffset ( Vector3 position, Vector3 rotationEuler )
    {
        positionOffset = position;
        rotationOffset = rotationEuler;
    }

    public void ResetOffset ( )
    {
        positionOffset = Vector3.zero;
        rotationOffset = Vector3.zero;
    }

    public void SetPoseTarget ( Vector3 worldPosition, Quaternion worldRotation, float weight )
    {
        hasPoseTarget = true;
        poseTargetPosition = worldPosition;
        poseTargetRotation = worldRotation;
        poseWeight = Mathf.Clamp01(weight);
    }

    public void SetPoseWeight ( float weight )
    {
        poseWeight = Mathf.Clamp01(weight);

        if (poseWeight <= 0.0001f)
            hasPoseTarget = false;
    }

    public void ClearPoseTarget ( )
    {
        hasPoseTarget = false;
        poseWeight = 0f;
    }

    public void SetCinematicLookAt ( Vector3 worldPosition, float weight )
    {
        hasCinematicLookAt = true;
        cinematicLookAtPosition = worldPosition;
        cinematicLookAtWeight = Mathf.Clamp01(weight);
    }

    public void SetCinematicLookAtWeight ( float weight )
    {
        cinematicLookAtWeight = Mathf.Clamp01(weight);

        if (cinematicLookAtWeight <= 0.0001f)
            hasCinematicLookAt = false;
    }

    public void ClearCinematicLookAt ( )
    {
        hasCinematicLookAt = false;
        cinematicLookAtWeight = 0f;
    }

    public void SetLockOn ( Vector3 worldPosition, float weight, Vector3 shoulderOffset )
    {
        hasLockOn = true;
        lockOnPosition = worldPosition;
        lockOnWeight = Mathf.Clamp01(weight);
        lockOnShoulderOffset = shoulderOffset;
    }

    public void ClearLockOn ( )
    {
        hasLockOn = false;
        lockOnWeight = 0f;
        lockOnShoulderOffset = Vector3.zero;
    }

    public void ResetEverything ( )
    {
        ResetOffset();
        ClearPoseTarget();
        ClearCinematicLookAt();
        ClearLockOn();
    }

    protected override void PostPipelineStageCallback (
        CinemachineVirtualCameraBase vcam,
        CinemachineCore.Stage stage,
        ref CameraState state,
        float deltaTime )
    {
        if (stage != CinemachineCore.Stage.Finalize)
            return;

        Vector3 currentPosition =
            state.RawPosition +
            state.PositionCorrection;

        Quaternion currentRotation =
            state.RawOrientation *
            state.OrientationCorrection;

        // Absolute cinematic pose.
        if (hasPoseTarget && poseWeight > 0f)
        {
            Vector3 desiredPosition = Vector3.LerpUnclamped(
                currentPosition,
                poseTargetPosition,
                poseWeight
            );

            Quaternion desiredRotation = Quaternion.SlerpUnclamped(
                currentRotation,
                poseTargetRotation,
                poseWeight
            );

            state.PositionCorrection += desiredPosition - currentPosition;
            state.OrientationCorrection =
                Quaternion.Inverse(state.RawOrientation) *
                desiredRotation;

            currentPosition = desiredPosition;
            currentRotation = desiredRotation;
        }

        // Cinematic LookAt has priority over lock-on.
        if (hasCinematicLookAt && cinematicLookAtWeight > 0f)
        {
            ApplyLookAt(
                ref state,
                ref currentRotation,
                currentPosition,
                cinematicLookAtPosition,
                cinematicLookAtWeight
            );
        }
        else if (hasLockOn && lockOnWeight > 0f)
        {
            Vector3 shoulderWorld =
                currentRotation *
                lockOnShoulderOffset;

            state.PositionCorrection += shoulderWorld;
            currentPosition += shoulderWorld;

            ApplyLookAt(
                ref state,
                ref currentRotation,
                currentPosition,
                lockOnPosition,
                lockOnWeight
            );
        }

        // Additive shake after all other camera effects.
        Vector3 worldPositionOffset =
            currentRotation *
            positionOffset;

        state.PositionCorrection += worldPositionOffset;

        state.OrientationCorrection *=
            Quaternion.Euler(rotationOffset);
    }

    private static void ApplyLookAt (
        ref CameraState state,
        ref Quaternion currentRotation,
        Vector3 currentPosition,
        Vector3 targetPosition,
        float weight )
    {
        Vector3 direction =
            targetPosition -
            currentPosition;

        if (direction.sqrMagnitude <= 0.000001f)
            return;

        Quaternion lookRotation =
            Quaternion.LookRotation(
                direction.normalized,
                Vector3.up
            );

        Quaternion desiredRotation =
            Quaternion.SlerpUnclamped(
                currentRotation,
                lookRotation,
                Mathf.Clamp01(weight)
            );

        state.OrientationCorrection =
            Quaternion.Inverse(state.RawOrientation) *
            desiredRotation;

        currentRotation = desiredRotation;
    }
}
