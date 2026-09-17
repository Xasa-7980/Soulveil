using UnityEditor;
using UnityEngine;
namespace FantasiaTools.Editor
{
 [CustomEditor(typeof(BezierRoad))]
 public sealed class BezierRoadEditor:UnityEditor.Editor
 {
  [MenuItem("GameObject/Fantasia/Camino Bezier",false,10)]
  static void Create()
  {
   var go=new GameObject("Camino Bezier");Undo.RegisterCreatedObjectUndo(go,"Create road");go.AddComponent<BezierRoad>();
   var ids=AssetDatabase.FindAssets("Road_Stone t:Material");if(ids.Length>0)go.GetComponent<MeshRenderer>().sharedMaterial=AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(ids[0]));
   Selection.activeGameObject=go;
  }
  public override void OnInspectorGUI()
  {
   EditorGUILayout.HelpBox("Selecciona un nudo en Scene: punto azul = posicion, asas amarillas = tangentes. La malla y el collider siguen la curva al editar. Width se mide en metros. Evita cruces y curvas mas cerradas que el ancho.",MessageType.Info);
   if(DrawDefaultInspector())((BezierRoad)target).MarkDirty();
   var road=(BezierRoad)target;
   if(GUILayout.Button("Anadir nudo al final"))
   {
    Undo.RecordObject(road,"Add road knot");
    Vector3 p=road.knots.Count>0?road.knots[road.knots.Count-1].position+new Vector3(0,0,6):Vector3.zero;
    road.knots.Add(new RoadKnot(p));road.MarkDirty();EditorUtility.SetDirty(road);
   }
   if(GUILayout.Button("Actualizar despues de esculpir Terrain"))road.MarkDirty();
   if(GUILayout.Button("Guardar copia de la malla"))
   {
    road.Rebuild();var mesh=road.GetComponent<MeshFilter>().sharedMesh;if(mesh==null)return;
    string path=EditorUtility.SaveFilePanelInProject("Guardar malla","Camino_Baked","asset","Elige donde guardar una copia reutilizable.");
    if(!string.IsNullOrEmpty(path)){var copy=Instantiate(mesh);copy.hideFlags=HideFlags.None;AssetDatabase.CreateAsset(copy,path);AssetDatabase.SaveAssets();}
   }
  }
  void OnSceneGUI()
  {
   var road=(BezierRoad)target;if(road.knots==null)return;var t=road.transform;
   for(int i=0;i<road.knots.Count;i++)
   {
    var knot=road.knots[i];if(knot==null)continue;Vector3 p=t.TransformPoint(knot.position);
    Handles.color=new Color(.3f,.8f,1);Handles.Label(p+Vector3.up*.3f,"Nudo "+i);
    EditorGUI.BeginChangeCheck();Vector3 moved=Handles.PositionHandle(p,Quaternion.identity);
    Handles.color=Color.yellow;Vector3 a=t.TransformPoint(knot.position+knot.tangentIn),b=t.TransformPoint(knot.position+knot.tangentOut);
    Handles.DrawLine(p,a);Handles.DrawLine(p,b);
    float size=HandleUtility.GetHandleSize(p)*.08f;
    Vector3 ai=Handles.FreeMoveHandle(a,size,Vector3.zero,Handles.SphereHandleCap),bo=Handles.FreeMoveHandle(b,size,Vector3.zero,Handles.SphereHandleCap);
    if(EditorGUI.EndChangeCheck())
    {
     Undo.RecordObject(road,"Edit Bezier road");
     // Tangents are offsets, so moving a knot carries both handles with it.
     if(moved!=p)knot.position=t.InverseTransformPoint(moved);
     else{knot.tangentIn=t.InverseTransformPoint(ai)-knot.position;knot.tangentOut=t.InverseTransformPoint(bo)-knot.position;}
     road.MarkDirty();EditorUtility.SetDirty(road);
    }
   }
   Handles.color=new Color(.3f,.85f,1);
   int spans=road.closed?road.knots.Count:road.knots.Count-1;
   for(int i=0;i<spans;i++)
   {var a=road.knots[i];var b=road.knots[(i+1)%road.knots.Count];if(a==null || b==null)continue;Handles.DrawBezier(t.TransformPoint(a.position),t.TransformPoint(b.position),t.TransformPoint(a.position+a.tangentOut),t.TransformPoint(b.position+b.tangentIn),Handles.color,null,3);}
  }
 }
}
