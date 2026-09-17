using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
namespace HarmonieFX
{
 [Serializable, VolumeComponentMenu("Harmonie/Light - Glow - Atmosphere")]
 [SupportedOnRenderPipeline(typeof(UniversalRenderPipelineAsset))]
 public sealed class HarmonieVolume : VolumeComponent, IPostProcessComponent
 {
  [Header("Master")]
  public ClampedFloatParameter intensity=new ClampedFloatParameter(0,0,1);
  [Tooltip("2 = half width and height; 4 = quarter. Larger means cheaper.")]
  public ClampedIntParameter downsample=new ClampedIntParameter(4,2,4);
  [Header("1. Sun shafts")]
  public ClampedFloatParameter shafts=new ClampedFloatParameter(.45f,0,2);
  public ColorParameter shaftColor=new ColorParameter(new Color(1,.82f,.55f,1),false,false,true);
  public ClampedIntParameter shaftSamples=new ClampedIntParameter(16,8,32);
  public ClampedFloatParameter shaftReach=new ClampedFloatParameter(.8f,.1f,1.5f);
  public ClampedFloatParameter shaftRadius=new ClampedFloatParameter(.35f,.05f,1);
  [Header("2. Soft HDR glow")]
  public ClampedFloatParameter glow=new ClampedFloatParameter(.22f,0,2);
  public MinFloatParameter glowThreshold=new MinFloatParameter(1.1f,0);
  public MinFloatParameter glowClamp=new MinFloatParameter(8,1);
  public ClampedFloatParameter glowRadius=new ClampedFloatParameter(1.6f,.5f,4);
  public ColorParameter glowTint=new ColorParameter(Color.white,false,false,true);
  [Header("3. Aerial perspective")]
  public ClampedFloatParameter atmosphere=new ClampedFloatParameter(.3f,0,1);
  public ColorParameter nearHazeColor=new ColorParameter(new Color(.18f,.28f,.30f,1),false,false,true);
  public ColorParameter farHazeColor=new ColorParameter(new Color(.52f,.62f,.66f,1),false,false,true);
  public MinFloatParameter hazeStart=new MinFloatParameter(18,0);
  public MinFloatParameter hazeEnd=new MinFloatParameter(110,.1f);
  public FloatParameter seaLevel=new FloatParameter(0);
  public MinFloatParameter heightFalloff=new MinFloatParameter(.025f,0);
  public ClampedFloatParameter distantDesaturation=new ClampedFloatParameter(.18f,0,1);
  public bool IsActive()=>active && intensity.value>.0001f && (shafts.value+glow.value+atmosphere.value)>.0001f;
  public bool IsTileCompatible()=>false;
 }
}
