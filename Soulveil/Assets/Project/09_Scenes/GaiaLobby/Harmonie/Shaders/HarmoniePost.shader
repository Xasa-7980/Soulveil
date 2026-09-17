Shader "Hidden/Harmonie/LightGlowAtmosphere"
{
 SubShader
 {
  Tags {"RenderPipeline"="UniversalPipeline"}
  ZTest Always ZWrite Off Cull Off Blend One Zero
  HLSLINCLUDE
  #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
  #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
  #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
  TEXTURE2D_X(_HarmonieLow); SAMPLER(sampler_HarmonieLow);
  float _Intensity,_Shafts,_ShaftSamples,_ShaftReach,_ShaftRadius,_Glow,_GlowThreshold,_GlowClamp,_GlowRadius;
  float _Atmosphere,_HazeStart,_HazeEnd,_SeaLevel,_HeightFalloff,_DistantDesaturation,_SunVisibility;
  float4 _ShaftColor,_GlowTint,_NearHazeColor,_FarHazeColor,_SunUV,_SunRGB;
  float Luma(float3 c){return dot(c,float3(.2126,.7152,.0722));}
  float Sky(float raw)
  {
   #if UNITY_REVERSED_Z
    return step(raw,.000001);
   #else
    return step(.999999,raw);
   #endif
  }
  float3 Bright(float3 c)
  {
   c=min(max(c,0),_GlowClamp);
   float brightness=max(c.r,max(c.g,c.b));
   float knee=max(_GlowThreshold*.5,.001);
   float soft=clamp(brightness-_GlowThreshold+knee,0,2*knee);
   soft=soft*soft/(4*knee+.0001);
   return c*(max(soft,brightness-_GlowThreshold)/max(brightness,.0001));
  }
  half4 Extract(Varyings i):SV_Target
  {
   UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
   float2 uv=i.texcoord;float3 rgb=0;
   if(_Glow>.0001)
   {
    float2 o=_BlitTexture_TexelSize.xy*.5;
    // Karis weighting suppresses isolated fireflies before the blur.
    float3 a=Bright(SAMPLE_TEXTURE2D_X(_BlitTexture,sampler_LinearClamp,uv+float2(-o.x,-o.y)).rgb);
    float3 b=Bright(SAMPLE_TEXTURE2D_X(_BlitTexture,sampler_LinearClamp,uv+float2(o.x,-o.y)).rgb);
    float3 c=Bright(SAMPLE_TEXTURE2D_X(_BlitTexture,sampler_LinearClamp,uv+float2(-o.x,o.y)).rgb);
    float3 d=Bright(SAMPLE_TEXTURE2D_X(_BlitTexture,sampler_LinearClamp,uv+float2(o.x,o.y)).rgb);
    float4 w=rcp(1+float4(Luma(a),Luma(b),Luma(c),Luma(d)));
    rgb=(a*w.x+b*w.y+c*w.z+d*w.w)/dot(w,float4(1,1,1,1));
   }
   float mask=0;
   if(_Shafts>.0001 && _SunVisibility>.0001)
   {
    float2 aspect=float2(_ScreenParams.x/max(_ScreenParams.y,1),1);
    float halo=1-smoothstep(_ShaftRadius*.15,_ShaftRadius,length((uv-_SunUV.xy)*aspect));
    mask=Sky(SampleSceneDepth(uv))*halo;
   }
   return half4(rgb,mask);
  }
  half4 Radial(Varyings i):SV_Target
  {
   UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
   float2 uv=i.texcoord;
   half4 center=SAMPLE_TEXTURE2D_X(_BlitTexture,sampler_LinearClamp,uv);
   float2 stepUV=(_SunUV.xy-uv)*_ShaftReach/max(_ShaftSamples,1);
   float sum=0,weight=1,total=0;float2 p=uv;
   [loop] for(int k=0;k<(int)_ShaftSamples;k++)
   {
    p+=stepUV;
    float inside=step(0,p.x)*step(p.x,1)*step(0,p.y)*step(p.y,1);
    sum+=SAMPLE_TEXTURE2D_X(_BlitTexture,sampler_LinearClamp,saturate(p)).a*weight*inside;
    total+=weight;weight*=.96;
   }
   return half4(center.rgb,sum/max(total,.0001));
  }
  half4 Blur(float2 uv,float2 axis)
  {
   float2 o=axis*_BlitTexture_TexelSize.xy*_GlowRadius;
   half4 c=SAMPLE_TEXTURE2D_X(_BlitTexture,sampler_LinearClamp,uv)*.227027;
   c+=SAMPLE_TEXTURE2D_X(_BlitTexture,sampler_LinearClamp,uv+o*1.384615)*.316216;
   c+=SAMPLE_TEXTURE2D_X(_BlitTexture,sampler_LinearClamp,uv-o*1.384615)*.316216;
   c+=SAMPLE_TEXTURE2D_X(_BlitTexture,sampler_LinearClamp,uv+o*3.230769)*.070270;
   c+=SAMPLE_TEXTURE2D_X(_BlitTexture,sampler_LinearClamp,uv-o*3.230769)*.070270;
   return c;
  }
  half4 BlurH(Varyings i):SV_Target {UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);return Blur(i.texcoord,float2(1,0));}
  half4 BlurV(Varyings i):SV_Target {UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);return Blur(i.texcoord,float2(0,1));}
  half4 Composite(Varyings i):SV_Target
  {
   UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
   float2 uv=i.texcoord;half4 src=SAMPLE_TEXTURE2D_X(_BlitTexture,sampler_LinearClamp,uv);
   float3 color=src.rgb;
   if(_Atmosphere>.0001)
   {
    float raw=SampleSceneDepth(uv);
    if(Sky(raw)<.5)
    {
     #if UNITY_REVERSED_Z
      float depth=raw;
     #else
      float depth=lerp(UNITY_NEAR_CLIP_VALUE,1,raw);
     #endif
     float3 world=ComputeWorldSpacePosition(uv,depth,UNITY_MATRIX_I_VP);
     float distanceToCamera=distance(world,_WorldSpaceCameraPos);
     float k=smoothstep(_HazeStart,max(_HazeEnd,_HazeStart+.1),distanceToCamera);
     float height=exp2(-max(world.y-_SeaLevel,0)*_HeightFalloff);
     float amount=k*height*_Atmosphere;
     float3 haze=lerp(_NearHazeColor.rgb,_FarHazeColor.rgb,k);
     color=lerp(color,Luma(color).xxx,k*_DistantDesaturation*_Atmosphere);
     color=lerp(color,haze,amount);
    }
   }
   if(_Glow>.0001 || (_Shafts>.0001 && _SunVisibility>.0001))
   {
    half4 low=SAMPLE_TEXTURE2D_X(_HarmonieLow,sampler_LinearClamp,uv);
    color+=low.rgb*_GlowTint.rgb*_Glow;
    color+=low.a*_ShaftColor.rgb*_SunRGB.rgb*_Shafts*_SunVisibility;
   }
   return half4(lerp(src.rgb,color,_Intensity),src.a);
  }
  ENDHLSL
  Pass {Name "Extract" 
   HLSLPROGRAM
   #pragma target 3.5
   #pragma vertex Vert
   #pragma fragment Extract
   ENDHLSL
  }
  Pass {Name "Shafts" 
   HLSLPROGRAM
   #pragma target 3.5
   #pragma vertex Vert
   #pragma fragment Radial
   ENDHLSL
  }
  Pass {Name "BlurH" 
   HLSLPROGRAM
   #pragma target 3.5
   #pragma vertex Vert
   #pragma fragment BlurH
   ENDHLSL
  }
  Pass {Name "BlurV" 
   HLSLPROGRAM
   #pragma target 3.5
   #pragma vertex Vert
   #pragma fragment BlurV
   ENDHLSL
  }
  Pass {Name "Composite" 
   HLSLPROGRAM
   #pragma target 3.5
   #pragma vertex Vert
   #pragma fragment Composite
   ENDHLSL
  }
 }
 Fallback Off
}
