using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;

namespace ElementalVillages
{
    // Required one-time renderer integration. The Volume owns all artistic controls.
    public sealed class ElementalCelFeature : ScriptableRendererFeature
    {
        [Tooltip("Assign the supplied ElementalCel_Post material to preserve the shader in builds.")]
        public Material effectMaterial;
        public bool showInSceneView = true;
        CelPass pass;
        public override void Create()
        {
            pass?.Dispose();
            pass = new CelPass { renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing };
        }
        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData data)
        {
            if (effectMaterial == null || pass == null) return;
            var camera = data.cameraData;
            if (camera.cameraType == CameraType.Preview || camera.cameraType == CameraType.Reflection) return;
            if (camera.isSceneViewCamera && !showInSceneView) return;
            // Apply once, on the final camera in a stack, so the whole rendered scene is processed.
            if (!camera.resolveFinalTarget || (!camera.isSceneViewCamera && !camera.postProcessEnabled)) return;
            var volume = VolumeManager.instance.stack?.GetComponent<ElementalCelVolume>();
            if (volume == null || !volume.IsActive()) return;
            pass.Set(effectMaterial, new Settings(volume));
            pass.ConfigureInput(ScriptableRenderPassInput.Color | ScriptableRenderPassInput.Depth | ScriptableRenderPassInput.Normal);
            renderer.EnqueuePass(pass);
        }
        protected override void Dispose(bool disposing) { pass?.Dispose(); pass = null; }

        struct Settings
        {
            public float intensity,bands,whitePoint,preserveColor,outlineStrength,outlineWidth,depthThreshold,normalThreshold,normalEdges,affectSky;
            public Color outlineColor;
            public Settings(ElementalCelVolume v)
            {
                intensity=v.intensity.value; bands=v.bands.value; whitePoint=v.whitePoint.value;
                preserveColor=v.preserveColor.value; outlineStrength=v.outlineStrength.value;
                outlineWidth=v.outlineWidth.value; depthThreshold=v.depthThreshold.value;
                normalThreshold=v.normalThreshold.value; normalEdges=v.normalEdges.value;
                affectSky=v.affectSky.value?1f:0f; outlineColor=v.outlineColor.value;
            }
            public void Apply(Material m)
            {
                m.SetFloat("_Intensity",intensity);m.SetFloat("_Bands",bands);m.SetFloat("_WhitePoint",whitePoint);
                m.SetFloat("_PreserveColor",preserveColor);m.SetFloat("_OutlineStrength",outlineStrength);
                m.SetFloat("_OutlineWidth",outlineWidth);m.SetFloat("_DepthThreshold",depthThreshold);
                m.SetFloat("_NormalThreshold",normalThreshold);m.SetFloat("_NormalEdges",normalEdges);
                m.SetFloat("_AffectSky",affectSky);m.SetColor("_OutlineColor",outlineColor);
            }
        }
        sealed class CelPass : ScriptableRenderPass
        {
            Material material; Settings settings; RTHandle temporary;
            public void Set(Material m, Settings s) { material=m; settings=s; }
            public void Dispose() { temporary?.Release(); temporary=null; }
            sealed class PassData { public TextureHandle source; public Material material; public Settings settings; }

            public override void RecordRenderGraph(RenderGraph graph, ContextContainer frameData)
            {
                var resources=frameData.Get<UniversalResourceData>();
                if (resources.isActiveTargetBackBuffer || material == null) return;
                var source=resources.activeColorTexture;
                var desc=graph.GetTextureDesc(source);desc.name="Elemental Cel Color";desc.clearBuffer=false;desc.msaaSamples=MSAASamples.None;
                var destination=graph.CreateTexture(desc);
                using (var builder=graph.AddRasterRenderPass<PassData>("Elemental Cel + Outlines",out var data))
                {
                    data.source=source;data.material=material;data.settings=settings;
                    builder.UseTexture(source,AccessFlags.Read);
                    if (resources.cameraDepthTexture.IsValid()) builder.UseTexture(resources.cameraDepthTexture,AccessFlags.Read);
                    if (resources.cameraNormalsTexture.IsValid()) builder.UseTexture(resources.cameraNormalsTexture,AccessFlags.Read);
                    builder.SetRenderAttachment(destination,0,AccessFlags.Write);
                    builder.SetRenderFunc((PassData d,RasterGraphContext ctx)=>
                    {
                        d.settings.Apply(d.material);
                        Blitter.BlitTexture(ctx.cmd,d.source,new Vector4(1,1,0,0),d.material,0);
                    });
                }
                resources.cameraColor=destination;
            }

            // Also supports the Unity 6 Compatibility Mode path.
            public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData data)
            {
                var desc=data.cameraData.cameraTargetDescriptor;desc.depthBufferBits=0;desc.msaaSamples=1;
                RenderingUtils.ReAllocateHandleIfNeeded(ref temporary,desc,FilterMode.Bilinear,TextureWrapMode.Clamp,name:"ElementalCelTemporary");
            }
            public override void Execute(ScriptableRenderContext context,ref RenderingData data)
            {
                if (material==null || temporary==null) return;
                var source=data.cameraData.renderer.cameraColorTargetHandle;
                var cmd=CommandBufferPool.Get("Elemental Cel + Outlines");
                try
                {
                    settings.Apply(material);
                    Blitter.BlitCameraTexture(cmd,source,temporary,material,0);
                    Blitter.BlitCameraTexture(cmd,temporary,source);
                    context.ExecuteCommandBuffer(cmd);
                }
                finally { CommandBufferPool.Release(cmd); }
            }
        }
    }
}
