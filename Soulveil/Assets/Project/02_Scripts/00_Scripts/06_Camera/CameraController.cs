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
    private float originalFarClip;

    private float currentRadius;
    private float currentFOV;
    private float currentDutch;
    private float fovPulse;

    private bool inCombat;
    private bool inUmbral;

    private Coroutine stateTransition;
    private Coroutine pulseTransition;
    private Coroutine clipTransition;

    private void Awake ( )
    {
        orbitalFollow = cinemachineCamera.GetComponent<CinemachineOrbitalFollow>();

        originalRadius = orbitalFollow.Radius;
        originalFOV = cinemachineCamera.Lens.FieldOfView;
        originalDutch = cinemachineCamera.Lens.Dutch;
        originalNearClip = cinemachineCamera.Lens.NearClipPlane;
        originalFarClip = cinemachineCamera.Lens.FarClipPlane;

        currentRadius = originalRadius;
        currentFOV = originalFOV;
        currentDutch = originalDutch;
    }

    //Testeo
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
            Debug.Log("Esto es una prueba de: Dodge + entrar al Umbral simultáneamente");
            PlayDodgeCamera();
            EnterUmbralCamera();
        }
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

    public void PlayHeavyAttackCamera ( )
    {
        PlayFOVPulse(heavyAttackFOVOffset, heavyAttackDuration);
    }

    public void PlayDodgeCamera ( )
    {
        PlayFOVPulse(dodgeFOVOffset, dodgeDuration);
    }

    private void RefreshCameraState ( )
    {
        if (inUmbral)
        {
            StartCameraTransition(umbralRadius, umbralFOV, umbralDutch);
            return;
        }

        if (inCombat)
        {
            StartCameraTransition(combatRadius, combatFOV, originalDutch);
            return;
        }

        StartCameraTransition(originalRadius, originalFOV, originalDutch);
    }

    private void StartCameraTransition ( float radius, float fov, float dutch )
    {
        if (stateTransition != null) StopCoroutine(stateTransition);
        stateTransition = StartCoroutine(AnimateCameraState(radius, fov, dutch));
    }

    private IEnumerator AnimateCameraState ( float targetRadius, float targetFOV, float targetDutch )
    {
        float startRadius = currentRadius;
        float startFOV = currentFOV;
        float startDutch = currentDutch;
        float time = 0f;

        while (time < transitionDuration)
        {
            time += Time.deltaTime;

            float t = Mathf.Clamp01(time / transitionDuration);
            float curveValue = transitionCurve.Evaluate(t);

            currentRadius = Mathf.Lerp(startRadius, targetRadius, curveValue);
            currentFOV = Mathf.Lerp(startFOV, targetFOV, curveValue);
            currentDutch = Mathf.Lerp(startDutch, targetDutch, curveValue);

            ApplyCamera();
            yield return null;
        }

        currentRadius = targetRadius;
        currentFOV = targetFOV;
        currentDutch = targetDutch;

        ApplyCamera();
        stateTransition = null;
    }

    private void PlayFOVPulse ( float offset, float duration )
    {
        if (pulseTransition != null) StopCoroutine(pulseTransition);

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
    }

    public void SetClipDistance ( float nearClip, float farClip = 1000f )
    {
        if (clipTransition != null) StopCoroutine(clipTransition);
        clipTransition = StartCoroutine(AnimateClipDistance(nearClip, farClip));
    }

    public void SetOriginalClipDistance ( )
    {
        SetClipDistance(originalNearClip, originalFarClip);
    }

    private IEnumerator AnimateClipDistance ( float targetNear, float targetFar )
    {
        float startNear = cinemachineCamera.Lens.NearClipPlane;
        float startFar = cinemachineCamera.Lens.FarClipPlane;
        float time = 0f;

        while (time < transitionDuration)
        {
            time += Time.deltaTime;

            float t = Mathf.Clamp01(time / transitionDuration);
            float curveValue = transitionCurve.Evaluate(t);

            cinemachineCamera.Lens.NearClipPlane = Mathf.Lerp(startNear, targetNear, curveValue);
            cinemachineCamera.Lens.FarClipPlane = Mathf.Lerp(startFar, targetFar, curveValue);

            yield return null;
        }

        cinemachineCamera.Lens.NearClipPlane = targetNear;
        cinemachineCamera.Lens.FarClipPlane = targetFar;

        clipTransition = null;
    }
}