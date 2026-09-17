using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;
namespace FantasiaTools
{
 public sealed class MelancholyFeature : ScriptableRendererFeature
 {
  [Tooltip("Assign Melancholy_Post so the shader is kept in builds.")]
  public Material effectMaterial;
  public bool showInSceneView=true;
  GradePass pass;
  public override void Create(){pass?.Dispose();pass=new GradePass{renderPassEvent=RenderPassEvent.AfterRenderingPostProcessing};}
  public override void AddRenderPasses(ScriptableRenderer renderer,ref RenderingData data)
  {
   var c=data.cameraData;
   if(effectMaterial==null || pass==null || c.camera.stereoEnabled || c.cameraType==CameraType.Preview || c.cameraType==CameraType.Reflection)return;
   if(!c.resolveFinalTarget || (c.isSceneViewCamera?!showInSceneView:!c.postProcessEnabled))return;
   var v=VolumeManager.instance.stack?.GetComponent<MelancholyVolume>();if(v==null || !v.IsActive())return;
   pass.Set(effectMaterial,new Snapshot(v));pass.ConfigureInput(ScriptableRenderPassInput.Color);renderer.EnqueuePass(pass);
  }
  protected override void Dispose(bool disposing){pass?.Dispose();pass=null;}
  struct Snapshot
  {
   readonly Vector4 grade,finish;readonly Color shadows,highlights;readonly float split;
   public Snapshot(MelancholyVolume v)
   {grade=new Vector4(v.intensity.value,v.saturation.value,v.exposure.value,v.contrast.value);finish=new Vector4(v.blackLift.value,v.vignette.value,v.grain.value,v.animateGrain.value?Time.unscaledTime:0);shadows=v.shadowTint.value;highlights=v.highlightTint.value;split=v.splitStrength.value;}
   public void Apply(Material m){m.SetVector("_Grade",grade);m.SetVector("_Finish",finish);m.SetColor("_Shadows",shadows);m.SetColor("_Highlights",highlights);m.SetFloat("_Split",split);}
  }
  sealed class GradePass:ScriptableRenderPass
  {
   Material material;Snapshot snapshot;RTHandle temporary;
   public void Set(Material m,Snapshot s){material=m;snapshot=s;}
   public void Dispose(){temporary?.Release();temporary=null;}
   sealed class Data{public TextureHandle source;public Material material;public Snapshot snapshot;}
   public override void RecordRenderGraph(RenderGraph graph,ContextContainer context)
   {
    var r=context.Get<UniversalResourceData>();if(r.isActiveTargetBackBuffer || material==null)return;
    var source=r.activeColorTexture;var desc=graph.GetTextureDesc(source);desc.name="Melancholy color";desc.clearBuffer=false;desc.msaaSamples=MSAASamples.None;desc.bindTextureMS=false;
    var target=graph.CreateTexture(desc);
    using(var b=graph.AddRasterRenderPass<Data>("Melancholy grade",out var d))
    {
     d.source=source;d.material=material;d.snapshot=snapshot;b.UseTexture(source,AccessFlags.Read);b.SetRenderAttachment(target,0,AccessFlags.Write);
     b.SetRenderFunc((Data x,RasterGraphContext ctx)=>{x.snapshot.Apply(x.material);Blitter.BlitTexture(ctx.cmd,x.source,new Vector4(1,1,0,0),x.material,0);});
    }
    r.cameraColor=target;
   }
   public override void OnCameraSetup(CommandBuffer cmd,ref RenderingData data)
   {
    var d=data.cameraData.cameraTargetDescriptor;d.depthBufferBits=0;d.msaaSamples=1;d.bindMS=false;
    RenderingUtils.ReAllocateHandleIfNeeded(ref temporary,d,FilterMode.Bilinear,TextureWrapMode.Clamp,name:"Melancholy temporary");
   }
   public override void Execute(ScriptableRenderContext context,ref RenderingData data)
   {
    if(material==null || temporary==null)return;var cmd=CommandBufferPool.Get("Melancholy grade");
    try{snapshot.Apply(material);var src=data.cameraData.renderer.cameraColorTargetHandle;Blitter.BlitCameraTexture(cmd,src,temporary,material,0);Blitter.BlitCameraTexture(cmd,temporary,src);context.ExecuteCommandBuffer(cmd);}
    finally{CommandBufferPool.Release(cmd);}
   }
  }
 }
}
