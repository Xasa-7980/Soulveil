using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Experimental.Rendering;
namespace HarmonieFX
{
 public sealed class HarmonieFeature : ScriptableRendererFeature
 {
  [Tooltip("Assign Materials/Harmonie_Post.mat. Its shader reference is retained in builds.")]
  public Material effectMaterial;
  public bool showInSceneView=true;
  HarmoniePass pass;
  public override void Create(){pass?.Dispose();pass=new HarmoniePass{renderPassEvent=RenderPassEvent.BeforeRenderingPostProcessing};}
  public override void AddRenderPasses(ScriptableRenderer renderer,ref RenderingData data)
  {
   var camera=data.cameraData;
   if(effectMaterial==null || pass==null || camera.camera.stereoEnabled)return;
   if(camera.cameraType==CameraType.Preview || camera.cameraType==CameraType.Reflection)return;
   if(camera.isSceneViewCamera && !showInSceneView)return;
   if(!camera.resolveFinalTarget || (!camera.isSceneViewCamera && !camera.postProcessEnabled))return;
   var v=VolumeManager.instance.stack?.GetComponent<HarmonieVolume>();if(v==null || !v.IsActive())return;
   var s=new Settings(v);
   var cam=camera.camera;
   int index=data.lightData.mainLightIndex;
   Light sun=null;
   if(index>=0 && index<data.lightData.visibleLights.Length)sun=data.lightData.visibleLights[index].light;
   if(sun==null)sun=RenderSettings.sun;
   if(sun!=null && sun.type==LightType.Directional && sun.isActiveAndEnabled)
   {
    Vector3 direction=-sun.transform.forward;
    Vector3 viewport=cam.WorldToViewportPoint(cam.transform.position+direction*Mathf.Max(cam.nearClipPlane+1,100));
    float border=Mathf.Max(Mathf.Abs(viewport.x-.5f)*2,Mathf.Abs(viewport.y-.5f)*2);
    float front=Vector3.Dot(cam.transform.forward,direction);
    s.sunVisibility=Mathf.SmoothStep(0,1,Mathf.Clamp01(front/.15f))*(1-Mathf.SmoothStep(0,1,Mathf.Clamp01((border-1)/.4f)));
    if(viewport.z<=0)s.sunVisibility=0;
    s.sunUV=new Vector4(viewport.x,viewport.y,0,0);
    Color c=sun.color.linear*Mathf.Min(sun.intensity,4);s.sunRGB=new Vector4(c.r,c.g,c.b,1);
   }
   pass.Set(effectMaterial,s);
   var inputs=ScriptableRenderPassInput.Color;
   if(s.atmosphere>.0001f || s.HasShafts)inputs|=ScriptableRenderPassInput.Depth;
   pass.ConfigureInput(inputs);renderer.EnqueuePass(pass);
  }
  protected override void Dispose(bool disposing){pass?.Dispose();pass=null;}
  struct Settings
  {
   public float intensity,shafts,shaftReach,shaftRadius,glow,glowThreshold,glowClamp,glowRadius,atmosphere,hazeStart,hazeEnd,seaLevel,heightFalloff,distantDesaturation,sunVisibility;
   public int divisor,shaftSamples;
   public Color shaftColor,glowTint,nearHazeColor,farHazeColor;
   public Vector4 sunUV,sunRGB;
   public bool HasShafts=>shafts>.0001f && sunVisibility>.0001f;
   public bool NeedsLow=>glow>.0001f || HasShafts;
   public Settings(HarmonieVolume v)
   {
    intensity=v.intensity.value;shafts=v.shafts.value;shaftReach=v.shaftReach.value;shaftRadius=v.shaftRadius.value;
    glow=v.glow.value;glowThreshold=v.glowThreshold.value;glowClamp=v.glowClamp.value;glowRadius=v.glowRadius.value;
    atmosphere=v.atmosphere.value;hazeStart=v.hazeStart.value;hazeEnd=v.hazeEnd.value;seaLevel=v.seaLevel.value;
    heightFalloff=v.heightFalloff.value;distantDesaturation=v.distantDesaturation.value;
    divisor=v.downsample.value>=3?4:2;shaftSamples=v.shaftSamples.value;
    shaftColor=v.shaftColor.value;glowTint=v.glowTint.value;nearHazeColor=v.nearHazeColor.value;farHazeColor=v.farHazeColor.value;
    sunVisibility=0;sunUV=Vector4.zero;sunRGB=Vector4.zero;
   }
   public void Apply(Material m)
   {
    m.SetFloat("_Intensity",intensity);m.SetFloat("_Shafts",shafts);m.SetFloat("_ShaftReach",shaftReach);m.SetFloat("_ShaftRadius",shaftRadius);
    m.SetFloat("_ShaftSamples",shaftSamples);m.SetFloat("_Glow",glow);m.SetFloat("_GlowThreshold",glowThreshold);m.SetFloat("_GlowClamp",glowClamp);m.SetFloat("_GlowRadius",glowRadius);
    m.SetFloat("_Atmosphere",atmosphere);m.SetFloat("_HazeStart",hazeStart);m.SetFloat("_HazeEnd",hazeEnd);m.SetFloat("_SeaLevel",seaLevel);
    m.SetFloat("_HeightFalloff",heightFalloff);m.SetFloat("_DistantDesaturation",distantDesaturation);
    m.SetColor("_ShaftColor",shaftColor);m.SetColor("_GlowTint",glowTint);m.SetColor("_NearHazeColor",nearHazeColor);m.SetColor("_FarHazeColor",farHazeColor);
    m.SetVector("_SunUV",sunUV);m.SetVector("_SunRGB",sunRGB);m.SetFloat("_SunVisibility",sunVisibility);
   }
  }
  sealed class HarmoniePass : ScriptableRenderPass
  {
   static readonly int LowID=Shader.PropertyToID("_HarmonieLow");
   Material material;Settings settings;RTHandle lowA,lowB,full;
   public void Set(Material m,Settings s){material=m;settings=s;}
   public void Dispose(){lowA?.Release();lowB?.Release();full?.Release();lowA=lowB=full=null;}
   sealed class PassData{public TextureHandle source;public Material material;public Settings settings;public int shaderPass;}
   void AddPass(RenderGraph graph,string label,TextureHandle source,TextureHandle target,TextureHandle depth,int shaderPass,TextureHandle low=default,bool publishLow=false)
   {
    using(var builder=graph.AddRasterRenderPass<PassData>(label,out var data))
    {
     data.source=source;data.material=material;data.settings=settings;data.shaderPass=shaderPass;
     builder.UseTexture(source,AccessFlags.Read);
     if(depth.IsValid())builder.UseTexture(depth,AccessFlags.Read);
     if(low.IsValid())builder.UseTexture(low,AccessFlags.Read);
     builder.SetRenderAttachment(target,0,AccessFlags.Write);
     if(publishLow)builder.SetGlobalTextureAfterPass(target,LowID);
     builder.SetRenderFunc((PassData d,RasterGraphContext ctx)=>
     {
      d.settings.Apply(d.material);
      Blitter.BlitTexture(ctx.cmd,d.source,new Vector4(1,1,0,0),d.material,d.shaderPass);
     });
    }
   }
   public override void RecordRenderGraph(RenderGraph graph,ContextContainer frameData)
   {
    var resources=frameData.Get<UniversalResourceData>();var camera=frameData.Get<UniversalCameraData>();
    if(resources.isActiveTargetBackBuffer || material==null)return;
    TextureHandle source=resources.activeColorTexture,low=default;
    var desc=graph.GetTextureDesc(source);desc.msaaSamples=MSAASamples.None;desc.bindTextureMS=false;desc.clearBuffer=false;
    desc.name="Harmonie Output";var output=graph.CreateTexture(desc);
    var depth=resources.cameraDepthTexture;
    if(settings.NeedsLow)
    {
     desc.sizeMode=TextureSizeMode.Explicit;desc.filterMode=FilterMode.Bilinear;desc.wrapMode=TextureWrapMode.Clamp;
     desc.useMipMap=false;desc.autoGenerateMips=false;
     desc.width=Mathf.Max(1,camera.cameraTargetDescriptor.width/settings.divisor);
     desc.height=Mathf.Max(1,camera.cameraTargetDescriptor.height/settings.divisor);
     desc.colorFormat=GraphicsFormat.R16G16B16A16_SFloat;desc.name="Harmonie Prefilter";
     var first=graph.CreateTexture(desc);
     bool blur=settings.glow>.0001f;
     AddPass(graph,"Harmonie / Prefilter",source,first,settings.HasShafts?depth:default,0,publishLow:!settings.HasShafts && !blur);
     low=first;
     if(settings.HasShafts)
     {
      desc.name="Harmonie Shafts";var radial=graph.CreateTexture(desc);
      AddPass(graph,"Harmonie / Sun shafts",low,radial,default,1,publishLow:!blur);low=radial;
     }
     if(blur)
     {
      desc.name="Harmonie Blur H";var horizontal=graph.CreateTexture(desc);
      AddPass(graph,"Harmonie / Blur H",low,horizontal,default,2);
      desc.name="Harmonie Blur V";var vertical=graph.CreateTexture(desc);
      AddPass(graph,"Harmonie / Blur V",horizontal,vertical,default,3,publishLow:true);low=vertical;
     }
    }
    AddPass(graph,"Harmonie / Composite",source,output,settings.atmosphere>.0001f?depth:default,4,low);
    resources.cameraColor=output;
   }
   public override void OnCameraSetup(CommandBuffer cmd,ref RenderingData data)
   {
    var desc=data.cameraData.cameraTargetDescriptor;desc.depthBufferBits=0;desc.msaaSamples=1;desc.bindMS=false;
    RenderingUtils.ReAllocateHandleIfNeeded(ref full,desc,FilterMode.Bilinear,TextureWrapMode.Clamp,name:"Harmonie Output");
    if(settings.NeedsLow)
    {
     desc.width=Mathf.Max(1,desc.width/settings.divisor);desc.height=Mathf.Max(1,desc.height/settings.divisor);
     desc.graphicsFormat=GraphicsFormat.R16G16B16A16_SFloat;
     RenderingUtils.ReAllocateHandleIfNeeded(ref lowA,desc,FilterMode.Bilinear,TextureWrapMode.Clamp,name:"Harmonie Low A");
     RenderingUtils.ReAllocateHandleIfNeeded(ref lowB,desc,FilterMode.Bilinear,TextureWrapMode.Clamp,name:"Harmonie Low B");
    }
    else{lowA?.Release();lowB?.Release();lowA=lowB=null;}
   }
   public override void Execute(ScriptableRenderContext context,ref RenderingData data)
   {
    if(material==null || full==null)return;
    var source=data.cameraData.renderer.cameraColorTargetHandle;var cmd=CommandBufferPool.Get("Harmonie");
    try
    {
     settings.Apply(material);
     if(settings.NeedsLow)
     {
      Blitter.BlitCameraTexture(cmd,source,lowA,material,0);RTHandle current=lowA,other=lowB;
      if(settings.HasShafts){Blitter.BlitCameraTexture(cmd,current,other,material,1);var swap=current;current=other;other=swap;}
      if(settings.glow>.0001f)
      {
       Blitter.BlitCameraTexture(cmd,current,other,material,2);var swap=current;current=other;other=swap;
       Blitter.BlitCameraTexture(cmd,current,other,material,3);current=other;
      }
      cmd.SetGlobalTexture(LowID,current.nameID);
     }
     Blitter.BlitCameraTexture(cmd,source,full,material,4);Blitter.BlitCameraTexture(cmd,full,source);
     context.ExecuteCommandBuffer(cmd);
    }
    finally{CommandBufferPool.Release(cmd);}
   }
  }
 }
}
