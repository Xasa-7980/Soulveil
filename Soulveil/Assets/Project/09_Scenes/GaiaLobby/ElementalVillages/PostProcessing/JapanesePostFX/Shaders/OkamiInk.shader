Shader "Hidden/JapanesePostFX/OkamiInk"
{
 SubShader
 {
  Tags {"RenderPipeline"="UniversalPipeline"}
  ZTest Always ZWrite Off Cull Off Blend One Zero
  Pass
  {
   Name "OkamiInk"
   HLSLPROGRAM
   #pragma target 3.5
   #pragma vertex Vert
   #pragma fragment FragInk
   #pragma multi_compile_fragment _ _GBUFFER_NORMALS_OCT
   #include "PaintCommon.hlsl"
   ENDHLSL
  }
 }
 Fallback Off
}
