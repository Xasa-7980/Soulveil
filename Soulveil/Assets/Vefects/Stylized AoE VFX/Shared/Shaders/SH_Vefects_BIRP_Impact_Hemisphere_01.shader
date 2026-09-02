Shader "Vefects/SH_Vefects_BIRP_Impact_Hemisphere_01"
{
    Properties
{
		_Emission("Emission", Float) = 1
		_ParticleColorLUT("Particle Color / LUT", Float) = 0
		_ErosionSmoothness("Erosion Smoothness", Float) = 1
		[Space(33)][Header(Main Texture)][Space(13)]_MainTexture("Main Texture", 2D) = "white" {}
		_NoiseTextureSelector("Noise Texture Selector", Vector) = (0,1,0,0)
		[Space(33)][Header(Distortion)][Space(13)]_DistortionNoise("Distortion Noise", 2D) = "white" {}
		_DistortionNoiseTextureSelector("Distortion Noise Texture Selector", Vector) = (0,1,0,0)
		_DistortionNoiseUVScale("Distortion Noise UV Scale", Vector) = (1,1,0,0)
		_DistortionNoiseUVPanSpeed("Distortion Noise UV Pan Speed", Vector) = (0.05,-0.2,0,0)
		_DistortionIntensity("Distortion Intensity", Float) = 0.03
		_FresnelErosion("Fresnel Erosion", Float) = 0.1
		_FresnelErosionSmoothness("Fresnel Erosion Smoothness", Float) = 0.3
		_FresnelInvert("Fresnel Invert", Float) = 0
		[Space(33)][Header(LUT)][Space(13)]_LUT("LUT", 2D) = "white" {}
		_LUTAmplitude("LUT Amplitude", Float) = 1
		_LUTOffset("LUT Offset", Float) = 0
		_LUTSpeed("LUT Speed", Float) = 0
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
            			float4 uv_texcoord;
            			float4 uv2_texcoord2;
            			float4 screenPos;
            			float3 worldPos;
            			float3 worldNormal;
            		};
            
            		uniform float _ZWrite;
            		uniform float _ZTest;
            		uniform float _Cull;
            		uniform float _Src;
            		uniform float _Dst;
            		uniform sampler2D _LUT;
            		uniform float _LUTSpeed;
            		uniform sampler2D _MainTexture;
            		uniform sampler2D _DistortionNoise;
            		uniform float2 _DistortionNoiseUVPanSpeed;
            		uniform float2 _DistortionNoiseUVScale;
            		uniform float4 _DistortionNoiseTextureSelector;
            		uniform float _DistortionIntensity;
            		uniform float4 _NoiseTextureSelector;
            		uniform float _LUTAmplitude;
            		uniform float _LUTOffset;
            		uniform float _ParticleColorLUT;
            		uniform float _Emission;
            		uniform float _ErosionSmoothness;
            		UNITY_DECLARE_DEPTH_TEXTURE( _CameraDepthTexture );
            		uniform float4 _CameraDepthTexture_TexelSize;
            		uniform float _FresnelErosion;
            		uniform float _FresnelErosionSmoothness;
            		uniform float _FresnelInvert;
            
            
            		void surf( Input i , inout ASESurfaceOutput o )
            		{
            			float2 temp_cast_0 = (_LUTSpeed).xx;
            			float2 panner59 = ( 1.0 * _Time.y * _DistortionNoiseUVPanSpeed + ( i.uv_texcoord.xy * _DistortionNoiseUVScale ));
            			float randomValue81 = i.uv2_texcoord2.y;
            			float pan84 = i.uv2_texcoord2.z;
            			float2 appendResult92 = (float2(( 1.0 - pan84 ) , pan84));
            			float dotResult62 = dot( tex2D( _DistortionNoise, ( ( panner59 + randomValue81 ) + appendResult92 ) ) , _DistortionNoiseTextureSelector );
            			float UVDist66 = ( ( saturate( dotResult62 ) + -0.5 ) * 2.0 );
            			float2 appendResult88 = (float2(( 1.0 - pan84 ) , pan84));
            			float dotResult79 = dot( tex2D( _MainTexture, ( ( i.uv_texcoord.xy + ( UVDist66 * _DistortionIntensity ) ) + appendResult88 ) ) , _NoiseTextureSelector );
            			float temp_output_80_0 = saturate( dotResult79 );
            			float preLUTmult99 = i.uv2_texcoord2.w;
            			float2 temp_cast_3 = (( ( temp_output_80_0 * ( _LUTAmplitude * preLUTmult99 ) ) + _LUTOffset )).xx;
            			float2 panner44 = ( 1.0 * _Time.y * temp_cast_0 + temp_cast_3);
            			float4 lerpResult37 = lerp( i.vertexColor , ( float4( tex2D( _LUT, panner44 ).rgb , 0.0 ) * i.vertexColor ) , _ParticleColorLUT);
            			o.Emission = ( lerpResult37 * ( _Emission * i.uv_texcoord.z ) ).rgb;
            			float smoothstepResult75 = smoothstep( i.uv2_texcoord2.x , ( i.uv2_texcoord2.x + _ErosionSmoothness ) , temp_output_80_0);
            			float4 ase_screenPos = float4( i.screenPos.xyz , i.screenPos.w + 0.00000000001 );
            			float4 ase_screenPosNorm = ase_screenPos / ase_screenPos.w;
            			ase_screenPosNorm.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_screenPosNorm.z : ase_screenPosNorm.z * 0.5 + 0.5;
            			float screenDepth26 = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, ase_screenPosNorm.xy ));
            			float distanceDepth26 = saturate( ( screenDepth26 - LinearEyeDepth( ase_screenPosNorm.z ) ) / ( i.uv_texcoord.w ) );
            			float3 ase_worldPos = i.worldPos;
            			float3 ase_viewVectorWS = ( _WorldSpaceCameraPos.xyz - ase_worldPos );
            			float3 ase_viewDirWS = normalize( ase_viewVectorWS );
            			float3 ase_worldNormal = i.worldNormal;
            			float fresnelNdotV47 = dot( ase_worldNormal, ase_viewDirWS );
            			float fresnelNode47 = ( 0.0 + 1.0 * pow( max( 1.0 - fresnelNdotV47 , 0.0001 ), 1.0 ) );
            			float smoothstepResult49 = smoothstep( _FresnelErosion , ( _FresnelErosion + _FresnelErosionSmoothness ) , ( 1.0 - fresnelNode47 ));
            			float temp_output_52_0 = saturate( smoothstepResult49 );
            			float lerpResult94 = lerp( temp_output_52_0 , ( 1.0 - temp_output_52_0 ) , _FresnelInvert);
            			o.Alpha = saturate( ( saturate( ( saturate( ( saturate( smoothstepResult75 ) * i.vertexColor.a ) ) * distanceDepth26 ) ) * saturate( lerpResult94 ) ) );
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
