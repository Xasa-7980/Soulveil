using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace HarmonieFX
{
 [DisallowMultipleComponent]
 public sealed class HarmonieWindLayer : MonoBehaviour
 {
  [Header("Selection (applied in Play Mode)")]
  public LayerMask vegetationLayers;
  [Tooltip("Optional: search only below these roots. Empty searches loaded scenes once.")]
  public Transform[] roots;
  [Tooltip("Assign Materials/Vegetation_Wind.mat. Source albedo, tint and normal map are copied.")]
  public Material windTemplate;
  public bool refreshOnSceneLoad=true;
  [Header("Wind")]
  public Camera viewCamera;
  public Vector2 direction=new Vector2(1,.35f);
  [Range(0,1.5f)] public float amplitude=.18f;
  [Range(0,5)] public float speed=1.1f;
  [Range(0,1)] public float gustStrength=.35f;
  [Range(.001f,1)] public float gustScale=.08f;
  [Min(0)] public float fadeStart=35;
  [Min(.1f)] public float fadeEnd=85;
  [Header("Conversion")]
  public bool twoSided=true;
  public bool alphaCutout=true;
  [Range(0,1)] public float fallbackCutoff=.4f;
  [Range(0,1)] public float leafBacklight=.25f;
  [Tooltip("0: mesh height; 1: UV.y; 2: vertex red. The root mask must be zero.")]
  [Range(0,2)] public int maskMode=0;
  [Tooltip("Conservative local bounds expansion supports up to 1.5 metres of wind amplitude.")]
  public bool expandBounds=true;
  public int BoundRenderers=>bindings.Count;
  static HarmonieWindLayer active;
  static readonly int Wind=Shader.PropertyToID("_HWind"),Gust=Shader.PropertyToID("_HWindGust"),Focus=Shader.PropertyToID("_HWindFocus"),Fade=Shader.PropertyToID("_HWindFade");
  sealed class Binding{public MeshRenderer renderer;public Material[] original,assigned;public Bounds bounds;public bool expanded;}
  readonly List<Binding> bindings=new List<Binding>();
  readonly Dictionary<(int,int,int),Material> materials=new Dictionary<(int,int,int),Material>();
  void OnEnable()
  {
   if(active!=null && active!=this){Debug.LogWarning("Only one HarmonieWindLayer controller should publish global wind.",this);enabled=false;return;}
   active=this;if(viewCamera==null)viewCamera=Camera.main;
   SceneManager.sceneLoaded+=SceneLoaded;RefreshSelection();Publish();
  }
  void SceneLoaded(Scene scene,LoadSceneMode mode){if(refreshOnSceneLoad)RefreshSelection();}
  [ContextMenu("Refresh selected vegetation (Play Mode)")]
  public void RefreshSelection()
  {
   if(!Application.isPlaying || active!=this)return;
   Restore();
   if(windTemplate==null || vegetationLayers.value==0)return;
   var candidates=new HashSet<MeshRenderer>();
   if(roots!=null && roots.Length>0)
   {
    foreach(var root in roots)if(root!=null)foreach(var r in root.GetComponentsInChildren<MeshRenderer>(true))candidates.Add(r);
   }
   else foreach(var r in Object.FindObjectsByType<MeshRenderer>(FindObjectsInactive.Include,FindObjectsSortMode.None))candidates.Add(r);
   int skipped=0;
   foreach(var r in candidates)
   {
    if(r==null || (vegetationLayers.value&(1<<r.gameObject.layer))==0)continue;
    if(r.isPartOfStaticBatch){skipped++;continue;}
    var mf=r.GetComponent<MeshFilter>();if(mf==null || mf.sharedMesh==null)continue;
    Bounds meshBounds=mf.sharedMesh.bounds;Material[] original=r.sharedMaterials;Material[] assigned=new Material[original.Length];
    int rootKey=Mathf.RoundToInt(meshBounds.min.y*1000),heightKey=Mathf.RoundToInt(Mathf.Max(meshBounds.size.y,.001f)*1000);
    for(int i=0;i<original.Length;i++)
    {
     Material source=original[i];if(source==null)continue;
     var key=(source.GetInstanceID(),rootKey,heightKey);
     if(!materials.TryGetValue(key,out var mat))
     {
      mat=new Material(windTemplate){name=source.name+" [Harmonie runtime]",enableInstancing=true};
      string baseTex=source.HasProperty("_BaseMap")?"_BaseMap":source.HasProperty("_MainTex")?"_MainTex":null;
      if(baseTex!=null){mat.SetTexture("_BaseMap",source.GetTexture(baseTex));mat.SetTextureScale("_BaseMap",source.GetTextureScale(baseTex));mat.SetTextureOffset("_BaseMap",source.GetTextureOffset(baseTex));}
      if(source.HasProperty("_BaseColor"))mat.SetColor("_BaseColor",source.GetColor("_BaseColor"));
      else if(source.HasProperty("_Color"))mat.SetColor("_BaseColor",source.GetColor("_Color"));
      if(source.HasProperty("_BumpMap"))mat.SetTexture("_BumpMap",source.GetTexture("_BumpMap"));
      if(source.HasProperty("_BumpScale"))mat.SetFloat("_BumpScale",source.GetFloat("_BumpScale"));
      if(source.HasProperty("_Smoothness"))mat.SetFloat("_Smoothness",source.GetFloat("_Smoothness"));
      mat.SetFloat("_Cutoff",source.HasProperty("_Cutoff")?source.GetFloat("_Cutoff"):fallbackCutoff);
      mat.SetFloat("_AlphaClip",alphaCutout?1:0);mat.SetFloat("_Cull",twoSided?0:2);
      mat.SetFloat("_RootY",rootKey*.001f);mat.SetFloat("_PlantHeight",Mathf.Max(heightKey*.001f,.001f));
      mat.SetFloat("_MaskMode",maskMode);mat.SetFloat("_Transmission",leafBacklight);
      materials.Add(key,mat);
     }
     assigned[i]=mat;
    }
    var binding=new Binding{renderer=r,original=original,assigned=assigned,bounds=r.localBounds,expanded=expandBounds};
    r.sharedMaterials=assigned;
    if(expandBounds)
    {
     Vector3 scale=r.transform.lossyScale;float smallest=Mathf.Max(.001f,Mathf.Min(Mathf.Abs(scale.x),Mathf.Min(Mathf.Abs(scale.y),Mathf.Abs(scale.z))));
     var bound=r.localBounds;bound.Expand(3f/smallest);r.localBounds=bound;
    }
    bindings.Add(binding);
   }
   if(skipped>0)Debug.LogWarning($"Harmonie skipped {skipped} statically batched renderers. Disable Batching Static on vegetation before entering Play Mode.",this);
  }
  void Update(){Publish();}
  void Publish()
  {
   if(active!=this)return;
   Vector2 d=direction.sqrMagnitude>.00001f?direction.normalized:Vector2.right;
   Shader.SetGlobalVector(Wind,new Vector4(d.x,d.y,Mathf.Clamp(amplitude,0,1.5f),speed));
   Shader.SetGlobalVector(Gust,new Vector4(gustStrength,gustScale,0,0));
   Vector3 p=viewCamera!=null?viewCamera.transform.position:transform.position;
   Shader.SetGlobalVector(Focus,new Vector4(p.x,p.y,p.z,1));Shader.SetGlobalVector(Fade,new Vector4(fadeStart,Mathf.Max(fadeStart+.1f,fadeEnd),0,0));
  }
  void Restore()
  {
   foreach(var b in bindings)if(b.renderer!=null)
   {
    var current=b.renderer.sharedMaterials;
    if(current.Length==b.assigned.Length)
    {
     for(int i=0;i<current.Length;i++)if(current[i]==b.assigned[i])current[i]=b.original[i];
     b.renderer.sharedMaterials=current;
    }
    if(b.expanded)b.renderer.localBounds=b.bounds;
   }
   bindings.Clear();foreach(var m in materials.Values)if(m!=null)Destroy(m);materials.Clear();
  }
  void OnDisable()
  {
   SceneManager.sceneLoaded-=SceneLoaded;Restore();
   if(active==this){Shader.SetGlobalVector(Wind,Vector4.zero);active=null;}
  }
 }
}
