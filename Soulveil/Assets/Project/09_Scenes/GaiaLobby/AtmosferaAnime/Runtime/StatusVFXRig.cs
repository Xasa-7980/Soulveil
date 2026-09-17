using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
namespace AtmosferaAnime
{
 [DefaultExecutionOrder(200),DisallowMultipleComponent]
 public sealed class StatusVFXRig : MonoBehaviour
 {
  [Header("Connections")]
  [Tooltip("Camera that renders the effect. Use one rig per camera.")]
  public Camera targetCamera;
  [Tooltip("Player root or attachment point followed by world-space layers.")]
  public Transform player;
  [Tooltip("Optional character renderer used to fit the screen aura around the visible body.")]
  public Renderer playerBounds;
  public Vector3 playerScreenOffset=new Vector3(0,1,0);
  [Min(.1f)] public float playerHeight=1.8f;
  [Tooltip("Assign the same Global Volume used by your status controller. Particle layers read this profile directly.")]
  public Volume sourceVolume;
  public StatusVFXLibrary library;
  public Material spriteTemplate;
  [Header("Budgets")]
  [Range(0,1)] public float vfxIntensity=1;
  [Tooltip("Shared live-particle cap for generated sprites only. Custom prefabs keep their own particle limits.")]
  [Range(32,4096)] public int totalSpriteParticles=256;
  [Range(1,32)] public int maximumLayers=24;
  [Range(0,32)] public int maximumPrefabInstances=12;
  public bool useUnscaledTime;
  public int LiveLayerCount=>layers.Count;
  static readonly Dictionary<int,StatusVFXRig> cameras=new Dictionary<int,StatusVFXRig>();
  Camera registeredCamera;
  StatusVFXLibrary workingLibrary;GameObject host;
  readonly List<RuntimeLayer> layers=new List<RuntimeLayer>();
  readonly Dictionary<string,RuntimeLayer> ids=new Dictionary<string,RuntimeLayer>(StringComparer.Ordinal);
  readonly Dictionary<(int,int,int,int),Material> materials=new Dictionary<(int,int,int,int),Material>();
  ParticleSystem.Particle[] trimBuffer;
  float clock;
  sealed class PrefabParticle
  {
   public ParticleSystem ps;
   public float timeRate,distanceRate;
   public bool subEmitter;
  }
  sealed class RuntimeLayer
  {
   public StatusVFXLayer config;public GameObject anchor,prefab;public Vector3 basePrefabScale;
   public ParticleSystem sprites;public ParticleSystemRenderer spriteRenderer;
   public MaterialPropertyBlock block=new MaterialPropertyBlock();
   public PrefabParticle[] prefabParticles;public StatusVFXPrefabReceiver[] receivers;
   public float level,target,manual,expires=-1,stoppedAt=-1;
   public bool manualOverride,emitting,prefabActive;
   public int cap;public bool budgeted;
  }
  bool editorPreview;
  void OnEnable(){if(Application.isPlaying){RegisterCamera();Rebuild();}}
  void RegisterCamera()
  {
   if(targetCamera==null)targetCamera=Camera.main;
   if(registeredCamera==targetCamera)return;
   UnregisterCamera();registeredCamera=targetCamera;
   if(registeredCamera!=null)
   {
    int id=registeredCamera.GetInstanceID();
    if(cameras.TryGetValue(id,out var other) && other!=null && other!=this)
     Debug.LogWarning("Use one StatusVFXRig per camera. The latest enabled rig supplies the screen anchor.",this);
    cameras[id]=this;
   }
  }
  void UnregisterCamera()
  {
   if(registeredCamera!=null && cameras.TryGetValue(registeredCamera.GetInstanceID(),out var value) && value==this)cameras.Remove(registeredCamera.GetInstanceID());
   registeredCamera=null;
  }
  public static void GetPlayerAnchor(Camera camera,out Vector4 data,out float assigned)
  {
   data=new Vector4(.5f,.5f,0,0);assigned=0;
   if(camera==null || !cameras.TryGetValue(camera.GetInstanceID(),out var rig) || rig==null || !rig.isActiveAndEnabled || rig.player==null)return;
   assigned=1;
   Vector3 center=rig.playerBounds!=null?rig.playerBounds.bounds.center:rig.player.TransformPoint(rig.playerScreenOffset);
   float radius=rig.playerBounds!=null?Mathf.Max(rig.playerBounds.bounds.extents.y,rig.playerBounds.bounds.extents.x):rig.playerHeight*.5f;
   Vector3 p=camera.WorldToViewportPoint(center);
   if(p.z<=camera.nearClipPlane || p.x<-.25f || p.x>1.25f || p.y<-.25f || p.y>1.25f)return;
   Vector3 top=camera.WorldToViewportPoint(center+camera.transform.up*radius);
   float size=Mathf.Clamp(Mathf.Abs(top.y-p.y),.005f,.8f);
   data=new Vector4(p.x,p.y,size,1);
  }
  [ContextMenu("Rebuild VFX from library (Play Mode)")]
  public void Rebuild()
  {
   if(!Application.isPlaying && !editorPreview)return;
   DestroyRuntime();if(library==null)return;
   workingLibrary=Instantiate(library);workingLibrary.name=library.name+" (runtime)";
   host=new GameObject("Status VFX [runtime]"){hideFlags=HideFlags.DontSave};
   if(editorPreview)host.transform.SetParent(transform,false);
   int prefabCount=0,limit=Mathf.Clamp(maximumLayers,1,32);
   foreach(var cfg in workingLibrary.layers)
   {
    if(cfg==null || !cfg.enabled || layers.Count>=limit)continue;
    if(string.IsNullOrWhiteSpace(cfg.id) || ids.ContainsKey(cfg.id))
    {Debug.LogWarning("Every VFX layer needs a non-empty, unique ID. Skipping: "+cfg.id,this);continue;}
    var rt=new RuntimeLayer{config=cfg};
    rt.anchor=new GameObject(cfg.id){layer=gameObject.layer};rt.anchor.transform.SetParent(host.transform,false);rt.anchor.SetActive(false);
    if(cfg.sprites && spriteTemplate!=null)CreateSprites(rt);
    if(!editorPreview && cfg.prefab!=null && prefabCount<Mathf.Clamp(maximumPrefabInstances,0,32))
    {
     rt.prefab=Instantiate(cfg.prefab,rt.anchor.transform,false);rt.prefab.name=cfg.prefab.name+" [status instance]";
     rt.prefab.transform.localPosition=Vector3.zero;rt.prefab.transform.localRotation=Quaternion.identity;
     rt.basePrefabScale=Vector3.Scale(rt.prefab.transform.localScale,cfg.prefabScale);rt.prefab.transform.localScale=rt.basePrefabScale;
     var systems=rt.prefab.GetComponentsInChildren<ParticleSystem>(true);rt.prefabParticles=new PrefabParticle[systems.Length];
     var subEmitters=new HashSet<ParticleSystem>();
     foreach(var system in systems)
     {
      var module=system.subEmitters;
      for(int j=0;j<module.subEmittersCount;j++)
      {var child=module.GetSubEmitterSystem(j);if(child!=null)subEmitters.Add(child);}
     }
     for(int i=0;i<systems.Length;i++)
     {
      var ps=systems[i];var em=ps.emission;var main=ps.main;main.stopAction=ParticleSystemStopAction.None;
      rt.prefabParticles[i]=new PrefabParticle{ps=ps,timeRate=em.rateOverTimeMultiplier,distanceRate=em.rateOverDistanceMultiplier,subEmitter=subEmitters.Contains(ps)};
      ps.Stop(false,ParticleSystemStopBehavior.StopEmittingAndClear);
     }
     rt.receivers=rt.prefab.GetComponentsInChildren<StatusVFXPrefabReceiver>(true);
     rt.prefab.SetActive(false);prefabCount++;
    }
    rt.anchor.SetActive(true);layers.Add(rt);ids.Add(cfg.id,rt);
   }
   if(spriteTemplate==null)Debug.LogWarning("Assign the supplied StatusSprite_Default material to enable generated sprites.",this);
  }
  static ParticleSystem.MinMaxCurve Range(Vector2 value,float multiplier=1,float floor=0)
  {
   float lo=Mathf.Max(floor,Mathf.Min(value.x,value.y)),hi=Mathf.Max(lo,Mathf.Max(value.x,value.y));
   return new ParticleSystem.MinMaxCurve(lo*multiplier,hi*multiplier);
  }
  Material SpriteMaterial(StatusVFXLayer cfg,Texture texture,bool textured)
  {
   int shape=textured?0:(int)cfg.fallbackShape;
   var key=(texture!=null?texture.GetInstanceID():0,shape,cfg.additive?1:0,cfg.depthTest?1:0);
   if(materials.TryGetValue(key,out var cached))return cached;
   var m=new Material(spriteTemplate){name="Status sprite [runtime]"};
   m.SetTexture("_MainTex",texture!=null?texture:Texture2D.whiteTexture);m.SetFloat("_Shape",shape);
   m.SetFloat("_DstBlend",cfg.additive?(float)BlendMode.One:(float)BlendMode.OneMinusSrcAlpha);
   m.SetFloat("_ZTest",cfg.depthTest?(float)CompareFunction.LessEqual:(float)CompareFunction.Always);
   m.SetColor("_Tint",Color.white);materials.Add(key,m);return m;
  }
  void CreateSprites(RuntimeLayer rt)
  {
   var c=rt.config;var go=new GameObject("Sprite emitter"){layer=gameObject.layer};go.transform.SetParent(rt.anchor.transform,false);
   var ps=go.AddComponent<ParticleSystem>();ps.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
   var main=ps.main;main.playOnAwake=false;main.loop=true;main.duration=10;
   main.startLifetime=Range(c.lifetime,1,.05f);main.startSpeed=0;main.startSize=c.useParticleCurves?c.startSizeMode:Range(c.size,1,.0001f);
   main.startRotation=Range(c.startRotationDegrees,Mathf.Deg2Rad,-100000);
   main.startColor=c.useParticleCurves?c.startColorMode:new ParticleSystem.MinMaxGradient(c.tint);main.gravityModifier=c.gravity;
   main.simulationSpace=ParticleSystemSimulationSpace.Local;main.scalingMode=ParticleSystemScalingMode.Hierarchy;
   main.useUnscaledTime=useUnscaledTime;main.cullingMode=ParticleSystemCullingMode.AlwaysSimulate;
   main.maxParticles=Mathf.Clamp(c.maxParticles,1,512);main.stopAction=ParticleSystemStopAction.None;
   ps.useAutoRandomSeed=false;ps.randomSeed=c.seed==0?1:c.seed;
   var emission=ps.emission;emission.rateOverTime=0;emission.rateOverDistance=0;
   var shape=ps.shape;shape.enabled=true;
   shape.shapeType=c.shape==StatusVFXShape.Box?ParticleSystemShapeType.Box:c.shape==StatusVFXShape.Ring?ParticleSystemShapeType.Circle:ParticleSystemShapeType.Sphere;
   shape.radius=Mathf.Max(0,c.radius);shape.radiusThickness=c.shape==StatusVFXShape.Ring?0:1;
   shape.scale=c.shape==StatusVFXShape.Box?new Vector3(Mathf.Max(0,c.area.x),Mathf.Max(0,c.area.y),Mathf.Max(0,c.area.z)):Vector3.one;shape.rotation=c.shapeRotation;
   var velocity=ps.velocityOverLifetime;velocity.enabled=true;velocity.space=ParticleSystemSimulationSpace.Local;
   velocity.x=c.velocity.x;velocity.y=c.velocity.y;velocity.z=c.velocity.z;
   velocity.orbitalX=c.orbit.x;velocity.orbitalY=c.orbit.y;velocity.orbitalZ=c.orbit.z;
   var color=ps.colorOverLifetime;color.enabled=true;color.color=c.useParticleCurves?c.colorLifetimeMode:new ParticleSystem.MinMaxGradient(c.colorOverLifetime);
   var size=ps.sizeOverLifetime;size.enabled=true;size.size=c.useParticleCurves?c.sizeLifetimeMode:new ParticleSystem.MinMaxCurve(1,c.sizeOverLifetime);
   var rotation=ps.rotationOverLifetime;rotation.enabled=true;rotation.z=Range(c.rotationSpeedDegrees,Mathf.Deg2Rad,-100000);
   var noise=ps.noise;noise.enabled=c.noise;noise.strength=Mathf.Max(0,c.noiseStrength);noise.frequency=Mathf.Max(.001f,c.noiseFrequency);
   noise.scrollSpeed=Mathf.Max(0,c.noiseScrollSpeed);noise.quality=c.noiseQuality;noise.damping=true;
   Texture texture=c.texture;bool hasFrames=false;
   var sheet=ps.textureSheetAnimation;
   if(c.frames!=null)
   {
    foreach(var frame in c.frames)if(frame!=null){texture=frame.texture;hasFrames=true;break;}
   }
   sheet.enabled=texture!=null;
   if(hasFrames)
   {
    sheet.mode=ParticleSystemAnimationMode.Sprites;
    foreach(var frame in c.frames)if(frame!=null)
    {
     if(frame.texture==texture)sheet.AddSprite(frame);
     else Debug.LogWarning("All sprite frames in a layer must use the same texture/atlas. Skipped frame in "+c.id,this);
    }
   }
   else if(texture!=null)
   {
    sheet.mode=ParticleSystemAnimationMode.Grid;sheet.numTilesX=Mathf.Max(1,c.columns);sheet.numTilesY=Mathf.Max(1,c.rows);
   }
   if(texture!=null)
   {
    sheet.animation=ParticleSystemAnimationType.WholeSheet;sheet.cycleCount=Mathf.Max(1,c.animationCycles);
    sheet.frameOverTime=new ParticleSystem.MinMaxCurve(1,AnimationCurve.Linear(0,0,1,1));
    sheet.startFrame=c.randomStartFrame?new ParticleSystem.MinMaxCurve(0,1):new ParticleSystem.MinMaxCurve(0);
   }
   var renderer=ps.GetComponent<ParticleSystemRenderer>();renderer.renderMode=ParticleSystemRenderMode.Billboard;
   renderer.sharedMaterial=SpriteMaterial(c,texture,texture!=null);renderer.sortingOrder=c.sortingOrder;
   renderer.shadowCastingMode=ShadowCastingMode.Off;renderer.receiveShadows=false;
   rt.sprites=ps;rt.spriteRenderer=renderer;rt.cap=main.maxParticles;
  }
  StatusScreenVolume ReadStatus(out float weight)
  {
   weight=0;if(sourceVolume==null || !sourceVolume.isActiveAndEnabled)return null;
   var profile=sourceVolume.HasInstantiatedProfile()?sourceVolume.profile:sourceVolume.sharedProfile;
   if(profile==null || !profile.TryGet<StatusScreenVolume>(out var state))return null;
   weight=Mathf.Clamp01(sourceVolume.weight);return state;
  }
  bool Place(RuntimeLayer rt)
  {
   var c=rt.config;Transform anchor=rt.anchor.transform;
   if(c.anchor==StatusVFXAnchor.CameraPlane)
   {
    if(targetCamera==null)return false;
    float distance=Mathf.Max(targetCamera.nearClipPlane+.05f,c.cameraDistance);
    float height=targetCamera.orthographic?targetCamera.orthographicSize*2:2*distance*Mathf.Tan(targetCamera.fieldOfView*Mathf.Deg2Rad*.5f);
    Vector3 center=targetCamera.ViewportToWorldPoint(new Vector3(c.viewportCenter.x,c.viewportCenter.y,distance));
    anchor.SetPositionAndRotation(center+targetCamera.transform.rotation*(c.offset*height),targetCamera.transform.rotation*Quaternion.Euler(c.rotation));
    anchor.localScale=Vector3.one*height;
    if(rt.sprites!=null && c.shape==StatusVFXShape.Box)
    {var shape=rt.sprites.shape;shape.scale=new Vector3(Mathf.Max(0,c.area.x)*targetCamera.aspect,Mathf.Max(0,c.area.y),Mathf.Max(0,c.area.z));}
   }
   else
   {
    if(player==null)return false;
    Quaternion basis=c.anchor==StatusVFXAnchor.PlayerFacingCamera && targetCamera!=null?targetCamera.transform.rotation:c.inheritPlayerRotation?player.rotation:Quaternion.identity;
    anchor.SetPositionAndRotation(player.position+basis*c.offset,basis*Quaternion.Euler(c.rotation));anchor.localScale=Vector3.one;
   }
   return true;
  }
  void AllocateBudgets()
  {
   int remaining=Mathf.Clamp(totalSpriteParticles,32,4096),requested=0,count=0;
   foreach(var rt in layers)
   {
    rt.budgeted=rt.sprites!=null && (rt.level>.001f || rt.sprites.IsAlive(false));
    if(rt.budgeted){requested+=Mathf.Clamp(rt.config.maxParticles,1,512);count++;}
   }
   if(trimBuffer==null || trimBuffer.Length<512)trimBuffer=new ParticleSystem.Particle[512];
   foreach(var rt in layers)if(rt.budgeted)
   {
    int wish=Mathf.Clamp(rt.config.maxParticles,1,512);
    int cap=Mathf.Min(wish,Mathf.Clamp(Mathf.FloorToInt((float)remaining*wish/Mathf.Max(1,requested)),1,Mathf.Max(1,remaining-count+1)));
    remaining-=cap;requested-=wish;count--;
    if(cap!=rt.cap){var main=rt.sprites.main;main.maxParticles=cap;rt.cap=cap;}
    if(rt.sprites.particleCount>cap){int n=rt.sprites.GetParticles(trimBuffer);rt.sprites.SetParticles(trimBuffer,Mathf.Min(cap,n));}
   }
  }
  void Drive(RuntimeLayer rt)
  {
   var c=rt.config;bool on=rt.level>.005f;
   if(rt.sprites!=null)
   {
    var emission=rt.sprites.emission;emission.rateOverTime=Mathf.Max(0,c.emissionRate)*rt.level;
    Color tint=c.useParticleCurves?c.tint:Color.white;tint.a*=Mathf.Clamp01(rt.level);
    rt.block.SetColor("_Tint",tint);rt.spriteRenderer.SetPropertyBlock(rt.block);
    if(on && !rt.emitting){rt.sprites.Play(false);if(c.burstOnActivation>0)rt.sprites.Emit(Mathf.Min(rt.cap,Mathf.RoundToInt(c.burstOnActivation*rt.target)));}
    if(!on && rt.emitting)rt.sprites.Stop(false,ParticleSystemStopBehavior.StopEmitting);
    rt.emitting=on;
   }
   if(rt.prefab==null)return;
   if(on && !rt.prefabActive)
   {
    rt.prefab.SetActive(true);rt.prefabActive=true;rt.stoppedAt=-1;
    foreach(var p in rt.prefabParticles)if(p.ps!=null && !p.subEmitter && p.ps.gameObject.activeInHierarchy)p.ps.Play(false);
    foreach(var receiver in rt.receivers)if(receiver!=null)receiver.Activate();
   }
   else if(!on && rt.prefabActive && rt.stoppedAt<0)
   {
    rt.stoppedAt=clock;
    foreach(var p in rt.prefabParticles)if(p.ps!=null)p.ps.Stop(false,c.clearPrefabOnStop?ParticleSystemStopBehavior.StopEmittingAndClear:ParticleSystemStopBehavior.StopEmitting);
    foreach(var receiver in rt.receivers)if(receiver!=null)receiver.Deactivate();
   }
   if(on && rt.stoppedAt>=0)
   {
    rt.stoppedAt=-1;foreach(var p in rt.prefabParticles)if(p.ps!=null && !p.subEmitter && p.ps.gameObject.activeInHierarchy)p.ps.Play(false);
    foreach(var receiver in rt.receivers)if(receiver!=null)receiver.Activate();
   }
   if(c.scalePrefabEmission)
    foreach(var p in rt.prefabParticles)if(p.ps!=null){var emission=p.ps.emission;emission.rateOverTimeMultiplier=p.timeRate*rt.level;emission.rateOverDistanceMultiplier=p.distanceRate*rt.level;}
   foreach(var receiver in rt.receivers)if(receiver!=null)receiver.SetIntensity(rt.level);
   if(rt.stoppedAt>=0 && clock-rt.stoppedAt>=Mathf.Max(0,c.prefabStopDelay))
   {rt.prefab.SetActive(false);rt.prefabActive=false;rt.stoppedAt=-1;}
  }
  void LateUpdate()
  {
   if(editorPreview)return;RegisterCamera();Tick(useUnscaledTime?Time.unscaledDeltaTime:Time.deltaTime);
  }
  void Tick(float dt)
  {
   clock+=Mathf.Max(0,dt);
   var status=ReadStatus(out float weight);
   foreach(var rt in layers)
   {
    if(rt.expires>=0 && clock>=rt.expires){rt.manualOverride=false;rt.expires=-1;}
    float input=rt.manualOverride?rt.manual:status!=null && rt.config.trigger!=StatusVFXTrigger.Manual?status.GetState((int)rt.config.trigger)*weight:0;
    bool placed=Place(rt);rt.target=placed?Mathf.Clamp01(input*rt.config.strength*vfxIntensity):0;
    float duration=rt.target>rt.level?rt.config.fadeIn:rt.config.fadeOut;
    rt.level=Mathf.MoveTowards(rt.level,rt.target,dt/Mathf.Max(.01f,duration));
   }
   AllocateBudgets();foreach(var rt in layers)Drive(rt);
  }
  public void SetEntryIntensity(string id,float intensity)
  {if(ids.TryGetValue(id,out var rt)){rt.manualOverride=true;rt.manual=Mathf.Clamp01(intensity);rt.expires=-1;}}
  public void PlayEntry(string id)=>SetEntryIntensity(id,1);
  public void StopEntry(string id)=>SetEntryIntensity(id,0);
  public void FollowVolume(string id){if(ids.TryGetValue(id,out var rt)){rt.manualOverride=false;rt.expires=-1;}}
  public void PulseEntry(string id)
  {if(ids.TryGetValue(id,out var rt)){rt.manualOverride=true;rt.manual=1;rt.expires=clock+Mathf.Max(.01f,rt.config.manualDuration);}}
  public void StopAllEntries(){foreach(var rt in layers){rt.manualOverride=true;rt.manual=0;rt.expires=-1;}}
  public void FollowVolumeAll(){foreach(var rt in layers){rt.manualOverride=false;rt.expires=-1;}}
  void DestroyRuntime()
  {
   foreach(var rt in layers)
   {
    if(rt.sprites!=null)rt.sprites.Stop(false,ParticleSystemStopBehavior.StopEmittingAndClear);
    if(rt.prefab!=null)rt.prefab.SetActive(false);
   }
   if(host!=null){host.SetActive(false);ReleaseObject(host);}host=null;
   foreach(var m in materials.Values)if(m!=null)ReleaseObject(m);materials.Clear();layers.Clear();ids.Clear();
   if(workingLibrary!=null)ReleaseObject(workingLibrary);workingLibrary=null;
  }
  void ReleaseObject(UnityEngine.Object item){if(Application.isPlaying && !editorPreview)Destroy(item);else DestroyImmediate(item);}
#if UNITY_EDITOR
  // Hidden preview scene only; never adds objects or references to the user's scene.
  public void BeginEditorPreview(Camera camera,StatusVFXLibrary asset,Material template,Transform standIn)
  {
   editorPreview=true;enabled=false;targetCamera=camera;library=asset;spriteTemplate=template;player=standIn;
   maximumPrefabInstances=0;Rebuild();
  }
  public void EndEditorPreview(){if(editorPreview)DestroyRuntime();}
  public void StepEditorPreview(float dt,string id)
  {
   if(!editorPreview)return;StopAllEntries();PlayEntry(id);Tick(Mathf.Max(0,dt));
   foreach(var layer in layers)if(layer.sprites!=null)layer.sprites.Simulate(Mathf.Max(0,dt),false,false,true);
  }
#endif
  void OnDisable(){UnregisterCamera();DestroyRuntime();}
 }
}
