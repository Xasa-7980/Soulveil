Shader "Harmonie/Vegetation Wind"
{
 Properties
 {
  [MainTexture] _BaseMap("Albedo",2D)="white"{}
  [MainColor] _BaseColor("Color",Color)=(1,1,1,1)
  [Toggle] _AlphaClip("Alpha cutout",Float)=1
  _Cutoff("Alpha cutoff",Range(0,1))=.4
  [Enum(UnityEngine.Rendering.CullMode)] _Cull("Cull",Float)=0
  [Normal] _BumpMap("Normal map",2D)="bump"{}
  _BumpScale("Normal strength",Range(0,2))=1
  _Smoothness("Smoothness",Range(0,1))=.25
  _Transmission("Leaf backlight",Range(0,1))=.25
  _RootY("Root Y in mesh coordinates",Float)=0
  _PlantHeight("Height in mesh coordinates",Float)=1
  [Enum(Height,0,UV_Y,1,Vertex_Red,2)] _MaskMode("Wind mask",Float)=0
  _WindMultiplier("Wind multiplier",Range(0,1))=1
 }
 SubShader
 {
  Tags {"RenderType"="TransparentCutout" "Queue"="AlphaTest" "RenderPipeline"="UniversalPipeline" "DisableBatching"="True"}
  HLSLINCLUDE
  #include "WindCommon.hlsl"
  struct Varyings
  {
   float4 positionCS:SV_POSITION;float3 positionWS:TEXCOORD0;half3 normalWS:TEXCOORD1;
   float2 uv:TEXCOORD2;float4 tangentWS:TEXCOORD3;half3 vertexLighting:TEXCOORD4;half fog:TEXCOORD5;
   UNITY_VERTEX_INPUT_INSTANCE_ID
   UNITY_VERTEX_OUTPUT_STEREO
  };
  Varyings WindVert(Attributes input)
  {
   Varyings o=(Varyings)0;UNITY_SETUP_INSTANCE_ID(input);UNITY_TRANSFER_INSTANCE_ID(input,o);UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
   WindVertex(input,o.positionWS,o.normalWS);o.positionCS=TransformWorldToHClip(o.positionWS);
   o.uv=TRANSFORM_TEX(input.uv,_BaseMap);
   o.tangentWS=float4(TransformObjectToWorldDir(input.tangentOS.xyz,false),input.tangentOS.w*GetOddNegativeScale());
   o.vertexLighting=VertexLighting(o.positionWS,o.normalWS);o.fog=ComputeFogFactor(o.positionCS.z);return o;
  }
  half3 WindNormal(Varyings i,bool front)
  {
   float3 n=normalize(i.normalWS);float3 t=i.tangentWS.xyz-n*dot(n,i.tangentWS.xyz);
   if(dot(t,t)>.00001 && abs(i.tangentWS.w)>.1)
   {
    t=normalize(t);float3 b=cross(n,t)*i.tangentWS.w;
    half3 nt=UnpackNormalScale(SAMPLE_TEXTURE2D(_BumpMap,sampler_BumpMap,i.uv),_BumpScale);
    n=normalize(t*nt.x+b*nt.y+n*nt.z);
   }
   return front?n:-n;
  }
  ENDHLSL
  Pass
  {
   Name "Forward" Tags {"LightMode"="UniversalForwardOnly"}
   Cull [_Cull] ZWrite On Blend One Zero
   HLSLPROGRAM
   #pragma target 3.5
   #pragma vertex WindVert
   #pragma fragment Frag
   #pragma multi_compile_instancing
   #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
   #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
   #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
   #pragma multi_compile_fragment _ _SHADOWS_SOFT
   #pragma multi_compile _ _FORWARD_PLUS
   #pragma multi_compile_fog
   half4 Frag(Varyings i,FRONT_FACE_TYPE face:FRONT_FACE_SEMANTIC):SV_Target
   {
    UNITY_SETUP_INSTANCE_ID(i);UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);WindAlpha(i.uv);
    half3 n=WindNormal(i,IS_FRONT_VFACE(face,true,false));
    SurfaceData surface=(SurfaceData)0;
    surface.albedo=SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,i.uv).rgb*_BaseColor.rgb;
    surface.alpha=1;surface.metallic=0;surface.specular=half3(.04,.04,.04);
    surface.smoothness=_Smoothness;surface.normalTS=half3(0,0,1);surface.occlusion=1;
    InputData data=(InputData)0;data.positionWS=i.positionWS;data.normalWS=n;
    data.viewDirectionWS=GetWorldSpaceNormalizeViewDir(i.positionWS);
    #if defined(_MAIN_LIGHT_SHADOWS_SCREEN)
     data.shadowCoord=ComputeScreenPos(TransformWorldToHClip(i.positionWS));
    #else
     data.shadowCoord=TransformWorldToShadowCoord(i.positionWS);
    #endif
    data.bakedGI=SampleSH(n);data.vertexLighting=i.vertexLighting;data.shadowMask=half4(1,1,1,1);
    data.normalizedScreenSpaceUV=GetNormalizedScreenSpaceUV(i.positionCS);
    half4 result=UniversalFragmentPBR(data,surface);
    Light light=GetMainLight(data.shadowCoord);
    half back=pow(saturate(dot(data.viewDirectionWS,-light.direction)),3);
    result.rgb+=surface.albedo*light.color*back*_Transmission*light.shadowAttenuation;
    result.rgb=MixFog(result.rgb,i.fog);
    return half4(result.rgb,1);
   }
   ENDHLSL
  }
  Pass
  {
   Name "ShadowCaster" Tags {"LightMode"="ShadowCaster"}
   Cull [_Cull] ZWrite On ZTest LEqual ColorMask 0
   HLSLPROGRAM
   #pragma target 3.5
   #pragma vertex WindShadow
   #pragma fragment ShadowFrag
   #pragma multi_compile_instancing
   #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW
   float3 _LightDirection,_LightPosition;
   struct ShadowVaryings{float4 positionCS:SV_POSITION;float2 uv:TEXCOORD0;UNITY_VERTEX_OUTPUT_STEREO};
   ShadowVaryings WindShadow(Attributes v)
   {
    UNITY_SETUP_INSTANCE_ID(v);ShadowVaryings o=(ShadowVaryings)0;UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
    float3 p,n;WindVertex(v,p,n);
    #if defined(_CASTING_PUNCTUAL_LIGHT_SHADOW)
     float3 ld=normalize(_LightPosition-p);
    #else
     float3 ld=_LightDirection;
    #endif
    o.positionCS=TransformWorldToHClip(ApplyShadowBias(p,n,ld));
    #if UNITY_REVERSED_Z
     o.positionCS.z=min(o.positionCS.z,UNITY_NEAR_CLIP_VALUE*o.positionCS.w);
    #else
     o.positionCS.z=max(o.positionCS.z,UNITY_NEAR_CLIP_VALUE*o.positionCS.w);
    #endif
    o.uv=TRANSFORM_TEX(v.uv,_BaseMap);return o;
   }
   half4 ShadowFrag(ShadowVaryings i):SV_Target {WindAlpha(i.uv);return 0;}
   ENDHLSL
  }
  Pass
  {
   Name "DepthOnly" Tags {"LightMode"="DepthOnly"}
   Cull [_Cull] ZWrite On ColorMask R
   HLSLPROGRAM
   #pragma target 3.5
   #pragma vertex WindVert
   #pragma fragment DepthFrag
   #pragma multi_compile_instancing
   half4 DepthFrag(Varyings i):SV_Target {UNITY_SETUP_INSTANCE_ID(i);WindAlpha(i.uv);return 0;}
   ENDHLSL
  }
  Pass
  {
   Name "DepthNormals" Tags {"LightMode"="DepthNormalsOnly"}
   Cull [_Cull] ZWrite On
   HLSLPROGRAM
   #pragma target 3.5
   #pragma vertex WindVert
   #pragma fragment NormalFrag
   #pragma multi_compile_instancing
   #pragma multi_compile_fragment _ _GBUFFER_NORMALS_OCT
   half4 NormalFrag(Varyings i,FRONT_FACE_TYPE face:FRONT_FACE_SEMANTIC):SV_Target
   {
    UNITY_SETUP_INSTANCE_ID(i);UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);WindAlpha(i.uv);
    half3 n=WindNormal(i,IS_FRONT_VFACE(face,true,false));
    #if defined(_GBUFFER_NORMALS_OCT)
     return half4(PackFloat2To888(saturate(PackNormalOctQuadEncode(n)*.5+.5)),0);
    #else
     return half4(n,0);
    #endif
   }
   ENDHLSL
  }
 }
 Fallback Off
}
