using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace ElementalVillages
{
    [Serializable, VolumeComponentMenu("Elemental Villages/Global Cel and Outlines")]
    [SupportedOnRenderPipeline(typeof(UniversalRenderPipelineAsset))]
    public sealed class ElementalCelVolume : VolumeComponent, IPostProcessComponent
    {
        [Tooltip("Zero disables the effect. Override this field in your Volume.")]
        public ClampedFloatParameter intensity = new ClampedFloatParameter(0f, 0f, 1f);
        public ClampedIntParameter bands = new ClampedIntParameter(5, 2, 16);
        public ClampedFloatParameter whitePoint = new ClampedFloatParameter(1.5f, .1f, 8f);
        public ClampedFloatParameter preserveColor = new ClampedFloatParameter(1f, 0f, 1f);
        public ClampedFloatParameter outlineStrength = new ClampedFloatParameter(.85f, 0f, 1f);
        public ClampedFloatParameter outlineWidth = new ClampedFloatParameter(1.25f, .5f, 5f);
        public ColorParameter outlineColor = new ColorParameter(new Color(.055f,.045f,.085f,1f), false, false, true);
        public ClampedFloatParameter depthThreshold = new ClampedFloatParameter(.018f, .001f, .2f);
        public ClampedFloatParameter normalThreshold = new ClampedFloatParameter(.35f, .01f, 2f);
        public ClampedFloatParameter normalEdges = new ClampedFloatParameter(.75f, 0f, 1f);
        public BoolParameter affectSky = new BoolParameter(false);
        public bool IsActive() => active && intensity.value > .0001f;
        public bool IsTileCompatible() => false;
    }
}
