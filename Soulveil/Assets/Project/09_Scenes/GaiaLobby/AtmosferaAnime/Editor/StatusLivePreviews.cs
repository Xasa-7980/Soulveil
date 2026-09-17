using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace AtmosferaAnime.Editor
{
 // Isolated particle simulation. No scene object, Play Mode, or assigned rig required.
 internal sealed class StatusLibraryLivePreview : IDisposable
 {
  PreviewRenderUtility preview;
  GameObject host,standIn;
  StatusVFXRig rig;
  StatusVFXLibrary single;
  Material material;
  string fingerprint;
  double last;
  bool playing=true;
  float elapsed;float worldViewDistance=3;
  public void Draw(StatusVFXLibrary library,int index)
  {
   EditorGUILayout.LabelField("Preview de particulas en directo (sin Play)",EditorStyles.boldLabel);
   playing=EditorGUILayout.Toggle("Reproducir preview",playing);
   worldViewDistance=EditorGUILayout.Slider("Distancia para Player World",worldViewDistance,1,12);
   if(GUILayout.Button("Reiniciar preview"))fingerprint=null;
   Rect rect=GUILayoutUtility.GetRect(100,240,GUILayout.ExpandWidth(true));
   if(library==null || library.layers==null || index<0 || index>=library.layers.Count)return;
   var config=library.layers[index];if(config==null)return;
   if(preview==null)Create();
   preview.camera.transform.position=new Vector3(0,0,-worldViewDistance);
   if(material==null){EditorGUI.HelpBox(rect,"No se encuentra el shader AtmosferaAnime/StatusSprite. Importa el shader incluido.",MessageType.Warning);return;}
   string signature=EditorJsonUtility.ToJson(library)+"/"+index;
   bool changed=signature!=fingerprint;
   if(changed)
   {
    fingerprint=signature;
    if(single!=null)UnityEngine.Object.DestroyImmediate(single);
    single=UnityEngine.Object.Instantiate(library);single.hideFlags=HideFlags.HideAndDontSave;
    single.layers=new System.Collections.Generic.List<StatusVFXLayer>{single.layers[index]};
    rig.BeginEditorPreview(preview.camera,single,material,standIn.transform);
    host.SetActive(true);elapsed=0;
    for(int i=0;i<30;i++)rig.StepEditorPreview(1f/30,config.id);
    elapsed=1;
   }
   if(Event.current.type!=EventType.Repaint)return;
   double now=EditorApplication.timeSinceStartup;
   float dt=last==0?0:Mathf.Clamp((float)(now-last),0,.05f);last=now;
   if(playing){rig.StepEditorPreview(dt,config.id);elapsed+=dt;}
   preview.BeginPreview(rect,GUIStyle.none);
   preview.camera.aspect=rect.width/rect.height;
   preview.Render(true,false);
   GUI.DrawTexture(rect,preview.EndPreview(),ScaleMode.StretchToFill,false);
   GUI.Label(new Rect(rect.x+8,rect.y+6,rect.width-16,20),$"{config.id}  |  {elapsed:F1}s",EditorStyles.whiteMiniLabel);

  }
  void Create()
  {
   preview=new PreviewRenderUtility();preview.camera.nearClipPlane=.01f;preview.camera.farClipPlane=100;
   preview.camera.fieldOfView=50;preview.camera.transform.SetPositionAndRotation(new Vector3(0,0,-5),Quaternion.identity);
   preview.camera.clearFlags=CameraClearFlags.SolidColor;preview.camera.backgroundColor=new Color(.055f,.075f,.1f);
   preview.camera.allowHDR=true;
   host=new GameObject("Status library preview"){hideFlags=HideFlags.HideAndDontSave};preview.AddSingleGO(host);
   standIn=new GameObject("Player preview origin"){hideFlags=HideFlags.HideAndDontSave};standIn.transform.SetParent(host.transform,false);
   host.SetActive(false);rig=host.AddComponent<StatusVFXRig>();
   var shader=Shader.Find("AtmosferaAnime/StatusSprite");
   if(shader==null)shader=Shader.Find("Atmosfera Anime/Status Sprite");
   if(shader!=null)material=new Material(shader){hideFlags=HideFlags.HideAndDontSave};
  }
  public void Dispose()
  {
   if(rig!=null)rig.EndEditorPreview();
   preview?.Cleanup();preview=null;
   if(single!=null)UnityEngine.Object.DestroyImmediate(single);single=null;
   if(material!=null)UnityEngine.Object.DestroyImmediate(material);material=null;
  }
 }
 internal sealed class StatusStyleLivePreview : IDisposable
 {
  Material material;Mesh mesh;RenderTexture output;StatusScreenVolume volume;
        MaterialPropertyBlock block; readonly Vector4[][] samples={new Vector4[96],new Vector4[96],new Vector4[96],new Vector4[96]};
  bool playing=true;float time;double last;Texture2D backdrop;Color previewStateColor=Color.white;
  public void Draw(StatusEdgeStyle style)
  {
            if (block == null)
                block = new MaterialPropertyBlock();

            EditorGUILayout.LabelField(
                "Preview conjunto: fondo + sprites (sin Play)",
                EditorStyles.boldLabel
            );
            EditorGUILayout.LabelField("Preview conjunto: fondo + sprites (sin Play)",EditorStyles.boldLabel);
   playing=EditorGUILayout.Toggle("Reproducir preview",playing);
   previewStateColor=EditorGUILayout.ColorField("Color de estado de prueba",previewStateColor);
   backdrop=(Texture2D)EditorGUILayout.ObjectField("Imagen de fondo de prueba",backdrop,typeof(Texture2D),false);
   time=EditorGUILayout.Slider("Tiempo de preview",time,0,30);
   Rect rect=GUILayoutUtility.GetRect(100,230,GUILayout.ExpandWidth(true));
   if(Event.current.type!=EventType.Repaint)return;
   if(material==null)
   {
    var shader=Shader.Find("Hidden/AtmosferaAnime/StatusScreen");
    if(shader==null){EditorGUI.HelpBox(rect,"Falta StatusScreen.shader.",MessageType.Warning);return;}
    material=new Material(shader){hideFlags=HideFlags.HideAndDontSave};mesh=StatusInteriorFrame.CreateMesh();
    volume=ScriptableObject.CreateInstance<StatusScreenVolume>();volume.hideFlags=HideFlags.HideAndDontSave;
   }
   double now=EditorApplication.timeSinceStartup;
   if(playing && last>0)time=(time+Mathf.Clamp((float)(now-last),0,.05f))%30;last=now;
   int width=640,height=Mathf.Max(64,Mathf.RoundToInt(640*rect.height/rect.width));
   if(output==null || output.height!=height)
   {if(output!=null){output.Release();UnityEngine.Object.DestroyImmediate(output);}output=new RenderTexture(width,height,0,RenderTextureFormat.ARGBHalf){hideFlags=HideFlags.HideAndDontSave};output.Create();}
   volume.intensity.value=1;volume.frost.value=1;volume.proceduralStrength.value=0;volume.edgeImagesStrength.value=1;volume.frostEdgeStyle.value=style;volume.frostColor.value=previewStateColor;
   var settings=new StatusScreenFeature.Settings(volume);settings.Apply(material);
   new StatusEdgeSnapshot(style,1,previewStateColor,time).Apply(material,7);
   var cmd=new CommandBuffer{name="Status style live preview"};
   Vector4 screen=Shader.GetGlobalVector("_ScreenParams");
   try
   {
    cmd.SetRenderTarget(output);cmd.ClearRenderTarget(false,true,new Color(.09f,.11f,.14f));
    cmd.SetGlobalVector("_ScreenParams",new Vector4(width,height,1+1f/width,1+1f/height));
    material.SetTexture("_BlitTexture",backdrop!=null?backdrop:Texture2D.grayTexture);
    material.SetVector("_BlitScaleBias",new Vector4(1,1,0,0));material.SetVector("_BlitTexture_TexelSize",new Vector4(1f/width,1f/height,width,height));
    cmd.DrawProcedural(Matrix4x4.identity,material,0,MeshTopology.Triangles,3);
    int n=0;
    if(style.interiorSprites!=null && material.passCount>=4)
    foreach(var layer in style.interiorSprites)
    {
     if(n>=4)break;if(layer==null || !layer.HasVisual())continue;
     var shot=new StatusInteriorSnapshot(layer,style.opacity,previewStateColor,time,samples[n++]);shot.Apply(block);
     cmd.DrawMesh(mesh,Matrix4x4.identity,material,0,shot.ShaderPass,block);
    }
    cmd.SetGlobalVector("_ScreenParams",screen);Graphics.ExecuteCommandBuffer(cmd);
   }
   finally{cmd.Release();}
   GUI.DrawTexture(rect,output,ScaleMode.StretchToFill,false);
  }
  public void Dispose()
  {
   if(output!=null){output.Release();UnityEngine.Object.DestroyImmediate(output);}output=null;
   if(material!=null)UnityEngine.Object.DestroyImmediate(material);material=null;
   if(mesh!=null)UnityEngine.Object.DestroyImmediate(mesh);mesh=null;
   if(volume!=null)UnityEngine.Object.DestroyImmediate(volume);volume=null;
  }
 }
}
