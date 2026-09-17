using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
namespace AtmosferaAnime
{
 [Serializable, VolumeComponentMenu("Atmosfera Anime/Estados del personaje")]
 [SupportedOnRenderPipeline(typeof(UniversalRenderPipelineAsset))]
 public sealed class StatusScreenVolume : VolumeComponent, IPostProcessComponent
 {
  public ClampedFloatParameter intensity=new ClampedFloatParameter(1,0,1);
  public ClampedFloatParameter poison=new ClampedFloatParameter(0,0,1);
  public ClampedFloatParameter fire=new ClampedFloatParameter(0,0,1);
  public ClampedFloatParameter darkness=new ClampedFloatParameter(0,0,1);
  public ClampedFloatParameter buff=new ClampedFloatParameter(0,0,1);
  public ClampedFloatParameter wet=new ClampedFloatParameter(0,0,1);
  public ClampedFloatParameter underwater=new ClampedFloatParameter(0,0,1);
  public ClampedFloatParameter electric=new ClampedFloatParameter(0,0,1);
  public ClampedFloatParameter frost=new ClampedFloatParameter(0,0,1);
  public ClampedFloatParameter bleeding=new ClampedFloatParameter(0,0,1);
  public ColorParameter poisonColor=new ColorParameter(new Color(.25f,.4f,.035f,1),false,false,true);
  public ColorParameter fireColor=new ColorParameter(new Color(1,.15f,.015f,1),false,false,true);
  public ColorParameter darknessColor=new ColorParameter(new Color(.035f,.01f,.075f,1),false,false,true);
  public ColorParameter buffColor=new ColorParameter(new Color(.9f,.65f,.15f,1),false,false,true);
  public ColorParameter waterColor=new ColorParameter(new Color(.035f,.28f,.4f,1),false,false,true);
  public ColorParameter electricColor=new ColorParameter(new Color(.35f,.6f,1,1),false,false,true);
  public ColorParameter frostColor=new ColorParameter(new Color(.5f,.7f,.8f,1),false,false,true);
  public ColorParameter bloodColor=new ColorParameter(new Color(.35f,.015f,.025f,1),false,false,true);
  public ClampedFloatParameter edgeWidth=new ClampedFloatParameter(.3f,.05f,.8f);
  public ClampedFloatParameter distortionPixels=new ClampedFloatParameter(2,0,12);
  public ClampedFloatParameter animationSpeed=new ClampedFloatParameter(1,0,3);
  public ClampedFloatParameter pulseAmount=new ClampedFloatParameter(.25f,0,1);
  [Header("Screen layers")]
  public ClampedFloatParameter proceduralStrength=new ClampedFloatParameter(1.0f,0.0f,1.0f);
  public ClampedFloatParameter poisonOverlay=new ClampedFloatParameter(1.0f,0.0f,1.0f);
  public ClampedFloatParameter fireOverlay=new ClampedFloatParameter(1.0f,0.0f,1.0f);
  public ClampedFloatParameter darknessOverlay=new ClampedFloatParameter(1.0f,0.0f,1.0f);
  public ClampedFloatParameter buffOverlay=new ClampedFloatParameter(1.0f,0.0f,1.0f);
  public ClampedFloatParameter wetOverlay=new ClampedFloatParameter(1.0f,0.0f,1.0f);
  public ClampedFloatParameter underwaterOverlay=new ClampedFloatParameter(1.0f,0.0f,1.0f);
  public ClampedFloatParameter electricOverlay=new ClampedFloatParameter(1.0f,0.0f,1.0f);
  public ClampedFloatParameter frostOverlay=new ClampedFloatParameter(1.0f,0.0f,1.0f);
  public ClampedFloatParameter bleedingOverlay=new ClampedFloatParameter(1.0f,0.0f,1.0f);
  [Header("Poison")]
  public ClampedFloatParameter poisonNoiseScale=new ClampedFloatParameter(13.0f,1.0f,60.0f);
  public ClampedFloatParameter poisonMotion=new ClampedFloatParameter(0.25f,0.0f,4.0f);
  [Header("Fire")]
  public ClampedFloatParameter fireHeight=new ClampedFloatParameter(0.25f,0.02f,1.0f);
  public ClampedFloatParameter fireTurbulence=new ClampedFloatParameter(28.0f,1.0f,80.0f);
  public ClampedFloatParameter fireSpeed=new ClampedFloatParameter(1.4f,0.0f,8.0f);
  [Header("Darkness")]
  public ClampedFloatParameter darknessNoiseScale=new ClampedFloatParameter(13.0f,1.0f,60.0f);
  public ClampedFloatParameter darknessCoverage=new ClampedFloatParameter(0.78f,0.0f,1.0f);
  public ClampedFloatParameter darknessSpeed=new ClampedFloatParameter(0.25f,0.0f,4.0f);
  [Header("Buff / player aura")]
  public BoolParameter buffAnchorToPlayer=new BoolParameter(true);
  public BoolParameter buffFitPlayer=new BoolParameter(true);
  public ClampedFloatParameter buffSizeMultiplier=new ClampedFloatParameter(1.2f,0.2f,3.0f);
  public ClampedFloatParameter buffRadius=new ClampedFloatParameter(0.16f,0.02f,0.6f);
  public ClampedFloatParameter buffThickness=new ClampedFloatParameter(3.0f,0.5f,16.0f);
  public ClampedFloatParameter buffOrbitSpeed=new ClampedFloatParameter(1.0f,0.0f,6.0f);
  public ClampedFloatParameter buffMoteDensity=new ClampedFloatParameter(24.0f,4.0f,64.0f);
  public ClampedFloatParameter buffMoteSize=new ClampedFloatParameter(3.0f,1.0f,12.0f);
  public ClampedFloatParameter buffGlow=new ClampedFloatParameter(0.5f,0.0f,2.0f);
  [Header("Water")]
  public ClampedFloatParameter wetDensity=new ClampedFloatParameter(32.0f,4.0f,80.0f);
  public ClampedFloatParameter wetSpeed=new ClampedFloatParameter(0.7f,0.0f,4.0f);
  public ClampedFloatParameter wetSize=new ClampedFloatParameter(5.0f,1.0f,16.0f);
  public ClampedFloatParameter underwaterFrequency=new ClampedFloatParameter(35.0f,1.0f,80.0f);
  public ClampedFloatParameter underwaterSpeed=new ClampedFloatParameter(2.0f,0.0f,6.0f);
  [Header("Electricity / noise")]
  public ClampedIntParameter electricBoltCount=new ClampedIntParameter(3,1,6);
  public ClampedFloatParameter electricNoiseFrequency=new ClampedFloatParameter(9.0f,1.0f,40.0f);
  public ClampedFloatParameter electricNoiseAmplitude=new ClampedFloatParameter(0.055f,0.0f,0.2f);
  public ClampedFloatParameter electricNoiseSpeed=new ClampedFloatParameter(8.0f,0.0f,30.0f);
  public ClampedFloatParameter electricWidth=new ClampedFloatParameter(1.5f,0.5f,8.0f);
  public ClampedFloatParameter electricBranchStrength=new ClampedFloatParameter(0.45f,0.0f,1.0f);
  public ClampedFloatParameter electricFlicker=new ClampedFloatParameter(0.25f,0.0f,1.0f);
  [Header("Frost / snow")]
  public ClampedFloatParameter frostCrystalScale=new ClampedFloatParameter(110.0f,20.0f,240.0f);
  public ClampedFloatParameter snowAmount=new ClampedFloatParameter(0.4f,0.0f,1.0f);
  public ClampedFloatParameter snowDensity=new ClampedFloatParameter(12.0f,4.0f,32.0f);
  public ClampedFloatParameter snowSize=new ClampedFloatParameter(3.0f,1.0f,12.0f);
  public ClampedFloatParameter snowSpeed=new ClampedFloatParameter(0.25f,0.0f,2.0f);
  public ClampedFloatParameter snowDrift=new ClampedFloatParameter(0.4f,0.0f,2.0f);
  [Header("Bleeding")]
  public ClampedFloatParameter bleedNoiseScale=new ClampedFloatParameter(18.0f,2.0f,60.0f);
  public ClampedFloatParameter bleedCoverage=new ClampedFloatParameter(0.65f,0.0f,1.0f);
  [Header("Custom border images / PNG or Sprite") ]
  public ClampedFloatParameter edgeImagesStrength=new ClampedFloatParameter(1,0,1);
  [Tooltip("Shared maximum number of sprites drawn by all active Edge Styles. Does not limit the optional world VFX rig.")]
  public ClampedIntParameter interiorSpriteBudget=new ClampedIntParameter(192,0,1024);
  public StatusEdgeStyleParameter poisonEdgeStyle=new StatusEdgeStyleParameter();
  public StatusEdgeStyleParameter fireEdgeStyle=new StatusEdgeStyleParameter();
  public StatusEdgeStyleParameter darknessEdgeStyle=new StatusEdgeStyleParameter();
  public StatusEdgeStyleParameter buffEdgeStyle=new StatusEdgeStyleParameter();
  public StatusEdgeStyleParameter wetEdgeStyle=new StatusEdgeStyleParameter();
  public StatusEdgeStyleParameter underwaterEdgeStyle=new StatusEdgeStyleParameter();
  public StatusEdgeStyleParameter electricEdgeStyle=new StatusEdgeStyleParameter();
  public StatusEdgeStyleParameter frostEdgeStyle=new StatusEdgeStyleParameter();
  public StatusEdgeStyleParameter bleedingEdgeStyle=new StatusEdgeStyleParameter();
  public float GetState(int index)
  {
   if(!active)return 0;
   ClampedFloatParameter state;
   switch(index)
   {
    case 0:state=poison;break;case 1:state=fire;break;case 2:state=darkness;break;
    case 3:state=buff;break;case 4:state=wet;break;case 5:state=underwater;break;
    case 6:state=electric;break;case 7:state=frost;break;case 8:state=bleeding;break;
    default:return 0;
   }
   // This accessor reads a source profile; unchecked overrides must not spawn particles.
   return state.overrideState?Mathf.Clamp01(state.value*(intensity.overrideState?intensity.value:1)):0;
  }
  bool HasImage(StatusEdgeStyleParameter style,float state)=>state>.0001f && style.value!=null && style.value.HasVisuals(interiorSpriteBudget.value>0);
  bool HasCustomEdges()=>edgeImagesStrength.value>.0001f && (
   HasImage(poisonEdgeStyle,poison.value) ||
   HasImage(fireEdgeStyle,fire.value) ||
   HasImage(darknessEdgeStyle,darkness.value) ||
   HasImage(buffEdgeStyle,buff.value) ||
   HasImage(wetEdgeStyle,wet.value) ||
   HasImage(underwaterEdgeStyle,underwater.value) ||
   HasImage(electricEdgeStyle,electric.value) ||
   HasImage(frostEdgeStyle,frost.value) ||
   HasImage(bleedingEdgeStyle,bleeding.value));
  public bool HasScreenEffect()=>IsActive() && (HasCustomEdges() || (proceduralStrength.value>.0001f &&
   (poison.value*poisonOverlay.value+fire.value*fireOverlay.value+darkness.value*darknessOverlay.value+
    buff.value*buffOverlay.value+wet.value*wetOverlay.value+underwater.value*underwaterOverlay.value+
    electric.value*electricOverlay.value+frost.value*frostOverlay.value+bleeding.value*bleedingOverlay.value)>.0001f));
  public bool IsActive()=>active && intensity.value>0.0001f && (poison.value+fire.value+darkness.value+buff.value+wet.value+underwater.value+electric.value+frost.value+bleeding.value)>.0001f;
  public bool IsTileCompatible()=>false;
 }
}
