Shader "Hidden/ElementalVillages/GlobalCelOutlines"
{
 SubShader
 {
  Tags {"RenderPipeline"="UniversalPipeline"}
  ZTest Always ZWrite Off Cull Off
  Pass
  {
   Name "GlobalCelOutlines"
   HLSLPROGRAM
   #pragma target 3.5
   #pragma vertex Vert
   #pragma fragment Frag
   #pragma multi_compile_fragment _ _GBUFFER_NORMALS_OCT
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
   #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareNormalsTexture.hlsl"
   float _Intensity,_Bands,_WhitePoint,_PreserveColor,_OutlineStrength,_OutlineWidth;
   float _DepthThreshold,_NormalThreshold,_NormalEdges,_AffectSky;
   float4 _OutlineColor;
   float EyeDepth(float raw)
   {
    float p=LinearEyeDepth(raw,_ZBufferParams);
    #if UNITY_REVERSED_Z
     float ortho=lerp(_ProjectionParams.z,_ProjectionParams.y,raw);
    #else
     float ortho=lerp(_ProjectionParams.y,_ProjectionParams.z,raw);
    #endif
    return lerp(p,ortho,unity_OrthoParams.w);
   }
   float IsSky(float raw)
   {
    #if UNITY_REVERSED_Z
     return step(raw,0.000001);
    #else
     return step(.999999,raw);
    #endif
   }
   float3 Cel(float3 color)
   {
    float l=max(dot(max(color,0),float3(.2126,.7152,.0722)),.00001);
    float w=max(_WhitePoint,.1),levels=max(_Bands-1,1);
    float q=floor(saturate(l/w)*levels+.5)/levels*w;
    q=max(q,.025*w)+max(0,l-w);
    float3 huePreserving=color*min(q/l,4);
    float3 rgbQuant=floor(saturate(color/w)*levels+.5)/levels*w+max(0,color-w);
    return lerp(rgbQuant,huePreserving,_PreserveColor);
   }
   half4 Frag(Varyings i):SV_Target
   {
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
    float2 uv=i.texcoord;
    half4 original=SAMPLE_TEXTURE2D_X(_BlitTexture,sampler_LinearClamp,uv);
    float raw=SampleSceneDepth(uv),sky=IsSky(raw);
    float2 pixel=_OutlineWidth*_BlitTexture_TexelSize.xy;
    float2 a=saturate(uv+pixel*float2(-.5,-.5)),b=saturate(uv+pixel*float2(.5,.5));
    float2 c=saturate(uv+pixel*float2(-.5,.5)),d=saturate(uv+pixel*float2(.5,-.5));
    float z1=EyeDepth(SampleSceneDepth(a)),z2=EyeDepth(SampleSceneDepth(b));
    float z3=EyeDepth(SampleSceneDepth(c)),z4=EyeDepth(SampleSceneDepth(d));
    float edgeD=max(abs(z1-z2)/max(min(z1,z2),.1),abs(z3-z4)/max(min(z3,z4),.1));
    float3 n1=SampleSceneNormals(a),n2=SampleSceneNormals(b),n3=SampleSceneNormals(c),n4=SampleSceneNormals(d);
    float edgeN=max(length(n1-n2),length(n3-n4));
    float depthEdge=smoothstep(_DepthThreshold,_DepthThreshold*1.8,edgeD);
    float normalEdge=smoothstep(_NormalThreshold,_NormalThreshold*1.5,edgeN)*_NormalEdges;
    float edge=saturate(max(depthEdge,normalEdge));
    float3 cel=lerp(Cel(original.rgb),original.rgb,sky*(1-_AffectSky));
    cel=lerp(cel,_OutlineColor.rgb,edge*_OutlineStrength);
    return half4(lerp(original.rgb,cel,_Intensity),original.a);
   }
   ENDHLSL
  }
 }
 Fallback Off
}
