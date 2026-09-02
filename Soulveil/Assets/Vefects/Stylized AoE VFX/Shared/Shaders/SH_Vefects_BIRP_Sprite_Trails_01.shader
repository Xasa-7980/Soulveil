Shader "Vefects/SH_Vefects_BIRP_Sprite_Trails_01"
{
    Properties
{
		[Space(33)][Header(Noise)][Space(13)]_NoiseTexture("Noise Texture", 2D) = "white" {}
		_NoiseUVScale("Noise UV Scale", Vector) = (0.3,1,0,0)
		_NoiseUVSpeed("Noise UV Speed", Vector) = (-0.5,0.01,0,0)
		_NoiseErosion("Noise Erosion", Float) = 0
		_NoiseErosionSmoothness("Noise Erosion Smoothness", Float) = 1
		_Emission("Emission", Float) = 1
		_DepthFade("Depth Fade", Float) = 1
		[Space(33)][Header(Distortion)][Space(13)]_DistortionNoise("Distortion Noise", 2D) = "white" {}
		_DistortionIntensity("Distortion Intensity", Float) = 0.1
		_DistortionNoiseUVScale("Distortion Noise UV Scale", Vector) = (1,1,0,0)
		_DistortionNoiseUVPanSpeed("Distortion Noise UV Pan Speed", Vector) = (0.05,-0.2,0,0)
		[Space(33)][Header(Cutout)][Space(13)]_CutoutTexture("Cutout Texture", 2D) = "white" {}
		_CutoutErosion("Cutout Erosion", Float) = 0
		_CutoutErosionSmoothness("Cutout Erosion Smoothness", Float) = 1
		[Space(33)][Header(AR)][Space(13)]_Cull("Cull", Float) = 2
		_Src("Src", Float) = 5
		_Dst("Dst", Float) = 10
		_ZWrite("ZWrite", Float) = 0
		_ZTest("ZTest", Float) = 2
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

            struct Input
            		{
            			float4 vertexColor : COLOR;
            			float2 uv_texcoord;
            			float4 screenPos;
            		};
            
            		uniform float _Dst;
            		uniform float _ZWrite;
            		uniform float _ZTest;
            		uniform float _Cull;
            		uniform float _Src;
            		uniform float _NoiseErosion;
            		uniform float _NoiseErosionSmoothness;
            		uniform sampler2D _NoiseTexture;
            		uniform float2 _NoiseUVSpeed;
            		uniform float2 _NoiseUVScale;
            		uniform sampler2D _DistortionNoise;
            		uniform float2 _DistortionNoiseUVPanSpeed;
            		uniform float2 _DistortionNoiseUVScale;
            		uniform float _DistortionIntensity;
            		uniform float _Emission;
            		uniform float _CutoutErosion;
            		uniform float _CutoutErosionSmoothness;
            		uniform sampler2D _CutoutTexture;
            		uniform float4 _CutoutTexture_ST;
            		UNITY_DECLARE_DEPTH_TEXTURE( _CameraDepthTexture );
            		uniform float4 _CameraDepthTexture_TexelSize;
            		uniform float _DepthFade;
            
            
            		void surf( Input i , inout ASESurfaceOutput o )
            		{
            			float2 panner61 = ( 1.0 * _Time.y * _NoiseUVSpeed + ( i.uv_texcoord * _NoiseUVScale ));
            			float2 panner47 = ( 1.0 * _Time.y * _DistortionNoiseUVPanSpeed + ( i.uv_texcoord * _DistortionNoiseUVScale ));
            			float2 lerpResult55 = lerp( float2( 0,0.15 ) , ( ( (tex2D( _DistortionNoise, panner47 ).rgb).xy + -0.5 ) * 2.0 ) , _DistortionIntensity);
            			float2 disUV58 = ( panner61 + lerpResult55 );
            			float smoothstepResult29 = smoothstep( _NoiseErosion , ( _NoiseErosion + _NoiseErosionSmoothness ) , tex2D( _NoiseTexture, disUV58 ).g);
            			float temp_output_30_0 = saturate( smoothstepResult29 );
            			o.Emission = ( ( i.vertexColor * temp_output_30_0 ) * _Emission ).rgb;
            			float2 uv_CutoutTexture = i.uv_texcoord * _CutoutTexture_ST.xy + _CutoutTexture_ST.zw;
            			float smoothstepResult36 = smoothstep( _CutoutErosion , ( _CutoutErosion + _CutoutErosionSmoothness ) , tex2D( _CutoutTexture, uv_CutoutTexture ).g);
            			float4 ase_screenPos = float4( i.screenPos.xyz , i.screenPos.w + 0.00000000001 );
            			float4 ase_screenPosNorm = ase_screenPos / ase_screenPos.w;
            			ase_screenPosNorm.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_screenPosNorm.z : ase_screenPosNorm.z * 0.5 + 0.5;
            			float screenDepth26 = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, ase_screenPosNorm.xy ));
            			float distanceDepth26 = saturate( ( screenDepth26 - LinearEyeDepth( ase_screenPosNorm.z ) ) / ( _DepthFade ) );
            			o.Alpha = saturate( ( saturate( ( i.vertexColor.a * saturate( ( temp_output_30_0 - ( 1.0 - saturate( smoothstepResult36 ) ) ) ) ) ) * distanceDepth26 ) );
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
