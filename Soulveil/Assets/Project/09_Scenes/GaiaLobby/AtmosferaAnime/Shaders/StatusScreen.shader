Shader "Hidden/AtmosferaAnime/StatusScreen"
{
 SubShader
 {
  Tags {"RenderPipeline"="UniversalPipeline"}
  ZTest Always ZWrite Off Cull Off Blend One Zero
  Pass
  {
   HLSLPROGRAM
   #pragma target 3.5
   #pragma vertex Vert
   #pragma fragment Frag
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
   #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
   #include "StatusShapes.hlsl"
   #include "StatusEdgeImages.hlsl"
   float _Intensity,_Poison,_Fire,_Darkness,_Buff,_Wet,_Underwater,_Electric,_Frost,_Bleeding,_EdgeWidth,_DistortionPixels,_AnimationSpeed,_PulseAmount;
   float4 _PoisonColor,_FireColor,_DarknessColor,_BuffColor,_WaterColor,_ElectricColor,_FrostColor,_BloodColor,_PlayerData;
   float _PlayerAssigned;
   float _ProceduralStrength,_PoisonNoiseScale,_PoisonMotion,_FireHeight,_FireTurbulence,_FireSpeed,_DarknessNoiseScale,_DarknessCoverage,_DarknessSpeed,_BuffAnchorToPlayer,_BuffFitPlayer,_BuffSizeMultiplier,_BuffRadius,_BuffThickness,_BuffOrbitSpeed,_BuffMoteDensity,_BuffMoteSize,_BuffGlow,_WetDensity,_WetSpeed,_WetSize,_UnderwaterFrequency,_UnderwaterSpeed,_ElectricBoltCount,_ElectricNoiseFrequency,_ElectricNoiseAmplitude,_ElectricNoiseSpeed,_ElectricWidth,_ElectricBranchStrength,_ElectricFlicker,_FrostCrystalScale,_SnowAmount,_SnowDensity,_SnowSize,_SnowSpeed,_SnowDrift,_BleedNoiseScale,_BleedCoverage;
   float Hash(float2 p){p=frac(p*float2(123.34,456.21));p+=dot(p,p+45.32);return frac(p.x*p.y);}
   float Noise(float2 p){float2 i=floor(p),f=frac(p);f=f*f*(3-2*f);return lerp(lerp(Hash(i),Hash(i+float2(1,0)),f.x),lerp(Hash(i+float2(0,1)),Hash(i+1),f.x),f.y);}
   float2 EdgePoint(int side,float along)
   {
    along=lerp(.08,.92,along);
    if(side==0)return float2(.025,along);
    if(side==1)return float2(along,.975);
    if(side==2)return float2(.975,1-along);
    return float2(1-along,.025);
   }
   float Wiggle(float u,float seed,float t)
   {
    return ((Noise(float2(u*_ElectricNoiseFrequency,seed+t))-.5)*2+
            (Noise(float2(u*_ElectricNoiseFrequency*2.37,seed*3-t*.6))-.5)*.65)*sin(saturate(u)*3.14159265);
   }
   float Bolt(float2 uv,float2 a,float2 b,float seed,float t,float width,float amplitude)
   {
    float2 d=b-a;float len=max(length(d),.0001);float2 axis=d/len,perp=float2(-axis.y,axis.x);
    float u=dot(uv-a,axis)/len;
    float v=dot(uv-a,perp)-Wiggle(u,seed,t)*amplitude;
    float clipLine=step(0,u)*step(u,1);
    float core=1-smoothstep(width,width*2,abs(v));
    float glow=(1-smoothstep(width*2,width*6,abs(v)))*.18;
    return (core+glow)*clipLine;
   }
   float Electricity(float2 uv,float t)
   {
    float aspect=_ScreenParams.x/max(_ScreenParams.y,1);float2 scale=float2(aspect,1);uv*=scale;
    float width=_ElectricWidth/max(_ScreenParams.y,1),sum=0;
    [loop] for(int j=0;j<(int)_ElectricBoltCount;j++)
    {
     float seed=j*7.31+5;float shift=Noise(float2(t*.04,seed));
     float2 a=EdgePoint(j%4,shift)*scale;
     float2 b=EdgePoint((j+1)%4,Noise(float2(seed,t*.055+21)))*scale;
     float value=Bolt(uv,a,b,seed,t,width,_ElectricNoiseAmplitude);
     if(_ElectricBranchStrength>.001)
     {
      float2 dir=normalize(b-a),perp=float2(-dir.y,dir.x);
      float2 branch=a+(b-a)*.55+perp*Wiggle(.55,seed,t)*_ElectricNoiseAmplitude;
      float2 end=branch+dir*.12+perp*(.08+.07*Hash(float2(seed,4)));
      value+=Bolt(uv,branch,end,seed+43,t,width*.7,_ElectricNoiseAmplitude*.45)*_ElectricBranchStrength;
     }
     float light=lerp(1,.45+.55*Noise(float2(seed,t*.6)),_ElectricFlicker);
     sum+=value*light;
    }
    return min(sum,2);
   }
   float Snow(float2 uv,float t)
   {
    float aspect=_ScreenParams.x/max(_ScreenParams.y,1),result=0;
    [unroll] for(int layer=0;layer<2;layer++)
    {
     float density=_SnowDensity*(1+layer*.45);
     float2 g=uv*float2(aspect,1)*density;
     g+=float2(t*_SnowDrift*.2,t*_SnowSpeed)*(1+layer*.3);
     float2 cell=floor(g),p=frac(g)-.5;
     p-=float2(Hash(cell+7),Hash(cell+29))*.45-.225;
     p.x+=sin(t*.7+Hash(cell+41)*6.28)*.08*_SnowDrift;
     float radius=max(_SnowSize*density/max(_ScreenParams.y,1),.002)*(1-layer*.2);
     float chance=step(Hash(cell+layer*53),_SnowAmount*.6);
     result+=SSFlake(p/radius)*chance*(1-layer*.35);
    }
    return saturate(result);
   }
   half4 Frag(Varyings i):SV_Target
   {
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
    float2 sampleUV=i.texcoord;float2 uv=sampleUV;
    // Procedural coordinates use a bottom-left origin, matching WorldToViewportPoint.
    #if UNITY_UV_STARTS_AT_TOP
     uv.y=1-uv.y;
    #endif
    float t=_Time.y*_AnimationSpeed;
    half4 original=SAMPLE_TEXTURE2D_X(_BlitTexture,sampler_LinearClamp,sampleUV);
    float border=min(min(uv.x,1-uv.x),min(uv.y,1-uv.y));
    float edge=1-smoothstep(0,_EdgeWidth,border);
    float pulse=lerp(1,.8+.2*sin(t*3.5),_PulseAmount);
    float2 warp=float2(sin(uv.y*_UnderwaterFrequency+t*_UnderwaterSpeed),cos(uv.x*_UnderwaterFrequency*.8+t*_UnderwaterSpeed*.8));
    float dist=(_Underwater*.65+_Wet*.3+_Poison*.25*edge+_Fire*.5*edge)*_DistortionPixels;
    float3 col=original.rgb;
    if(dist>.0001)
    {
     float2 offset=warp*dist*_BlitTexture_TexelSize.xy;
     #if UNITY_UV_STARTS_AT_TOP
      offset.y=-offset.y;
     #endif
     col=SAMPLE_TEXTURE2D_X(_BlitTexture,sampler_LinearClamp,saturate(sampleUV+offset)).rgb;
    }
    if(_Underwater>.0001)col=lerp(col,col*.65+_WaterColor.rgb*.35,_Underwater*.65);
    if(_Poison>.0001)
    {
     float n=Noise(uv*_PoisonNoiseScale+float2(0,-t*_PoisonMotion));
     col=lerp(col,_PoisonColor.rgb,_Poison*edge*(.2+.4*n)*pulse);
    }
    if(_Fire>.0001)
    {
     float n=Noise(float2(uv.x*_FireTurbulence,uv.y*9-t*_FireSpeed));
     float flames=1-smoothstep(.01,_FireHeight*(.6+.7*n),uv.y);
     flames=max(flames,edge*.4)*(.55+.45*n);
     col=lerp(col,_FireColor.rgb,_Fire*flames*.65);col+=_FireColor.rgb*_Fire*flames*.18;
    }
    if(_Darkness>.0001)
    {
     float n=Noise(uv*_DarknessNoiseScale+float2(t*_DarknessSpeed,0));
     col=lerp(col,_DarknessColor.rgb,_Darkness*saturate(edge+n*.25-.12)*_DarknessCoverage*pulse);
    }
    if(_Buff>.0001)
    {
     float aura=edge*.12;
     float2 moteUV=uv;
     if(_BuffAnchorToPlayer>.5 && _PlayerAssigned>.5)
     {
      aura=0;
      if(_PlayerData.w>.5)
      {
       float aspect=_ScreenParams.x/max(_ScreenParams.y,1);
       float2 p=(uv-_PlayerData.xy)*float2(aspect,1);
       float radius=(_BuffFitPlayer>.5 && _PlayerData.z>.001)?_PlayerData.z*_BuffSizeMultiplier:_BuffRadius;
       radius=max(radius,.015);
       float r=length(p),angle=atan2(p.y,p.x);
       float wobble=1+.025*sin(angle*5-t*_BuffOrbitSpeed*2);
       float width=_BuffThickness/max(_ScreenParams.y,1);
       float ring=1-smoothstep(width,width*2,abs(r-radius*wobble));
       float broken=.25+.75*pow(.5+.5*sin(angle*3-t*_BuffOrbitSpeed*2),3);
       float halo=exp2(-abs(r-radius)*30/max(radius,.05))*.14;
       aura=(ring*broken+halo)*_BuffGlow;
       moteUV=(uv-_PlayerData.xy)/max(radius,.02)+.5;
       aura+=(1-smoothstep(radius,radius*1.65,r))*.015;
      }
     }
     float2 grid=moteUV*_BuffMoteDensity+float2(0,-t*_BuffOrbitSpeed*.8);
     float2 cell=floor(grid),f=frac(grid)-.5;
     float size=_BuffMoteSize*_BuffMoteDensity/max(_ScreenParams.y,1);
     if(_BuffAnchorToPlayer>.5 && _PlayerAssigned>.5)
     {
      float radius=(_BuffFitPlayer>.5 && _PlayerData.z>.001)?_PlayerData.z*_BuffSizeMultiplier:_BuffRadius;
      size/=max(radius,.02);
     }
     float spark=(1-smoothstep(size*.5,max(size,.001),length(f)))*step(.86,Hash(cell));
     float area=1;
     if(_BuffAnchorToPlayer>.5 && _PlayerAssigned>.5)
     {
      float radius=(_BuffFitPlayer>.5 && _PlayerData.z>.001)?_PlayerData.z*_BuffSizeMultiplier:_BuffRadius;
      float2 d=(uv-_PlayerData.xy)*float2(_ScreenParams.x/max(_ScreenParams.y,1),1);
      area=(1-smoothstep(radius,radius*1.6,length(d)))*_PlayerData.w;
     }
     col+=_BuffColor.rgb*_Buff*(aura+spark*area*.5)*pulse;
    }
    if(_Wet>.0001)
    {
     float2 rain=uv*float2(_WetDensity,_WetDensity*.375)+float2(0,t*_WetSpeed);
     float2 rf=frac(rain)-.5;float size=_WetSize*_WetDensity/max(_ScreenParams.y,1);
     float drop=(1-smoothstep(size*.5,max(size,.001),length(rf*float2(2,.5))))*step(.65,Hash(floor(rain)));
     col=lerp(col,col*.8+_WaterColor.rgb*.2,_Wet*edge*.35);col+=drop*_Wet*edge*.12;
    }
    if(_Electric>.0001)col+=_ElectricColor.rgb*_Electric*Electricity(uv,t*_ElectricNoiseSpeed)*.55;
    if(_Frost>.0001)
    {
     float crystal=pow(saturate(sin((uv.x+uv.y)*_FrostCrystalScale)*sin((uv.x-uv.y)*_FrostCrystalScale*.86)),3);
     col=lerp(col,_FrostColor.rgb,_Frost*edge*(.3+.35*crystal));
     if(_SnowAmount>.001)col=lerp(col,lerp(_FrostColor.rgb,float3(1,1,1),.6),Snow(uv,t)*_Frost*.9);
    }
    if(_Bleeding>.0001)
    {
     float blood=saturate(edge*(.6+.65*Noise(uv*_BleedNoiseScale)));
     col=lerp(col,_BloodColor.rgb,_Bleeding*blood*_BleedCoverage*pulse);
    }
    col=lerp(original.rgb,col,_Intensity*_ProceduralStrength);
    col=SEApply(col,TEXTURE2D_ARGS(_StatusEdgeTex0,sampler_LinearClamp),uv,_StatusEdgeRect0,_StatusEdgeColor0,_StatusEdgeLayout0,_StatusEdgeMotion0,_StatusEdgeOffset0,_StatusEdgeNoise0,_StatusEdgeAnimation0,_StatusEdgeControls0,_StatusEdgeSides0,_StatusEdgeExtra0);
    col=SEApply(col,TEXTURE2D_ARGS(_StatusEdgeTex1,sampler_LinearClamp),uv,_StatusEdgeRect1,_StatusEdgeColor1,_StatusEdgeLayout1,_StatusEdgeMotion1,_StatusEdgeOffset1,_StatusEdgeNoise1,_StatusEdgeAnimation1,_StatusEdgeControls1,_StatusEdgeSides1,_StatusEdgeExtra1);
    col=SEApply(col,TEXTURE2D_ARGS(_StatusEdgeTex2,sampler_LinearClamp),uv,_StatusEdgeRect2,_StatusEdgeColor2,_StatusEdgeLayout2,_StatusEdgeMotion2,_StatusEdgeOffset2,_StatusEdgeNoise2,_StatusEdgeAnimation2,_StatusEdgeControls2,_StatusEdgeSides2,_StatusEdgeExtra2);
    col=SEApply(col,TEXTURE2D_ARGS(_StatusEdgeTex3,sampler_LinearClamp),uv,_StatusEdgeRect3,_StatusEdgeColor3,_StatusEdgeLayout3,_StatusEdgeMotion3,_StatusEdgeOffset3,_StatusEdgeNoise3,_StatusEdgeAnimation3,_StatusEdgeControls3,_StatusEdgeSides3,_StatusEdgeExtra3);
    col=SEApply(col,TEXTURE2D_ARGS(_StatusEdgeTex4,sampler_LinearClamp),uv,_StatusEdgeRect4,_StatusEdgeColor4,_StatusEdgeLayout4,_StatusEdgeMotion4,_StatusEdgeOffset4,_StatusEdgeNoise4,_StatusEdgeAnimation4,_StatusEdgeControls4,_StatusEdgeSides4,_StatusEdgeExtra4);
    col=SEApply(col,TEXTURE2D_ARGS(_StatusEdgeTex5,sampler_LinearClamp),uv,_StatusEdgeRect5,_StatusEdgeColor5,_StatusEdgeLayout5,_StatusEdgeMotion5,_StatusEdgeOffset5,_StatusEdgeNoise5,_StatusEdgeAnimation5,_StatusEdgeControls5,_StatusEdgeSides5,_StatusEdgeExtra5);
    col=SEApply(col,TEXTURE2D_ARGS(_StatusEdgeTex6,sampler_LinearClamp),uv,_StatusEdgeRect6,_StatusEdgeColor6,_StatusEdgeLayout6,_StatusEdgeMotion6,_StatusEdgeOffset6,_StatusEdgeNoise6,_StatusEdgeAnimation6,_StatusEdgeControls6,_StatusEdgeSides6,_StatusEdgeExtra6);
    col=SEApply(col,TEXTURE2D_ARGS(_StatusEdgeTex7,sampler_LinearClamp),uv,_StatusEdgeRect7,_StatusEdgeColor7,_StatusEdgeLayout7,_StatusEdgeMotion7,_StatusEdgeOffset7,_StatusEdgeNoise7,_StatusEdgeAnimation7,_StatusEdgeControls7,_StatusEdgeSides7,_StatusEdgeExtra7);
    col=SEApply(col,TEXTURE2D_ARGS(_StatusEdgeTex8,sampler_LinearClamp),uv,_StatusEdgeRect8,_StatusEdgeColor8,_StatusEdgeLayout8,_StatusEdgeMotion8,_StatusEdgeOffset8,_StatusEdgeNoise8,_StatusEdgeAnimation8,_StatusEdgeControls8,_StatusEdgeSides8,_StatusEdgeExtra8);
    return half4(col,original.a);
   }
   ENDHLSL
  }
  Pass
  {
   Name "StatusInteriorAlpha"
   Blend SrcAlpha OneMinusSrcAlpha, Zero One
   ZTest Always ZWrite Off Cull Off
   HLSLPROGRAM
   #pragma target 3.5
   #pragma vertex SIVert
   #pragma fragment SIFrag
   #include "StatusInteriorSprites.hlsl"
   ENDHLSL
  }
  Pass
  {
   Name "StatusInteriorAdditive"
   Blend SrcAlpha One, Zero One
   ZTest Always ZWrite Off Cull Off
   HLSLPROGRAM
   #pragma target 3.5
   #pragma vertex SIVert
   #pragma fragment SIFrag
   #include "StatusInteriorSprites.hlsl"
   ENDHLSL
  }
  Pass
  {
   Name "StatusInteriorMultiply"
   Blend DstColor OneMinusSrcAlpha, Zero One
   ZTest Always ZWrite Off Cull Off
   HLSLPROGRAM
   #pragma target 3.5
   #pragma vertex SIVert
   #pragma fragment SIFrag
   #include "StatusInteriorSprites.hlsl"
   ENDHLSL
  }
 }
 Fallback Off
}
