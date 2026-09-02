Shader "Vefects/SH_Vefects_BIRP_Unlit_Master_01"
{
    Properties
{
		_TexturesMultiplySoftLight("Textures Multiply / Soft Light", Float) = 0
		_MaskMultiplySubtract("Mask Multiply / Subtract", Float) = 0
		_ParticleColorLUT("Particle Color / LUT", Float) = 0
		_BlendWithSecondaryTexture("Blend With Secondary Texture", Float) = 0
		[Space(33)][Header(Noise)][Space(13)]_NoiseTexture("Noise Texture", 2D) = "white" {}
		_NoiseTextureSelector("Noise Texture Selector", Vector) = (0,1,0,0)
		_NoiseUVScale("Noise UV Scale", Vector) = (0.3,1,0,0)
		_NoiseUVSpeed("Noise UV Speed", Vector) = (0,0,0,0)
		[Space(33)][Header(Secondary Noise)][Space(13)]_SecondaryNoiseTexture("Secondary Noise Texture", 2D) = "white" {}
		_SecondaryNoiseTextureSelector("Secondary Noise Texture Selector", Vector) = (0,1,0,0)
		_SecondaryNoiseUVScale("Secondary Noise UV Scale", Vector) = (0.3,1,0,0)
		_SecondaryNoiseUVSpeed("Secondary Noise UV Speed", Vector) = (0,0,0,0)
		_ErosionSmoothness("Erosion Smoothness", Float) = 1
		_Emission("Emission", Float) = 1
		_DepthFade("Depth Fade", Float) = 1
		[Space(33)][Header(Distortion)][Space(13)]_DistortionNoise("Distortion Noise", 2D) = "white" {}
		_DistortionNoiseTextureSelector("Distortion Noise Texture Selector", Vector) = (0,1,0,0)
		_DistortionIntensity1("Distortion Intensity", Float) = 0.1
		_DistortionNoiseUVScale("Distortion Noise UV Scale", Vector) = (1,1,0,0)
		_DistortionNoiseUVPanSpeed("Distortion Noise UV Pan Speed", Vector) = (0.05,-0.2,0,0)
		[Space(33)][Header(Cutout)][Space(13)]_CutoutTexture("Cutout Texture", 2D) = "white" {}
		_CutoutTextureSelector("Cutout Texture Selector", Vector) = (0,1,0,0)
		_CutoutErosion("Cutout Erosion", Float) = 0
		_CutoutErosionSmoothness("Cutout Erosion Smoothness", Float) = 1
		_CutoutMaskStrength("Cutout Mask Strength", Float) = 1
		_FresnelErosion("Fresnel Erosion", Float) = 0.1
		_FresnelErosionSmoothness("Fresnel Erosion Smoothness", Float) = 0.3
		_FresnelFade("Fresnel Fade", Float) = 0
		[Space(33)][Header(LUT)][Space(13)]_LUT("LUT", 2D) = "white" {}
		_LUTAmplitude("LUT Amplitude", Float) = 1
		_LUTOffset("LUT Offset", Float) = 0
		_LUTPanSpeed("LUT Pan Speed", Float) = 0
		[Space(33)][Header(AR)][Space(13)]_Cull("Cull", Float) = 2
		_Src("Src", Float) = 5
		_Dst("Dst", Float) = 10
		_ZWrite("ZWrite", Float) = 0
		_ZTest("ZTest", Float) = 2
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] _texcoord2( "", 2D ) = "white" {}
		[HideInInspector] _texcoord3( "", 2D ) = "white" {}
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
            			float4 uv3_texcoord3;
            			float3 worldPos;
            			float3 worldNormal;
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
            		uniform sampler2D _NoiseTexture;
            		uniform float2 _NoiseUVSpeed;
            		uniform float2 _NoiseUVScale;
            		uniform sampler2D _DistortionNoise;
            		uniform float2 _DistortionNoiseUVPanSpeed;
            		uniform float2 _DistortionNoiseUVScale;
            		uniform float4 _DistortionNoiseTextureSelector;
            		uniform float _DistortionIntensity1;
            		uniform float4 _NoiseTextureSelector;
            		uniform sampler2D _SecondaryNoiseTexture;
            		uniform float2 _SecondaryNoiseUVSpeed;
            		uniform float2 _SecondaryNoiseUVScale;
            		uniform float4 _SecondaryNoiseTextureSelector;
            		uniform float _TexturesMultiplySoftLight;
            		uniform float _BlendWithSecondaryTexture;
            		uniform float _CutoutErosion;
            		uniform float _CutoutErosionSmoothness;
            		uniform sampler2D _CutoutTexture;
            		uniform float4 _CutoutTexture_ST;
            		uniform float4 _CutoutTextureSelector;
            		uniform float _CutoutMaskStrength;
            		uniform float _MaskMultiplySubtract;
            		uniform float _LUTAmplitude;
            		uniform float _LUTOffset;
            		uniform float _ParticleColorLUT;
            		uniform float _Emission;
            		uniform float _FresnelErosion;
            		uniform float _FresnelErosionSmoothness;
            		uniform float _FresnelFade;
            		UNITY_DECLARE_DEPTH_TEXTURE( _CameraDepthTexture );
            		uniform float4 _CameraDepthTexture_TexelSize;
            		uniform float _DepthFade;
            
            
            		void surf( Input i , inout ASESurfaceOutput o )
            		{
            			float2 temp_cast_0 = (_LUTPanSpeed).xx;
            			float2 panner61 = ( 1.0 * _Time.y * _NoiseUVSpeed + ( i.uv_texcoord.xy * _NoiseUVScale ));
            			float2 appendResult139 = (float2(i.uv_texcoord.z , i.uv_texcoord.w));
            			float2 panner47 = ( 1.0 * _Time.y * _DistortionNoiseUVPanSpeed + ( i.uv_texcoord.xy * _DistortionNoiseUVScale ));
            			float dotResult80 = dot( tex2D( _DistortionNoise, panner47 ) , _DistortionNoiseTextureSelector );
            			float UVNoise83 = ( ( ( saturate( dotResult80 ) + -0.5 ) * 2.0 ) * i.uv3_texcoord3.x );
            			float2 temp_cast_2 = (UVNoise83).xx;
            			float2 lerpResult55 = lerp( float2( 0,0 ) , temp_cast_2 , _DistortionIntensity1);
            			float dotResult98 = dot( tex2D( _NoiseTexture, ( ( panner61 + appendResult139 ) + lerpResult55 ) ) , _NoiseTextureSelector );
            			float temp_output_99_0 = saturate( dotResult98 );
            			float2 panner90 = ( 1.0 * _Time.y * _SecondaryNoiseUVSpeed + ( i.uv_texcoord.xy * _SecondaryNoiseUVScale ));
            			float2 appendResult141 = (float2(i.uv2_texcoord2.x , i.uv2_texcoord2.y));
            			float2 temp_cast_4 = (UVNoise83).xx;
            			float2 lerpResult94 = lerp( float2( 0,0 ) , temp_cast_4 , _DistortionIntensity1);
            			float dotResult101 = dot( tex2D( _SecondaryNoiseTexture, ( ( panner90 + appendResult141 ) + lerpResult94 ) ) , _SecondaryNoiseTextureSelector );
            			float temp_output_102_0 = saturate( dotResult101 );
            			float lerpResult104 = lerp( saturate( ( temp_output_99_0 * temp_output_102_0 ) ) , saturate( ( 1.0 - ( ( 1.0 - temp_output_99_0 ) * ( 1.0 - temp_output_102_0 ) ) ) ) , _TexturesMultiplySoftLight);
            			float lerpResult113 = lerp( temp_output_99_0 , lerpResult104 , _BlendWithSecondaryTexture);
            			float2 uv_CutoutTexture = i.uv_texcoord * _CutoutTexture_ST.xy + _CutoutTexture_ST.zw;
            			float dotResult77 = dot( float4( tex2D( _CutoutTexture, uv_CutoutTexture ).rgb , 0.0 ) , _CutoutTextureSelector );
            			float smoothstepResult36 = smoothstep( _CutoutErosion , ( _CutoutErosion + _CutoutErosionSmoothness ) , saturate( dotResult77 ));
            			float temp_output_40_0 = saturate( ( saturate( smoothstepResult36 ) * _CutoutMaskStrength ) );
            			float lerpResult115 = lerp( saturate( ( lerpResult113 * temp_output_40_0 ) ) , saturate( ( lerpResult113 - ( 1.0 - temp_output_40_0 ) ) ) , _MaskMultiplySubtract);
            			float smoothstepResult29 = smoothstep( i.uv2_texcoord2.z , ( i.uv2_texcoord2.z + _ErosionSmoothness ) , saturate( lerpResult115 ));
            			float temp_output_30_0 = saturate( smoothstepResult29 );
            			float2 temp_cast_7 = (( ( temp_output_30_0 * ( _LUTAmplitude * i.uv3_texcoord3.y ) ) + _LUTOffset )).xx;
            			float2 panner133 = ( 1.0 * _Time.y * temp_cast_0 + temp_cast_7);
            			float4 lerpResult135 = lerp( i.vertexColor , ( i.vertexColor * float4( tex2D( _LUT, panner133 ).rgb , 0.0 ) ) , _ParticleColorLUT);
            			o.Emission = ( lerpResult135 * ( i.uv2_texcoord2.w * _Emission ) ).rgb;
            			float3 ase_worldPos = i.worldPos;
            			float3 ase_viewVectorWS = ( _WorldSpaceCameraPos.xyz - ase_worldPos );
            			float3 ase_viewDirWS = normalize( ase_viewVectorWS );
            			float3 ase_worldNormal = i.worldNormal;
            			float fresnelNdotV67 = dot( ase_worldNormal, ase_viewDirWS );
            			float fresnelNode67 = ( 0.0 + 1.0 * pow( max( 1.0 - fresnelNdotV67 , 0.0001 ), 1.0 ) );
            			float smoothstepResult71 = smoothstep( _FresnelErosion , ( _FresnelErosion + _FresnelErosionSmoothness ) , ( 1.0 - fresnelNode67 ));
            			float lerpResult146 = lerp( temp_output_30_0 , saturate( ( temp_output_30_0 * saturate( smoothstepResult71 ) ) ) , _FresnelFade);
            			float4 ase_screenPos = float4( i.screenPos.xyz , i.screenPos.w + 0.00000000001 );
            			float4 ase_screenPosNorm = ase_screenPos / ase_screenPos.w;
            			ase_screenPosNorm.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_screenPosNorm.z : ase_screenPosNorm.z * 0.5 + 0.5;
            			float screenDepth26 = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, ase_screenPosNorm.xy ));
            			float distanceDepth26 = saturate( ( screenDepth26 - LinearEyeDepth( ase_screenPosNorm.z ) ) / ( _DepthFade ) );
            			o.Alpha = saturate( ( saturate( ( i.vertexColor.a * lerpResult146 ) ) * distanceDepth26 ) );
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
