#ifndef STATUS_INTERIOR_SPRITES_INCLUDED
#define STATUS_INTERIOR_SPRITES_INCLUDED
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "StatusShapes.hlsl"
TEXTURE2D(_SITexture);SAMPLER(sampler_SITexture);
float4 _SIRect,_SITint,_SIStartColor,_SIEndColor,_SITimeLife,_SISize,_SIArea,_SIMotion,_SIRotation,_SIAnimation,_SIOptions,_SIAlpha,_SIDraw,_SIExtra,_SICurve0,_SICurve1;
float4 _SISamples[96];
float4 SISample(float t,int offset){float p=saturate(t)*31;int i=(int)floor(p);return lerp(_SISamples[offset+i],_SISamples[offset+min(i+1,31)],frac(p));}
struct SIAttributes{float3 positionOS:POSITION;float2 uv:TEXCOORD0;float2 id:TEXCOORD1;};
struct SIVaryings{float4 positionCS:SV_POSITION;float2 uv:TEXCOORD0;float2 shapeUV:TEXCOORD1;float4 color:COLOR;};
float SIHash(float v){return frac(sin(v*12.9898+78.233)*43758.5453);}
float SICurveAt(int i){return i<4?_SICurve0[clamp(i,0,3)]:_SICurve1[clamp(i-4,0,3)];}
float SICurve(float age){float t=saturate(age)*7;int i=(int)floor(t);return lerp(SICurveAt(i),SICurveAt(min(i+1,7)),frac(t));}
SIVaryings SIVert(SIAttributes v)
{
 SIVaryings o=(SIVaryings)0;
 [branch] if(v.id.x>=_SIDraw.x){o.positionCS=float4(2,2,0,1);return o;}
 float id=v.id.x+_SITimeLife.w*.013;
 float life=lerp(_SITimeLife.y,_SITimeLife.z,SIHash(id+1));
 // Staggered ages make the population visible immediately, including first activation.
 float clock=_SITimeLife.x/max(life,.05)+SIHash(id+3);
 float age=frac(clock),cycle=floor(clock),seconds=age*life;
 float random=id+cycle*71;
 float2 centre=_SIArea.xy+(float2(SIHash(random+11),SIHash(random+17))-.5)*_SIArea.zw;
 if(_SIOptions.z>.5)
 {
  int edge=(int)floor(SIHash(random+19)*4);float along=SIHash(random+23),depth=SIHash(random+29)*_SIOptions.w;
  if(edge==0)centre=float2(depth,along);
  else if(edge==1)centre=float2(1-depth,along);
  else if(edge==2)centre=float2(along,1-depth);
  else centre=float2(along,depth);
 }
 centre+=_SIMotion.xy*seconds;
 centre+=float2(sin(_SITimeLife.x*_SIMotion.w+random),cos(_SITimeLife.x*_SIMotion.w*.83+random*2))*_SIMotion.z;
 if(_SIExtra.z>.5)centre=frac(centre);
 float2 sizeRange=SISample(age,0).xy;
 float height=lerp(sizeRange.x,sizeRange.y,SIHash(random+31));
 float angle=lerp(_SIRotation.x,_SIRotation.y,SIHash(random+37))+seconds*lerp(_SIRotation.z,_SIRotation.w,SIHash(random+41));
 float2 p=v.positionOS.xy*height*float2(_SISize.z,1);
 float sn,cs;sincos(angle,sn,cs);p=float2(p.x*cs-p.y*sn,p.x*sn+p.y*cs);
 p.x/=max(_ScreenParams.x/max(_ScreenParams.y,1),.001);
 o.positionCS=float4((centre+p)*2-1,0,1);
 float frames=max(_SIAnimation.x*_SIAnimation.y,1);
 float frame=floor((_SIOptions.x>.5?_SITimeLife.x:seconds)*_SIAnimation.z)+_SIAnimation.w;
 if(_SIOptions.y>.5)frame+=floor(SIHash(random+43)*frames);
 frame=frame-floor(frame/frames)*frames;
 float2 tile=float2(frame-floor(frame/_SIAnimation.x)*_SIAnimation.x,_SIAnimation.y-1-floor(frame/_SIAnimation.x));
 float2 inset=min(_SIExtra.xy,.5);float2 uv=clamp(v.uv,inset,1-inset);
 o.uv=_SIRect.xy+((uv+tile)/_SIAnimation.xy)*_SIRect.zw;o.shapeUV=v.uv;
 float colorTime=_SISize.w>.5?SIHash(random+53):age;
 o.color=_SITint*lerp(SISample(colorTime,32),SISample(colorTime,64),SIHash(random+59));
 o.color.a*=_SIDraw.y*smoothstep(0,_SIAlpha.x,age)*(1-smoothstep(1-_SIAlpha.y,1,age));
 return o;
}
half4 SIFrag(SIVaryings i):SV_Target
{
 float4 tex=SAMPLE_TEXTURE2D(_SITexture,sampler_SITexture,i.uv);
 float mask=_SIAlpha.z<.5?tex.a:_SIAlpha.z<1.5?tex.r:dot(tex.rgb,float3(.2126,.7152,.0722));
 float alpha=saturate(mask*i.color.a*SSShape(i.shapeUV,_SIDraw.z));clip(alpha-.001);
 float3 rgb=(_SIAlpha.w>.5?float3(1,1,1):tex.rgb)*i.color.rgb;
 if(_SIDraw.w>1.5)rgb*=alpha;
 return half4(rgb,alpha);
}
#endif
