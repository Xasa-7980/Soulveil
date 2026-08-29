using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    [SerializeField] private CinemachineCamera cinemachineCamera;

    [Header("Transition")]
    [SerializeField] private float transitionDuration = 0.5f;
    [SerializeField] private AnimationCurve transitionCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Combat")]
    [SerializeField] private float combatRadius = 3.5f;
    [SerializeField] private float combatFOV = 63f;

    [Header("Umbral")]
    [SerializeField] private float umbralRadius = 2.7f;
    [SerializeField] private float umbralFOV = 64f;
    [SerializeField] private float umbralDutch = 1.5f;

    [Header("Aim")]
    [SerializeField] private float aimRadius = 0.1f;
    [SerializeField] private float aimNearClip = 1f;
    [SerializeField] private float aimTransitionDuration = 0.25f;

    [Header("Heavy Attack")]
    [SerializeField] private float heavyAttackFOVOffset = -4f;
    [SerializeField] private float heavyAttackDuration = 0.18f;

    [Header("Dodge")]
    [SerializeField] private float dodgeFOVOffset = 4f;
    [SerializeField] private float dodgeDuration = 0.2f;

    [Header("Pulse")]
    [SerializeField]
    private AnimationCurve pulseCurve = new AnimationCurve(
        new Keyframe(0f, 0f),
        new Keyframe(0.5f, 1f),
        new Keyframe(1f, 0f)
    );

    private CinemachineOrbitalFollow orbitalFollow;

    private float originalRadius;
    private float originalFOV;
    private float originalDutch;
    private float originalNearClip;

    private float currentRadius;
    private float currentFOV;
    private float currentDutch;
    private float currentNearClip;
    private float fovPulse;

    private bool inCombat;
    private bool inUmbral;
    private bool isAiming;

    private Coroutine stateTransition;
    private Coroutine pulseTransition;

    private void Awake ( )
    {
        orbitalFollow = cinemachineCamera.GetComponent<CinemachineOrbitalFollow>();

        originalRadius = orbitalFollow.Radius;
        originalFOV = cinemachineCamera.Lens.FieldOfView;
        originalDutch = cinemachineCamera.Lens.Dutch;
        originalNearClip = cinemachineCamera.Lens.NearClipPlane;

        currentRadius = originalRadius;
        currentFOV = originalFOV;
        currentDutch = originalDutch;
        currentNearClip = originalNearClip;
    }

    public void EnterCombatCamera ( )
    {
        inCombat = true;
        RefreshCameraState();
    }

    public void ExitCombatCamera ( )
    {
        inCombat = false;
        RefreshCameraState();
    }

    public void EnterUmbralCamera ( )
    {
        inUmbral = true;
        RefreshCameraState();
    }

    public void ExitUmbralCamera ( )
    {
        inUmbral = false;
        RefreshCameraState();
    }

    public void OnAim ( InputAction.CallbackContext context )
    {
        if (context.started)
            EnterAimCamera();

        if (context.canceled)
            ExitAimCamera();
    }

    public void EnterAimCamera ( )
    {
        isAiming = true;
        RefreshCameraState(aimTransitionDuration);
    }

    public void ExitAimCamera ( )
    {
        isAiming = false;
        RefreshCameraState(aimTransitionDuration);
    }

    public void PlayHeavyAttackCamera ( )
    {
        PlayFOVPulse(heavyAttackFOVOffset, heavyAttackDuration);
    }

    public void PlayDodgeCamera ( )
    {
        PlayFOVPulse(dodgeFOVOffset, dodgeDuration);
    }

    private void RefreshCameraState ( float duration = -1f )
    {
        float radius = originalRadius;
        float fov = originalFOV;
        float dutch = originalDutch;
        float nearClip = originalNearClip;

        if (inCombat)
        {
            radius = combatRadius;
            fov = combatFOV;
        }

        if (inUmbral)
        {
            radius = umbralRadius;
            fov = umbralFOV;
            dutch = umbralDutch;
        }

        if (isAiming)
        {
            radius = aimRadius;
            nearClip = aimNearClip;
        }

        StartCameraTransition(radius, fov, dutch, nearClip, duration < 0f ? transitionDuration : duration);
    }

    private void StartCameraTransition ( float radius, float fov, float dutch, float nearClip, float duration )
    {
        if (stateTransition != null)
            StopCoroutine(stateTransition);

        stateTransition = StartCoroutine(AnimateCameraState(radius, fov, dutch, nearClip, duration));
    }

    private IEnumerator AnimateCameraState ( float targetRadius, float targetFOV, float targetDutch, float targetNearClip, float duration )
    {
        float startRadius = currentRadius;
        float startFOV = currentFOV;
        float startDutch = currentDutch;
        float startNearClip = currentNearClip;
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;

            float t = Mathf.Clamp01(time / duration);
            float curveValue = transitionCurve.Evaluate(t);

            currentRadius = Mathf.Lerp(startRadius, targetRadius, curveValue);
            currentFOV = Mathf.Lerp(startFOV, targetFOV, curveValue);
            currentDutch = Mathf.Lerp(startDutch, targetDutch, curveValue);
            currentNearClip = Mathf.Lerp(startNearClip, targetNearClip, curveValue);

            ApplyCamera();
            yield return null;
        }

        currentRadius = targetRadius;
        currentFOV = targetFOV;
        currentDutch = targetDutch;
        currentNearClip = targetNearClip;

        ApplyCamera();
        stateTransition = null;
    }

    private void PlayFOVPulse ( float offset, float duration )
    {
        if (pulseTransition != null)
            StopCoroutine(pulseTransition);

        fovPulse = 0f;
        pulseTransition = StartCoroutine(AnimateFOVPulse(offset, duration));
    }

    private IEnumerator AnimateFOVPulse ( float offset, float duration )
    {
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;

            float t = Mathf.Clamp01(time / duration);
            fovPulse = offset * pulseCurve.Evaluate(t);

            ApplyCamera();
            yield return null;
        }

        fovPulse = 0f;
        ApplyCamera();

        pulseTransition = null;
    }

    private void ApplyCamera ( )
    {
        orbitalFollow.Radius = currentRadius;
        cinemachineCamera.Lens.FieldOfView = currentFOV + fovPulse;
        cinemachineCamera.Lens.Dutch = currentDutch;
        cinemachineCamera.Lens.NearClipPlane = currentNearClip;
    }

