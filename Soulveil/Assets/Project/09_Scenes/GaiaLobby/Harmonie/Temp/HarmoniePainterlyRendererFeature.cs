using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;

namespace HarmonieFX
{
    public sealed class HarmoniePainterlyRendererFeature : ScriptableRendererFeature
    {
        [Tooltip("Asigna un Material creado con el shader Hidden/HarmonieFX/Painterly.")]
        public Material effectMaterial;

        public bool showInSceneView = true;

        private HarmoniePainterlyPass pass;

        public override void Create ( )
        {
            pass?.Dispose();

            pass = new HarmoniePainterlyPass
            {
                renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing
            };
        }

        public override void AddRenderPasses (
            ScriptableRenderer renderer,
            ref RenderingData data )
        {
            var cameraData = data.cameraData;

            if (effectMaterial == null || pass == null)
                return;

            if (cameraData.camera == null)
                return;

            if (cameraData.camera.stereoEnabled)
                return;

            if (cameraData.cameraType == CameraType.Preview ||
                cameraData.cameraType == CameraType.Reflection)
                return;

            if (cameraData.isSceneViewCamera && !showInSceneView)
                return;

            // Mismo patrón que tu Renderer Feature funcional.
            if (!cameraData.resolveFinalTarget)
                return;

            if (!cameraData.isSceneViewCamera &&
                !cameraData.postProcessEnabled)
                return;

            HarmoniePainterlyVolume volume =
                VolumeManager.instance.stack?
                    .GetComponent<HarmoniePainterlyVolume>();

            if (volume == null || !volume.IsActive())
                return;

            pass.Set(
                effectMaterial,
                new Settings(volume)
            );

            pass.ConfigureInput(
                ScriptableRenderPassInput.Color
            );

            renderer.EnqueuePass(pass);
        }

        protected override void Dispose ( bool disposing )
        {
            pass?.Dispose();
            pass = null;
        }

        // =========================================================
        // SETTINGS SNAPSHOT
        // =========================================================

        private struct Settings
        {
            public float intensity;
            public float brushBlend;
            public float blurRadius;
            public float colorSteps;

            public float outlineStrength;
            public float outlineThreshold;

            public float saturation;
            public float contrast;
            public float warmth;
            public float glow;

            public float paperGrain;

            public float motionStrength;
            public float motionSpeed;

            public Settings ( HarmoniePainterlyVolume volume )
            {
                intensity = volume.intensity.value;
                brushBlend = volume.brushBlend.value;
                blurRadius = volume.blurRadius.value;
                colorSteps = volume.colorSteps.value;

                outlineStrength = volume.outlineStrength.value;
                outlineThreshold = volume.outlineThreshold.value;

                saturation = volume.saturation.value;
                contrast = volume.contrast.value;
                warmth = volume.warmth.value;
                glow = volume.glow.value;

                paperGrain = volume.paperGrain.value;

                motionStrength = volume.motionStrength.value;
                motionSpeed = volume.motionSpeed.value;
            }

            public void Apply ( Material material )
            {
                material.SetFloat("_Intensity", intensity);
                material.SetFloat("_BrushBlend", brushBlend);
                material.SetFloat("_BlurRadius", blurRadius);
                material.SetFloat("_ColorSteps", colorSteps);

                material.SetFloat("_OutlineStrength", outlineStrength);
                material.SetFloat("_OutlineThreshold", outlineThreshold);

                material.SetFloat("_Saturation", saturation);
                material.SetFloat("_Contrast", contrast);
                material.SetFloat("_Warmth", warmth);
                material.SetFloat("_Glow", glow);

                material.SetFloat("_PaperGrain", paperGrain);

                material.SetFloat("_MotionStrength", motionStrength);
                material.SetFloat("_MotionSpeed", motionSpeed);
            }
        }

        // =========================================================
        // PASS
        // =========================================================

        private sealed class HarmoniePainterlyPass : ScriptableRenderPass
        {
            private Material material;
            private Settings settings;

            // Compatibility path.
            private RTHandle full;

            private sealed class PassData
            {
                public TextureHandle source;
                public Material material;
                public Settings settings;
            }

            public void Set (
                Material effectMaterial,
                Settings effectSettings )
            {
                material = effectMaterial;
                settings = effectSettings;
            }

            public void Dispose ( )
            {
                full?.Release();
                full = null;
            }

            // =====================================================
            // RENDER GRAPH
            // =====================================================

            public override void RecordRenderGraph (
                RenderGraph graph,
                ContextContainer frameData )
            {
                UniversalResourceData resources =
                    frameData.Get<UniversalResourceData>();

                if (resources.isActiveTargetBackBuffer ||
                    material == null)
                {
                    return;
                }

                TextureHandle source =
                    resources.activeColorTexture;

                TextureDesc desc =
                    graph.GetTextureDesc(source);

                desc.name = "Harmonie Painterly Output";
                desc.clearBuffer = false;
                desc.depthBufferBits = 0;

                TextureHandle output =
                    graph.CreateTexture(desc);

                using (
                    var builder =
                        graph.AddRasterRenderPass<PassData>(
                            "Harmonie / Painterly",
                            out var passData
                        )
                )
                {
                    passData.source = source;
                    passData.material = material;
                    passData.settings = settings;

                    builder.UseTexture(
                        source,
                        AccessFlags.Read
                    );

                    builder.SetRenderAttachment(
                        output,
                        0,
                        AccessFlags.Write
                    );

                    builder.SetRenderFunc(
                        ( PassData data, RasterGraphContext ctx ) =>
                        {
                            data.settings.Apply(
                                data.material
                            );

                            Blitter.BlitTexture(
                                ctx.cmd,
                                data.source,
                                new Vector4(1f, 1f, 0f, 0f),
                                data.material,
                                0
                            );
                        }
                    );
                }

                // Fundamental: publicar el resultado como cameraColor.
                resources.cameraColor = output;
            }

            // =====================================================
            // COMPATIBILITY MODE / CLASSIC URP
            // =====================================================

            public override void OnCameraSetup (
                CommandBuffer cmd,
                ref RenderingData data )
            {
                RenderTextureDescriptor desc =
                    data.cameraData
                        .cameraTargetDescriptor;

                desc.depthBufferBits = 0;
                desc.msaaSamples = 1;
                desc.bindMS = false;

                RenderingUtils.ReAllocateHandleIfNeeded(
                    ref full,
                    desc,
                    FilterMode.Bilinear,
                    TextureWrapMode.Clamp,
                    name: "Harmonie Painterly Output"
                );
            }

            public override void Execute (
                ScriptableRenderContext context,
                ref RenderingData data )
            {
                if (material == null || full == null)
                    return;

                RTHandle source =
                    data.cameraData
                        .renderer
                        .cameraColorTargetHandle;

                CommandBuffer cmd =
                    CommandBufferPool.Get(
                        "Harmonie Painterly"
                    );

                try
                {
                    settings.Apply(material);

                    Blitter.BlitCameraTexture(
                        cmd,
                        source,
                        full,
                        material,
                        0
                    );

                    Blitter.BlitCameraTexture(
                        cmd,
                        full,
                        source
                    );

                    context.ExecuteCommandBuffer(cmd);
                }
                finally
                {
                    CommandBufferPool.Release(cmd);
                }
            }
        }
    }
}
