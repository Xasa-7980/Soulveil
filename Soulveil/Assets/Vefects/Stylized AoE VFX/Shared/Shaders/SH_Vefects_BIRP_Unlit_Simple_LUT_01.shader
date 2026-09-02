Shader "Vefects/SH_Vefects_BIRP_Unlit_Simple_LUT_01"
{
    Properties
{
		[Space(33)][Header(Main Texture)][Space(13)]_MainTexture("Main Texture", 2D) = "white" {}
		_Emission("Emission", Float) = 1
		_ErosionSmoothness("Erosion Smoothness", Float) = 1
		_ParticleColorLUT("Particle Color / LUT", Float) = 0
		_IsAdd("Is Add", Float) = 0
		[Space(33)][Header(LUT)][Space(13)]_LUT("LUT", 2D) = "white" {}
		_LUTAmplitude("LUT Amplitude", Float) = 1
		_LUTOffset("LUT Offset", Float) = 0
		_LUTPanSpeed("LUT Pan Speed", Float) = 0
		_LUTErosionSmoothness("LUT Erosion Smoothness", Float) = 1
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
            		uniform sampler2D _MainTexture;
            		uniform float4 _MainTexture_ST;
            		uniform float _LUTAmplitude;
            		uniform float _LUTOffset;
            		uniform float _ParticleColorLUT;
            		uniform float _ErosionSmoothness;
            		UNITY_DECLARE_DEPTH_TEXTURE( _CameraDepthTexture );
            		uniform float4 _CameraDepthTexture_TexelSize;
            		uniform float _IsAdd;
            		uniform float _Emission;
            
            
            		void surf( Input i , inout ASESurfaceOutput o )
            		{
            			float2 temp_cast_0 = (_LUTPanSpeed).xx;
            			float2 uv_MainTexture = i.uv_texcoord * _MainTexture_ST.xy + _MainTexture_ST.zw;
            			float4 tex2DNode16 = tex2D( _MainTexture, uv_MainTexture );
            			float smoothstepResult49 = smoothstep( i.uv2_texcoord2.x , ( i.uv2_texcoord2.x + _LUTErosionSmoothness ) , tex2DNode16.b);
            			float2 temp_cast_1 = (( ( saturate( smoothstepResult49 ) * _LUTAmplitude ) + _LUTOffset )).xx;
            			float2 panner42 = ( 1.0 * _Time.y * temp_cast_0 + temp_cast_1);
            			float4 lerpResult35 = lerp( i.vertexColor , ( i.vertexColor * float4( tex2D( _LUT, panner42 ).rgb , 0.0 ) ) , _ParticleColorLUT);
            			float smoothstepResult29 = smoothstep( i.uv2_texcoord2.x , ( i.uv2_texcoord2.x + _ErosionSmoothness ) , tex2DNode16.g);
            			float temp_output_30_0 = saturate( smoothstepResult29 );
            			float4 ase_screenPos = float4( i.screenPos.xyz , i.screenPos.w + 0.00000000001 );
            			float4 ase_screenPosNorm = ase_screenPos / ase_screenPos.w;
            			ase_screenPosNorm.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_screenPosNorm.z : ase_screenPosNorm.z * 0.5 + 0.5;
            			float screenDepth26 = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, ase_screenPosNorm.xy ));
            			float distanceDepth26 = saturate( ( screenDepth26 - LinearEyeDepth( ase_screenPosNorm.z ) ) / ( i.uv_texcoord.w ) );
            			float temp_output_18_0 = saturate( ( saturate( ( temp_output_30_0 * i.vertexColor.a ) ) * distanceDepth26 ) );
            			float4 lerpResult44 = lerp( lerpResult35 , ( ( lerpResult35 * temp_output_30_0 ) * temp_output_18_0 ) , _IsAdd);
            			o.Emission = ( lerpResult44 * ( _Emission * i.uv_texcoord.z ) ).rgb;
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
