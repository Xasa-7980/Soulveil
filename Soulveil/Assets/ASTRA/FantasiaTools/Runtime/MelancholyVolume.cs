using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
namespace FantasiaTools
{
 [Serializable,VolumeComponentMenu("Fantasia/Melancholy Grade")]
 public sealed class MelancholyVolume : VolumeComponent,IPostProcessComponent
 {
  public ClampedFloatParameter intensity=new ClampedFloatParameter(0,0,1);
  public ClampedFloatParameter saturation=new ClampedFloatParameter(.42f,0,1.5f);
  public ClampedFloatParameter exposure=new ClampedFloatParameter(-.3f,-3,2);
  public ClampedFloatParameter contrast=new ClampedFloatParameter(.92f,.2f,2);
  public ColorParameter shadowTint=new ColorParameter(new Color(.38f,.46f,.56f),false,false,true);
  public ColorParameter highlightTint=new ColorParameter(new Color(.79f,.74f,.64f),false,false,true);
  public ClampedFloatParameter splitStrength=new ClampedFloatParameter(.28f,0,1);
  public ClampedFloatParameter blackLift=new ClampedFloatParameter(.018f,0,.15f);
  public ClampedFloatParameter vignette=new ClampedFloatParameter(.22f,0,1);
  public ClampedFloatParameter grain=new ClampedFloatParameter(.012f,0,.08f);
  public BoolParameter animateGrain=new BoolParameter(false);
  public bool IsActive()=>active && intensity.value>.0001f;
  public bool IsTileCompatible()=>false;
 }
}
