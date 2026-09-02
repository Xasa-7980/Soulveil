Shader "Vefects/SH_VFX_Vefects_Slash_BIRP_New"
{
    Properties
{
		[Space(13)][Header(Slash)][Space(13)] _Slash_Texture( "Slash Texture", 2D ) = "white" {}
		_Slash_Scale( "Slash Scale", Float ) = 1
		_Slash_Speed( "Slash Speed", Float ) = 1
		[Space(13)][Header(Slash Noise)][Space(13)] _Slash_Noise_Texture( "Slash Noise Texture", 2D ) = "white" {}
		_Slash_Noise_Scale( "Slash Noise Scale", Vector ) = ( 1, 1, 0, 0 )
		_Slash_Noise_Speed( "Slash Noise Speed", Vector ) = ( -1, 0.5, 0, 0 )
		_Slash_Noise_Intensity( "Slash Noise Intensity", Float ) = 1
		[Space(13)][Header(Emissive)][Space(13)] _Emissive_Slash_Texture( "Emissive Slash Texture", 2D ) = "white" {}
		_Emissive_Slash_Scale( "Emissive Slash Scale", Float ) = 1
		_Emissive_Slash_Speed( "Emissive Slash Speed", Float ) = 1
		_Emissive_Intensity( "Emissive Intensity", Float ) = 3
		[Space(13)][Header(Emissive Dissolve)][Space(13)] _Emissive_Dissolve_Texture( "Emissive Dissolve Texture", 2D ) = "white" {}
		_Emissive_Dissolve_Scale( "Emissive Dissolve Scale", Vector ) = ( 1, 1, 0, 0 )
		_Emissive_Dissolve_Speed( "Emissive Dissolve Speed", Vector ) = ( 1, 1, 0, 0 )
		[Space(13)][Header(Distortion)][Space(13)] _Distortion_Noise_Texture( "Distortion Noise Texture", 2D ) = "white" {}
		_Distortion_Noise_Scale( "Distortion Noise Scale", Vector ) = ( 1, 1, 0, 0 )
		_Distortion_Noise_Speed( "Distortion Noise Speed", Vector ) = ( 1, 1, 0, 0 )
		_Distortion_Intensity( "Distortion Intensity", Float ) = 1
		[Space(13)][Header(Color Noise)][Space(13)] _Color_Noise_Texture( "Color Noise Texture", 2D ) = "white" {}
		_ColorNoise_Scale( "Color Noise Scale", Vector ) = ( 1, 1, 0, 0 )
		_ColorNoise_Speed( "Color Noise Speed", Vector ) = ( 1, 1, 0, 0 )
		_Color_Boost( "Color Boost", Float ) = 1
		[Space(13)][Header(Opacity)][Space(13)] _Mask( "Mask", 2D ) = "white" {}
		_Opacity_Boost( "Opacity Boost", Float ) = 1
		[Space(13)][Header(Colors)][Space(13)] _Color_1( "Color 01", Color ) = ( 1, 0, 0.6261435, 0 )
		_Color_2( "Color 02", Color ) = ( 0.06587124, 0, 1, 0 )
		_Emissive_Color( "Emissive Color", Color ) = ( 1, 0, 0.6261435, 0 )
		_AdditiveLerp( "Additive Lerp", Float ) = 0
		[Space(33)][Header(Cutout)][Space(13)] _Cutout( "Cutout", 2D ) = "white" {}
		_CutoutErosion( "Cutout Erosion", Float ) = 0
		_CutoutErosionSmoothness( "Cutout Erosion Smoothness", Float ) = 0.05
		_CutoutRotation( "Cutout Rotation", Float ) = 0
		_CutoutOffset( "Cutout Offset", Vector ) = ( 0, 0, 0, 0 )
		[Space(33)][Header(AR)][Space(13)] _Cull( "Cull", Float ) = 2
		_Src( "Src", Float ) = 5
		_Dst( "Dst", Float ) = 10
		_ZWrite( "ZWrite", Float ) = 0
		_ZTest( "ZTest", Float ) = 2
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] _texcoord2( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}
    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalPipeline"
            "RenderType"="Transparent"
            "Queue"="Transparent"
        }
        Cull Back
        ZWrite [_ZWrite]
        ZTest [_ZTest]
        Blend [_Src] [_Dst]

        Pass
        {
            Name "UniversalForward"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv0        : TEXCOORD0;
                float4 uv1        : TEXCOORD1;
                float4 color      : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv0         : TEXCOORD0;
                float4 uv1         : TEXCOORD1;
                float4 color       : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            struct ASESurfaceOutput
            {
                half3 Albedo;
                half3 Normal;
                half3 Emission;
                half Metallic;
                half Smoothness;
                half Occlusion;
                half Alpha;
            };

            #undef TRANSFORM_TEX
            		#define TRANSFORM_TEX(tex,name) float4(tex.xy * name##_ST.xy + name##_ST.zw, tex.z, tex.w)
            		struct Input
            		{
            			float2 uv_texcoord;
            			float4 vertexColor : COLOR;
            			float4 uv2_texcoord2;
            		};
            
            		uniform float _ZTest;
            		uniform float _Src;
            		uniform float _Dst;
            		uniform float _ZWrite;
            		uniform float _Cull;
            		uniform float4 _Color_1;
            		uniform float4 _Color_2;
            		uniform sampler2D _Color_Noise_Texture;
            		uniform float2 _ColorNoise_Scale;
            		uniform float2 _ColorNoise_Speed;
            		uniform float _Color_Boost;
            		uniform sampler2D _Mask;
            		uniform float4 _Mask_ST;
            		uniform sampler2D _Slash_Texture;
            		uniform float _Slash_Scale;
            		uniform float _Slash_Speed;
            		uniform sampler2D _Distortion_Noise_Texture;
            		uniform float2 _Distortion_Noise_Scale;
            		uniform float2 _Distortion_Noise_Speed;
            		uniform float _Distortion_Intensity;
            		uniform float _Slash_Noise_Intensity;
            		uniform sampler2D _Slash_Noise_Texture;
            		uniform float2 _Slash_Noise_Scale;
            		uniform float2 _Slash_Noise_Speed;
            		uniform sampler2D _Emissive_Slash_Texture;
            		uniform float _Emissive_Slash_Scale;
            		uniform float _Emissive_Slash_Speed;
            		uniform sampler2D _Emissive_Dissolve_Texture;
            		uniform float2 _Emissive_Dissolve_Scale;
            		uniform float2 _Emissive_Dissolve_Speed;
            		uniform float4 _Emissive_Color;
            		uniform float _Emissive_Intensity;
            		uniform float _Opacity_Boost;
            		uniform float _CutoutErosion;
            		uniform float _CutoutErosionSmoothness;
            		uniform sampler2D _Cutout;
            		uniform float2 _CutoutOffset;
            		uniform float _CutoutRotation;
            		uniform float _AdditiveLerp;
            
            
            		void surf( Input i , inout ASESurfaceOutput o )
            		{
            			float2 uv_TexCoord67 = i.uv_texcoord * _ColorNoise_Scale + ( _Time.y * _ColorNoise_Speed );
            			float3 lerpResult86 = lerp( (_Color_1).rgb , (_Color_2).rgb , tex2D( _Color_Noise_Texture, uv_TexCoord67 ).r);
            			float2 uv_Mask = i.uv_texcoord * _Mask_ST.xy + _Mask_ST.zw;
            			float4 tex2DNode62 = tex2D( _Mask, uv_Mask );
            			float2 appendResult30 = (float2(_Slash_Scale , 1.0));
            			float2 appendResult25 = (float2(_Slash_Speed , 0.0));
            			float2 uv_TexCoord38 = i.uv_texcoord * appendResult30 + ( _Time.y * appendResult25 );
            			float2 uv_TexCoord14 = i.uv_texcoord * _Distortion_Noise_Scale + ( _Time.y * _Distortion_Noise_Speed );
            			float Distortion31 = ( ( tex2D( _Distortion_Noise_Texture, uv_TexCoord14 ).r * 0.1 ) * _Distortion_Intensity );
            			float2 uv_TexCoord49 = i.uv_texcoord * _Slash_Noise_Scale + ( _Time.y * _Slash_Noise_Speed );
            			float clampResult66 = clamp( ( ( tex2D( _Slash_Texture, ( uv_TexCoord38 + Distortion31 ) ).r * _Slash_Noise_Intensity ) + tex2D( _Slash_Noise_Texture, uv_TexCoord49 ).g ) , 0.0 , 1.0 );
            			float temp_output_69_0 = ( tex2DNode62.r * clampResult66 );
            			float2 appendResult32 = (float2(_Emissive_Slash_Scale , 1.0));
            			float2 appendResult23 = (float2(_Emissive_Slash_Speed , 0.0));
            			float2 uv_TexCoord39 = i.uv_texcoord * appendResult32 + ( _Time.y * appendResult23 );
            			float2 uv_TexCoord43 = i.uv_texcoord * _Emissive_Dissolve_Scale + ( _Time.y * _Emissive_Dissolve_Speed );
            			float3 temp_output_107_0 = ( ( ( lerpResult86 * _Color_Boost ) * temp_output_69_0 ) + ( (i.vertexColor).rgb * ( ( ( saturate( (  (-1.0 + ( ( 1.0 - i.uv2_texcoord2.w ) - 0.0 ) * ( 0.0 - -1.0 ) / ( 1.0 - 0.0 ) ) + saturate( ( tex2D( _Emissive_Slash_Texture, ( uv_TexCoord39 + Distortion31 ) ).g * tex2D( _Emissive_Dissolve_Texture, uv_TexCoord43 ).r ) ) ) ) * tex2DNode62.r ) * (_Emissive_Color).rgb ) * _Emissive_Intensity ) ) );
            			float2 temp_output_129_0 = ( i.uv_texcoord + _CutoutOffset );
            			float cos131 = cos( radians( _CutoutRotation ) );
            			float sin131 = sin( radians( _CutoutRotation ) );
            			float2 rotator131 = mul( temp_output_129_0 - float2( 0.5,0.5 ) , float2x2( cos131 , -sin131 , sin131 , cos131 )) + float2( 0.5,0.5 );
            			float smoothstepResult135 = smoothstep( _CutoutErosion , ( _CutoutErosion + _CutoutErosionSmoothness ) , tex2D( _Cutout, rotator131 ).g);
            			float cutout136 = smoothstepResult135;
            			float temp_output_118_0 = saturate( ( saturate( ( i.vertexColor.a * saturate( (  (0.0 + ( ( 1.0 - i.uv2_texcoord2.z ) - 0.0 ) * ( 2.0 - 0.0 ) / ( 1.0 - 0.0 ) ) + saturate( ( saturate( temp_output_69_0 ) * _Opacity_Boost ) ) ) ) ) ) * cutout136 ) );
            			float3 lerpResult120 = lerp( temp_output_107_0 , saturate( ( temp_output_107_0 * temp_output_118_0 ) ) , _AdditiveLerp);
            			o.Emission = lerpResult120;
            			o.Alpha = temp_output_118_0;
            		}

            Varyings vert(Attributes input)
            {
                Varyings output = (Varyings)0;

                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                VertexPositionInputs pos =
                    GetVertexPositionInputs(input.positionOS.xyz);

                output.positionHCS = pos.positionCS;
                output.uv0 = input.uv0;
                output.uv1 = input.uv1;
                output.color = input.color;

                return output;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(IN);

                Input i = (Input)0;
                i.uv_texcoord = IN.uv0;
                i.uv2_texcoord2 = IN.uv1;
                i.vertexColor = IN.color;
                ASESurfaceOutput o =
                    (ASESurfaceOutput)0;

                o.Albedo = 0;
                o.Emission = 0;
                o.Alpha = 1;
                o.Occlusion = 1;
                o.Smoothness = 0;

                surf(i, o);

                half3 finalColor =
                    o.Albedo + o.Emission;

                return half4(finalColor, o.Alpha);
            }

            ENDHLSL
        }
    }

    FallBack Off
}
