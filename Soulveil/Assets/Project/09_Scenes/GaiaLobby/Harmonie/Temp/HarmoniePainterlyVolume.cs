using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace HarmonieFX
{
    [Serializable]
    [VolumeComponentMenu("Harmonie FX/Painterly")]
    public sealed class HarmoniePainterlyVolume :
        VolumeComponent,
        IPostProcessComponent
    {
        public BoolParameter enabledEffect =
            new BoolParameter(false);

        [Header("Painterly")]
        public ClampedFloatParameter intensity =
            new ClampedFloatParameter(
                0.90f,
                0f,
                1f
            );

        public ClampedFloatParameter brushBlend =
            new ClampedFloatParameter(
                0.65f,
                0f,
                1f
            );

        public ClampedFloatParameter blurRadius =
            new ClampedFloatParameter(
                1.35f,
                0f,
                4f
            );

        public ClampedIntParameter colorSteps =
            new ClampedIntParameter(
                6,
                2,
                16
            );

        [Header("Outline")]
        public ClampedFloatParameter outlineStrength =
            new ClampedFloatParameter(
                0.30f,
                0f,
                2f
            );

        public ClampedFloatParameter outlineThreshold =
            new ClampedFloatParameter(
                0.06f,
                0.001f,
                1f
            );

        [Header("Color")]
        public ClampedFloatParameter saturation =
            new ClampedFloatParameter(
                1.15f,
                0f,
                2f
            );

        public ClampedFloatParameter contrast =
            new ClampedFloatParameter(
                1.05f,
                0f,
                2f
            );

        public ClampedFloatParameter warmth =
            new ClampedFloatParameter(
                0.12f,
                -1f,
                1f
            );

        public ClampedFloatParameter glow =
            new ClampedFloatParameter(
                0.18f,
                0f,
                1f
            );

        [Header("Paper / Pigment")]
        public ClampedFloatParameter paperGrain =
            new ClampedFloatParameter(
                0.08f,
                0f,
                1f
            );

        [Header("Living Paint")]
        public ClampedFloatParameter motionStrength =
            new ClampedFloatParameter(
                0.30f,
                0f,
                2f
            );

        public ClampedFloatParameter motionSpeed =
            new ClampedFloatParameter(
                0.70f,
                0f,
                5f
            );

        public bool IsActive ( )
        {
            return
                active &&
                enabledEffect.value &&
                intensity.value > 0.0001f;
        }

        public bool IsTileCompatible ( )
        {
            return false;
        }
    }
}
