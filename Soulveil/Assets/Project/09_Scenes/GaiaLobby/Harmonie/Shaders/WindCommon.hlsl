#ifndef HARMONIE_WIND_COMMON
#define HARMONIE_WIND_COMMON
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Packing.hlsl"
TEXTURE2D(_BaseMap);SAMPLER(sampler_BaseMap);
TEXTURE2D(_BumpMap);SAMPLER(sampler_BumpMap);
CBUFFER_START(UnityPerMaterial)
 float4 _BaseMap_ST;
 half4 _BaseColor;
 half _Cutoff,_AlphaClip,_Cull,_BumpScale,_Smoothness,_Transmission;
 float _RootY,_PlantHeight,_MaskMode,_WindMultiplier;
CBUFFER_END
// One global update per frame. All depth/shadow/color passes share these values.
float4 _HWind; // direction X,Z; amplitude metres; speed
float4 _HWindGust; // gust strength, gust spatial frequency
float4 _HWindFocus; // camera position
float4 _HWindFade; // near, far
struct Attributes
{
 float4 positionOS:POSITION;float3 normalOS:NORMAL;float4 tangentOS:TANGENT;
 float2 uv:TEXCOORD0;float4 color:COLOR;
 UNITY_VERTEX_INPUT_INSTANCE_ID
};
void WindVertex(Attributes input,out float3 p,out float3 n)
{
 p=TransformObjectToWorld(input.positionOS.xyz);n=TransformObjectToWorldNormal(input.normalOS);
 if(_HWind.z<=.00001 || _WindMultiplier<=.00001)return;
 float3 root=TransformObjectToWorld(float3(0,_RootY,0));
 float fade=1-smoothstep(_HWindFade.x,max(_HWindFade.x+.01,_HWindFade.y),distance(root,_HWindFocus.xyz));
 if(fade<=.0001)return;
 float height=saturate((input.positionOS.y-_RootY)/max(_PlantHeight,.001));
 float mask=height;
 if(_MaskMode>.5 && _MaskMode<1.5)mask=saturate(input.uv.y);
 if(_MaskMode>=1.5)mask=saturate(input.color.r);
 mask*=mask;
 float3 dir=float3(_HWind.x,0,_HWind.y);
 float phase=dot(p.xz,float2(.37,.53))+_Time.y*_HWind.w;
 float wave=sin(phase+height*.55)*.7+sin(phase*1.73+1.2)*.3;
 float gust=.5+.5*sin(dot(p.xz,float2(.8,.6))*_HWindGust.y-_Time.y*_HWind.w*.33);
 float strength=_HWind.z*_WindMultiplier*fade*lerp(1,gust,_HWindGust.x);
 float bend=wave*strength; p+=dir*(bend*mask);
 // Approximate inverse-transpose for vertical bending; no finite-difference resampling.
 // Spatial derivatives of the gust and phase are intentionally omitted for cost.
 if(_MaskMode<.5)
 {
  float worldHeight=max(length(TransformObjectToWorldDir(float3(0,_PlantHeight,0),false)),.001);
  float3 up=normalize(TransformObjectToWorldNormal(float3(0,1,0)));
  float slope=2*height*bend/worldHeight;
  n=normalize(n-up*(dot(n,dir)*slope));
 }
}
void WindAlpha(float2 uv)
{
 if(_AlphaClip>.5)clip(SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,uv).a*_BaseColor.a-_Cutoff);
}
#endif
