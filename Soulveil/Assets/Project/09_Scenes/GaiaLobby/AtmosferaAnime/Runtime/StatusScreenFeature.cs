using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;

namespace AtmosferaAnime
{
    // Required one-time renderer integration. The Volume owns all artistic controls.
    public sealed class StatusScreenFeature : ScriptableRendererFeature
    {
        [Tooltip("Assign the supplied StatusScreen_Post material to preserve the shader in builds.")]
        public Material effectMaterial;
        public bool showInSceneView = true;
        CelPass pass;
        bool warnedSpritePass;
        public override void Create()
        {
            pass?.Dispose();
            pass = new CelPass { renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing };
        }
        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData data)
        {
            if (effectMaterial == null || pass == null) return;
            var camera = data.cameraData;
            if(camera.camera.stereoEnabled)return;
            if (camera.cameraType == CameraType.Preview || camera.cameraType == CameraType.Reflection) return;
            if (camera.isSceneViewCamera && !showInSceneView) return;
            // Apply once, on the final camera in a stack, so the whole rendered scene is processed.
            if (!camera.resolveFinalTarget || (!camera.isSceneViewCamera && !camera.postProcessEnabled)) return;
            var volume = VolumeManager.instance.stack?.GetComponent<StatusScreenVolume>();
            if (volume == null || !volume.HasScreenEffect()){pass.ClearSprites(camera.camera);return;}
            var settings=new Settings(volume);
            StatusVFXRig.GetPlayerAnchor(camera.camera,out settings.playerData,out settings.playerAssigned);
            var sprites=pass.CaptureSprites(camera.camera,volume);
            if(sprites.count>0 && effectMaterial.passCount<4)
            {
                if(!warnedSpritePass)Debug.LogWarning("Update StatusScreen.shader from the complete Status VFX 2.4 patch. Interior sprites need its additional shader passes.");
                warnedSpritePass=true;
            }
            else warnedSpritePass=false;
            pass.Set(effectMaterial,settings,sprites);
            pass.ConfigureInput(ScriptableRenderPassInput.Color);
            renderer.EnqueuePass(pass);
        }
        protected override void Dispose(bool disposing) { pass?.Dispose(); pass = null; }

