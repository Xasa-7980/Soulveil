Shader "Vefects/SH_Vefects_BIRP_Sound_Trail_01"
{
    Properties
{
		_IsAdd("Is Add", Float) = 0
		_EmissionOverall("Emission Overall", Float) = 1
		_EmissionParticleColor("Emission Particle Color", Float) = 1
		_EmissionLUT("Emission LUT", Float) = 1
		[Space(33)][Header(Main Texture)][Space(13)]_MainTexture("Main Texture", 2D) = "white" {}
		_MainTextureUVScale("Main Texture UV Scale", Vector) = (5,1,0,0)
		_MainTextureUVPanSpeed("Main Texture UV Pan Speed", Vector) = (0,0,0,0)
		_ErosionSmoothness("Erosion Smoothness", Float) = 1
		[Space(33)][Header(Distortion)][Space(13)]_DistortionNoise("Distortion Noise", 2D) = "white" {}
		_DistortionNoiseTextureSelector("Distortion Noise Texture Selector", Vector) = (0,1,0,0)
		_DistortionNoiseUVScale("Distortion Noise UV Scale", Vector) = (1,1,0,0)
		_DistortionNoiseUVPanSpeed("Distortion Noise UV Pan Speed", Vector) = (0.05,-0.2,0,0)
		_DistortionIntensity("Distortion Intensity", Float) = 0.03
		[Space(33)][Header(LUT)][Space(13)]_LUT("LUT", 2D) = "white" {}
		_LUTAmplitude("LUT Amplitude", Float) = 3
		_LUTOffset("LUT Offset", Float) = 0
		_LUTPanSpeed("LUT Pan Speed", Float) = 0.3
		[Space(33)][Header(WPO)][Space(13)]_WPONoise("WPO Noise", 2D) = "white" {}
		_WPONoiseTextureSelector("WPO Noise Texture Selector", Vector) = (0,1,0,0)
		_WPONoiseUVScale("WPO Noise UV Scale", Vector) = (1,1,0,0)
		_WPONoiseUVPanSpeed("WPO Noise UV Pan Speed", Vector) = (0.05,-0.2,0,0)
		_WPOIntensity("WPO Intensity", Float) = 1
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
            			float4 uv_texcoord;
            			float4 vertexColor : COLOR;
            			float4 uv2_texcoord2;
            			float4 screenPos;
            		};
            
            		uniform float _Dst;
            		uniform float _ZWrite;
            		uniform float _ZTest;
            		uniform float _Cull;
            		uniform float _Src;
            		uniform sampler2D _WPONoise;
            		uniform float2 _WPONoiseUVPanSpeed;
            		uniform float2 _WPONoiseUVScale;
            		uniform float4 _WPONoiseTextureSelector;
            		uniform float _WPOIntensity;
            		uniform sampler2D _LUT;
            		uniform float _LUTPanSpeed;
            		uniform float _LUTAmplitude;
            		uniform float _LUTOffset;
            		uniform float _EmissionLUT;
            		uniform float _EmissionParticleColor;
            		uniform sampler2D _MainTexture;
            		uniform float2 _MainTextureUVPanSpeed;
            		uniform float2 _MainTextureUVScale;
            		uniform sampler2D _DistortionNoise;
            		uniform float2 _DistortionNoiseUVPanSpeed;
            		uniform float2 _DistortionNoiseUVScale;
            		uniform float4 _DistortionNoiseTextureSelector;
            		uniform float _DistortionIntensity;
            		uniform float _EmissionOverall;
            		uniform float _ErosionSmoothness;
            		UNITY_DECLARE_DEPTH_TEXTURE( _CameraDepthTexture );
            		uniform float4 _CameraDepthTexture_TexelSize;
            		uniform float _IsAdd;
            
            		void vertexDataFunc( inout appdata_full v, out Input o )
            		{
            			UNITY_INITIALIZE_OUTPUT( Input, o );
            			float2 panner82 = ( 1.0 * _Time.y * _WPONoiseUVPanSpeed + ( v.texcoord.xy * _WPONoiseUVScale ));
            			float dotResult85 = dot( tex2Dlod( _WPONoise, float4( panner82, 0, 0.0) ) , _WPONoiseTextureSelector );
            			float temp_output_86_0 = saturate( dotResult85 );
            			float3 ase_vertexNormal = v.normal.xyz;
            			float3 WPO89 = ( ( temp_output_86_0 * ase_vertexNormal ) * ( _WPOIntensity * v.texcoord1.y ) );
            			v.vertex.xyz += WPO89;
            			v.vertex.w = 1;
            		}
            
            
            		void surf( Input i , inout ASESurfaceOutput o )
            		{
            			float2 temp_cast_0 = (_LUTPanSpeed).xx;
            			float2 temp_cast_1 = (( ( i.uv_texcoord.xy.x * _LUTAmplitude ) + _LUTOffset )).xx;
            			float2 panner66 = ( 1.0 * _Time.y * temp_cast_0 + temp_cast_1);
            			float2 panner59 = ( 1.0 * _Time.y * _MainTextureUVPanSpeed + ( i.uv_texcoord.xy * _MainTextureUVScale ));
            			float2 panner38 = ( 1.0 * _Time.y * _DistortionNoiseUVPanSpeed + ( i.uv_texcoord.xy * _DistortionNoiseUVScale ));
            			float dotResult41 = dot( tex2D( _DistortionNoise, panner38 ) , _DistortionNoiseTextureSelector );
            			float UVDist47 = ( ( saturate( dotResult41 ) + -0.5 ) * 2.0 );
            			float4 tex2DNode16 = tex2D( _MainTexture, ( panner59 + ( UVDist47 * _DistortionIntensity ) ) );
            			float4 lerpResult61 = lerp( float4( ( tex2D( _LUT, panner66 ).rgb * _EmissionLUT ) , 0.0 ) , ( i.vertexColor * _EmissionParticleColor ) , tex2DNode16.r);
            			float4 temp_output_21_0 = ( lerpResult61 * ( _EmissionOverall * i.uv_texcoord.z ) );
            			float smoothstepResult29 = smoothstep( i.uv2_texcoord2.x , ( i.uv2_texcoord2.x + _ErosionSmoothness ) , tex2DNode16.g);
            			float4 ase_screenPos = float4( i.screenPos.xyz , i.screenPos.w + 0.00000000001 );
            			float4 ase_screenPosNorm = ase_screenPos / ase_screenPos.w;
            			ase_screenPosNorm.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_screenPosNorm.z : ase_screenPosNorm.z * 0.5 + 0.5;
            			float screenDepth26 = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, ase_screenPosNorm.xy ));
            			float distanceDepth26 = saturate( ( screenDepth26 - LinearEyeDepth( ase_screenPosNorm.z ) ) / ( i.uv_texcoord.w ) );
            			float temp_output_18_0 = saturate( ( saturate( ( saturate( smoothstepResult29 ) * i.vertexColor.a ) ) * distanceDepth26 ) );
            			float4 lerpResult54 = lerp( temp_output_21_0 , ( temp_output_21_0 * temp_output_18_0 ) , _IsAdd);
            			o.Emission = lerpResult54.rgb;
            			o.Alpha = temp_output_18_0;
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
