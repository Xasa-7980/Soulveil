Shader "Vefects/SH_Vefects_BIRP_Coaster_01"
{
    Properties
{
		_Emission("Emission", Float) = 1
		_IsAdd("Is Add", Float) = 0
		_ErosionSmoothness("Erosion Smoothness", Float) = 1
		_ParticleColorLUT("Particle Color / LUT", Float) = 1
		[Space(33)][Header(Main Texture)][Space(13)]_MainTexture("Main Texture", 2D) = "white" {}
		_MainTextureSelector("Main Texture Selector", Vector) = (1,0,0,0)
		_MainTextureUVScale("Main Texture UV Scale", Vector) = (1,1,0,0)
		[Space(33)][Header(LUT)][Space(13)]_LUT("LUT", 2D) = "white" {}
		_LUTAmplitude("LUT Amplitude", Float) = 1
		_LUTOffset("LUT Offset", Float) = 0
		_LUTPanSpeed("LUT Pan Speed", Float) = 0
		_LUTErosionSmoothness("LUT Erosion Smoothness", Float) = 1
		[Space(33)][Header(Distortion Texture)][Space(13)]_DistortionTexture("Distortion Texture", 2D) = "white" {}
		_DistortionUVScale("Distortion UV Scale", Vector) = (1,1,0,0)
		_DistortionUVPanSpeed("Distortion UV Pan Speed", Vector) = (-0.02,0.5,0,0)
		_DistortionAmount("Distortion Amount", Float) = 0.1
		[Space(33)][Header(Cutout Mask)][Space(33)]_CutoutMask("Cutout Mask", 2D) = "white" {}
		_CutoutMaskErosion("Cutout Mask Erosion", Float) = 0
		_CutoutMaskErosionSmoothness("Cutout Mask Erosion Smoothness", Float) = 0.2
		[Space(33)][Header(Fresnel Opacity Edges)][Space(13)]_FresnelScale("Fresnel Scale", Float) = 1
		_FresnelPower("Fresnel Power", Float) = 1
		_FresnelBias("Fresnel Bias", Float) = 0
		_FrenselErosion("Frensel Erosion", Float) = 0
		_FrenselErosionSmoothstep("Frensel Erosion Smoothstep", Float) = 1
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

            #ifdef UNITY_PASS_SHADOWCASTER
            			#undef INTERNAL_DATA
            			#undef WorldReflectionVector
            			#undef WorldNormalVector
            			#define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
            			#define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
            			#define WorldNormalVector(data,normal) half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))
            		#endif
            		#undef TRANSFORM_TEX
            		#define TRANSFORM_TEX(tex,name) float4(tex.xy * name##_ST.xy + name##_ST.zw, tex.z, tex.w)
            		struct Input
            		{
            			float4 vertexColor : COLOR;
            			float4 uv2_texcoord2;
            			float4 uv_texcoord;
            			float4 screenPos;
            			float3 worldPos;
            			half ASEIsFrontFacing : VFACE;
            			float3 worldNormal;
            			INTERNAL_DATA
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
            		uniform float2 _MainTextureUVScale;
            		uniform sampler2D _DistortionTexture;
            		uniform float2 _DistortionUVPanSpeed;
            		uniform float2 _DistortionUVScale;
            		uniform float _DistortionAmount;
            		uniform float4 _MainTextureSelector;
            		uniform float _CutoutMaskErosion;
            		uniform float _CutoutMaskErosionSmoothness;
            		uniform sampler2D _CutoutMask;
            		uniform float4 _CutoutMask_ST;
            		uniform float _LUTAmplitude;
            		uniform float _LUTOffset;
            		uniform float _ParticleColorLUT;
            		uniform float _ErosionSmoothness;
            		uniform float _IsAdd;
            		uniform float _Emission;
            		UNITY_DECLARE_DEPTH_TEXTURE( _CameraDepthTexture );
            		uniform float4 _CameraDepthTexture_TexelSize;
            		uniform float _FrenselErosion;
            		uniform float _FrenselErosionSmoothstep;
            		uniform float _FresnelBias;
            		uniform float _FresnelScale;
            		uniform float _FresnelPower;
            
            
            		void surf( Input i , inout ASESurfaceOutput o )
            		{
            			float2 temp_cast_0 = (_LUTPanSpeed).xx;
            			float Pan_Offset78 = i.uv2_texcoord2.y;
            			float2 appendResult104 = (float2(Pan_Offset78 , 0.0));
            			float2 panner94 = ( 1.0 * _Time.y * _DistortionUVPanSpeed + ( i.uv_texcoord.xy * _DistortionUVScale ));
            			float dotResult46 = dot( tex2D( _MainTexture, ( ( ( i.uv_texcoord.xy * _MainTextureUVScale ) + appendResult104 ) + ( (tex2D( _DistortionTexture, panner94 ).rgb).xy * _DistortionAmount ) ) ) , _MainTextureSelector );
            			float2 uv_CutoutMask = i.uv_texcoord * _CutoutMask_ST.xy + _CutoutMask_ST.zw;
            			float smoothstepResult109 = smoothstep( _CutoutMaskErosion , ( _CutoutMaskErosion + _CutoutMaskErosionSmoothness ) , tex2D( _CutoutMask, uv_CutoutMask ).g);
            			float temp_output_50_0 = saturate( ( saturate( dotResult46 ) - ( 1.0 - saturate( smoothstepResult109 ) ) ) );
            			float smoothstepResult53 = smoothstep( i.uv2_texcoord2.x , ( i.uv2_texcoord2.x + _LUTErosionSmoothness ) , temp_output_50_0);
            			float2 temp_cast_2 = (( ( saturate( smoothstepResult53 ) * _LUTAmplitude ) + _LUTOffset )).xx;
            			float2 panner42 = ( 1.0 * _Time.y * temp_cast_0 + temp_cast_2);
            			float4 lerpResult35 = lerp( i.vertexColor , ( i.vertexColor * float4( tex2D( _LUT, panner42 ).rgb , 0.0 ) ) , _ParticleColorLUT);
            			float smoothstepResult29 = smoothstep( i.uv2_texcoord2.x , ( i.uv2_texcoord2.x + _ErosionSmoothness ) , temp_output_50_0);
            			float temp_output_30_0 = saturate( smoothstepResult29 );
            			float4 lerpResult44 = lerp( lerpResult35 , ( lerpResult35 * temp_output_30_0 ) , _IsAdd);
            			o.Emission = ( lerpResult44 * ( _Emission * i.uv_texcoord.z ) ).rgb;
            			float4 ase_screenPos = float4( i.screenPos.xyz , i.screenPos.w + 0.00000000001 );
            			float4 ase_screenPosNorm = ase_screenPos / ase_screenPos.w;
            			ase_screenPosNorm.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_screenPosNorm.z : ase_screenPosNorm.z * 0.5 + 0.5;
            			float screenDepth26 = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, ase_screenPosNorm.xy ));
            			float distanceDepth26 = saturate( ( screenDepth26 - LinearEyeDepth( ase_screenPosNorm.z ) ) / ( i.uv_texcoord.w ) );
            			float3 ase_worldPos = i.worldPos;
            			float3 ase_viewVectorWS = ( _WorldSpaceCameraPos.xyz - ase_worldPos );
            			float3 ase_viewDirWS = normalize( ase_viewVectorWS );
            			float3 ase_worldNormal = i.worldNormal;
            			float3 ase_vertexNormal = mul( unity_WorldToObject, float4( ase_worldNormal, 0 ) );
            			ase_vertexNormal = normalize( ase_vertexNormal );
            			float fresnelNdotV120 = dot( normalize( ( ( i.ASEIsFrontFacing > 0 ? +1 : -1 ) * ase_vertexNormal ) ), ase_viewDirWS );
            			float fresnelNode120 = ( _FresnelBias + _FresnelScale * pow( max( 1.0 - fresnelNdotV120 , 0.0001 ), _FresnelPower ) );
            			float smoothstepResult125 = smoothstep( _FrenselErosion , ( _FrenselErosion + _FrenselErosionSmoothstep ) , saturate( fresnelNode120 ));
            			o.Alpha = saturate( ( saturate( ( saturate( ( temp_output_30_0 * i.vertexColor.a ) ) * distanceDepth26 ) ) * saturate( ( 1.0 - saturate( smoothstepResult125 ) ) ) ) );
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