        public struct Settings
        {
            public StatusEdgeSnapshot edge0,edge1,edge2,edge3,edge4,edge5,edge6,edge7,edge8;
            public Vector4 playerData;
            public float playerAssigned;
            public float proceduralStrength;
            public float poisonNoiseScale;
            public float poisonMotion;
            public float fireHeight;
            public float fireTurbulence;
            public float fireSpeed;
            public float darknessNoiseScale;
            public float darknessCoverage;
            public float darknessSpeed;
            public float buffAnchorToPlayer;
            public float buffFitPlayer;
            public float buffSizeMultiplier;
            public float buffRadius;
            public float buffThickness;
            public float buffOrbitSpeed;
            public float buffMoteDensity;
            public float buffMoteSize;
            public float buffGlow;
            public float wetDensity;
            public float wetSpeed;
            public float wetSize;
            public float underwaterFrequency;
            public float underwaterSpeed;
            public float electricBoltCount;
            public float electricNoiseFrequency;
            public float electricNoiseAmplitude;
            public float electricNoiseSpeed;
            public float electricWidth;
            public float electricBranchStrength;
            public float electricFlicker;
            public float frostCrystalScale;
            public float snowAmount;
            public float snowDensity;
            public float snowSize;
            public float snowSpeed;
            public float snowDrift;
            public float bleedNoiseScale;
            public float bleedCoverage;
            public float intensity;
            public float poison;
            public float fire;
            public float darkness;
            public float buff;
            public float wet;
            public float underwater;
            public float electric;
            public float frost;
            public float bleeding;
            public Color poisonColor;
            public Color fireColor;
            public Color darknessColor;
            public Color buffColor;
            public Color waterColor;
            public Color electricColor;
            public Color frostColor;
            public Color bloodColor;
            public float edgeWidth;
            public float distortionPixels;
            public float animationSpeed;
            public float pulseAmount;
            public Settings(StatusScreenVolume v)
            {
                playerData=new Vector4(.5f,.5f,0,0);playerAssigned=0;
                intensity=v.intensity.value;
                proceduralStrength=v.proceduralStrength.value;
                poisonNoiseScale=v.poisonNoiseScale.value;
                poisonMotion=v.poisonMotion.value;
                fireHeight=v.fireHeight.value;
                fireTurbulence=v.fireTurbulence.value;
                fireSpeed=v.fireSpeed.value;
                darknessNoiseScale=v.darknessNoiseScale.value;
                darknessCoverage=v.darknessCoverage.value;
                darknessSpeed=v.darknessSpeed.value;
                buffAnchorToPlayer=v.buffAnchorToPlayer.value?1f:0f;
                buffFitPlayer=v.buffFitPlayer.value?1f:0f;
                buffSizeMultiplier=v.buffSizeMultiplier.value;
                buffRadius=v.buffRadius.value;
                buffThickness=v.buffThickness.value;
                buffOrbitSpeed=v.buffOrbitSpeed.value;
                buffMoteDensity=v.buffMoteDensity.value;
                buffMoteSize=v.buffMoteSize.value;
                buffGlow=v.buffGlow.value;
                wetDensity=v.wetDensity.value;
                wetSpeed=v.wetSpeed.value;
                wetSize=v.wetSize.value;
                underwaterFrequency=v.underwaterFrequency.value;
                underwaterSpeed=v.underwaterSpeed.value;
                electricBoltCount=v.electricBoltCount.value;
                electricNoiseFrequency=v.electricNoiseFrequency.value;
                electricNoiseAmplitude=v.electricNoiseAmplitude.value;
                electricNoiseSpeed=v.electricNoiseSpeed.value;
                electricWidth=v.electricWidth.value;
                electricBranchStrength=v.electricBranchStrength.value;
                electricFlicker=v.electricFlicker.value;
                frostCrystalScale=v.frostCrystalScale.value;
                snowAmount=v.snowAmount.value;
                snowDensity=v.snowDensity.value;
                snowSize=v.snowSize.value;
                snowSpeed=v.snowSpeed.value;
                snowDrift=v.snowDrift.value;
                bleedNoiseScale=v.bleedNoiseScale.value;
                bleedCoverage=v.bleedCoverage.value;

                poison=v.poison.value*v.poisonOverlay.value;
                fire=v.fire.value*v.fireOverlay.value;
                darkness=v.darkness.value*v.darknessOverlay.value;
                buff=v.buff.value*v.buffOverlay.value;
                wet=v.wet.value*v.wetOverlay.value;
                underwater=v.underwater.value*v.underwaterOverlay.value;
                electric=v.electric.value*v.electricOverlay.value;
                frost=v.frost.value*v.frostOverlay.value;
                bleeding=v.bleeding.value*v.bleedingOverlay.value;
                poisonColor=v.poisonColor.value;
                fireColor=v.fireColor.value;
                darknessColor=v.darknessColor.value;
                buffColor=v.buffColor.value;
                waterColor=v.waterColor.value;
                electricColor=v.electricColor.value;
                frostColor=v.frostColor.value;
                bloodColor=v.bloodColor.value;
                edgeWidth=v.edgeWidth.value;
                distortionPixels=v.distortionPixels.value;
                animationSpeed=v.animationSpeed.value;
                pulseAmount=v.pulseAmount.value;
                float edgeAmount=v.intensity.value*v.edgeImagesStrength.value;
                float edgeTime=Time.time*v.animationSpeed.value;
                edge0=new StatusEdgeSnapshot(v.poisonEdgeStyle.value,edgeAmount*v.poison.value,v.poisonColor.value,edgeTime,v.interiorSpriteBudget.value>0);
                if(edge0.replaceProcedural)poison=0;
                edge1=new StatusEdgeSnapshot(v.fireEdgeStyle.value,edgeAmount*v.fire.value,v.fireColor.value,edgeTime,v.interiorSpriteBudget.value>0);
                if(edge1.replaceProcedural)fire=0;
                edge2=new StatusEdgeSnapshot(v.darknessEdgeStyle.value,edgeAmount*v.darkness.value,v.darknessColor.value,edgeTime,v.interiorSpriteBudget.value>0);
                if(edge2.replaceProcedural)darkness=0;
                edge3=new StatusEdgeSnapshot(v.buffEdgeStyle.value,edgeAmount*v.buff.value,v.buffColor.value,edgeTime,v.interiorSpriteBudget.value>0);
                if(edge3.replaceProcedural)buff=0;
                edge4=new StatusEdgeSnapshot(v.wetEdgeStyle.value,edgeAmount*v.wet.value,v.waterColor.value,edgeTime,v.interiorSpriteBudget.value>0);
                if(edge4.replaceProcedural)wet=0;
                edge5=new StatusEdgeSnapshot(v.underwaterEdgeStyle.value,edgeAmount*v.underwater.value,v.waterColor.value,edgeTime,v.interiorSpriteBudget.value>0);
                if(edge5.replaceProcedural)underwater=0;
                edge6=new StatusEdgeSnapshot(v.electricEdgeStyle.value,edgeAmount*v.electric.value,v.electricColor.value,edgeTime,v.interiorSpriteBudget.value>0);
                if(edge6.replaceProcedural)electric=0;
                edge7=new StatusEdgeSnapshot(v.frostEdgeStyle.value,edgeAmount*v.frost.value,v.frostColor.value,edgeTime,v.interiorSpriteBudget.value>0);
                if(edge7.replaceProcedural)frost=0;
                edge8=new StatusEdgeSnapshot(v.bleedingEdgeStyle.value,edgeAmount*v.bleeding.value,v.bloodColor.value,edgeTime,v.interiorSpriteBudget.value>0);
                if(edge8.replaceProcedural)bleeding=0;

            }
            public void Apply(Material m)
            {
                m.SetVector("_PlayerData",playerData);m.SetFloat("_PlayerAssigned",playerAssigned);
                m.SetFloat("_ProceduralStrength",proceduralStrength);
                m.SetFloat("_PoisonNoiseScale",poisonNoiseScale);
                m.SetFloat("_PoisonMotion",poisonMotion);
                m.SetFloat("_FireHeight",fireHeight);
                m.SetFloat("_FireTurbulence",fireTurbulence);
                m.SetFloat("_FireSpeed",fireSpeed);
                m.SetFloat("_DarknessNoiseScale",darknessNoiseScale);
                m.SetFloat("_DarknessCoverage",darknessCoverage);
                m.SetFloat("_DarknessSpeed",darknessSpeed);
                m.SetFloat("_BuffAnchorToPlayer",buffAnchorToPlayer);
                m.SetFloat("_BuffFitPlayer",buffFitPlayer);
                m.SetFloat("_BuffSizeMultiplier",buffSizeMultiplier);
                m.SetFloat("_BuffRadius",buffRadius);
                m.SetFloat("_BuffThickness",buffThickness);
                m.SetFloat("_BuffOrbitSpeed",buffOrbitSpeed);
                m.SetFloat("_BuffMoteDensity",buffMoteDensity);
                m.SetFloat("_BuffMoteSize",buffMoteSize);
                m.SetFloat("_BuffGlow",buffGlow);
                m.SetFloat("_WetDensity",wetDensity);
                m.SetFloat("_WetSpeed",wetSpeed);
                m.SetFloat("_WetSize",wetSize);
                m.SetFloat("_UnderwaterFrequency",underwaterFrequency);
                m.SetFloat("_UnderwaterSpeed",underwaterSpeed);
                m.SetFloat("_ElectricBoltCount",electricBoltCount);
                m.SetFloat("_ElectricNoiseFrequency",electricNoiseFrequency);
                m.SetFloat("_ElectricNoiseAmplitude",electricNoiseAmplitude);
                m.SetFloat("_ElectricNoiseSpeed",electricNoiseSpeed);
                m.SetFloat("_ElectricWidth",electricWidth);
                m.SetFloat("_ElectricBranchStrength",electricBranchStrength);
                m.SetFloat("_ElectricFlicker",electricFlicker);
                m.SetFloat("_FrostCrystalScale",frostCrystalScale);
                m.SetFloat("_SnowAmount",snowAmount);
                m.SetFloat("_SnowDensity",snowDensity);
                m.SetFloat("_SnowSize",snowSize);
                m.SetFloat("_SnowSpeed",snowSpeed);
                m.SetFloat("_SnowDrift",snowDrift);
                m.SetFloat("_BleedNoiseScale",bleedNoiseScale);
                m.SetFloat("_BleedCoverage",bleedCoverage);
                m.SetFloat("_Intensity",intensity);
                m.SetFloat("_Poison",poison);
                m.SetFloat("_Fire",fire);
                m.SetFloat("_Darkness",darkness);
                m.SetFloat("_Buff",buff);
                m.SetFloat("_Wet",wet);
                m.SetFloat("_Underwater",underwater);
                m.SetFloat("_Electric",electric);
                m.SetFloat("_Frost",frost);
                m.SetFloat("_Bleeding",bleeding);
                m.SetColor("_PoisonColor",poisonColor);
                m.SetColor("_FireColor",fireColor);
                m.SetColor("_DarknessColor",darknessColor);
                m.SetColor("_BuffColor",buffColor);
                m.SetColor("_WaterColor",waterColor);
                m.SetColor("_ElectricColor",electricColor);
                m.SetColor("_FrostColor",frostColor);
                m.SetColor("_BloodColor",bloodColor);
                m.SetFloat("_EdgeWidth",edgeWidth);
                m.SetFloat("_DistortionPixels",distortionPixels);
                m.SetFloat("_AnimationSpeed",animationSpeed);
                m.SetFloat("_PulseAmount",pulseAmount);
                edge0.Apply(m,0);
                edge1.Apply(m,1);
                edge2.Apply(m,2);
                edge3.Apply(m,3);
                edge4.Apply(m,4);
                edge5.Apply(m,5);
                edge6.Apply(m,6);
                edge7.Apply(m,7);
                edge8.Apply(m,8);

            }
        }
        sealed class CelPass : ScriptableRenderPass
        {
            Material material; Settings settings; RTHandle temporary;
            Mesh spriteMesh;StatusInteriorFrame sprites;
            readonly Dictionary<int,StatusInteriorFrame> cameraFrames=new Dictionary<int,StatusInteriorFrame>();
            readonly List<int> staleCameras=new List<int>();
            public void Set(Material m, Settings s,StatusInteriorFrame frame) { material=m; settings=s;sprites=frame; }
            public StatusInteriorFrame CaptureSprites(Camera camera,StatusScreenVolume volume)
            {
                int id=camera.GetInstanceID();
                if(!cameraFrames.TryGetValue(id,out var frame) || frame.camera!=camera)
                {
                    staleCameras.Clear();
                    foreach(var pair in cameraFrames)if(pair.Value.camera==null)staleCameras.Add(pair.Key);
                    foreach(int key in staleCameras)cameraFrames.Remove(key);
                    frame=new StatusInteriorFrame(camera);cameraFrames[id]=frame;
                }
                frame.Capture(volume);
                if(frame.count>0 && spriteMesh==null)spriteMesh=StatusInteriorFrame.CreateMesh();
                return frame;
            }
            public void ClearSprites(Camera camera)
            {if(camera!=null && cameraFrames.TryGetValue(camera.GetInstanceID(),out var frame))frame.Clear();}
            public void Dispose()
            {
                temporary?.Release();temporary=null;CoreUtils.Destroy(spriteMesh);spriteMesh=null;
                cameraFrames.Clear();staleCameras.Clear();sprites=null;
            }
            sealed class PassData
            {public TextureHandle source;public Material material;public Settings settings;public StatusInteriorFrame sprites;public Mesh spriteMesh;}

