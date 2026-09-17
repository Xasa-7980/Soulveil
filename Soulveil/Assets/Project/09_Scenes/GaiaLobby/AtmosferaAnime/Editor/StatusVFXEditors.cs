using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace AtmosferaAnime.Editor
{
 internal sealed class StatusVFXPreviewPanel
 {
  string selectedId="";
  readonly StatusLibraryLivePreview live=new StatusLibraryLivePreview();
  public void Dispose()=>live.Dispose();
  bool autoGame;string gameSignature;
  readonly List<StatusVFXLayer> entries=new List<StatusVFXLayer>();
  readonly List<string> labels=new List<string>();
  public void Draw(StatusVFXRig rig,StatusVFXLibrary library,bool imagePreview)
  {
   entries.Clear();labels.Clear();
   if(library!=null && library.layers!=null)
   foreach(var entry in library.layers)if(entry!=null)
   {entries.Add(entry);labels.Add((string.IsNullOrWhiteSpace(entry.id)?"<sin ID>":entry.id)+" ["+entry.trigger+"]"+(entry.enabled?"":" (desactivada)"));}
   if(entries.Count==0){EditorGUILayout.HelpBox("Anade una capa a esta biblioteca.",MessageType.Info);return;}
   int selected=entries.FindIndex(x=>x.id==selectedId);if(selected<0)selected=0;
   selected=EditorGUILayout.Popup("Capa de preview",selected,labels.ToArray());var current=entries[selected];selectedId=current.id;
   if(imagePreview)
   {
    if(!current.useParticleCurves && GUILayout.Button("Activar modos de tamano/color conservando valores"))
    {
     Undo.RecordObject(library,"Enable particle modes");
     current.startSizeMode=new ParticleSystem.MinMaxCurve(Mathf.Max(.0001f,Mathf.Min(current.size.x,current.size.y)),Mathf.Max(.0001f,Mathf.Max(current.size.x,current.size.y)));
     current.sizeLifetimeMode=new ParticleSystem.MinMaxCurve(1,current.sizeOverLifetime);
     current.startColorMode=new ParticleSystem.MinMaxGradient(Color.white);
     current.colorLifetimeMode=new ParticleSystem.MinMaxGradient(current.colorOverLifetime);
     current.useParticleCurves=true;EditorUtility.SetDirty(library);
    }
    int index=library.layers.IndexOf(current);
    live.Draw(library,index);
    EditorGUILayout.HelpBox("Simulacion de sprites sin Play. Los cambios se aplican automaticamente y reinician la simulacion con un segundo de precalentamiento. Player World usa un origen de prueba; los prefabs externos y la escena se prueban en Game.",MessageType.None);
   }
   EditorGUILayout.LabelField("Prueba completa en Game",EditorStyles.boldLabel);
   if(rig==null){EditorGUILayout.HelpBox("Asigna un rig de la escena que utilice esta biblioteca. Despues entra en Play Mode para reproducir la capa completa.",MessageType.Info);return;}
   bool sameLibrary=rig.library==library;
   if(!sameLibrary)EditorGUILayout.HelpBox("El rig elegido utiliza otra biblioteca. Asigna esta biblioteca en el campo Library de ese rig o elige el rig correcto.",MessageType.Warning);
   if(rig.targetCamera==null)EditorGUILayout.HelpBox("El rig necesita Target Camera para las capas Camera Plane.",MessageType.Warning);
   else if((rig.targetCamera.cullingMask&(1<<rig.gameObject.layer))==0)EditorGUILayout.HelpBox("La camara no incluye la Layer del rig en Culling Mask.",MessageType.Warning);
   if(current.sprites && rig.spriteTemplate==null)EditorGUILayout.HelpBox("Asigna StatusSprite_Default al campo Sprite Template del rig.",MessageType.Warning);
   int matches=0;foreach(var e in entries)if(e.enabled && e.id==selectedId)matches++;
   bool valid=current.enabled && !string.IsNullOrWhiteSpace(selectedId) && matches==1;
   if(!valid)EditorGUILayout.HelpBox("La capa debe estar habilitada y tener un ID unico.",MessageType.Warning);
   if(current.anchor==StatusVFXAnchor.CameraPlane && (current.viewportCenter.x<.1f || current.viewportCenter.x>.9f || current.viewportCenter.y<.1f || current.viewportCenter.y>.9f))
    EditorGUILayout.HelpBox("Para una primera prueba usa Viewport Center (0.5, 0.5). (1, 1) coloca el emisor en una esquina.",MessageType.Info);
   if(Application.isPlaying)EditorGUILayout.LabelField("Capas preparadas",rig.LiveLayerCount.ToString());
   using(new EditorGUI.DisabledScope(!Application.isPlaying || !rig.isActiveAndEnabled || !sameLibrary || !valid))
   {
    if(GUILayout.Button("Probar en Game y actualizar al editar",GUILayout.Height(30))){autoGame=true;gameSignature=null;}
    if(GUILayout.Button("Detener esta prueba")){autoGame=false;rig.StopEntry(selectedId);}
    if(GUILayout.Button("Volver al control del Volume")){autoGame=false;rig.FollowVolume(selectedId);}
    if(GUILayout.Button("Aplicar biblioteca sin activar una prueba"))rig.Rebuild();
   }
   if(autoGame && Application.isPlaying && sameLibrary && valid)
   {
    string signature=EditorJsonUtility.ToJson(library)+selectedId;
    if(signature!=gameSignature){gameSignature=signature;rig.Rebuild();rig.PlayEntry(selectedId);}
   }
   EditorGUILayout.HelpBox("La prueba continua activa esta capa hasta detenerla. No cambia el Volume ni activa sus bordes. Manual se queda apagado al volver al Volume. Mira la pestana Game; los botones se habilitan al entrar en Play Mode.",MessageType.Info);
  }

 }
 [CustomEditor(typeof(StatusVFXLibrary))]
 public sealed class StatusVFXLibraryEditor : UnityEditor.Editor
 {
  StatusVFXRig previewRig;
  readonly StatusVFXPreviewPanel preview=new StatusVFXPreviewPanel();
  public override bool RequiresConstantRepaint()=>true;
  void OnEnable(){FindRig();}
  void OnDisable(){preview.Dispose();}
  void FindRig()
  {
   previewRig=null;
   foreach(var rig in Object.FindObjectsByType<StatusVFXRig>(FindObjectsInactive.Include,FindObjectsSortMode.None))
    if(rig.gameObject.scene.IsValid() && rig.library==(StatusVFXLibrary)target){previewRig=rig;break;}
  }
  public override void OnInspectorGUI()
  {
   var library=(StatusVFXLibrary)target;
   EditorGUILayout.HelpBox("Edita el aspecto y usa Preview abajo. Los estados de esta biblioteca se activan desde el rig. Para un fondo y sprites unidos desde el Volume, utiliza Interior Sprites dentro de Status Edge Style.",MessageType.Info);
   EditorGUILayout.Space();EditorGUILayout.LabelField("Preview",EditorStyles.boldLabel);
   previewRig=(StatusVFXRig)EditorGUILayout.ObjectField("Rig de prueba",previewRig,typeof(StatusVFXRig),true);
   if(GUILayout.Button("Buscar rig que usa esta biblioteca"))FindRig();
   preview.Draw(previewRig,library,true);
   EditorGUILayout.Space();EditorGUILayout.LabelField("Configuracion de la biblioteca",EditorStyles.boldLabel);
   DrawDefaultInspector();
   if(GUILayout.Button("Anadir una capa manual"))
   {
    Undo.RecordObject(library,"Add status VFX layer");if(library.layers==null)library.layers=new List<StatusVFXLayer>();
    library.layers.Add(new StatusVFXLayer{id="custom_"+System.Guid.NewGuid().ToString("N").Substring(0,8),trigger=StatusVFXTrigger.Manual,useParticleCurves=true});EditorUtility.SetDirty(library);
   }
  }
 }
 [CustomEditor(typeof(StatusVFXRig))]
 public sealed class StatusVFXRigEditor : UnityEditor.Editor
 {
  readonly StatusVFXPreviewPanel preview=new StatusVFXPreviewPanel();
  void OnDisable(){preview.Dispose();}
  public override bool RequiresConstantRepaint()=>Application.isPlaying;
  public override void OnInspectorGUI()
  {
   DrawDefaultInspector();EditorGUILayout.Space();var rig=(StatusVFXRig)target;
   EditorGUILayout.LabelField("Prueba en runtime",EditorStyles.boldLabel);preview.Draw(rig,rig.library,false);
  }
 }
}
