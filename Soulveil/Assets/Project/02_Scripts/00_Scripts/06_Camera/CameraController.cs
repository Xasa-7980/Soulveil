using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
public sealed class CameraController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CinemachineCamera cinemachineCamera;
    [SerializeField] private Camera renderedCamera;
    [SerializeField] private Transform proximityOrigin;
    [SerializeField] private Transform wallCheckOrigin;

    [Header("General")]
    [SerializeField] private bool useUnscaledTime = false;

    [Header("State Transition")]
    [Min(0.01f)][SerializeField] private float transitionDuration = 0.5f;
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
    [SerializeField] private float aimNearClip = 0.1f;
    [Min(0.01f)][SerializeField] private float aimTransitionDuration = 0.25f;

    [Header("Heavy Attack")]
    [SerializeField] private float heavyAttackFOVOffset = -6f;
    [SerializeField] private float heavyAttackRadiusOffset = -0.35f;
    [SerializeField] private float heavyAttackRoll = 2f;
    [Min(0.01f)][SerializeField] private float heavyAttackDuration = 0.35f;
    [SerializeField]
    private AnimationCurve heavyAttackCurve = new AnimationCurve(
        new Keyframe(0f, 0f),
        new Keyframe(0.10f, 0.60f),
        new Keyframe(0.24f, 0.40f),
        new Keyframe(0.45f, 1f),
        new Keyframe(1f, 0f)
    );

    [Header("Dodge")]
    [SerializeField] private float dodgeFOVOffset = 4f;
    [SerializeField] private float dodgeRadiusOffset = 0.2f;
    [Min(0.01f)][SerializeField] private float dodgeDuration = 0.2f;
    [SerializeField]
    private AnimationCurve dodgeCurve = new AnimationCurve(
        new Keyframe(0f, 0f),
        new Keyframe(0.5f, 1f),
        new Keyframe(1f, 0f)
    );

    [Header("Perfect Dodge")]
    [SerializeField] private float perfectDodgeFOVOffset = -7f;
    [SerializeField] private float perfectDodgeRadiusOffset = -0.25f;
    [SerializeField] private float perfectDodgeRoll = 3f;
    [Min(0.01f)][SerializeField] private float perfectDodgeDuration = 0.35f;
    [SerializeField] private bool perfectDodgeShake = true;
    [SerializeField]
    private AnimationCurve perfectDodgeCurve = new AnimationCurve(
        new Keyframe(0f, 0f),
        new Keyframe(0.12f, 1f),
        new Keyframe(0.38f, 0.75f),
        new Keyframe(1f, 0f)
    );

    [Header("Impact Shake")]
    [Min(0.01f)][SerializeField] private float shakeDuration = 0.3f;
    [Min(0f)][SerializeField] private float shakePosition = 0.05f;
    [Min(0f)][SerializeField] private float shakeRotation = 1.5f;
    [Min(0f)][SerializeField] private float shakeFrequency = 22f;
    [SerializeField] private AnimationCurve shakeEnvelope = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);

    [Header("Anime Move")]
    [Tooltip("La cámara copia la posición y rotación mundial de este Transform.")]
    [SerializeField] private Transform animeTarget;
    [Min(0.01f)][SerializeField] private float animeMoveDuration = 0.6f;
    [SerializeField] private AnimationCurve animeMoveCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    [Min(0f)][SerializeField] private float animeHoldDuration = 0.15f;
    [SerializeField] private bool animeReturnAfterMove = true;
    [Min(0.01f)][SerializeField] private float animeReturnDuration = 0.5f;
    [SerializeField] private AnimationCurve animeReturnCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Look At Effect")]
    [SerializeField] private Transform lookAtTarget;
    [Min(0.01f)][SerializeField] private float lookAtDuration = 1f;
    [SerializeField]
    private AnimationCurve lookAtCurve = new AnimationCurve(
        new Keyframe(0f, 0f),
        new Keyframe(0.2f, 1f),
        new Keyframe(0.75f, 1f),
        new Keyframe(1f, 0f)
    );

    [Header("Dynamic Lock-On")]
    [SerializeField] private Transform lockOnTarget;
    [SerializeField] private Transform targetSwitchTestTarget;
    [SerializeField] private bool lockOnEnabled;
    [Range(0f, 1f)][SerializeField] private float lockOnNearWeight = 0.45f;
    [Range(0f, 1f)][SerializeField] private float lockOnFarWeight = 0.20f;
    [Min(0.1f)][SerializeField] private float lockOnNearDistance = 2f;
    [Min(0.1f)][SerializeField] private float lockOnFarDistance = 12f;
    [SerializeField] private Vector3 lockOnShoulderOffset = new Vector3(0.25f, 0.08f, 0f);
    [Min(0.01f)][SerializeField] private float lockOnBlendDuration = 0.25f;
    [SerializeField] private AnimationCurve lockOnBlendCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Target Switch")]
    [Min(0.01f)][SerializeField] private float targetSwitchDuration = 0.22f;
    [SerializeField] private AnimationCurve targetSwitchCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    [SerializeField] private float targetSwitchFOVKick = -2f;

    [Header("Finisher Camera")]
    [Tooltip("Punto exacto de posición/rotación para el finisher.")]
    [SerializeField] private Transform finisherCameraPoint;
    [Tooltip("Opcional. Si existe, durante el finisher la cámara mirará a este objetivo.")]
    [SerializeField] private Transform finisherLookTarget;
    [Min(0.01f)][SerializeField] private float finisherMoveDuration = 0.35f;
    [Min(0f)][SerializeField] private float finisherHoldDuration = 0.45f;
    [Min(0.01f)][SerializeField] private float finisherReturnDuration = 0.45f;
    [SerializeField] private float finisherFOVOffset = -5f;
    [SerializeField] private bool finisherImpactShake = true;
    [SerializeField] private AnimationCurve finisherCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Boss Introduction")]
    [Tooltip("Punto exacto desde el que se presenta el boss.")]
    [SerializeField] private Transform bossCameraPoint;
    [Tooltip("Boss o punto al que debe mirar la cámara.")]
    [SerializeField] private Transform bossLookTarget;
    [Min(0.01f)][SerializeField] private float bossIntroMoveDuration = 0.8f;
    [Min(0f)][SerializeField] private float bossIntroHoldDuration = 1.5f;
    [Min(0.01f)][SerializeField] private float bossIntroReturnDuration = 0.8f;
    [SerializeField] private float bossIntroFOVOffset = -3f;
    [SerializeField] private AnimationCurve bossIntroCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Low Health Camera")]
    [Range(0.01f, 1f)][SerializeField] private float lowHealthThreshold = 0.3f;
    [SerializeField] private float lowHealthFOVOffset = 2f;
    [SerializeField] private float lowHealthDutchAmount = 0.8f;
    [Min(0f)][SerializeField] private float lowHealthPulseFrequency = 1.4f;
    [Range(0f, 1f)][SerializeField] private float testHealthNormalized = 1f;

    [Header("Heavy Enemy Proximity")]
    [SerializeField] private Transform heavyEnemy;
    [Min(0.1f)][SerializeField] private float heavyEnemyNearDistance = 2f;
    [Min(0.1f)][SerializeField] private float heavyEnemyFarDistance = 7f;
    [SerializeField] private float heavyEnemyFOVIncrease = 6f;
    [SerializeField] private float heavyEnemyRadiusReduction = 0.45f;
    [SerializeField] private float heavyEnemyDutch = 0.4f;

    [Header("Wall / Corridor Compression")]
    [SerializeField] private bool wallCompressionEnabled = true;
    [SerializeField] private LayerMask wallMask = ~0;
    [Min(0.01f)][SerializeField] private float wallSphereRadius = 0.18f;
    [Min(0.1f)][SerializeField] private float corridorSideCheckDistance = 1.4f;
    [Min(0.1f)][SerializeField] private float wallBackCheckDistance = 4f;
    [SerializeField] private float wallRadiusReduction = 0.8f;
    [SerializeField] private float wallFOVIncrease = 7f;
    [Min(0f)][SerializeField] private float wallEffectSmooth = 8f;

    private CinemachineOrbitalFollow orbitalFollow;
    private AnimeCameraExtension animeExtension;

    private float originalRadius;
    private float originalFOV;
    private float originalDutch;
    private float originalNearClip;

    private float currentRadius;
    private float currentFOV;
    private float currentDutch;
    private float currentNearClip;

    private float impactRadius;
    private float impactFOV;
    private float impactDutch;

    private float cinematicFOV;

    private float lowHealthFOV;
    private float lowHealthDutch;
    private float healthNormalized = 1f;

    private float proximityRadius;
    private float proximityFOV;
    private float proximityDutch;

    private float wallRadius;
    private float wallFOV;
    private float wallAmount;

    private Vector3 currentShakePosition;
    private Vector3 currentShakeRotation;

    private bool inCombat;
    private bool inUmbral;
    private bool isAiming;
    private bool targetSwitching;

    private float lockOnBlend;
    private float lockOnBlendVelocity;

    private Coroutine stateCoroutine;
    private Coroutine impactCoroutine;
    private Coroutine shakeCoroutine;
    private Coroutine cinematicCoroutine;
    private Coroutine lookAtCoroutine;
    private Coroutine targetSwitchCoroutine;

    private float DeltaTime => useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
    private float CurrentTime => useUnscaledTime ? Time.unscaledTime : Time.time;

    private void Awake ( )
    {
        if (cinemachineCamera == null)
            cinemachineCamera = GetComponent<CinemachineCamera>();

        if (cinemachineCamera == null)
        {
            Debug.LogError("CameraController necesita CinemachineCamera.", this);
            enabled = false;
            return;
        }

        orbitalFollow = cinemachineCamera.GetComponent<CinemachineOrbitalFollow>();

        if (orbitalFollow == null)
        {
            Debug.LogError("La CinemachineCamera necesita CinemachineOrbitalFollow.", this);
            enabled = false;
            return;
        }

        animeExtension = cinemachineCamera.GetComponent<AnimeCameraExtension>();
        if (animeExtension == null)
            animeExtension = cinemachineCamera.gameObject.AddComponent<AnimeCameraExtension>();

        if (renderedCamera == null)
            renderedCamera = Camera.main;

        if (proximityOrigin == null)
            proximityOrigin = transform;

        if (wallCheckOrigin == null)
            wallCheckOrigin = proximityOrigin;

        CacheOriginalValues();
        animeExtension.ResetEverything();
        ApplyCamera();
    }

    private void Update ( )
    {
        UpdateLockOn();
        UpdateLowHealthEffect();
        UpdateHeavyEnemyProximity();
        UpdateWallCompression();
        ApplyCamera();
    }

    private void CacheOriginalValues ( )
    {
        originalRadius = orbitalFollow.Radius;
        originalFOV = cinemachineCamera.Lens.FieldOfView;
        originalDutch = cinemachineCamera.Lens.Dutch;
        originalNearClip = cinemachineCamera.Lens.NearClipPlane;

        currentRadius = originalRadius;
        currentFOV = originalFOV;
        currentDutch = originalDutch;
        currentNearClip = originalNearClip;
    }

    // ============================================================
    // BASE STATES
    // ============================================================

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
        if (context.started) EnterAimCamera();
        if (context.canceled) ExitAimCamera();
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

    private void RefreshCameraState ( float duration = -1f )
    {
        float targetRadius = originalRadius;
        float targetFOV = originalFOV;
        float targetDutch = originalDutch;
        float targetNearClip = originalNearClip;

        if (inCombat)
        {
            targetRadius = combatRadius;
            targetFOV = combatFOV;
        }

        if (inUmbral)
        {
            targetRadius = umbralRadius;
            targetFOV = umbralFOV;
            targetDutch = umbralDutch;
        }

        if (isAiming)
        {
            targetRadius = aimRadius;
            targetNearClip = aimNearClip;
        }

        StartStateTransition(
            targetRadius,
            targetFOV,
            targetDutch,
            targetNearClip,
            duration >= 0f ? duration : transitionDuration
        );
    }

    private void StartStateTransition ( float radius, float fov, float dutch, float nearClip, float duration )
    {
        StopRoutine(ref stateCoroutine);
        stateCoroutine = StartCoroutine(AnimateCameraState(radius, fov, dutch, nearClip, duration));
    }

    private IEnumerator AnimateCameraState (
        float targetRadius,
        float targetFOV,
        float targetDutch,
        float targetNearClip,
        float duration )
    {
        float startRadius = currentRadius;
        float startFOV = currentFOV;
        float startDutch = currentDutch;
        float startNearClip = currentNearClip;

        duration = Mathf.Max(0.01f, duration);
        float time = 0f;

        while (time < duration)
        {
            time += DeltaTime;
            float t = Mathf.Clamp01(time / duration);
            float c = transitionCurve.Evaluate(t);

            currentRadius = Mathf.LerpUnclamped(startRadius, targetRadius, c);
            currentFOV = Mathf.LerpUnclamped(startFOV, targetFOV, c);
            currentDutch = Mathf.LerpUnclamped(startDutch, targetDutch, c);
            currentNearClip = Mathf.LerpUnclamped(startNearClip, targetNearClip, c);

            yield return null;
        }

        currentRadius = targetRadius;
        currentFOV = targetFOV;
        currentDutch = targetDutch;
        currentNearClip = targetNearClip;
        stateCoroutine = null;
    }

    // ============================================================
    // IMPACTS
    // ============================================================

    [ContextMenu("TEST / Heavy Attack")]
    public void PlayHeavyAttackCamera ( )
    {
        PlayImpact(
            heavyAttackFOVOffset,
            heavyAttackRadiusOffset,
            heavyAttackRoll,
            heavyAttackDuration,
            heavyAttackCurve
        );
        PlayShake();
    }

    [ContextMenu("TEST / Dodge")]
    public void PlayDodgeCamera ( )
    {
        PlayImpact(
            dodgeFOVOffset,
            dodgeRadiusOffset,
            0f,
            dodgeDuration,
            dodgeCurve
        );
    }

    [ContextMenu("TEST / Perfect Dodge")]
    public void PlayPerfectDodgeCamera ( )
    {
        PlayImpact(
            perfectDodgeFOVOffset,
            perfectDodgeRadiusOffset,
            perfectDodgeRoll,
            perfectDodgeDuration,
            perfectDodgeCurve
        );

        if (perfectDodgeShake)
            PlayShake();
    }

    private void PlayImpact ( float fovOffset, float radiusOffset, float dutchOffset, float duration, AnimationCurve curve )
    {
        StopRoutine(ref impactCoroutine);
        impactFOV = 0f;
        impactRadius = 0f;
        impactDutch = 0f;

        impactCoroutine = StartCoroutine(
            AnimateImpact(fovOffset, radiusOffset, dutchOffset, duration, curve)
        );
    }

    private IEnumerator AnimateImpact (
        float fovOffset,
        float radiusOffset,
        float dutchOffset,
        float duration,
        AnimationCurve curve )
    {
        duration = Mathf.Max(0.01f, duration);
        float time = 0f;

        while (time < duration)
        {
            time += DeltaTime;
            float t = Mathf.Clamp01(time / duration);
            float amount = curve.Evaluate(t);

            impactFOV = fovOffset * amount;
            impactRadius = radiusOffset * amount;
            impactDutch = dutchOffset * amount;

            yield return null;
        }

        impactFOV = 0f;
        impactRadius = 0f;
        impactDutch = 0f;
        impactCoroutine = null;
    }

    // ============================================================
    // SHAKE
    // ============================================================

    [ContextMenu("TEST / Shake")]
    public void PlayShake ( )
    {
        StopRoutine(ref shakeCoroutine);
        currentShakePosition = Vector3.zero;
        currentShakeRotation = Vector3.zero;
        ApplyShake();

        shakeCoroutine = StartCoroutine(AnimateShake());
    }

    private IEnumerator AnimateShake ( )
    {
        float duration = Mathf.Max(0.01f, shakeDuration);
        float time = 0f;

        float seedX = Random.Range(0f, 1000f);
        float seedY = Random.Range(0f, 1000f);
        float seedZ = Random.Range(0f, 1000f);

        while (time < duration)
        {
            time += DeltaTime;

            float t = Mathf.Clamp01(time / duration);
            float envelope = shakeEnvelope.Evaluate(t);
            float noiseTime = time * shakeFrequency;

            float x = Mathf.PerlinNoise(seedX, noiseTime) * 2f - 1f;
            float y = Mathf.PerlinNoise(seedY, noiseTime) * 2f - 1f;
            float z = Mathf.PerlinNoise(seedZ, noiseTime) * 2f - 1f;

            currentShakePosition = new Vector3(x, y, z * 0.3f) * shakePosition * envelope;
            currentShakeRotation = new Vector3(
                y * shakeRotation,
                x * shakeRotation,
                z * shakeRotation * 0.5f
            ) * envelope;

            ApplyShake();
            yield return null;
        }

        currentShakePosition = Vector3.zero;
        currentShakeRotation = Vector3.zero;
        ApplyShake();
        shakeCoroutine = null;
    }

    private void ApplyShake ( )
    {
        if (animeExtension != null)
            animeExtension.SetOffset(currentShakePosition, currentShakeRotation);
    }

    // ============================================================
    // ANIME MOVE / GENERIC POSE
    // ============================================================

    [ContextMenu("TEST / Anime Move")]
    public void PlayAnimeMove ( )
    {
        if (animeTarget == null)
        {
            Debug.LogWarning("Anime Target no está asignado.", this);
            return;
        }

        StartPoseCinematic(
            animeTarget,
            null,
            animeMoveDuration,
            animeHoldDuration,
            animeReturnDuration,
            animeMoveCurve,
            animeReturnCurve,
            0f,
            animeReturnAfterMove
        );
    }

    public void ReturnAnimeMoveToRest ( )
    {
        StartReturnFromPose(animeReturnDuration, animeReturnCurve);
    }

    public void ReturnPivotToRest ( ) => ReturnAnimeMoveToRest();

    // ============================================================
    // LOOK AT EFFECT
    // ============================================================

    [ContextMenu("TEST / Look At")]
    public void PlayLookAt ( )
    {
        if (lookAtTarget == null)
        {
            Debug.LogWarning("Look At Target no está asignado.", this);
            return;
        }

        StopRoutine(ref lookAtCoroutine);
        lookAtCoroutine = StartCoroutine(AnimateStandaloneLookAt());
    }

    private IEnumerator AnimateStandaloneLookAt ( )
    {
        float duration = Mathf.Max(0.01f, lookAtDuration);
        float time = 0f;

        while (time < duration)
        {
            time += DeltaTime;
            float t = Mathf.Clamp01(time / duration);
            float weight = Mathf.Clamp01(lookAtCurve.Evaluate(t));

            if (lookAtTarget != null)
                animeExtension.SetCinematicLookAt(lookAtTarget.position, weight);

            yield return null;
        }

        animeExtension.ClearCinematicLookAt();
        lookAtCoroutine = null;
    }

    public void StopLookAt ( )
    {
        StopRoutine(ref lookAtCoroutine);
        if (animeExtension != null)
            animeExtension.ClearCinematicLookAt();
    }

    // ============================================================
    // DYNAMIC LOCK-ON
    // ============================================================

    public void EnableLockOn ( )
    {
        if (lockOnTarget == null)
        {
            Debug.LogWarning("Lock-On Target no está asignado.", this);
            return;
        }

        lockOnEnabled = true;
    }

    public void SetLockOnTarget ( Transform target )
    {
        lockOnTarget = target;
        lockOnEnabled = target != null;
    }

    public void DisableLockOn ( )
    {
        lockOnEnabled = false;
    }

    public Transform GetLockOnTarget ( ) => lockOnTarget;

    private void UpdateLockOn ( )
    {
        float desiredBlend = lockOnEnabled && lockOnTarget != null ? 1f : 0f;

        lockOnBlend = Mathf.SmoothDamp(
            lockOnBlend,
            desiredBlend,
            ref lockOnBlendVelocity,
            Mathf.Max(0.01f, lockOnBlendDuration),
            Mathf.Infinity,
            Mathf.Max(0.0001f, DeltaTime)
        );

        if (targetSwitching)
            return;

        if (lockOnTarget != null && lockOnBlend > 0.001f)
        {
            float distance = proximityOrigin != null
                ? Vector3.Distance(proximityOrigin.position, lockOnTarget.position)
                : Vector3.Distance(transform.position, lockOnTarget.position);

            float distance01 = Mathf.InverseLerp(lockOnNearDistance, lockOnFarDistance, distance);
            float dynamicWeight = Mathf.Lerp(lockOnNearWeight, lockOnFarWeight, distance01);

            animeExtension.SetLockOn(
                lockOnTarget.position,
                dynamicWeight * lockOnBlend,
                lockOnShoulderOffset * lockOnBlend
            );
        }
        else
        {
            animeExtension.ClearLockOn();
        }
    }

    public void SwitchLockOnTarget ( Transform newTarget )
    {
        if (newTarget == null)
            return;

        StopRoutine(ref targetSwitchCoroutine);
        targetSwitchCoroutine = StartCoroutine(AnimateTargetSwitch(newTarget));
    }

    [ContextMenu("TEST / Target Switch")]
    public void TestTargetSwitch ( )
    {
        if (targetSwitchTestTarget == null)
        {
            Debug.LogWarning("Target Switch Test Target no está asignado.", this);
            return;
        }

        SwitchLockOnTarget(targetSwitchTestTarget);
    }

    private IEnumerator AnimateTargetSwitch ( Transform newTarget )
    {
        targetSwitching = true;

        Vector3 startPosition = lockOnTarget != null
            ? lockOnTarget.position
            : newTarget.position;

        float duration = Mathf.Max(0.01f, targetSwitchDuration);
        float time = 0f;

        while (time < duration)
        {
            time += DeltaTime;
            float t = Mathf.Clamp01(time / duration);
            float c = targetSwitchCurve.Evaluate(t);

            if (newTarget == null)
                break;

            Vector3 blendedTarget = Vector3.Lerp(startPosition, newTarget.position, c);

            float kick = Mathf.Sin(t * Mathf.PI) * targetSwitchFOVKick;
            impactFOV = kick;

            animeExtension.SetLockOn(
                blendedTarget,
                Mathf.Lerp(lockOnNearWeight, 1f, Mathf.Sin(t * Mathf.PI) * 0.25f),
                lockOnShoulderOffset
            );

            yield return null;
        }

        impactFOV = 0f;
        lockOnTarget = newTarget;
        lockOnEnabled = newTarget != null;
        targetSwitching = false;
        targetSwitchCoroutine = null;
    }

    // ============================================================
    // FINISHER
    // ============================================================

    [ContextMenu("TEST / Finisher Camera")]
    public void PlayFinisherCamera ( )
    {
        if (finisherCameraPoint == null)
        {
            Debug.LogWarning("Finisher Camera Point no está asignado.", this);
            return;
        }

        StartPoseCinematic(
            finisherCameraPoint,
            finisherLookTarget,
            finisherMoveDuration,
            finisherHoldDuration,
            finisherReturnDuration,
            finisherCurve,
            finisherCurve,
            finisherFOVOffset,
            true
        );
    }

    public void PlayFinisherCamera ( Transform cameraPoint, Transform lookTarget = null )
    {
        if (cameraPoint == null)
            return;

        finisherCameraPoint = cameraPoint;
        finisherLookTarget = lookTarget;
        PlayFinisherCamera();
    }

    // ============================================================
    // BOSS INTRO
    // ============================================================

    [ContextMenu("TEST / Boss Introduction")]
    public void PlayBossIntroduction ( )
    {
        if (bossCameraPoint == null)
        {
            Debug.LogWarning("Boss Camera Point no está asignado.", this);
            return;
        }

        StartPoseCinematic(
            bossCameraPoint,
            bossLookTarget,
            bossIntroMoveDuration,
            bossIntroHoldDuration,
            bossIntroReturnDuration,
            bossIntroCurve,
            bossIntroCurve,
            bossIntroFOVOffset,
            true
        );
    }

    public void PlayBossIntroduction ( Transform cameraPoint, Transform boss )
    {
        if (cameraPoint == null)
            return;

        bossCameraPoint = cameraPoint;
        bossLookTarget = boss;
        PlayBossIntroduction();
    }

    // ============================================================
    // GENERIC CINEMATIC POSE SYSTEM
    // ============================================================

    private void StartPoseCinematic (
        Transform cameraPoint,
        Transform cinematicLookTarget,
        float moveDuration,
        float holdDuration,
        float returnDuration,
        AnimationCurve moveCurve,
        AnimationCurve returnCurve,
        float fovOffset,
        bool returnAfter )
    {
        if (cameraPoint == null || animeExtension == null)
            return;

        StopRoutine(ref cinematicCoroutine);
        StopLookAt();

        cinematicCoroutine = StartCoroutine(
            AnimatePoseCinematic(
                cameraPoint,
                cinematicLookTarget,
                moveDuration,
                holdDuration,
                returnDuration,
                moveCurve,
                returnCurve,
                fovOffset,
                returnAfter
            )
        );
    }

    private IEnumerator AnimatePoseCinematic (
        Transform cameraPoint,
        Transform cinematicLookTarget,
        float moveDuration,
        float holdDuration,
        float returnDuration,
        AnimationCurve moveCurve,
        AnimationCurve returnCurve,
        float fovOffset,
        bool returnAfter )
    {
        moveDuration = Mathf.Max(0.01f, moveDuration);
        float time = 0f;

        while (time < moveDuration)
        {
            time += DeltaTime;
            float t = Mathf.Clamp01(time / moveDuration);
            float weight = Mathf.Clamp01(moveCurve.Evaluate(t));

            if (cameraPoint != null)
                animeExtension.SetPoseTarget(cameraPoint.position, cameraPoint.rotation, weight);

            if (cinematicLookTarget != null)
                animeExtension.SetCinematicLookAt(cinematicLookTarget.position, weight);

            cinematicFOV = fovOffset * weight;
            yield return null;
        }

        if (cameraPoint != null)
            animeExtension.SetPoseTarget(cameraPoint.position, cameraPoint.rotation, 1f);

        if (cinematicLookTarget != null)
            animeExtension.SetCinematicLookAt(cinematicLookTarget.position, 1f);

        cinematicFOV = fovOffset;

        if (cameraPoint == finisherCameraPoint && finisherImpactShake)
            PlayShake();

        float hold = 0f;
        while (hold < holdDuration)
        {
            hold += DeltaTime;

            if (cameraPoint != null)
                animeExtension.SetPoseTarget(cameraPoint.position, cameraPoint.rotation, 1f);

            if (cinematicLookTarget != null)
                animeExtension.SetCinematicLookAt(cinematicLookTarget.position, 1f);

            yield return null;
        }

        if (returnAfter)
        {
            yield return AnimatePoseReturn(returnDuration, returnCurve, fovOffset, cinematicLookTarget != null);
        }

        cinematicCoroutine = null;
    }

    private IEnumerator AnimatePoseReturn (
        float duration,
        AnimationCurve curve,
        float startFOVOffset,
        bool clearLookAt )
    {
        duration = Mathf.Max(0.01f, duration);
        float time = 0f;

        while (time < duration)
        {
            time += DeltaTime;
            float t = Mathf.Clamp01(time / duration);
            float returnValue = Mathf.Clamp01(curve.Evaluate(t));
            float weight = 1f - returnValue;

            animeExtension.SetPoseWeight(weight);

            if (clearLookAt)
                animeExtension.SetCinematicLookAtWeight(weight);

            cinematicFOV = startFOVOffset * weight;

            yield return null;
        }

        animeExtension.ClearPoseTarget();
        animeExtension.ClearCinematicLookAt();
        cinematicFOV = 0f;
    }

    private void StartReturnFromPose ( float duration, AnimationCurve curve )
    {
        StopRoutine(ref cinematicCoroutine);
        cinematicCoroutine = StartCoroutine(ReturnPoseRoutine(duration, curve));
    }

    private IEnumerator ReturnPoseRoutine ( float duration, AnimationCurve curve )
    {
        yield return AnimatePoseReturn(duration, curve, cinematicFOV, true);
        cinematicCoroutine = null;
    }

    // ============================================================
    // LOW HEALTH
    // ============================================================

    public void SetHealthNormalized ( float normalizedHealth )
    {
        healthNormalized = Mathf.Clamp01(normalizedHealth);
    }

    public void SetHealth ( float currentHealth, float maxHealth )
    {
        healthNormalized = maxHealth <= 0f
            ? 0f
            : Mathf.Clamp01(currentHealth / maxHealth);
    }

    [ContextMenu("TEST / Apply Test Health")]
    public void ApplyTestHealth ( )
    {
        SetHealthNormalized(testHealthNormalized);
    }

    private void UpdateLowHealthEffect ( )
    {
        if (healthNormalized >= lowHealthThreshold)
        {
            lowHealthFOV = Mathf.MoveTowards(lowHealthFOV, 0f, DeltaTime * 8f);
            lowHealthDutch = Mathf.MoveTowards(lowHealthDutch, 0f, DeltaTime * 8f);
            return;
        }

        float severity = 1f - Mathf.InverseLerp(0f, lowHealthThreshold, healthNormalized);
        float pulse = (Mathf.Sin(CurrentTime * Mathf.PI * 2f * lowHealthPulseFrequency) + 1f) * 0.5f;
        float pulseAmount = Mathf.Lerp(0.35f, 1f, pulse);

        lowHealthFOV = lowHealthFOVOffset * severity * pulseAmount;
        lowHealthDutch = lowHealthDutchAmount * severity * pulseAmount;
    }

    // ============================================================
    // HEAVY ENEMY PROXIMITY
    // ============================================================

    public void SetHeavyEnemy ( Transform enemy )
    {
        heavyEnemy = enemy;
    }

    public void ClearHeavyEnemy ( )
    {
        heavyEnemy = null;
    }

    private void UpdateHeavyEnemyProximity ( )
    {
        if (heavyEnemy == null || proximityOrigin == null)
        {
            proximityRadius = Mathf.MoveTowards(proximityRadius, 0f, DeltaTime * 5f);
            proximityFOV = Mathf.MoveTowards(proximityFOV, 0f, DeltaTime * 10f);
            proximityDutch = Mathf.MoveTowards(proximityDutch, 0f, DeltaTime * 5f);
            return;
        }

        float distance = Vector3.Distance(proximityOrigin.position, heavyEnemy.position);

        float amount = 1f - Mathf.InverseLerp(
            heavyEnemyNearDistance,
            heavyEnemyFarDistance,
            distance
        );

        proximityRadius = -heavyEnemyRadiusReduction * amount;
        proximityFOV = heavyEnemyFOVIncrease * amount;
        proximityDutch = heavyEnemyDutch * amount;
    }

    // ============================================================
    // WALL / CORRIDOR COMPRESSION
    // ============================================================

    public void SetWallCompressionEnabled ( bool enabledState )
    {
        wallCompressionEnabled = enabledState;
    }

    private void UpdateWallCompression ( )
    {
        if (!wallCompressionEnabled || wallCheckOrigin == null || renderedCamera == null)
        {
            SmoothWallAmount(0f);
            return;
        }

        Vector3 origin = wallCheckOrigin.position;
        Vector3 cameraRight = renderedCamera.transform.right;
        Vector3 toCamera = renderedCamera.transform.position - origin;

        float sideAmount = 0f;
        bool leftHit = Physics.Raycast(
            origin,
            -cameraRight,
            out RaycastHit left,
            corridorSideCheckDistance,
            wallMask,
            QueryTriggerInteraction.Ignore
        );

        bool rightHit = Physics.Raycast(
            origin,
            cameraRight,
            out RaycastHit right,
            corridorSideCheckDistance,
            wallMask,
            QueryTriggerInteraction.Ignore
        );

        if (leftHit && rightHit)
        {
            float nearestSide = Mathf.Min(left.distance, right.distance);
            sideAmount = 1f - Mathf.Clamp01(nearestSide / corridorSideCheckDistance);
        }

        float backAmount = 0f;

        if (toCamera.sqrMagnitude > 0.0001f)
        {
            float checkDistance = Mathf.Min(wallBackCheckDistance, toCamera.magnitude);

            if (Physics.SphereCast(
                    origin,
                    wallSphereRadius,
                    toCamera.normalized,
                    out RaycastHit backHit,
                    checkDistance,
                    wallMask,
                    QueryTriggerInteraction.Ignore))
            {
                backAmount = 1f - Mathf.Clamp01(backHit.distance / checkDistance);
            }
        }

        SmoothWallAmount(Mathf.Max(sideAmount, backAmount));
    }

    private void SmoothWallAmount ( float target )
    {
        wallAmount = Mathf.MoveTowards(
            wallAmount,
            Mathf.Clamp01(target),
            Mathf.Max(0f, wallEffectSmooth) * DeltaTime
        );

        wallRadius = -wallRadiusReduction * wallAmount;
        wallFOV = wallFOVIncrease * wallAmount;
    }

    // ============================================================
    // APPLY
    // ============================================================

    private void ApplyCamera ( )
    {
        if (cinemachineCamera == null || orbitalFollow == null)
            return;

        float finalRadius =
            currentRadius +
            impactRadius +
            proximityRadius +
            wallRadius;

        float finalFOV =
            currentFOV +
            impactFOV +
            cinematicFOV +
            lowHealthFOV +
            proximityFOV +
            wallFOV;

        float finalDutch =
            currentDutch +
            impactDutch +
            lowHealthDutch +
            proximityDutch;

        orbitalFollow.Radius = Mathf.Max(0.01f, finalRadius);
        cinemachineCamera.Lens.FieldOfView = Mathf.Clamp(finalFOV, 1f, 179f);
        cinemachineCamera.Lens.Dutch = finalDutch;
        cinemachineCamera.Lens.NearClipPlane = Mathf.Max(0.001f, currentNearClip);
    }

    // ============================================================
    // RESET
    // ============================================================

    [ContextMenu("RESET CAMERA")]
    public void ResetCamera ( )
    {
        StopRoutine(ref stateCoroutine);
        StopRoutine(ref impactCoroutine);
        StopRoutine(ref shakeCoroutine);
        StopRoutine(ref cinematicCoroutine);
        StopRoutine(ref lookAtCoroutine);
        StopRoutine(ref targetSwitchCoroutine);

        inCombat = false;
        inUmbral = false;
        isAiming = false;
        targetSwitching = false;
        lockOnEnabled = false;
        lockOnBlend = 0f;
        lockOnBlendVelocity = 0f;

        currentRadius = originalRadius;
        currentFOV = originalFOV;
        currentDutch = originalDutch;
        currentNearClip = originalNearClip;

        impactRadius = 0f;
        impactFOV = 0f;
        impactDutch = 0f;
        cinematicFOV = 0f;

        lowHealthFOV = 0f;
        lowHealthDutch = 0f;
        healthNormalized = 1f;

        proximityRadius = 0f;
        proximityFOV = 0f;
        proximityDutch = 0f;

        wallRadius = 0f;
        wallFOV = 0f;
        wallAmount = 0f;

        currentShakePosition = Vector3.zero;
        currentShakeRotation = Vector3.zero;

        if (animeExtension != null)
            animeExtension.ResetEverything();

        ApplyCamera();
    }

    private void StopRoutine ( ref Coroutine routine )
    {
        if (routine == null)
            return;

        StopCoroutine(routine);
        routine = null;
    }

    private void OnDisable ( )
    {
        if (animeExtension != null)
            animeExtension.ResetEverything();
    }
}

