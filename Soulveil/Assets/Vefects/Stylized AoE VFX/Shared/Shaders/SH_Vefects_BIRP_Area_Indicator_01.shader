Shader "Vefects/SH_Vefects_BIRP_Area_Indicator_01"
{
    Properties
{
		_DepthFade("Depth Fade", Float) = 0
		_Emission("Emission", Float) = 1
		_IsAdd("Is Add", Float) = 0
		_ErosionSmoothness("Erosion Smoothness", Float) = 0.777
		[Space(33)][Header(Texture 01)][Space(13)]_Texture01("Texture 01", 2D) = "white" {}
		_Texture01Selector("Texture 01 Selector", Vector) = (1,0,0,0)
		_Texture01UVScale("Texture 01 UV Scale", Vector) = (1,1,0,0)
		_Texture01UVPanSpeed("Texture 01 UV Pan Speed", Vector) = (0,0,0,0)
		[Space(33)][Header(Texture 02)][Space(13)]_Texture02("Texture 02", 2D) = "white" {}
		_Texture02Selector("Texture 02 Selector", Vector) = (0,1,0,0)
		_Texture02UVScale("Texture 02 UV Scale", Vector) = (1,1,0,0)
		_Texture02UVPanSpeed("Texture 02 UV Pan Speed", Vector) = (0,0,0,0)
		[Space(33)][Header(LUT)][Space(13)]_LUT("LUT", 2D) = "white" {}
		_LUTAmplitude("LUT Amplitude", Float) = 1
		_LUTOffset("LUT Offset", Float) = 0
		_LUTPanSpeed("LUT Pan Speed", Float) = 0
		_LUTErosionSmoothness("LUT Erosion Smoothness", Float) = 0.777
		[Space(33)][Header(Distortion Texture)][Space(13)]_DistortionTexture("Distortion Texture", 2D) = "white" {}
		_DistortionUVScale("Distortion UV Scale", Vector) = (1,1,0,0)
		_DistortionUVPanSpeed("Distortion UV Pan Speed", Vector) = (-0.005,0.03,0,0)
		_DistortionAmount("Distortion Amount", Float) = 0.3
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
            		uniform float _LUTErosionSmoothness;
            		uniform sampler2D _Texture01;
            		uniform float2 _Texture01UVPanSpeed;
            		uniform float2 _Texture01UVScale;
            		uniform float4 _Texture01Selector;
            		uniform sampler2D _Texture02;
            		uniform float2 _Texture02UVPanSpeed;
            		uniform float2 _Texture02UVScale;
            		uniform sampler2D _DistortionTexture;
            		uniform float2 _DistortionUVPanSpeed;
            		uniform float2 _DistortionUVScale;
            		uniform float _DistortionAmount;
            		uniform float4 _Texture02Selector;
            		uniform float _LUTAmplitude;
            		uniform float _LUTOffset;
            		uniform float _ErosionSmoothness;
            		uniform float _IsAdd;
            		uniform float _Emission;
            		UNITY_DECLARE_DEPTH_TEXTURE( _CameraDepthTexture );
            		uniform float4 _CameraDepthTexture_TexelSize;
            		uniform float _DepthFade;
            
            
            		void surf( Input i , inout ASESurfaceOutput o )
            		{
            			float2 temp_cast_0 = (_LUTPanSpeed).xx;
            			float2 panner87 = ( 1.0 * _Time.y * _Texture01UVPanSpeed + ( i.uv_texcoord.xy * _Texture01UVScale ));
            			float dotResult46 = dot( tex2D( _Texture01, panner87 ) , _Texture01Selector );
            			float2 panner106 = ( 1.0 * _Time.y * _Texture02UVPanSpeed + ( i.uv_texcoord.xy * _Texture02UVScale ));
            			float2 panner94 = ( 1.0 * _Time.y * _DistortionUVPanSpeed + ( i.uv_texcoord.xy * _DistortionUVScale ));
            			float dotResult99 = dot( tex2D( _Texture02, ( panner106 + ( (tex2D( _DistortionTexture, panner94 ).rgb).xy * _DistortionAmount ) ) ) , _Texture02Selector );
            			float temp_output_97_0 = saturate( dotResult99 );
            			float lerpResult123 = lerp( temp_output_97_0 , 1.0 , 0.5);
            			float temp_output_114_0 = saturate( ( ( 1.0 - saturate( ( saturate( ( 1.0 - saturate( ( saturate( dotResult46 ) * lerpResult123 ) ) ) ) * ( 1.0 - ( temp_output_97_0 * 0.5 ) ) ) ) ) - saturate( pow( saturate( ( 1.0 - i.uv_texcoord.xy.y ) ) , i.uv_texcoord.w ) ) ) );
            			float smoothstepResult53 = smoothstep( i.uv2_texcoord2.x , ( i.uv2_texcoord2.x + _LUTErosionSmoothness ) , temp_output_114_0);
            			float2 temp_cast_3 = (( ( saturate( smoothstepResult53 ) * _LUTAmplitude ) + _LUTOffset )).xx;
            			float2 panner42 = ( 1.0 * _Time.y * temp_cast_0 + temp_cast_3);
            			float4 temp_output_36_0 = ( i.vertexColor * float4( tex2D( _LUT, panner42 ).rgb , 0.0 ) );
            			float smoothstepResult29 = smoothstep( i.uv2_texcoord2.x , ( i.uv2_texcoord2.x + _ErosionSmoothness ) , temp_output_114_0);
            			float temp_output_30_0 = saturate( smoothstepResult29 );
            			float4 lerpResult44 = lerp( temp_output_36_0 , ( temp_output_36_0 * temp_output_30_0 ) , _IsAdd);
            			o.Emission = ( lerpResult44 * ( _Emission * i.uv_texcoord.z ) ).rgb;
            			float4 ase_screenPos = float4( i.screenPos.xyz , i.screenPos.w + 0.00000000001 );
            			float4 ase_screenPosNorm = ase_screenPos / ase_screenPos.w;
            			ase_screenPosNorm.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_screenPosNorm.z : ase_screenPosNorm.z * 0.5 + 0.5;
            			float screenDepth26 = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, ase_screenPosNorm.xy ));
            			float distanceDepth26 = saturate( ( screenDepth26 - LinearEyeDepth( ase_screenPosNorm.z ) ) / ( _DepthFade ) );
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
