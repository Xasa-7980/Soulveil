Shader "Vefects/SH_Vefects_BIRP_Fake_Decal_Unlit_Area_Whirlpool_Simple_01"
{
    Properties
{
		_Emissive("Emissive", Float) = 1
		[Space(33)][Header(Decal)][Space(13)]_DecalTexture("Decal Texture", 2D) = "white" {}
		_DecalTextureSelector("Decal Texture Selector", Vector) = (0,1,0,0)
		_DecalUVScale("Decal UV Scale", Vector) = (1,1,0,0)
		_DecalUVPanSpeed("Decal UV Pan Speed", Vector) = (0,0,0,0)
		_DecalRotation("Decal Rotation", Float) = 0
		_DecalScaleFromCenter("Decal Scale From Center", Float) = 1
		_DecalScaleFromCenterNonUniform("Decal Scale From Center Non Uniform", Vector) = (1,1,0,0)
		[Space(33)][Header(Decal Op)][Space(13)]_DecalOpTexture("Decal Op Texture", 2D) = "white" {}
		_DecalOpTextureSelector("Decal Op Texture Selector", Vector) = (0,1,0,0)
		[Space(33)][Header(Fake Decal)][Space(13)]_FakeDecalDepthFade("Fake Decal Depth Fade", Float) = 1
		_FakeDecalDepthFadeErosion("Fake Decal Depth Fade Erosion", Float) = 0
		_FakeDecalDepthFadeErosionSmoothness("Fake Decal Depth Fade Erosion Smoothness", Float) = 0.1
		_ErosionSmoothness("Erosion Smoothness", Float) = 0.05
		_OpacityBoost("Opacity Boost", Float) = 1
		[Space(33)][Header(Radial)][Space(13)]_RadialUVDistortNoise("Radial UV Distort Noise", 2D) = "white" {}
		_RadialUVDistortScale("Radial UV Distort Scale", Vector) = (1,1,0,0)
		_RadialUVDistortSpeed("Radial UV Distort Speed", Vector) = (0.1,0.01,0,0)
		_RadialUVDistortIntensity("Radial UV Distort Intensity", Float) = 0.1
		[Space(33)][Header(LUT)][Space(13)]_LUT("LUT", 2D) = "white" {}
		_LUTAmplitude("LUT Amplitude", Float) = 1
		_LUTOffset("LUT Offset", Float) = 0
		_LUTPanSpeed("LUT Pan Speed", Float) = 0
		_LUTErosionSmoothness("LUT Erosion Smoothness", Float) = 1
		_TwistIntensity("Twist Intensity", Float) = -0.42
		_PolarCoordinatesRadialScale("Polar Coordinates Radial Scale", Float) = 0.5
		_PolarCoordinatesLengthScale("Polar Coordinates Length Scale", Float) = 1
		_PolarCoordinatesPanSpeed("Polar Coordinates Pan Speed", Vector) = (0.3,0,0,0)
		[Space(33)][Header(AR)][Space(13)]_Cull("Cull", Float) = 2
		_Src("Src", Float) = 5
		_Dst("Dst", Float) = 10
		_ZWrite("ZWrite", Float) = 0
		_ZTest("ZTest", Float) = 2
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
            			float4 uv_texcoord;
            			float4 uv2_texcoord2;
            			float4 vertexColor : COLOR;
            			float4 screenPos;
            		};
            
            		uniform float _Src;
            		uniform float _Dst;
            		uniform float _ZWrite;
            		uniform float _ZTest;
            		uniform float _Cull;
            		uniform sampler2D _LUT;
            		uniform float _LUTPanSpeed;
            		uniform float _LUTErosionSmoothness;
            		uniform sampler2D _DecalTexture;
            		uniform float2 _DecalUVPanSpeed;
            		uniform float _PolarCoordinatesRadialScale;
            		uniform float2 _DecalScaleFromCenterNonUniform;
            		uniform float _DecalScaleFromCenter;
            		uniform float _DecalRotation;
            		uniform float _PolarCoordinatesLengthScale;
            		uniform float2 _PolarCoordinatesPanSpeed;
            		uniform float _TwistIntensity;
            		uniform sampler2D _RadialUVDistortNoise;
            		uniform float2 _RadialUVDistortScale;
            		uniform float2 _RadialUVDistortSpeed;
            		uniform float _RadialUVDistortIntensity;
            		uniform float2 _DecalUVScale;
            		uniform float4 _DecalTextureSelector;
            		uniform float _LUTAmplitude;
            		uniform float _LUTOffset;
            		uniform float _Emissive;
            		uniform float _ErosionSmoothness;
            		uniform sampler2D _DecalOpTexture;
            		uniform float4 _DecalOpTexture_ST;
            		uniform float4 _DecalOpTextureSelector;
            		uniform float _OpacityBoost;
            		uniform float _FakeDecalDepthFadeErosion;
            		uniform float _FakeDecalDepthFadeErosionSmoothness;
            		UNITY_DECLARE_DEPTH_TEXTURE( _CameraDepthTexture );
            		uniform float4 _CameraDepthTexture_TexelSize;
            		uniform float _FakeDecalDepthFade;
            
            
            		void surf( Input i , inout ASESurfaceOutput o )
            		{
            			float2 temp_cast_0 = (_LUTPanSpeed).xx;
            			float eros79 = i.uv_texcoord.w;
            			float2 _Vector1 = float2(0.5,0.5);
            			float randomRotate57 = i.uv2_texcoord2.x;
            			float cos52 = cos( ( ( ( _DecalRotation + randomRotate57 ) * ( 2.0 * UNITY_PI ) ) / 360.0 ) );
            			float sin52 = sin( ( ( ( _DecalRotation + randomRotate57 ) * ( 2.0 * UNITY_PI ) ) / 360.0 ) );
            			float2 rotator52 = mul( ( ( ( i.uv_texcoord.xy - _Vector1 ) / ( _DecalScaleFromCenterNonUniform * _DecalScaleFromCenter ) ) + _Vector1 ) - float2( 0.5,0.5 ) , float2x2( cos52 , -sin52 , sin52 , cos52 )) + float2( 0.5,0.5 );
            			float2 decalUV87 = rotator52;
            			float2 temp_output_34_0_g9 = ( decalUV87 - float2( 0.5,0.5 ) );
            			float2 break39_g9 = temp_output_34_0_g9;
            			float2 appendResult50_g9 = (float2(( _PolarCoordinatesRadialScale * ( length( temp_output_34_0_g9 ) * 2.0 ) ) , ( ( atan2( break39_g9.x , break39_g9.y ) * ( 1.0 / 6.28318548202515 ) ) * _PolarCoordinatesLengthScale )));
            			float2 panner326 = ( 1.0 * _Time.y * _PolarCoordinatesPanSpeed + float2( 0,0 ));
            			float2 break53_g9 = appendResult50_g9;
            			float2 twistedUVs342 = ( ( appendResult50_g9 + panner326 ) + ( break53_g9.x * _TwistIntensity ) );
            			float2 break234 = twistedUVs342;
            			float2 appendResult183 = (float2(( (_RadialUVDistortScale).x * break234.x ) , ( break234.y * (_RadialUVDistortScale).y )));
            			float2 panner165 = ( ( (_RadialUVDistortSpeed).x * _Time.y ) * float2( 1,0 ) + twistedUVs342);
            			float2 panner166 = ( ( _Time.y * (_RadialUVDistortSpeed).y ) * float2( 0,1 ) + twistedUVs342);
            			float2 appendResult182 = (float2((panner165).x , (panner166).y));
            			float2 UV_Dist345 = ( (tex2D( _RadialUVDistortNoise, ( appendResult183 + appendResult182 ) )).rg * _RadialUVDistortIntensity );
            			float2 panner247 = ( 1.0 * _Time.y * _DecalUVPanSpeed + ( ( twistedUVs342 + UV_Dist345 ) * _DecalUVScale ));
            			float dotResult104 = dot( tex2D( _DecalTexture, panner247 ) , _DecalTextureSelector );
            			float smoothstepResult93 = smoothstep( eros79 , ( eros79 + _LUTErosionSmoothness ) , saturate( dotResult104 ));
            			float LUTOffset109 = i.uv2_texcoord2.y;
            			float2 temp_cast_2 = (( ( saturate( smoothstepResult93 ) * _LUTAmplitude ) + ( _LUTOffset + LUTOffset109 ) )).xx;
            			float2 panner98 = ( 1.0 * _Time.y * temp_cast_0 + temp_cast_2);
            			float em78 = i.uv_texcoord.z;
            			o.Emission = ( ( tex2D( _LUT, panner98 ).rgb * (i.vertexColor).rgb ) * ( _Emissive * em78 ) );
            			float2 uv_DecalOpTexture = i.uv_texcoord * _DecalOpTexture_ST.xy + _DecalOpTexture_ST.zw;
            			float dotResult107 = dot( tex2D( _DecalOpTexture, uv_DecalOpTexture ) , _DecalOpTextureSelector );
            			float smoothstepResult82 = smoothstep( eros79 , ( eros79 + _ErosionSmoothness ) , saturate( dotResult107 ));
            			float4 ase_screenPos = float4( i.screenPos.xyz , i.screenPos.w + 0.00000000001 );
            			float4 ase_screenPosNorm = ase_screenPos / ase_screenPos.w;
            			ase_screenPosNorm.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_screenPosNorm.z : ase_screenPosNorm.z * 0.5 + 0.5;
            			float screenDepth369 = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, ase_screenPosNorm.xy ));
            			float distanceDepth369 = saturate( ( screenDepth369 - LinearEyeDepth( ase_screenPosNorm.z ) ) / ( _FakeDecalDepthFade ) );
            			float smoothstepResult372 = smoothstep( _FakeDecalDepthFadeErosion , ( _FakeDecalDepthFadeErosion + _FakeDecalDepthFadeErosionSmoothness ) , distanceDepth369);
            			o.Alpha = saturate( ( saturate( ( saturate( ( saturate( saturate( smoothstepResult82 ) ) * _OpacityBoost ) ) * i.vertexColor.a ) ) * ( 1.0 - saturate( smoothstepResult372 ) ) ) );
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
