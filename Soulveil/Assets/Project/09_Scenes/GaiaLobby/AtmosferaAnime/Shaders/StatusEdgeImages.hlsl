#ifndef STATUS_EDGE_IMAGES_INCLUDED
#define STATUS_EDGE_IMAGES_INCLUDED
float SEHash(float2 p)
{
    p=frac(p*float2(123.34,456.21));p+=dot(p,p+45.32);return frac(p.x*p.y);
}
float SENoise(float2 p)
{
    float2 cell=floor(p),f=frac(p);f=f*f*(3-2*f);
    return lerp(lerp(SEHash(cell),SEHash(cell+float2(1,0)),f.x),lerp(SEHash(cell+float2(0,1)),SEHash(cell+1),f.x),f.y);
}
float3 SEApply(float3 background,TEXTURE2D_PARAM(imageTex,imageSampler),float2 screenUV,
    float4 rect,float4 tint,float4 layout,float4 motion,float4 transform,float4 noise,
    float4 animation,float4 controls,float4 sides,float4 extra)
{
    // Uniform branch: unassigned or inactive states do not sample an image.
    [branch] if(controls.x<=.0001)return background;
    float2 uv=screenUV;float mask=1;
    float aspect=_ScreenParams.x/max(_ScreenParams.y,1);
    if(layout.x>.5 || layout.w>.5)
    {
        // All edge distances use units of screen height, keeping equal thickness.
        float distance=999,along=0,sideStrength=0;
        float4 distances=float4(uv.x*aspect,(1-uv.x)*aspect,1-uv.y,uv.y);
        if(sides.x>.001 && distances.x<distance){distance=distances.x;along=2*aspect+1+(1-uv.y);sideStrength=sides.x;}
        if(sides.y>.001 && distances.y<distance){distance=distances.y;along=aspect+uv.y;sideStrength=sides.y;}
        if(sides.z>.001 && distances.z<distance){distance=distances.z;along=aspect+1+(1-uv.x)*aspect;sideStrength=sides.z;}
        if(sides.w>.001 && distances.w<distance){distance=distances.w;along=uv.x*aspect;sideStrength=sides.w;}
        mask=(1-smoothstep(layout.y*(1-layout.z),layout.y,distance))*sideStrength;
        if(layout.x>.5)uv=float2(along/(2*aspect+2),distance/max(layout.y,.001));
    }
    float2 p=(uv-.5)*motion.xy;
    p=float2(p.x*transform.z-p.y*transform.w,p.x*transform.w+p.y*transform.z);
    p+=.5+transform.xy+motion.zw;
    if(noise.x>.0001)
    {
        float2 coord=screenUV*float2(aspect,1)*noise.y+noise.w;
        float2 warp=float2(SENoise(coord+float2(noise.z,0)),SENoise(coord+float2(37,-noise.z)))-.5;
        p+=warp*noise.x*2;
    }
    if(animation.w>.5)p=frac(p);
    else mask*=step(0,p.x)*step(p.x,1)*step(0,p.y)*step(p.y,1);
    // Clamp within a flipbook cell / sprite rect so bilinear sampling cannot
    // leak pixels from the next frame or adjacent atlas entry.
    float2 inset=min(extra.xy,.5);
    p=clamp(p,inset,1-inset);
    float columns=max(animation.x,1),rows=max(animation.y,1);
    float frame=floor(animation.z);
    float2 tile=float2(frame-floor(frame/columns)*columns,rows-1-floor(frame/columns));
    float2 imageUV=rect.xy+((p+tile)/float2(columns,rows))*rect.zw;
    float4 image=SAMPLE_TEXTURE2D_LOD(imageTex,imageSampler,imageUV,0);
    float alpha=controls.z<.5?image.a:controls.z<1.5?image.r:dot(image.rgb,float3(.2126,.7152,.0722));
    alpha=saturate(alpha*tint.a*controls.x*mask);
    float3 rgb=(controls.w>.5?float3(1,1,1):image.rgb)*tint.rgb;
    if(controls.y<.5)return lerp(background,rgb,alpha);
    if(controls.y<1.5)return background+rgb*alpha;
    return lerp(background,background*rgb,alpha);
}
TEXTURE2D(_StatusEdgeTex0);
float4 _StatusEdgeRect0,_StatusEdgeColor0,_StatusEdgeLayout0,_StatusEdgeMotion0,_StatusEdgeOffset0,_StatusEdgeNoise0,_StatusEdgeAnimation0,_StatusEdgeControls0,_StatusEdgeSides0,_StatusEdgeExtra0;
TEXTURE2D(_StatusEdgeTex1);
float4 _StatusEdgeRect1,_StatusEdgeColor1,_StatusEdgeLayout1,_StatusEdgeMotion1,_StatusEdgeOffset1,_StatusEdgeNoise1,_StatusEdgeAnimation1,_StatusEdgeControls1,_StatusEdgeSides1,_StatusEdgeExtra1;
TEXTURE2D(_StatusEdgeTex2);
float4 _StatusEdgeRect2,_StatusEdgeColor2,_StatusEdgeLayout2,_StatusEdgeMotion2,_StatusEdgeOffset2,_StatusEdgeNoise2,_StatusEdgeAnimation2,_StatusEdgeControls2,_StatusEdgeSides2,_StatusEdgeExtra2;
TEXTURE2D(_StatusEdgeTex3);
float4 _StatusEdgeRect3,_StatusEdgeColor3,_StatusEdgeLayout3,_StatusEdgeMotion3,_StatusEdgeOffset3,_StatusEdgeNoise3,_StatusEdgeAnimation3,_StatusEdgeControls3,_StatusEdgeSides3,_StatusEdgeExtra3;
TEXTURE2D(_StatusEdgeTex4);
float4 _StatusEdgeRect4,_StatusEdgeColor4,_StatusEdgeLayout4,_StatusEdgeMotion4,_StatusEdgeOffset4,_StatusEdgeNoise4,_StatusEdgeAnimation4,_StatusEdgeControls4,_StatusEdgeSides4,_StatusEdgeExtra4;
TEXTURE2D(_StatusEdgeTex5);
float4 _StatusEdgeRect5,_StatusEdgeColor5,_StatusEdgeLayout5,_StatusEdgeMotion5,_StatusEdgeOffset5,_StatusEdgeNoise5,_StatusEdgeAnimation5,_StatusEdgeControls5,_StatusEdgeSides5,_StatusEdgeExtra5;
TEXTURE2D(_StatusEdgeTex6);
float4 _StatusEdgeRect6,_StatusEdgeColor6,_StatusEdgeLayout6,_StatusEdgeMotion6,_StatusEdgeOffset6,_StatusEdgeNoise6,_StatusEdgeAnimation6,_StatusEdgeControls6,_StatusEdgeSides6,_StatusEdgeExtra6;
TEXTURE2D(_StatusEdgeTex7);
float4 _StatusEdgeRect7,_StatusEdgeColor7,_StatusEdgeLayout7,_StatusEdgeMotion7,_StatusEdgeOffset7,_StatusEdgeNoise7,_StatusEdgeAnimation7,_StatusEdgeControls7,_StatusEdgeSides7,_StatusEdgeExtra7;
TEXTURE2D(_StatusEdgeTex8);
float4 _StatusEdgeRect8,_StatusEdgeColor8,_StatusEdgeLayout8,_StatusEdgeMotion8,_StatusEdgeOffset8,_StatusEdgeNoise8,_StatusEdgeAnimation8,_StatusEdgeControls8,_StatusEdgeSides8,_StatusEdgeExtra8;

#endif
