using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace AtmosferaAnime
{
    public enum StatusEdgeLayout { FullFrame, EdgeStrip }
    public enum StatusEdgeBlend { Alpha, Additive, Multiply }
    public enum StatusEdgeAlpha { TextureAlpha, RedChannel, Luminance }

    [CreateAssetMenu(menuName="Atmosfera Anime/Status Edge Style",fileName="StatusEdgeStyle")]
    public sealed class StatusEdgeStyle : ScriptableObject
    {
        [Header("Your image")]
        [Tooltip("Optional sprite. Use an unpacked sprite or a rectangular, unrotated atlas entry. Takes priority over Texture.")]
        public Sprite sprite;
        [Tooltip("PNG with alpha, a grayscale mask, or a regular flipbook. No Read/Write flag is needed.")]
        public Texture2D texture;
        [Tooltip("FullFrame stretches your complete frame to the screen. EdgeStrip wraps one horizontal strip around the screen perimeter.")]
        public StatusEdgeLayout layout=StatusEdgeLayout.FullFrame;
        [Tooltip("Replaces the procedural filter for this state while this image is active. Interior sprites share this style. External rig particles and prefabs remain independent.")]
        public bool replaceProcedural=true;
        [Tooltip("Master opacity of this background AND its interior sprite layers.")]
        [Range(0,1)] public float opacity=1;
        [ColorUsage(true,true)] public Color tint=Color.white;
        public bool useStatusColor;
        public StatusEdgeBlend blend=StatusEdgeBlend.Alpha;
        public StatusEdgeAlpha alphaSource=StatusEdgeAlpha.TextureAlpha;
        [Tooltip("Uses the image only as an opacity mask; RGB comes from Tint. Useful for grayscale masks.")]
        public bool maskOnly;
        [Header("Border")]
        [Tooltip("FullFrame: keep the image in a soft border band. EdgeStrip always stays in that band.")]
        public bool limitToEdges;
        [Range(.01f,.49f)] public float width=.18f;
        [Range(.01f,1)] public float feather=.65f;
        [Tooltip("Left, Right, Top, Bottom strengths. Used by EdgeStrip or Limit To Edges.")]
        public Vector4 sides=Vector4.one;
        [Header("Image motion")]
        public Vector2 tiling=Vector2.one;
        public Vector2 offset;
        public Vector2 scroll;
        public float rotationDegrees;
        public float rotationSpeed;
        [Tooltip("FullFrame: repeat the image instead of using transparency outside its rectangle. EdgeStrip always repeats.")]
        public bool repeat;
        [Header("UV distortion")]
        [Range(0,.15f)] public float noiseAmount;
        [Range(.1f,80)] public float noiseFrequency=8;
        [Range(0,10)] public float noiseSpeed=.5f;
        public float noiseSeed=1;
        [Header("Flipbook (left to right, top to bottom)")]
        [Range(1,64)] public int columns=1;
        [Range(1,64)] public int rows=1;
        [Range(0,60)] public float framesPerSecond=12;
        [Min(0)] public int startFrame;

        [Header("Interior sprites / same Volume state")]
        [Tooltip("Up to 4 enabled layers rendered with this background. No StatusVFXRig or Manual trigger is needed.")]
        public List<StatusInteriorSpriteLayer> interiorSprites=new List<StatusInteriorSpriteLayer>();
        public bool HasInteriorSprites()
        {
            if(opacity<=0 || interiorSprites==null)return false;
            foreach(var layer in interiorSprites)if(layer!=null && layer.HasVisual())return true;
            return false;
        }
        public bool HasVisuals(bool includeInterior=true)=>HasImage() || (includeInterior && HasInteriorSprites());

        public bool TryGetSource(out Texture2D source,out Vector4 rect)
        {
            rect=new Vector4(0,0,1,1);source=texture;
            if(sprite!=null)
            {
                // textureRect throws for tightly packed atlas sprites. Reject these
                // explicitly instead of allocating an exception on every camera frame.
                if(sprite.packed && (sprite.packingMode==SpritePackingMode.Tight || sprite.packingRotation!=SpritePackingRotation.None))
                {source=null;return false;}
                source=sprite.texture;
                if(source==null)return false;
                Rect r=sprite.textureRect;
                rect=new Vector4(r.x/source.width,r.y/source.height,r.width/source.width,r.height/source.height);
            }
            return source!=null && source.width>0 && source.height>0 && rect.z>0 && rect.w>0;
        }
        public bool HasImage()=>opacity>0 && tint.a>0 && TryGetSource(out _,out _);
    }

    [Serializable]
    public sealed class StatusEdgeStyleParameter : VolumeParameter<StatusEdgeStyle>
    {
        public StatusEdgeStyleParameter(StatusEdgeStyle value=null,bool overrideState=false):base(value,overrideState){}
        public override void Interp(StatusEdgeStyle from,StatusEdgeStyle to,float t){value=t>0?to:from;}
    }

    // Values are snapshotted before a Render Graph pass is recorded. No mutable
    // style asset is read from inside the rendering callback.
    public struct StatusEdgeSnapshot
    {
        public Texture2D texture;
        public Vector4 rect,color,layout,motion,offset,noise,animation,controls,sides,extra;
        public bool replaceProcedural;
        static readonly int[,] ids=BuildIDs();
        static int[,] BuildIDs()
        {
            string[] names={"Tex","Rect","Color","Layout","Motion","Offset","Noise","Animation","Controls","Sides","Extra"};
            var result=new int[9,names.Length];
            for(int state=0;state<9;state++)for(int field=0;field<names.Length;field++)
                result[state,field]=Shader.PropertyToID("_StatusEdge"+names[field]+state);
            return result;
        }
        public StatusEdgeSnapshot(StatusEdgeStyle style,float amount,Color stateColor,float time,bool includeInterior=true)
        {
            this=default;
            if(style==null || amount<=.0001f)return;
            replaceProcedural=style.replaceProcedural && style.opacity*amount>.0001f && style.HasVisuals(includeInterior);
            if(!style.HasImage() || !style.TryGetSource(out texture,out rect))return;
            float alpha=Mathf.Clamp01(amount*style.opacity);
            Color tint=style.tint*(style.useStatusColor?stateColor:Color.white);color=tint;
            int columns=Mathf.Clamp(style.columns,1,64),rows=Mathf.Clamp(style.rows,1,64),frames=columns*rows;
            int frame=(Mathf.Max(0,style.startFrame)+Mathf.FloorToInt(Mathf.Max(0,time)*Mathf.Clamp(style.framesPerSecond,0,60)))%frames;
            float angle=(style.rotationDegrees+time*style.rotationSpeed)*Mathf.Deg2Rad;
            layout=new Vector4((float)style.layout,Mathf.Clamp(style.width,.01f,.49f),Mathf.Clamp(style.feather,.01f,1),style.limitToEdges?1:0);
            motion=new Vector4(style.tiling.x,style.tiling.y,style.scroll.x*time,style.scroll.y*time);
            offset=new Vector4(style.offset.x,style.offset.y,Mathf.Cos(angle),Mathf.Sin(angle));
            noise=new Vector4(Mathf.Clamp(style.noiseAmount,0,.15f),Mathf.Clamp(style.noiseFrequency,.1f,80),time*Mathf.Clamp(style.noiseSpeed,0,10),style.noiseSeed);
            animation=new Vector4(columns,rows,frame,style.repeat || style.layout==StatusEdgeLayout.EdgeStrip?1:0);
            controls=new Vector4(alpha,(float)style.blend,(float)style.alphaSource,style.maskOnly?1:0);
            sides=new Vector4(Mathf.Clamp01(style.sides.x),Mathf.Clamp01(style.sides.y),Mathf.Clamp01(style.sides.z),Mathf.Clamp01(style.sides.w));
            extra=new Vector4(.5f*columns/(texture.width*rect.z),.5f*rows/(texture.height*rect.w),0,0);

        }
        public void Apply(Material material,int state)
        {
            material.SetTexture(ids[state,0],texture!=null?texture:Texture2D.whiteTexture);
            material.SetVector(ids[state,1],rect);material.SetVector(ids[state,2],color);
            material.SetVector(ids[state,3],layout);material.SetVector(ids[state,4],motion);
            material.SetVector(ids[state,5],offset);material.SetVector(ids[state,6],noise);
            material.SetVector(ids[state,7],animation);material.SetVector(ids[state,8],controls);
            material.SetVector(ids[state,9],sides);material.SetVector(ids[state,10],extra);
        }
    }
}