            public override void RecordRenderGraph(RenderGraph graph, ContextContainer frameData)
            {
                var resources=frameData.Get<UniversalResourceData>();
                if (resources.isActiveTargetBackBuffer || material == null) return;
                var source=resources.activeColorTexture;
                var desc=graph.GetTextureDesc(source);desc.name="StatusScreen Color";desc.clearBuffer=false;desc.msaaSamples=MSAASamples.None;desc.bindTextureMS=false;
                var destination=graph.CreateTexture(desc);
                using (var builder=graph.AddRasterRenderPass<PassData>("StatusScreen V2.4",out var data))
                {
                    data.source=source;data.material=material;data.settings=settings;data.sprites=sprites;data.spriteMesh=spriteMesh;
                    builder.UseTexture(source,AccessFlags.Read);
                    builder.SetRenderAttachment(destination,0,AccessFlags.Write);
                    builder.SetRenderFunc((PassData d,RasterGraphContext ctx)=>
                    {
                        d.settings.Apply(d.material);
                        Blitter.BlitTexture(ctx.cmd,d.source,new Vector4(1,1,0,0),d.material,0);
                        if(d.sprites!=null && d.spriteMesh!=null && d.material.passCount>=4)
                        for(int i=0;i<d.sprites.count;i++)
                        {
                            var item=d.sprites.items[i];if(item.Count==0)continue;item.Apply(d.sprites.block);
                            ctx.cmd.DrawMesh(d.spriteMesh,Matrix4x4.identity,d.material,0,item.ShaderPass,d.sprites.block);
                        }
                    });
                }
                resources.cameraColor=destination;
            }