#if UNITY_EDITOR
    private void Update ( )
    {
        if (Keyboard.current.f1Key.wasPressedThisFrame)
        {
            Debug.Log("Esto es una prueba de: Entrar en combate");
            EnterCombatCamera();
        }

        if (Keyboard.current.f2Key.wasPressedThisFrame)
        {
            Debug.Log("Esto es una prueba de: Salir de combate");
            ExitCombatCamera();
        }

        if (Keyboard.current.f3Key.wasPressedThisFrame)
        {
            Debug.Log("Esto es una prueba de: Entrar al Umbral");
            EnterUmbralCamera();
        }

        if (Keyboard.current.f4Key.wasPressedThisFrame)
        {
            Debug.Log("Esto es una prueba de: Salir del Umbral");
            ExitUmbralCamera();
        }

        if (Keyboard.current.f5Key.wasPressedThisFrame)
        {
            Debug.Log("Esto es una prueba de: Ataque fuerte");
            PlayHeavyAttackCamera();
        }

        if (Keyboard.current.f6Key.wasPressedThisFrame)
        {
            Debug.Log("Esto es una prueba de: Dodge");
            PlayDodgeCamera();
        }

        if (Keyboard.current.f7Key.wasPressedThisFrame)
        {
            Debug.Log("Esto es una prueba de: Entrar en primera persona");
            EnterAimCamera();
        }

        if (Keyboard.current.f8Key.wasPressedThisFrame)
        {
            Debug.Log("Esto es una prueba de: Salir de primera persona");
            ExitAimCamera();
        }
    }
#endif
}