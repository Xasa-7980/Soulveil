Shader "Hidden/FantasiaTools/Melancholy"
{
 SubShader
 {
  Tags {"RenderPipeline"="UniversalPipeline"}
  Pass
  {
   ZTest Always ZWrite Off Cull Off
   HLSLPROGRAM
   #pragma vertex Vert
   #pragma fragment Frag
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
   #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
   float4 _Grade,_Finish,_Shadows,_Highlights;float _Split;
   half4 Frag(Varyings i):SV_Target
   {
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
    float2 uv=i.texcoord;half4 src=SAMPLE_TEXTURE2D_X(_BlitTexture,sampler_LinearClamp,uv);
    float3 c=max(0,src.rgb*exp2(_Grade.z));
    float lum=dot(c,float3(.2126,.7152,.0722));
    c=lerp(lum.xxx,c,_Grade.y);
    c=max(0,(c-.18)*_Grade.w+.18);
    float3 tint=lerp(_Shadows.rgb,_Highlights.rgb,smoothstep(.05,.8,lum));
    // Normalise tint brightness so colour choice does not crush visibility.
    tint/=max(.1,dot(tint,float3(.2126,.7152,.0722)));
    c*=lerp(float3(1,1,1),tint,_Split);
    c+=_Finish.x*(1-saturate(lum));
    float2 q=(uv-.5)*2;float vignette=smoothstep(.25,1.65,dot(q,q));
    c*=1-vignette*_Finish.y;
    float noise=frac(sin(dot(floor(uv*_ScreenParams.xy)+floor(_Finish.w*12),float2(12.9898,78.233)))*43758.5453)-.5;
    c=max(0,c+noise*_Finish.z);
    return half4(lerp(src.rgb,c,_Grade.x),src.a);
   }
   ENDHLSL
  }
 }
 FallBack Off
}
