Shader "Vefects/SH_Vefects_BIRP_Area_Whirlpool_01"
{
    Properties
{
		_Emission("Emission", Float) = 1
		_ErosionSmoothness("Erosion Smoothness", Float) = 1
		[Space(33)][Header(Main Texture)][Space(13)]_MainTexture("Main Texture", 2D) = "white" {}
		_RadialUVTile("Radial UV Tile", Vector) = (1,1,0,0)
		_RadialUVPanSpeed("Radial UV Pan Speed", Vector) = (0.01,-0.5,0,0)
		_RadialUVDistortNoise("Radial UV Distort Noise", 2D) = "white" {}
		_RadialUVDistortScale("Radial UV Distort Scale", Vector) = (1,1,0,0)
		_RadialUVDistortSpeed("Radial UV Distort Speed", Vector) = (0.1,0.01,0,0)
		_RadialUVDistortIntensity("Radial UV Distort Intensity", Float) = 0.1
		[Space(33)][Header(LUT)][Space(13)]_LUT("LUT", 2D) = "white" {}
		_LUTAmplitude("LUT Amplitude", Float) = 1
		_LUTPanSpeed("LUT Pan Speed", Float) = 0
		_LUTOffset("LUT Offset", Float) = 0
		[Space(33)][Header(Distortion)][Space(13)]_DistortionNoise("Distortion Noise", 2D) = "white" {}
		_DistortionNoiseTextureSelector("Distortion Noise Texture Selector", Vector) = (0,1,0,0)
		_DistortionNoiseUVScale("Distortion Noise UV Scale", Vector) = (1,1,0,0)
		_DistortionNoiseUVPanSpeed("Distortion Noise UV Pan Speed", Vector) = (0.05,-0.2,0,0)
		_DistortionIntensity("Distortion Intensity", Float) = 0.03
		[Space(33)][Header(Cutout)][Space(13)]_CutoutTexture("Cutout Texture", 2D) = "white" {}
		_CutoutEro("Cutout Ero", Float) = 0
		_CutoutEroSmooth("Cutout Ero Smooth", Float) = 0.3
		[Space(33)][Header(AR)][Space(13)]_Cull("Cull", Float) = 2
		_Src("Src", Float) = 5
		_Dst("Dst", Float) = 10
		_ZWrite("ZWrite", Float) = 0
		_ZTest("ZTest", Float) = 2
		[HideInInspector] _texcoord2( "", 2D ) = "white" {}
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
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
            			float4 vertexColor : COLOR;
            			float4 uv2_texcoord2;
            			float4 uv_texcoord;
            			float4 screenPos;
            		};
            
            		uniform float _Dst;
            		uniform float _ZWrite;
            		uniform float _ZTest;
            		uniform float _Cull;
            		uniform float _Src;
            		uniform sampler2D _LUT;
            		uniform float _LUTPanSpeed;
            		uniform float _ErosionSmoothness;
            		uniform sampler2D _MainTexture;
            		uniform sampler2D _RadialUVDistortNoise;
            		uniform float2 _RadialUVDistortScale;
            		uniform float2 _RadialUVDistortSpeed;
            		uniform float _RadialUVDistortIntensity;
            		uniform float2 _RadialUVTile;
            		uniform float2 _RadialUVPanSpeed;
            		uniform sampler2D _DistortionNoise;
            		uniform float2 _DistortionNoiseUVPanSpeed;
            		uniform float2 _DistortionNoiseUVScale;
            		uniform float4 _DistortionNoiseTextureSelector;
            		uniform float _DistortionIntensity;
            		uniform float _CutoutEro;
            		uniform float _CutoutEroSmooth;
            		uniform sampler2D _CutoutTexture;
            		uniform float4 _CutoutTexture_ST;
            		uniform float _LUTAmplitude;
            		uniform float _LUTOffset;
            		uniform float _Emission;
            		UNITY_DECLARE_DEPTH_TEXTURE( _CameraDepthTexture );
            		uniform float4 _CameraDepthTexture_TexelSize;
            
            
            		void surf( Input i , inout ASESurfaceOutput o )
            		{
            			float2 temp_cast_0 = (_LUTPanSpeed).xx;
            			float2 appendResult72 = (float2(( (_RadialUVDistortScale).x * i.uv_texcoord.xy.x ) , ( i.uv_texcoord.xy.y * (_RadialUVDistortScale).y )));
            			float2 panner69 = ( ( (_RadialUVDistortSpeed).x * _Time.y ) * float2( 1,0 ) + i.uv_texcoord.xy);
            			float2 panner73 = ( ( _Time.y * (_RadialUVDistortSpeed).y ) * float2( 0,1 ) + i.uv_texcoord.xy);
            			float2 appendResult74 = (float2((panner69).x , (panner73).y));
            			float2 uvs_TexCoord89 = i.uv_texcoord;
            			uvs_TexCoord89.xy = i.uv_texcoord.xy * float2( 2,2 );
            			float2 temp_output_103_0 = ( uvs_TexCoord89.xy - float2( 1,1 ) );
            			float2 appendResult109 = (float2(frac( ( atan2( (temp_output_103_0).x , (temp_output_103_0).y ) / 6.28318548202515 ) ) , length( temp_output_103_0 )));
            			float2 panner81 = ( ( (_RadialUVPanSpeed).x * _Time.y ) * float2( 1,0 ) + appendResult109);
            			float2 panner82 = ( ( _Time.y * (_RadialUVPanSpeed).y ) * float2( 0,1 ) + appendResult109);
            			float2 appendResult107 = (float2((panner81).x , (panner82).y));
            			float2 radialUVs140 = ( ( (tex2D( _RadialUVDistortNoise, ( appendResult72 + appendResult74 ) )).rg * _RadialUVDistortIntensity ) + ( _RadialUVTile * appendResult107 ) );
            			float2 panner38 = ( 1.0 * _Time.y * _DistortionNoiseUVPanSpeed + ( i.uv_texcoord.xy * _DistortionNoiseUVScale ));
            			float dotResult41 = dot( tex2D( _DistortionNoise, panner38 ) , _DistortionNoiseTextureSelector );
            			float UVDist47 = ( ( saturate( dotResult41 ) + -0.5 ) * 2.0 );
            			float2 uv_CutoutTexture = i.uv_texcoord * _CutoutTexture_ST.xy + _CutoutTexture_ST.zw;
            			float smoothstepResult177 = smoothstep( _CutoutEro , ( _CutoutEro + _CutoutEroSmooth ) , tex2D( _CutoutTexture, uv_CutoutTexture ).g);
            			float smoothstepResult29 = smoothstep( i.uv2_texcoord2.x , ( i.uv2_texcoord2.x + _ErosionSmoothness ) , saturate( ( tex2D( _MainTexture, ( radialUVs140 + ( UVDist47 * _DistortionIntensity ) ), float2( 0,0 ), float2( 0,0 ) ).g * saturate( smoothstepResult177 ) ) ));
            			float temp_output_30_0 = saturate( smoothstepResult29 );
            			float2 temp_cast_2 = (( ( temp_output_30_0 * _LUTAmplitude ) + _LUTOffset )).xx;
            			float2 panner165 = ( 1.0 * _Time.y * temp_cast_0 + temp_cast_2);
            			o.Emission = ( ( (i.vertexColor).rgb * tex2D( _LUT, panner165 ).rgb ) * ( _Emission * i.uv_texcoord.z ) );
            			float4 ase_screenPos = float4( i.screenPos.xyz , i.screenPos.w + 0.00000000001 );
            			float4 ase_screenPosNorm = ase_screenPos / ase_screenPos.w;
            			ase_screenPosNorm.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_screenPosNorm.z : ase_screenPosNorm.z * 0.5 + 0.5;
            			float screenDepth26 = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, ase_screenPosNorm.xy ));
            			float distanceDepth26 = saturate( ( screenDepth26 - LinearEyeDepth( ase_screenPosNorm.z ) ) / ( i.uv_texcoord.w ) );
            			o.Alpha = saturate( ( saturate( ( temp_output_30_0 * i.vertexColor.a ) ) * distanceDepth26 ) );
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
