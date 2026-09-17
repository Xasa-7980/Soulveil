using System;
using System.Collections.Generic;
using UnityEngine;
namespace AtmosferaAnime
{
 public enum StatusVFXTrigger { Poison, Fire, Darkness, Buff, Wet, Underwater, Electric, Frost, Bleeding, Manual }
 public enum StatusVFXAnchor { CameraPlane, PlayerWorld, PlayerFacingCamera }
 public enum StatusVFXShape { Box, Ring, Sphere }
 public enum StatusSpriteShape { Texture, SoftGlow, Snowflake, Ring, Spark, Droplet }
 [Serializable]
 public sealed class StatusVFXLayer
 {
  [Tooltip("Unique ID used by preview buttons, UnityEvents and gameplay calls.")]
  public string id="new_effect";
  public bool enabled=true;
  public StatusVFXTrigger trigger=StatusVFXTrigger.Poison;
  [Range(0,1)] public float strength=1;
  [Min(.01f)] public float fadeIn=.25f;
  [Min(.01f)] public float fadeOut=.5f;
  [Min(.01f)] public float manualDuration=2;
  [Header("Anchor / units")]
  [Tooltip("CameraPlane uses fractions of the visible screen height. Player anchors use world metres.")]
  public StatusVFXAnchor anchor=StatusVFXAnchor.CameraPlane;
  [Min(.05f)] public float cameraDistance=.7f;
  public Vector2 viewportCenter=new Vector2(.5f,.5f);
  public Vector3 offset;
  public Vector3 rotation;
  public bool inheritPlayerRotation;
  public bool depthTest;
  [Header("Sprites / particles")]
  public bool sprites=true;
  [Tooltip("Optional sprite sequence. All frames must use the same texture/atlas. Takes priority over Texture.")]
  public Sprite[] frames;
  public Texture2D texture;
  public StatusSpriteShape fallbackShape=StatusSpriteShape.SoftGlow;
  [Min(1)] public int columns=1;
  [Min(1)] public int rows=1;
  [Min(1)] public int animationCycles=1;
  public bool randomStartFrame;
  public bool additive;
  [ColorUsage(true,true)] public Color tint=Color.white;
  public Gradient colorOverLifetime=FadeGradient();
  [Min(0)] public float emissionRate=12;
  [Min(0)] public int burstOnActivation=4;
  [Range(1,512)] public int maxParticles=48;
  public Vector2 lifetime=new Vector2(1.5f,3);
  public Vector2 size=new Vector2(.02f,.05f);
  public AnimationCurve sizeOverLifetime=new AnimationCurve(new Keyframe(0,.2f),new Keyframe(.15f,1),new Keyframe(1,.1f));
  [Header("Particle System modes (optional upgrade)")]
  [Tooltip("Enable to use the native Constant / Curve / Two Constants / Two Curves controls below. Old fields remain intact when disabled.")]
  public bool useParticleCurves;
  public ParticleSystem.MinMaxCurve startSizeMode=new ParticleSystem.MinMaxCurve(.02f,.05f);
  public ParticleSystem.MinMaxCurve sizeLifetimeMode=new ParticleSystem.MinMaxCurve(1,AnimationCurve.Linear(0,1,1,0));
  public ParticleSystem.MinMaxGradient startColorMode=new ParticleSystem.MinMaxGradient(Color.white);
  public ParticleSystem.MinMaxGradient colorLifetimeMode=new ParticleSystem.MinMaxGradient(FadeGradient());
  public Vector2 startRotationDegrees=new Vector2(0,360);
  public Vector2 rotationSpeedDegrees=new Vector2(-35,35);
  public StatusVFXShape shape=StatusVFXShape.Box;
  [Tooltip("Box dimensions. For a camera box X is a fraction of screen width and Y of screen height. Ring/Sphere use Radius instead.")]
  public Vector3 area=new Vector3(1,1,0);
  [Min(0)] public float radius=.3f;
  public Vector3 shapeRotation;
  public Vector3 velocity=new Vector3(0,.1f,0);
  public Vector3 orbit;
  public float gravity;
  [Header("Particle noise")]
  public bool noise=true;
  [Min(0)] public float noiseStrength=.03f;
  [Min(.001f)] public float noiseFrequency=.5f;
  [Min(0)] public float noiseScrollSpeed=.2f;
  public ParticleSystemNoiseQuality noiseQuality=ParticleSystemNoiseQuality.Low;
  public uint seed=1729;
  public int sortingOrder;
  [Header("Optional prefab: particles, Visual Effect or your own VFX")]
  [Tooltip("Optional reusable prefab. Native Particle Systems play/stop automatically. Use StatusVFXPrefabReceiver events for custom controllers or VFX Graph properties.")]
  public GameObject prefab;
  public Vector3 prefabScale=Vector3.one;
  public bool scalePrefabEmission=true;
  public bool clearPrefabOnStop;
  [Min(0)] public float prefabStopDelay=3;
  public static Gradient FadeGradient()
  {
   var g=new Gradient();g.SetKeys(new[]{new GradientColorKey(Color.white,0),new GradientColorKey(Color.white,1)},
    new[]{new GradientAlphaKey(0,0),new GradientAlphaKey(1,.12f),new GradientAlphaKey(.8f,.65f),new GradientAlphaKey(0,1)});return g;
  }
 }
 [CreateAssetMenu(menuName="Atmosfera Anime/Status VFX Library",fileName="StatusVFXLibrary")]
 public sealed class StatusVFXLibrary : ScriptableObject
 {
  public List<StatusVFXLayer> layers=new List<StatusVFXLayer>();
 }
}
