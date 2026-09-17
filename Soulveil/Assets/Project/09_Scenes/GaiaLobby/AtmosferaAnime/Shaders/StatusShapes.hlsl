#ifndef STATUS_PARTICLE_SHAPES
#define STATUS_PARTICLE_SHAPES
float SSFlake(float2 p)
{
 float r=length(p);float best=1;
 [unroll] for(int k=0;k<3;k++)
 {
  float a=k*1.04719755;float2 q=float2(cos(a)*p.x+sin(a)*p.y,-sin(a)*p.x+cos(a)*p.y);
  q.x=abs(q.x);q.y=abs(q.y);
  float stem=q.y;
  float twig=abs(q.y-abs(q.x-.52)*.7);
  float branch=lerp(1,twig,step(.25,q.x)*step(q.x,.72)*step(q.y,.22));
  best=min(best,min(stem,branch));
 }
 return (1-smoothstep(.025,.085,best))*(1-smoothstep(.78,1,r));
}
float SSShape(float2 uv,float mode)
{
 float2 p=uv*2-1;float r=length(p);
 if(mode<.5)return 1;
 if(mode<1.5)return exp2(-r*r*5)*(1-smoothstep(.75,1,r));
 if(mode<2.5)return SSFlake(p);
 if(mode<3.5)return (1-smoothstep(.025,.08,abs(r-.66)))*(1-smoothstep(.9,1,r));
 if(mode<4.5)return (1-smoothstep(.02,.08,min(abs(p.x),abs(p.y))))*(1-smoothstep(.35,1,r));
 float2 drop=float2(p.x/(.55-.2*p.y),p.y+.1);
 return 1-smoothstep(.75,1,length(drop));
}
#endif
