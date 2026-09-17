Shader "Anime Village/Cel URP"
{
 Properties
 {
  [MainTexture] _BaseMap("Albedo",2D)="white"{}
  [MainColor] _BaseColor("Color",Color)=(1,1,1,1)
  _ShadowColor("Shadow tint",Color)=(0.48,0.43,0.56,1)
  _MidColor("Midtone tint",Color)=(0.78,0.76,0.83,1)
  _Threshold1("Shadow to midtone",Range(-1,1))=0.05
  _Threshold2("Midtone to light",Range(-1,1))=0.55
  _Softness("Band softness",Range(0.001,0.2))=0.025
  _Ambient("Ambient strength",Range(0,2))=0.5
  _AdditionalGain("Additional lights",Range(0,2))=0.7
  _Specular("Stylized highlight",Range(0,1))=0.08
  _SpecThreshold("Highlight threshold",Range(0.5,0.999))=0.97
  _RimColor("Rim color",Color)=(0.8,0.85,1,1)
  _RimStrength("Rim strength",Range(0,1))=0.1
  _RimPower("Rim power",Range(1,12))=5
  _InkStrength("Surface edge darkening",Range(0,1))=0.12
  [HDR] _EmissionColor("Emission",Color)=(0,0,0,1)
 }
 SubShader
 {
  Tags {"RenderType"="Opaque" "Queue"="Geometry" "RenderPipeline"="UniversalPipeline"}
  HLSLINCLUDE
  #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
  #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
  TEXTURE2D(_BaseMap); SAMPLER(sampler_BaseMap);
  CBUFFER_START(UnityPerMaterial)
   float4 _BaseMap_ST; half4 _BaseColor,_ShadowColor,_MidColor,_RimColor,_EmissionColor;
   half _Threshold1,_Threshold2,_Softness,_Ambient,_AdditionalGain;
   half _Specular,_SpecThreshold,_RimStrength,_RimPower,_InkStrength;
  CBUFFER_END
  struct Attr {float4 positionOS:POSITION;float3 normalOS:NORMAL;float2 uv:TEXCOORD0;};
  struct Var {float4 positionCS:SV_POSITION;float3 positionWS:TEXCOORD0;half3 normalWS:TEXCOORD1;float2 uv:TEXCOORD2;float4 shadowCoord:TEXCOORD3;half fog:TEXCOORD4;};
  Var Vert(Attr v)
  {
   Var o;VertexPositionInputs p=GetVertexPositionInputs(v.positionOS.xyz);
   o.positionCS=p.positionCS;o.positionWS=p.positionWS;o.normalWS=TransformObjectToWorldNormal(v.normalOS);
   o.uv=TRANSFORM_TEX(v.uv,_BaseMap);o.shadowCoord=GetShadowCoord(p);o.fog=ComputeFogFactor(p.positionCS.z);return o;
  }
  half Band(half x,half t) {return smoothstep(t-_Softness,t+_Softness,x);}
  half3 ExtraLight(Light l,half3 n)
  {
   half diffuse=lerp(0.0,0.55,Band(dot(n,l.direction),_Threshold1));
   diffuse=lerp(diffuse,1.0,Band(dot(n,l.direction),max(_Threshold2,_Threshold1+0.01)));
   return l.color*diffuse*l.distanceAttenuation*l.shadowAttenuation*_AdditionalGain;
  }
  ENDHLSL
  Pass
  {
   Name "CelForward" Tags {"LightMode"="UniversalForwardOnly"}
   Cull Back ZWrite On
   HLSLPROGRAM
   #pragma target 3.5
   #pragma vertex Vert
   #pragma fragment Frag
   #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
   #pragma multi_compile_fragment _ _SHADOWS_SOFT
   #pragma multi_compile _ _ADDITIONAL_LIGHTS
   #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
   #pragma multi_compile _ _FORWARD_PLUS
   #pragma multi_compile_fog
   half4 Frag(Var i):SV_Target
   {
    half3 n=normalize(i.normalWS),v=GetWorldSpaceNormalizeViewDir(i.positionWS);
    half3 albedo=SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,i.uv).rgb*_BaseColor.rgb;
    Light main=GetMainLight(i.shadowCoord);
    half nd=dot(n,main.direction),lit=smoothstep(0.35,0.65,main.shadowAttenuation);
    half b1=Band(nd,_Threshold1)*lit,b2=Band(nd,max(_Threshold2,_Threshold1+0.01))*lit;
    half3 ramp=lerp(_ShadowColor.rgb,_MidColor.rgb,b1);ramp=lerp(ramp,half3(1,1,1),b2);
    half3 color=albedo*(ramp*main.color*main.distanceAttenuation+max(SampleSH(n),0)*_Ambient);
    #if defined(_ADDITIONAL_LIGHTS)
     InputData inputData=(InputData)0;inputData.positionWS=i.positionWS;
     inputData.normalizedScreenSpaceUV=GetNormalizedScreenSpaceUV(i.positionCS);
     #if USE_FORWARD_PLUS
      UNITY_LOOP for(uint lightIndex=0;lightIndex<min(URP_FP_DIRECTIONAL_LIGHTS_COUNT,MAX_VISIBLE_LIGHTS);lightIndex++)
      {Light l=GetAdditionalLight(lightIndex,i.positionWS,half4(1,1,1,1));color+=albedo*ExtraLight(l,n);}
     #endif
     uint pixelLightCount=GetAdditionalLightsCount();
     LIGHT_LOOP_BEGIN(pixelLightCount)
      Light l=GetAdditionalLight(lightIndex,i.positionWS,half4(1,1,1,1));color+=albedo*ExtraLight(l,n);
     LIGHT_LOOP_END
    #endif
    half3 h=SafeNormalize(main.direction+v);
    color+=main.color*_Specular*Band(dot(n,h),_SpecThreshold)*b2;
    half rim=pow(saturate(1-dot(n,v)),_RimPower);
    color+=_RimColor.rgb*rim*_RimStrength*b1;
    color*=1-_InkStrength*smoothstep(0.82,0.99,rim);
    color+=_EmissionColor.rgb;
    return half4(MixFog(color,i.fog),1);
   }
   ENDHLSL
  }
  Pass
  {
   Name "ShadowCaster" Tags {"LightMode"="ShadowCaster"}
   ZWrite On ZTest LEqual ColorMask 0 Cull Back
   HLSLPROGRAM
   #pragma vertex ShadowVert
   #pragma fragment ShadowFrag
   #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW
   float3 _LightDirection;float3 _LightPosition;
   float4 ShadowVert(Attr v):SV_POSITION
   {
    float3 p=TransformObjectToWorld(v.positionOS.xyz),n=TransformObjectToWorldNormal(v.normalOS);
    #if defined(_CASTING_PUNCTUAL_LIGHT_SHADOW)
     float3 ld=normalize(_LightPosition-p);
    #else
     float3 ld=_LightDirection;
    #endif
    float4 c=TransformWorldToHClip(ApplyShadowBias(p,n,ld));
    #if UNITY_REVERSED_Z
     c.z=min(c.z,UNITY_NEAR_CLIP_VALUE*c.w);
    #else
     c.z=max(c.z,UNITY_NEAR_CLIP_VALUE*c.w);
    #endif
    return c;
   }
   half4 ShadowFrag():SV_Target{return 0;}
   ENDHLSL
  }
  Pass
  {
   Name "DepthOnly" Tags {"LightMode"="DepthOnly"}
   ZWrite On ColorMask R Cull Back
   HLSLPROGRAM
   #pragma vertex DepthVert
   #pragma fragment DepthFrag
   float4 DepthVert(Attr v):SV_POSITION{return TransformObjectToHClip(v.positionOS.xyz);}
   half4 DepthFrag():SV_Target{return 0;}
   ENDHLSL
  }
 }
 FallBack Off
}
