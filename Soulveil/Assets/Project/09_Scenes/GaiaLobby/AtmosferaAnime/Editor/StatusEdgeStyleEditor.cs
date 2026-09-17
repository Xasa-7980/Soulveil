using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Rendering;
using UnityEngine;

namespace AtmosferaAnime.Editor
{
 [VolumeParameterDrawer(typeof(StatusEdgeStyleParameter))]
 public sealed class StatusEdgeStyleParameterDrawer : VolumeParameterDrawer
 {
  public override bool OnGUI(SerializedDataParameter parameter,GUIContent title)
  {
   if(parameter.value.propertyType!=SerializedPropertyType.ObjectReference)return false;
   EditorGUILayout.PropertyField(parameter.value,title);return true;
  }
 }
 [CustomEditor(typeof(StatusEdgeStyle))]
 public sealed class StatusEdgeStyleEditor : UnityEditor.Editor
 {
  bool advanced;
  readonly StatusStyleLivePreview preview=new StatusStyleLivePreview();
  public override bool RequiresConstantRepaint()=>true;
  void OnDisable()=>preview.Dispose();
  static readonly string[] options={"tint","useStatusColor","blend","alphaSource","maskOnly","limitToEdges","width","feather","sides","tiling","offset","scroll","rotationDegrees","rotationSpeed","repeat","noiseAmount","noiseFrequency","noiseSpeed","noiseSeed","columns","rows","framesPerSecond","startFrame"};
  static AnimationCurve ScaleCurve(AnimationCurve source,float scale)
  {
   var keys=source.keys;
   for(int i=0;i<keys.Length;i++){var k=keys[i];k.value*=scale;k.inTangent*=scale;k.outTangent*=scale;keys[i]=k;}
   return new AnimationCurve(keys){preWrapMode=source.preWrapMode,postWrapMode=source.postWrapMode};
  }
  public override void OnInspectorGUI()
  {
   preview.Draw((StatusEdgeStyle)target);
   serializedObject.Update();
   EditorGUILayout.HelpBox("Este asset une fondo y sprites interiores. Asignalo a un Edge Style del Volume y activa ese estado. Ambos se reproducen juntos. No necesitas StatusVFXRig ni Trigger Manual para las capas de este asset. Los cambios se leen en directo.",MessageType.Info);
   EditorGUILayout.PropertyField(serializedObject.FindProperty("opacity"),new GUIContent("Intensidad del conjunto"));
   EditorGUILayout.PropertyField(serializedObject.FindProperty("replaceProcedural"),new GUIContent("Sustituir filtro procedural"));
   EditorGUILayout.Space();EditorGUILayout.LabelField("1. Fondo / borde",EditorStyles.boldLabel);
   EditorGUILayout.PropertyField(serializedObject.FindProperty("sprite"));
   EditorGUILayout.PropertyField(serializedObject.FindProperty("texture"));
   EditorGUILayout.PropertyField(serializedObject.FindProperty("layout"));
   advanced=EditorGUILayout.Foldout(advanced,"Ajustes y animacion del fondo",true);
   if(advanced)foreach(string field in options)EditorGUILayout.PropertyField(serializedObject.FindProperty(field),true);
   EditorGUILayout.Space();EditorGUILayout.LabelField("2. Sprites interiores",EditorStyles.boldLabel);
   EditorGUILayout.PropertyField(serializedObject.FindProperty("interiorSprites"),new GUIContent("Capas de sprites"),true);
   serializedObject.ApplyModifiedProperties();
   var style=(StatusEdgeStyle)target;
   if(GUILayout.Button("Anadir sprites a este efecto"))
   {
    Undo.RecordObject(style,"Add interior sprite layer");if(style.interiorSprites==null)style.interiorSprites=new List<StatusInteriorSpriteLayer>();
    style.interiorSprites.Add(new StatusInteriorSpriteLayer{useParticleCurves=true});EditorUtility.SetDirty(style);
    serializedObject.Update();serializedObject.FindProperty("interiorSprites").isExpanded=true;
   }
   if(style.interiorSprites!=null && GUILayout.Button("Activar modos de tamano/color conservando valores"))
   {
    Undo.RecordObject(style,"Enable interior modes");
    foreach(var layer in style.interiorSprites)if(layer!=null && !layer.useParticleCurves)
    {
     float lo=Mathf.Max(.01f,Mathf.Min(layer.sizePercent.x,layer.sizePercent.y)),hi=Mathf.Max(.01f,Mathf.Max(layer.sizePercent.x,layer.sizePercent.y));
     // Both curves share the legacy shape; the multiplier is the height in percent.
     var source=layer.sizeOverLifetime; if(source==null || source.length==0)source=AnimationCurve.Linear(0,1,1,1);
     var low=ScaleCurve(source,lo);var high=ScaleCurve(source,hi);
     layer.heightMode=new ParticleSystem.MinMaxCurve(1,low,high);
     var gradient=new Gradient();gradient.SetKeys(new[]{new GradientColorKey(layer.startColor,0),new GradientColorKey(layer.endColor,1)},new[]{new GradientAlphaKey(layer.startColor.a,0),new GradientAlphaKey(layer.endColor.a,1)});
     layer.colorMode=new ParticleSystem.MinMaxGradient(gradient);layer.useParticleCurves=true;
    }
    EditorUtility.SetDirty(style);
   }
   if(style.interiorSprites!=null)
   {
    int enabled=0;
    foreach(var layer in style.interiorSprites)if(layer!=null && layer.enabled)
    {
     enabled++;
     if(!layer.TryGetSource(out _,out _,out _))EditorGUILayout.HelpBox(layer.name+": asigna una imagen rectangular valida o elige una forma de respaldo como Soft Glow. No se admiten sprites con atlas Tight o rotado.",MessageType.Warning);
    }
    if(enabled>4)EditorGUILayout.HelpBox("Se dibujan como maximo las primeras 4 capas habilitadas con contenido por estilo.",MessageType.Warning);
   }
   if(!style.HasVisuals())EditorGUILayout.HelpBox("Asigna una imagen de fondo o anade una capa interior. Una capa sin imagen puede utilizar Soft Glow, Snowflake u otra forma de respaldo.",MessageType.Info);
   if((style.sprite!=null || style.texture!=null) && !style.TryGetSource(out _,out _))EditorGUILayout.HelpBox("El sprite de fondo tiene un atlas Tight o rotado no compatible. Usa el PNG original o un sprite rectangular sin rotacion de atlas.",MessageType.Warning);
   EditorGUILayout.HelpBox("Size Percent indica el porcentaje de altura de pantalla: 8 = 8%. Center Percent (50,50) es el centro. Activa el estado en el Volume y mira Game para ver fondo y sprites; no hace falta Rebuild.",MessageType.Info);
  }
 }
}