            // Also supports the Unity 6 Compatibility Mode path.
            public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData data)
            {
                var desc=data.cameraData.cameraTargetDescriptor;desc.depthBufferBits=0;desc.msaaSamples=1;desc.bindMS=false;
                RenderingUtils.ReAllocateHandleIfNeeded(ref temporary,desc,FilterMode.Bilinear,TextureWrapMode.Clamp,name:"StatusScreenTemporary");
            }
            public override void Execute(ScriptableRenderContext context,ref RenderingData data)
            {
                if (material==null || temporary==null) return;
                var source=data.cameraData.renderer.cameraColorTargetHandle;
                var cmd=CommandBufferPool.Get("StatusScreen V2.4");
                try
                {
                    settings.Apply(material);
                    Blitter.BlitCameraTexture(cmd,source,temporary,material,0);
                    if(sprites!=null && spriteMesh!=null && material.passCount>=4)
                    for(int i=0;i<sprites.count;i++)
                    {
                        var item=sprites.items[i];if(item.Count==0)continue;item.Apply(sprites.block);
                        cmd.DrawMesh(spriteMesh,Matrix4x4.identity,material,0,item.ShaderPass,sprites.block);
                    }
                    Blitter.BlitCameraTexture(cmd,temporary,source);
                    context.ExecuteCommandBuffer(cmd);
                }
                finally { CommandBufferPool.Release(cmd); }
            }
        }
    }
}
