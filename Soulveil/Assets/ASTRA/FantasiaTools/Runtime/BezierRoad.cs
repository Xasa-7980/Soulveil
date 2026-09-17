using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
namespace FantasiaTools
{
 [Serializable]
 public sealed class RoadKnot
 {
  public Vector3 position;
  public Vector3 tangentIn=new Vector3(0,0,-3);
  public Vector3 tangentOut=new Vector3(0,0,3);
  [Min(.1f)] public float width=3;
  public RoadKnot(Vector3 p){position=p;}
 }
 [ExecuteAlways,DisallowMultipleComponent,RequireComponent(typeof(MeshFilter),typeof(MeshRenderer),typeof(MeshCollider))]
 public sealed class BezierRoad:MonoBehaviour
 {
  public List<RoadKnot> knots=new List<RoadKnot>{new RoadKnot(Vector3.zero),new RoadKnot(new Vector3(3,0,9)),new RoadKnot(new Vector3(-2,0,18)),new RoadKnot(new Vector3(0,0,27))};
  public bool closed;
  [Range(.15f,4)] public float metresPerSegment=.5f;
  [Min(.1f)] public float metresPerTextureRepeat=2;
  [Range(0,.4f)] public float crown=.06f;
  public bool generateCollider=true;
  [Tooltip("Optional. Assign one Terrain and the road follows its height; the Terrain itself is not modified.")]
  public Terrain terrain;
  [Range(.01f,.5f)] public float surfaceOffset=.04f;
  Mesh generated;bool dirty=true;Matrix4x4 lastMatrix;Terrain lastTerrain;Vector3 lastTerrainPosition;Vector3 lastTerrainSize;
  public void MarkDirty()=>dirty=true;
  void OnValidate()=>dirty=true;
  void OnEnable()=>dirty=true;
  void Update()
  {
   var matrix=transform.localToWorldMatrix;
   var terrainPosition=terrain!=null?terrain.transform.position:Vector3.zero;
   var terrainSize=terrain!=null && terrain.terrainData!=null?terrain.terrainData.size:Vector3.zero;
   if(dirty || matrix!=lastMatrix || lastTerrain!=terrain || terrainPosition!=lastTerrainPosition || terrainSize!=lastTerrainSize)
   {Rebuild();lastMatrix=matrix;lastTerrain=terrain;lastTerrainPosition=terrainPosition;lastTerrainSize=terrainSize;}
  }
  public Vector3 Evaluate(int span,float t)
  {
   int next=(span+1)%knots.Count;var a=knots[span];var b=knots[next];float u=1-t;
   return u*u*u*a.position+3*u*u*t*(a.position+a.tangentOut)+3*u*t*t*(b.position+b.tangentIn)+t*t*t*b.position;
  }
  struct Sample{public Vector3 world;public float width;public Sample(Vector3 p,float w){world=p;width=w;}}
  [ContextMenu("Rebuild road")]
  public void Rebuild()
  {
   dirty=false;
   if(knots==null || knots.Count<2 || knots.Exists(k=>k==null)){ClearMesh();return;}
   int spans=closed?knots.Count:knots.Count-1;
   var raw=new List<Sample>();var lengths=new List<float>();float distance=0;
   // Dense Bezier samples, then resample by world distance for even topology and UVs.
   for(int span=0;span<spans;span++)
   {
    var a=knots[span];var b=knots[(span+1)%knots.Count];
    float estimate=Vector3.Distance(a.position,a.position+a.tangentOut)+Vector3.Distance(a.position+a.tangentOut,b.position+b.tangentIn)+Vector3.Distance(b.position+b.tangentIn,b.position);
    int steps=Mathf.Clamp(Mathf.CeilToInt(estimate*8),24,512);
    for(int j=span==0?0:1;j<=steps;j++)
    {
     float t=(float)j/steps;Vector3 p=transform.TransformPoint(Evaluate(span,t));
     if(raw.Count>0)distance+=Vector3.Distance(raw[raw.Count-1].world,p);
     raw.Add(new Sample(p,Mathf.Lerp(Mathf.Max(.1f,a.width),Mathf.Max(.1f,b.width),t)));lengths.Add(distance);
    }
   }
   if(distance<.01f){ClearMesh();return;}
   int segments=Mathf.Clamp(Mathf.CeilToInt(distance/Mathf.Max(.15f,metresPerSegment)),2,8192);
   var centers=new Vector3[segments+1];var widths=new float[segments+1];int cursor=1;
   for(int i=0;i<=segments;i++)
   {
    float d=distance*i/segments;
    while(cursor<lengths.Count-1 && lengths[cursor]<d)cursor++;
    float t=Mathf.InverseLerp(lengths[cursor-1],lengths[cursor],d);
    centers[i]=Vector3.Lerp(raw[cursor-1].world,raw[cursor].world,t);widths[i]=Mathf.Lerp(raw[cursor-1].width,raw[cursor].width,t);
   }
   const int across=5;var vertices=new Vector3[(segments+1)*across];var uv=new Vector2[vertices.Length];var indices=new int[segments*(across-1)*6];
   float[] lateral={-.5f,-.46f,0,.46f,.5f};Vector3 lastRight=transform.right;
   for(int i=0;i<=segments;i++)
   {
    Vector3 tangent=closed && (i==0 || i==segments)?centers[1]-centers[segments-1]:centers[Mathf.Min(segments,i+1)]-centers[Mathf.Max(0,i-1)];
    Vector3 right=Vector3.Cross(Vector3.up,tangent).normalized;if(right.sqrMagnitude<.001f)right=lastRight;lastRight=right;
    for(int x=0;x<across;x++)
    {
     Vector3 world=centers[i]+right*lateral[x]*widths[i];
     if(terrain!=null && terrain.terrainData!=null)
     {
      Vector3 p=world-terrain.transform.position;Vector3 size=terrain.terrainData.size;
      if(p.x>=0 && p.z>=0 && p.x<=size.x && p.z<=size.z)world.y=terrain.SampleHeight(world)+terrain.transform.position.y;
     }
     world.y+=surfaceOffset+crown*(1-4*lateral[x]*lateral[x]);
     int v=i*across+x;vertices[v]=transform.InverseTransformPoint(world);uv[v]=new Vector2(lateral[x]+.5f,distance*i/segments/Mathf.Max(.1f,metresPerTextureRepeat));
    }
    if(i==segments)continue;
    for(int x=0;x<across-1;x++)
    {
     int a=i*across+x,b=a+across,k=(i*(across-1)+x)*6;
     indices[k]=a;indices[k+1]=b;indices[k+2]=a+1;indices[k+3]=a+1;indices[k+4]=b;indices[k+5]=b+1;
    }
   }
   if(generated==null)generated=new Mesh{name="Bezier road generated",hideFlags=HideFlags.DontSave};
   var collider=GetComponent<MeshCollider>();collider.sharedMesh=null;
   generated.Clear();generated.indexFormat=vertices.Length>65535?IndexFormat.UInt32:IndexFormat.UInt16;
   generated.vertices=vertices;generated.uv=uv;generated.triangles=indices;generated.RecalculateNormals();generated.RecalculateTangents();generated.RecalculateBounds();
   GetComponent<MeshFilter>().sharedMesh=generated;collider.enabled=generateCollider;if(generateCollider)collider.sharedMesh=generated;
  }
  void ClearMesh(){var f=GetComponent<MeshFilter>();var c=GetComponent<MeshCollider>();if(f!=null)f.sharedMesh=null;if(c!=null)c.sharedMesh=null;}
  void OnDisable(){ClearMesh();if(generated!=null){if(Application.isPlaying)Destroy(generated);else DestroyImmediate(generated);}generated=null;}
 }
}
