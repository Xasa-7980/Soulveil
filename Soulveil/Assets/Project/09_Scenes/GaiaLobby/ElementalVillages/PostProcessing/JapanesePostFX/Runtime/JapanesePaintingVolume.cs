using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace JapanesePostFX
{
 [Serializable, VolumeComponentMenu("Japanese Post FX/Japanese Painting")]
 [SupportedOnRenderPipeline(typeof(UniversalRenderPipelineAsset))]
 public sealed class JapanesePaintingVolume : VolumeComponent, IPostProcessComponent
 {
  [Tooltip("Override this value above zero to enable the effect.")]
  public ClampedFloatParameter intensity = new ClampedFloatParameter(0.0f,0.0f,1.0f);
  public ClampedFloatParameter bands = new ClampedFloatParameter(9.0f,2.0f,16.0f);
  public ClampedFloatParameter whitePoint = new ClampedFloatParameter(1.5f,0.1f,8.0f);
  public ClampedFloatParameter saturation = new ClampedFloatParameter(0.72f,0.0f,2.0f);
  public ClampedFloatParameter inkStrength = new ClampedFloatParameter(0.4f,0.0f,1.0f);
  public ClampedFloatParameter inkWidth = new ClampedFloatParameter(1.1f,0.5f,6.0f);
  public ClampedFloatParameter depthThreshold = new ClampedFloatParameter(0.018f,0.001f,0.2f);
  public ClampedFloatParameter normalThreshold = new ClampedFloatParameter(0.4f,0.01f,2.0f);
  public ClampedFloatParameter normalEdges = new ClampedFloatParameter(0.3f,0.0f,1.0f);
  public ClampedFloatParameter colorEdges = new ClampedFloatParameter(0.15f,0.0f,1.0f);
  public ClampedFloatParameter paperStrength = new ClampedFloatParameter(0.65f,0.0f,1.0f);
  public ClampedFloatParameter paperScale = new ClampedFloatParameter(1.0f,0.25f,4.0f);
  public ClampedFloatParameter wobble = new ClampedFloatParameter(0.35f,0.0f,3.0f);
  public ClampedFloatParameter pigment = new ClampedFloatParameter(0.65f,0.0f,1.0f);
  public ClampedFloatParameter monochrome = new ClampedFloatParameter(0.0f,0.0f,1.0f);
  public ClampedFloatParameter washRadius = new ClampedFloatParameter(3.0f,0.0f,6.0f);
  public ClampedFloatParameter washStrength = new ClampedFloatParameter(0.75f,0.0f,1.0f);
  public ClampedFloatParameter bleed = new ClampedFloatParameter(0.4f,0.0f,1.0f);
  public ColorParameter paperColor = new ColorParameter(new Color(0.92f,0.84f,0.66f,1.0f),false,false,true);
  public ColorParameter inkColor = new ColorParameter(new Color(0.015f,0.012f,0.018f,1.0f),false,false,true);
  public bool IsActive() => active && intensity.value > 0.0001f;
  public bool IsTileCompatible() => false;
 }
}
