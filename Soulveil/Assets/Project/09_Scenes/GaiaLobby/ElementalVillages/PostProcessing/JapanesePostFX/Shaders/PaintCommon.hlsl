#ifndef JAPANESE_PAINT_COMMON
#define JAPANESE_PAINT_COMMON
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareNormalsTexture.hlsl"
float _Intensity,_Bands,_WhitePoint,_Saturation,_InkStrength,_InkWidth,_DepthThreshold,_NormalThreshold,_NormalEdges,_ColorEdges;
float _PaperStrength,_PaperScale,_Wobble,_WashRadius,_WashStrength,_Pigment,_Bleed,_Monochrome;
float4 _PaperColor,_InkColor;
float Hash(float2 p) {p=frac(p*float2(123.34,456.21));p+=dot(p,p+45.32);return frac(p.x*p.y);}
float Noise(float2 p) {float2 i=floor(p),f=frac(p);f=f*f*(3-2*f);return lerp(lerp(Hash(i),Hash(i+float2(1,0)),f.x),lerp(Hash(i+float2(0,1)),Hash(i+1),f.x),f.y);}
float Luma(float3 c) {return dot(c,float3(.2126,.7152,.0722));}
float3 Scene(float2 uv) {return SAMPLE_TEXTURE2D_X(_BlitTexture,sampler_LinearClamp,saturate(uv)).rgb;}
float3 Paint(float2 uv) {return LinearToSRGB(saturate(max(Scene(uv),0)/max(_WhitePoint,.1)));}
float Eye(float raw)
{
 float perspective=LinearEyeDepth(raw,_ZBufferParams);
 #if UNITY_REVERSED_Z
 float ortho=lerp(_ProjectionParams.z,_ProjectionParams.y,raw);
 #else
 float ortho=lerp(_ProjectionParams.y,_ProjectionParams.z,raw);
 #endif
 return lerp(perspective,ortho,unity_OrthoParams.w);
}
float Depth(float2 uv) {return Eye(SampleSceneDepth(saturate(uv)));}
float2 PaperUV(float2 uv) {return uv*float2(_ScreenParams.x/max(_ScreenParams.y,1),1)*_PaperScale;}
float Paper(float2 uv)
{
 float2 p=PaperUV(uv);
 float grain=Noise(p*140)*.5+Noise(p*37)*.32+Noise(p*float2(8,240))*.18;
 return grain;
}
float2 BrushUV(float2 uv)
{
 float2 p=PaperUV(uv);
 float2 offset=float2(Noise(p*17),Noise(p*17+71))-.5;
 return saturate(uv+offset*_Wobble*_BlitTexture_TexelSize.xy);
}
float Edges(float2 uv,float width)
{
 float2 off=max(width,.5)*_BlitTexture_TexelSize.xy;
 float2 a=saturate(uv+off*float2(-.5,-.5)),b=saturate(uv+off*float2(.5,.5));
 float2 c=saturate(uv+off*float2(-.5,.5)),d=saturate(uv+off*float2(.5,-.5));
 float za=Depth(a),zb=Depth(b),zc=Depth(c),zd=Depth(d);
 float e=max(abs(za-zb)/max(min(za,zb),.1),abs(zc-zd)/max(min(zc,zd),.1));
 float de=smoothstep(_DepthThreshold,_DepthThreshold*2,e);
 float ne=0;
 if(_NormalEdges>.001)
 {
  float n=max(length(SampleSceneNormals(a)-SampleSceneNormals(b)),length(SampleSceneNormals(c)-SampleSceneNormals(d)));
  ne=smoothstep(_NormalThreshold,_NormalThreshold*1.8,n)*_NormalEdges;
 }
 float ce=0;
 if(_ColorEdges>.001)
 {
  float k=max(abs(Luma(Paint(a))-Luma(Paint(b))),abs(Luma(Paint(c))-Luma(Paint(d))));
  ce=smoothstep(.06,.22,k)*_ColorEdges;
 }
 return saturate(max(de,max(ne,ce)));
}
float3 Grade(float3 c)
{
 float l=Luma(c);c=lerp(l.xxx,c,_Saturation);
 // Optional sumi-e palette. Color controls arrive as linear colors.
 float3 ink=LinearToSRGB(max(_InkColor.rgb,0));
 float3 mono=lerp(ink,float3(1,1,1),saturate(l));
 return saturate(lerp(c,mono,_Monochrome));
}
float3 PaperFinish(float3 c,float2 uv)
{
 float grain=Paper(uv);
 float3 tint=lerp(float3(1,1,1),LinearToSRGB(max(_PaperColor.rgb,0)),_PaperStrength);
 c*=tint;
 c*=1+(grain-.5)*_PaperStrength*.24;
 return saturate(c);
}
// Depth-aware four-quadrant Kuwahara: choose the region with least color variance.
// 36 color + 36 depth taps when enabled; keep radius modest for thin geometry.
float3 WaterWash(float2 uv)
{
 float3 original=Paint(uv);
 if(_WashStrength<.001 || _WashRadius<.01) return original;
 float z0=Depth(uv),best=1e10;float3 chosen=original;
 [unroll] for(int q=0;q<4;q++)
 {
  float2 signXY=float2((q==0||q==2)?-1:1,(q<2)?-1:1);
  float3 mean=0,square=0;float total=0;
  [unroll] for(int y=0;y<3;y++)
  [unroll] for(int x=0;x<3;x++)
  {
   float2 p=saturate(uv+float2(x,y)*signXY*_WashRadius*.5*_BlitTexture_TexelSize.xy);
   float3 col=Paint(p);float delta=abs(Depth(p)-z0)/max(z0,.1);
   float weight=exp2(-delta*160);mean+=col*weight;square+=col*col*weight;total+=weight;
  }
  mean/=max(total,.001);float variance=Luma(abs(square/max(total,.001)-mean*mean));
  if(variance<best){best=variance;chosen=mean;}
 }
 return lerp(original,chosen,_WashStrength);
}
half4 Finish(float2 uv,float3 paint)
{
 half4 src=SAMPLE_TEXTURE2D_X(_BlitTexture,sampler_LinearClamp,uv);
 // Preserve HDR excess for downstream bloom rather than clipping all highlights.
 float3 result=SRGBToLinear(saturate(paint))*_WhitePoint+max(src.rgb-_WhitePoint,0);
 return half4(lerp(src.rgb,result,_Intensity),src.a);
}
half4 FragInk(Varyings i):SV_Target
{
 UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
 float2 uv=i.texcoord,brush=BrushUV(uv);
 float3 col=Paint(uv);float l=max(Luma(col),.001),steps=max(_Bands-1,1);
 float q=floor(l*steps+.5)/steps;
 col=lerp(col,col*(max(q,.035)/l),.9);col=Grade(col);
 float edge=Edges(brush,_InkWidth*(.8+Noise(PaperUV(uv)*24)*.4));
 // Slightly broken ink density resembles a dry brush; no time-based flicker.
 float density=lerp(1,.65+Paper(uv)*.5,_Pigment);
 col=PaperFinish(col,uv);
 col=lerp(col,LinearToSRGB(max(_InkColor.rgb,0)),saturate(edge*_InkStrength*density));
 return Finish(uv,col);
}
half4 FragWash(Varyings i):SV_Target
{
 UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
 float2 uv=i.texcoord,brush=BrushUV(uv);
 float3 col=WaterWash(uv);float steps=max(_Bands-1,1);
 // Soft bands keep broad washes instead of a hard cartoon posterization.
 float3 band=floor(saturate(col)*steps+.5)/steps;
 col=lerp(col,band,.18);col=Grade(col);
 float grain=Paper(uv);
 float pooling=1-_Pigment*(1-grain)*(.12+.16*(1-Luma(col)));
 col*=pooling;
 float edge=Edges(brush,_InkWidth);
 float halo=0;
 if(_Bleed>.001) halo=Edges(brush,_InkWidth*2.5)*_Bleed*.20;
 col=PaperFinish(col,uv);
 float3 ink=LinearToSRGB(max(_InkColor.rgb,0));
 col=lerp(col,ink,saturate(halo));
 col=lerp(col,ink,saturate(edge*_InkStrength*(.55+grain*.45)));
 return Finish(uv,col);
}
#endif
