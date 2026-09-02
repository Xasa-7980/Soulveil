Shader "Vefects/SH_VFX_Vefects_Piercing_BIRP_New"
{
    Properties
{
		_Color_1( "Color 01", Color ) = ( 1, 0, 0.6261435, 0 )
		_Color_2( "Color 02", Color ) = ( 0.06587124, 0, 1, 0 )
		[Space(33)][Header(Emissive Noise)][Space(13)] _Emissive_Noise_Texture( "Emissive Noise Texture", 2D ) = "white" {}
		_Texture0( "Emissive Noise Mask Texture", 2D ) = "white" {}
		_EmissiveDissolve_Scale( "Emissive Noise Scale", Vector ) = ( 1, 1, 0, 0 )
		_EmissiveDissolve_Speed( "Emissive Noise Speed", Vector ) = ( 1, 1, 0, 0 )
		_Emissive_Color( "Emissive Color", Color ) = ( 1, 0, 0.6261435, 0 )
		_Emissive_Intensity( "Emissive Intensity", Float ) = 3
		[Space(33)][Header(Piercing Texture)][Space(13)] _Piercing_Texture( "Piercing Texture", 2D ) = "white" {}
		_TextureSample1( "Piercing Noises Texture", 2D ) = "white" {}
		_Piercing_Noise_Scale( "Piercing Noise Scale", Vector ) = ( 1, 1, 0, 0 )
		_Piercing_Noise_Speed( "Piercing Noise Speed", Vector ) = ( -1, 0.5, 0, 0 )
		_Piercing_Noise_Intesnity( "Piercing Noise Intensity", Float ) = 3
		[Space(33)][Header(Distortion Noise)][Space(13)] _Distortion_Noise_Texture( "Distortion Noise Texture", 2D ) = "white" {}
		_Distortion_Noise_Scale( "Distortion Noise Scale", Vector ) = ( 1, 1, 0, 0 )
		_Distortion_Noise_Speed( "Distortion Noise Speed", Vector ) = ( 1, 1, 0, 0 )
		_Distortion_Intensity( "Distortion Intensity", Float ) = 1
		_Distortion_Mask( "Distortion Mask", 2D ) = "white" {}
		[Space(33)][Header(Color Noise Texture)][Space(13)] _Color_Noise_Texture( "Color Noise Texture", 2D ) = "white" {}
		_ColorNoise_Scale( "Color Noise Scale", Vector ) = ( 1, 1, 0, 0 )
		_ColorNoise_Speed( "Color Noise Speed", Vector ) = ( 1, 1, 0, 0 )
		_Color_Boost( "Color Boost", Float ) = 1
		[Space(33)][Header(Opacity Mask)][Space(13)] _Texture1( "Opacity Mask Texture", 2D ) = "white" {}
		_Opacity_Boost( "Opacity Boost", Float ) = 1
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
            
            		uniform float _Dst;
            		uniform float _ZTest;
            		uniform float _ZWrite;
            		uniform float _Src;
            		uniform float _Cull;
            		uniform float4 _Color_1;
            		uniform float4 _Color_2;
            		uniform sampler2D _Color_Noise_Texture;
            		uniform float2 _ColorNoise_Scale;
            		uniform float2 _ColorNoise_Speed;
            		uniform float _Color_Boost;
            		uniform sampler2D _Piercing_Texture;
            		uniform sampler2D _Distortion_Noise_Texture;
            		uniform float2 _Distortion_Noise_Scale;
            		uniform float2 _Distortion_Noise_Speed;
            		uniform sampler2D _Distortion_Mask;
            		uniform float4 _Distortion_Mask_ST;
            		uniform float _Distortion_Intensity;
            		uniform sampler2D _TextureSample1;
            		uniform float2 _Piercing_Noise_Scale;
            		uniform float2 _Piercing_Noise_Speed;
            		uniform float _Piercing_Noise_Intesnity;
            		uniform sampler2D _Texture0;
            		uniform sampler2D _Emissive_Noise_Texture;
            		uniform float2 _EmissiveDissolve_Scale;
            		uniform float2 _EmissiveDissolve_Speed;
            		uniform float4 _Emissive_Color;
            		uniform float _Emissive_Intensity;
            		uniform float _Opacity_Boost;
            		uniform sampler2D _Texture1;
            		uniform float4 _Texture1_ST;
            
            
            		void surf( Input i , inout ASESurfaceOutput o )
            		{
            			float2 uv_TexCoord165 = i.uv_texcoord * _ColorNoise_Scale + ( _Time.y * _ColorNoise_Speed );
            			float3 lerpResult179 = lerp( (_Color_1).rgb , (_Color_2).rgb , tex2D( _Color_Noise_Texture, uv_TexCoord165 ).r);
            			float2 uv_TexCoord111 = i.uv_texcoord * _Distortion_Noise_Scale + ( _Time.y * _Distortion_Noise_Speed );
            			float2 uv_Distortion_Mask = i.uv_texcoord * _Distortion_Mask_ST.xy + _Distortion_Mask_ST.zw;
            			float Distortion122 = ( ( ( tex2D( _Distortion_Noise_Texture, uv_TexCoord111 ).r * ( 1.0 - tex2D( _Distortion_Mask, uv_Distortion_Mask ).r ) ) * 0.1 ) * _Distortion_Intensity );
            			float4 tex2DNode138 = tex2D( _Piercing_Texture, ( i.uv_texcoord + Distortion122 ) );
            			float2 uv_TexCoord126 = i.uv_texcoord * _Piercing_Noise_Scale + ( _Time.y * _Piercing_Noise_Speed );
            			float clampResult150 = clamp( ( tex2DNode138.r + ( tex2DNode138.r * ( tex2D( _TextureSample1, uv_TexCoord126 ).r * _Piercing_Noise_Intesnity ) ) ) , 0.0 , 1.0 );
            			float3 baseColor194 = ( ( lerpResult179 * _Color_Boost ) * clampResult150 );
            			float2 uv_TexCoord134 = i.uv_texcoord * _EmissiveDissolve_Scale + ( _Time.y * _EmissiveDissolve_Speed );
            			float3 emission195 = ( (i.vertexColor).rgb * ( ( saturate( (  (-1.0 + ( ( 1.0 - i.uv2_texcoord2.w ) - 0.0 ) * ( 0.0 - -1.0 ) / ( 1.0 - 0.0 ) ) + saturate( ( tex2D( _Texture0, ( i.uv_texcoord + Distortion122 ) ).g * tex2D( _Emissive_Noise_Texture, ( uv_TexCoord134 + Distortion122 ) ).r ) ) ) ) * (_Emissive_Color).rgb ) * _Emissive_Intensity ) );
            			o.Emission = ( baseColor194 + emission195 );
            			float2 uv_Texture1 = i.uv_texcoord * _Texture1_ST.xy + _Texture1_ST.zw;
            			float alpha196 = saturate( ( i.vertexColor.a * saturate( (  (0.0 + ( ( 1.0 - i.uv2_texcoord2.z ) - 0.0 ) * ( 2.0 - 0.0 ) / ( 1.0 - 0.0 ) ) + saturate( ( ( clampResult150 * _Opacity_Boost ) * tex2D( _Texture1, uv_Texture1 ).b ) ) ) ) ) );
            			o.Alpha = alpha196;
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
