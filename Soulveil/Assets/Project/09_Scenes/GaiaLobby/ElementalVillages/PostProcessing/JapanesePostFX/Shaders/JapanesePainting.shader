Shader "Hidden/JapanesePostFX/JapanesePainting"
{
 SubShader
 {
  Tags {"RenderPipeline"="UniversalPipeline"}
  ZTest Always ZWrite Off Cull Off Blend One Zero
  Pass
  {
   Name "JapanesePainting"
   HLSLPROGRAM
   #pragma target 3.5
   #pragma vertex Vert
   #pragma fragment FragWash
   #pragma multi_compile_fragment _ _GBUFFER_NORMALS_OCT
   #include "PaintCommon.hlsl"
   ENDHLSL
  }
 }
 Fallback Off
}
