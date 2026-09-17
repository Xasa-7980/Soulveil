Shader "Atmosfera Anime/Status Sprite"
{
 Properties
 {
  _MainTex("Sprite / flipbook",2D)="white"{}
  [HDR] _Tint("Tint",Color)=(1,1,1,1)
  [Enum(Texture,0,SoftGlow,1,Snowflake,2,Ring,3,Spark,4,Droplet,5)] _Shape("Fallback shape",Float)=1
  [Enum(UnityEngine.Rendering.BlendMode)] _DstBlend("Destination blend",Float)=10
  [Enum(UnityEngine.Rendering.CompareFunction)] _ZTest("Depth test",Float)=8
 }
 SubShader
 {
  Tags {"Queue"="Transparent" "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline" "IgnoreProjector"="True"}
  Blend SrcAlpha [_DstBlend]
  ZWrite Off Cull Off ZTest [_ZTest]
  Pass
  {
   Name "StatusSprite" Tags {"LightMode"="UniversalForward"}
   HLSLPROGRAM
   #pragma target 3.0
   #pragma vertex Vert
   #pragma fragment Frag
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
   #include "StatusShapes.hlsl"
   TEXTURE2D(_MainTex);SAMPLER(sampler_MainTex);
   CBUFFER_START(UnityPerMaterial)
    float4 _Tint;float _Shape,_DstBlend,_ZTest;
   CBUFFER_END
   struct Attr{float4 positionOS:POSITION;float4 color:COLOR;float2 uv:TEXCOORD0;};
   struct Var{float4 positionCS:SV_POSITION;float4 color:COLOR;float2 uv:TEXCOORD0;};
   Var Vert(Attr v){Var o;o.positionCS=TransformObjectToHClip(v.positionOS.xyz);o.color=v.color;o.uv=v.uv;return o;}
   half4 Frag(Var i):SV_Target
   {
    half4 tex=SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv);
    float alpha=tex.a*i.color.a*_Tint.a*SSShape(i.uv,_Shape);
    clip(alpha-.001);return half4(tex.rgb*i.color.rgb*_Tint.rgb,alpha);
   }
   ENDHLSL
  }
 }
 Fallback Off
}
