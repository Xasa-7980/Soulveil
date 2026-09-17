using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace AtmosferaAnime
{
 public enum StatusInteriorPlacement { Area, ScreenEdges }
 [Serializable]
 public sealed class StatusInteriorSpriteLayer
 {
  public string name="Interior sprites";
  public bool enabled=true;
  [Header("Image / animated texture")]
  public Sprite sprite;
  public Texture2D texture;
  public StatusSpriteShape fallbackShape=StatusSpriteShape.SoftGlow;
  [Range(1,64)] public int columns=1;
  [Range(1,64)] public int rows=1;
  [Range(0,60)] public float framesPerSecond=12;
  [Min(0)] public int startFrame;
  public bool synchronizeFrames=true;
  public bool randomStartFrame;
  [Header("Appearance")]
  [Range(1,64)] public int count=12;
  [Range(0,1)] public float opacity=1;
  [ColorUsage(true,true)] public Color tint=Color.white;
  public bool useStatusColor;
  public StatusEdgeBlend blend=StatusEdgeBlend.Alpha;
  public StatusEdgeAlpha alphaSource=StatusEdgeAlpha.TextureAlpha;
  public bool maskOnly;
  [Tooltip("Minimum and maximum HEIGHT as a percentage of the screen. 8 means 8%, not 8 metres.")]
  public Vector2 sizePercent=new Vector2(4,8);
  public bool keepImageAspect=true;
  public AnimationCurve sizeOverLifetime=new AnimationCurve(new Keyframe(0,.5f),new Keyframe(.2f,1),new Keyframe(1,.6f));
  public Color startColor=Color.white;
  public Color endColor=Color.white;
  [Header("Particle System modes (optional upgrade)")]
  public bool useParticleCurves;
  [Tooltip("Height in percent of screen. Curve time = normalised particle age. A random factor is kept stable for each particle life.")]
  public ParticleSystem.MinMaxCurve heightMode=new ParticleSystem.MinMaxCurve(4,8);
  public ParticleSystem.MinMaxGradient colorMode=new ParticleSystem.MinMaxGradient(Color.white);
  [Header("Motion / life")]
  public Vector2 lifetime=new Vector2(2,4);
  [Range(0,.9f)] public float fadeInFraction=.12f;
  [Range(0,.9f)] public float fadeOutFraction=.25f;
  public StatusInteriorPlacement placement=StatusInteriorPlacement.Area;
  [Tooltip("Viewport percentages. (50,50) is the centre of the screen.")]
  public Vector2 centerPercent=new Vector2(50,50);
  [Tooltip("Width and height of the spawn area, in viewport percentages.")]
  public Vector2 areaPercent=new Vector2(85,85);
  [Range(1,40)] public float edgeWidthPercent=8;
  [Tooltip("Viewport percent per second on X / Y. Negative Y makes flakes fall.")]
  public Vector2 velocityPercent=new Vector2(0,5);
  [Range(0,20)] public float driftPercent=1.5f;
  [Range(0,10)] public float driftSpeed=1;
  public Vector2 rotationDegrees=new Vector2(0,360);
  public Vector2 rotationSpeedDegrees=new Vector2(-25,25);
  public bool wrapAround=true;
  public int seed=1729;
  public bool TryGetSource(out Texture2D source,out Vector4 rect,out bool textured)
  {
   rect=new Vector4(0,0,1,1);source=texture;textured=sprite!=null || texture!=null;
   if(sprite!=null)
   {
    if(sprite.packed && (sprite.packingMode==SpritePackingMode.Tight || sprite.packingRotation!=SpritePackingRotation.None)){source=null;return false;}
    source=sprite.texture;if(source==null)return false;
    Rect r=sprite.textureRect;rect=new Vector4(r.x/source.width,r.y/source.height,r.width/source.width,r.height/source.height);
   }
   if(!textured && fallbackShape!=StatusSpriteShape.Texture)source=Texture2D.whiteTexture;
   return source!=null && source.width>0 && source.height>0 && rect.z>0 && rect.w>0;
  }
  public bool HasVisual()=>enabled && count>0 && opacity>0 && tint.a>0 && TryGetSource(out _,out _,out _);
 }

 public struct StatusInteriorSnapshot
 {
  public Texture2D texture;
  public Vector4 rect,tint,startColor,endColor,timeLife,size,area,motion,rotation,animation,options,alpha,draw,extra,curve0,curve1;
  public Vector4[] samples;
  public int Count=>Mathf.RoundToInt(draw.x);
  public int ShaderPass=>1+Mathf.Clamp(Mathf.RoundToInt(draw.w),0,2);
  static readonly string[] names={"Rect","Tint","StartColor","EndColor","TimeLife","Size","Area","Motion","Rotation","Animation","Options","Alpha","Draw","Extra","Curve0","Curve1"};
  static readonly int[] ids=BuildIDs();
  static readonly int textureID=Shader.PropertyToID("_SITexture");
  static int[] BuildIDs(){var result=new int[names.Length];for(int i=0;i<result.Length;i++)result[i]=Shader.PropertyToID("_SI"+names[i]);return result;}
  static Vector2 Ordered(Vector2 v,float min)=>new Vector2(Mathf.Max(min,Mathf.Min(v.x,v.y)),Mathf.Max(min,Mathf.Max(v.x,v.y)));
  static float Curve(AnimationCurve c,float t)=>c==null || c.length==0?1:Mathf.Max(0,c.Evaluate(t));
  public StatusInteriorSnapshot(StatusInteriorSpriteLayer c,float amount,Color stateColor,float time,Vector4[] cache=null)
  {
   this=default;
   if(c==null || !c.HasVisual() || !c.TryGetSource(out texture,out rect,out bool textured))return;
   Vector2 life=Ordered(c.lifetime,.05f),sizes=Ordered(c.sizePercent,.01f)*.01f;
   int cols=Mathf.Clamp(c.columns,1,64),rows=Mathf.Clamp(c.rows,1,64);
   tint=c.tint*(c.useStatusColor?stateColor:Color.white);startColor=c.startColor;endColor=c.endColor;
   timeLife=new Vector4(time,life.x,life.y,c.seed);
   size=new Vector4(sizes.x,sizes.y,c.keepImageAspect && textured?(texture.width*rect.z/cols)/(texture.height*rect.w/rows):1,0);
   area=new Vector4(c.centerPercent.x*.01f,c.centerPercent.y*.01f,Mathf.Abs(c.areaPercent.x)*.01f,Mathf.Abs(c.areaPercent.y)*.01f);
   motion=new Vector4(c.velocityPercent.x*.01f,c.velocityPercent.y*.01f,Mathf.Max(0,c.driftPercent)*.01f,Mathf.Max(0,c.driftSpeed));
   rotation=new Vector4(c.rotationDegrees.x,c.rotationDegrees.y,c.rotationSpeedDegrees.x,c.rotationSpeedDegrees.y)*Mathf.Deg2Rad;
   animation=new Vector4(cols,rows,Mathf.Max(0,c.framesPerSecond),Mathf.Max(0,c.startFrame));
   options=new Vector4(c.synchronizeFrames?1:0,c.randomStartFrame?1:0,(float)c.placement,Mathf.Clamp(c.edgeWidthPercent,1,40)*.01f);
   alpha=new Vector4(Mathf.Clamp(c.fadeInFraction,.001f,.9f),Mathf.Clamp(c.fadeOutFraction,.001f,.9f),(float)c.alphaSource,c.maskOnly?1:0);
   draw=new Vector4(Mathf.Clamp(c.count,1,64),Mathf.Clamp01(amount*c.opacity),textured?0:(int)c.fallbackShape,(float)c.blend);
   extra=new Vector4(.5f*cols/(texture.width*rect.z),.5f*rows/(texture.height*rect.w),c.wrapAround?1:0,0);
   curve0=new Vector4(Curve(c.sizeOverLifetime,0),Curve(c.sizeOverLifetime,1f/7),Curve(c.sizeOverLifetime,2f/7),Curve(c.sizeOverLifetime,3f/7));
   curve1=new Vector4(Curve(c.sizeOverLifetime,4f/7),Curve(c.sizeOverLifetime,5f/7),Curve(c.sizeOverLifetime,6f/7),Curve(c.sizeOverLifetime,1));
   // 32 samples for each endpoint; arrays are cached per camera/layer.
   samples=cache!=null && cache.Length==96?cache:new Vector4[96];
   for(int i=0;i<32;i++)
   {
    float age=i/31f;
    float lo=c.useParticleCurves?Mathf.Max(0,c.heightMode.Evaluate(age,0))*.01f:sizes.x*Curve(c.sizeOverLifetime,age);
    float hi=c.useParticleCurves?Mathf.Max(0,c.heightMode.Evaluate(age,1))*.01f:sizes.y*Curve(c.sizeOverLifetime,age);
    samples[i]=new Vector4(lo,hi,0,0);
    if(c.useParticleCurves)
    {
     // RandomColor samples a gradient by a stable random value, not by particle age.
     bool random=c.colorMode.mode==ParticleSystemGradientMode.RandomColor;
     samples[32+i]=c.colorMode.Evaluate(age,random?age:0);
     samples[64+i]=c.colorMode.Evaluate(age,random?age:1);
    }
    else samples[32+i]=samples[64+i]=Color.LerpUnclamped(c.startColor,c.endColor,age);
   }
   size.w=c.useParticleCurves && c.colorMode.mode==ParticleSystemGradientMode.RandomColor?1:0;
  }
  public void SetCount(int value){draw.x=value;}
  public void Apply(MaterialPropertyBlock b)
  {
   b.Clear();b.SetTexture(textureID,texture);b.SetVectorArray("_SISamples",samples);
   b.SetVector(ids[0],rect);b.SetVector(ids[1],tint);b.SetVector(ids[2],startColor);b.SetVector(ids[3],endColor);
   b.SetVector(ids[4],timeLife);b.SetVector(ids[5],size);b.SetVector(ids[6],area);b.SetVector(ids[7],motion);
   b.SetVector(ids[8],rotation);b.SetVector(ids[9],animation);b.SetVector(ids[10],options);b.SetVector(ids[11],alpha);
   b.SetVector(ids[12],draw);b.SetVector(ids[13],extra);b.SetVector(ids[14],curve0);b.SetVector(ids[15],curve1);
  }
 }

 // One buffer per camera, reused between frames. No particle GameObjects or rig.
 public sealed class StatusInteriorFrame
 {
  public readonly Camera camera;
  public readonly StatusInteriorSnapshot[] items=new StatusInteriorSnapshot[36];
  public readonly MaterialPropertyBlock block=new MaterialPropertyBlock();
  readonly int[] requested=new int[36];
  readonly Vector4[][] sampleCache=new Vector4[36][];
  public int count;
  public StatusInteriorFrame(Camera camera){this.camera=camera;}
  void Add(StatusEdgeStyle style,float amount,Color color,float time)
  {
   if(style==null || style.interiorSprites==null || style.opacity<=0 || amount<=.0001f)return;
   int accepted=0;
   foreach(var layer in style.interiorSprites)
   {
    if(accepted>=4 || count>=items.Length)break;
    if(layer==null || !layer.HasVisual())continue;
    if(sampleCache[count]==null)sampleCache[count]=new Vector4[96];
    items[count]=new StatusInteriorSnapshot(layer,amount*style.opacity,color,time,sampleCache[count]);
    requested[count]=items[count].Count;count++;accepted++;
   }
  }
  public void Clear(){for(int i=0;i<count;i++)items[i]=default;count=0;block.Clear();}
  public void Capture(StatusScreenVolume v)
  {
   Clear();
   int budget=Mathf.Clamp(v.interiorSpriteBudget.value,0,1024);if(budget==0)return;
   float amount=v.intensity.value*v.edgeImagesStrength.value,time=Time.time*v.animationSpeed.value;
   Add(v.poisonEdgeStyle.value,amount*v.poison.value,v.poisonColor.value,time);
   Add(v.fireEdgeStyle.value,amount*v.fire.value,v.fireColor.value,time);
   Add(v.darknessEdgeStyle.value,amount*v.darkness.value,v.darknessColor.value,time);
   Add(v.buffEdgeStyle.value,amount*v.buff.value,v.buffColor.value,time);
   Add(v.wetEdgeStyle.value,amount*v.wet.value,v.waterColor.value,time);
   Add(v.underwaterEdgeStyle.value,amount*v.underwater.value,v.waterColor.value,time);
   Add(v.electricEdgeStyle.value,amount*v.electric.value,v.electricColor.value,time);
   Add(v.frostEdgeStyle.value,amount*v.frost.value,v.frostColor.value,time);
   Add(v.bleedingEdgeStyle.value,amount*v.bleeding.value,v.bloodColor.value,time);
   int sum=0;for(int i=0;i<count;i++)sum+=requested[i];
   if(sum<=budget)return;
   int assigned=0;
   for(int i=0;i<count;i++){int n=Mathf.FloorToInt((float)requested[i]*budget/sum);items[i].SetCount(n);assigned+=n;}
   for(int i=0;i<count && assigned<budget;i++)if(items[i].Count<requested[i]){items[i].SetCount(items[i].Count+1);assigned++;}
  }
  public static Mesh CreateMesh()
  {
   const int particles=64;var vertices=new Vector3[particles*4];var uv=new Vector2[particles*4];var ids=new Vector2[particles*4];var triangles=new int[particles*6];
   for(int p=0;p<particles;p++)
   {
    int v=p*4,t=p*6;
    vertices[v]=new Vector3(-.5f,-.5f,0);vertices[v+1]=new Vector3(.5f,-.5f,0);vertices[v+2]=new Vector3(.5f,.5f,0);vertices[v+3]=new Vector3(-.5f,.5f,0);
    uv[v]=new Vector2(0,0);uv[v+1]=new Vector2(1,0);uv[v+2]=new Vector2(1,1);uv[v+3]=new Vector2(0,1);
    for(int k=0;k<4;k++)ids[v+k]=new Vector2(p,0);
    triangles[t]=v;triangles[t+1]=v+1;triangles[t+2]=v+2;triangles[t+3]=v;triangles[t+4]=v+2;triangles[t+5]=v+3;
   }
   var mesh=new Mesh{name="Status screen sprite quads",hideFlags=HideFlags.HideAndDontSave};
   mesh.vertices=vertices;mesh.uv=uv;mesh.uv2=ids;mesh.triangles=triangles;mesh.bounds=new Bounds(Vector3.zero,new Vector3(2,2,1));mesh.UploadMeshData(true);return mesh;
  }
 }
}
