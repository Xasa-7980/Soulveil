Shader "SH_Vefects_VFX_Fresnel"
{
    Properties
{
		_Noise_Color_Texture("Noise_Color_Texture", 2D) = "white" {}
		_Noise_01_Texture("Noise_01_Texture", 2D) = "white" {}
		_TextureSample0("Texture Sample 0", 2D) = "white" {}
		_Noise_02_Texture("Noise_02_Texture", 2D) = "white" {}
		_NoiseDistortion_Texture("NoiseDistortion_Texture", 2D) = "white" {}
		_NoiseColor_Scale("NoiseColor_Scale", Vector) = (1,1,0,0)
		_Noise_01_Scale("Noise_01_Scale", Vector) = (0.8,0.8,0,0)
		_Noise_02_Scale("Noise_02_Scale", Vector) = (1,1,0,0)
		_NoiseDistortion_Scale("NoiseDistortion_Scale", Vector) = (1,1,0,0)
		_Noise_01_Speed("Noise_01_Speed", Vector) = (0.5,0.5,0,0)
		_NoiseColor_Speed("NoiseColor_Speed", Vector) = (0,0,0,0)
		_Noise_02_Speed("Noise_02_Speed", Vector) = (-0.2,0.4,0,0)
		_NoiseColor_Power("NoiseColor_Power", Float) = 1
		_Vector0("Vector 0", Vector) = (1,1,0,0)
		_NoiseColor_Intensity("NoiseColor_Intensity", Float) = 1
		_Mask_Offset("Mask_Offset", Vector) = (0,0,0,0)
		_NoiseDistortion_Speed("NoiseDistortion_Speed", Vector) = (0.2,0.25,0,0)
		_NoiseDistortion_Intensity("NoiseDistortion_Intensity", Float) = 1
		_Opacity_Boost("Opacity_Boost", Float) = 10
		_Mask_Multiply("Mask_Multiply", Float) = 1
		_Opacity_Power("Opacity_Power", Float) = 1
		_Fresnel_Scale("Fresnel_Scale", Float) = 1
		_Fresnel_Power("Fresnel_Power", Float) = 5
		_Color_2("Color_2", Color) = (1,1,1,0)
		_Color_1("Color_1", Color) = (1,1,1,0)
		_Mask_Power("Mask_Power", Float) = 1
		_Opacity_DepthFade_Intensity("Opacity_DepthFade_Intensity", Float) = 1
		_DepthFade_Distance("DepthFade_Distance", Float) = 1
		_DistortionMask("DistortionMask", Float) = 0
		_Global_Speed("Global_Speed", Float) = 1
		_Dissolve("Dissolve", Float) = 0
		_Emissive_Intensity("Emissive_Intensity", Float) = 1
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
        ZWrite Off

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
            			float3 worldPos;
            			float3 worldNormal;
            			float4 screenPos;
            		};
            
            		uniform float4 _Color_1;
            		uniform float4 _Color_2;
            		uniform sampler2D _Noise_Color_Texture;
            		uniform sampler2D _NoiseDistortion_Texture;
            		uniform float _Global_Speed;
            		uniform float2 _NoiseDistortion_Speed;
            		uniform float2 _NoiseDistortion_Scale;
            		uniform float _NoiseDistortion_Intensity;
            		uniform float2 _NoiseColor_Speed;
            		uniform float2 _NoiseColor_Scale;
            		uniform float _NoiseColor_Intensity;
            		uniform float _NoiseColor_Power;
            		uniform float _Emissive_Intensity;
            		uniform float _Fresnel_Scale;
            		uniform float _Fresnel_Power;
            		UNITY_DECLARE_DEPTH_TEXTURE( _CameraDepthTexture );
            		uniform float4 _CameraDepthTexture_TexelSize;
            		uniform float _DepthFade_Distance;
            		uniform sampler2D _TextureSample0;
            		uniform float _DistortionMask;
            		uniform float2 _Vector0;
            		uniform float2 _Mask_Offset;
            		uniform float _Mask_Power;
            		uniform float _Mask_Multiply;
            		uniform sampler2D _Noise_01_Texture;
            		uniform float2 _Noise_01_Speed;
            		uniform float2 _Noise_01_Scale;
            		uniform sampler2D _Noise_02_Texture;
            		uniform float2 _Noise_02_Speed;
            		uniform float2 _Noise_02_Scale;
            		uniform float _Opacity_Power;
            		uniform float _Opacity_Boost;
            		uniform float _Dissolve;
            		uniform float _Opacity_DepthFade_Intensity;
            
            
            		void surf( Input i , inout ASESurfaceOutput o )
            		{
            			float global_speed178 = ( _Global_Speed * _Time.y );
            			float2 uvs_TexCoord30 = i.uv_texcoord;
            			uvs_TexCoord30.xy = i.uv_texcoord.xy * _NoiseDistortion_Scale;
            			float2 panner79 = ( global_speed178 * _NoiseDistortion_Speed + uvs_TexCoord30.xy);
            			float Distortion64 = ( ( tex2D( _NoiseDistortion_Texture, panner79 ).r * 0.1 ) * _NoiseDistortion_Intensity );
            			float2 uvs_TexCoord246 = i.uv_texcoord;
            			uvs_TexCoord246.xy = i.uv_texcoord.xy * _NoiseColor_Scale;
            			float2 panner248 = ( 1.0 * _Time.y * _NoiseColor_Speed + uvs_TexCoord246.xy);
            			float clampResult235 = clamp( pow( ( tex2D( _Noise_Color_Texture, ( Distortion64 + panner248 ) ).r * _NoiseColor_Intensity ) , _NoiseColor_Power ) , 0.0 , 1.0 );
            			float3 lerpResult239 = lerp( (_Color_1).rgb , (_Color_2).rgb , clampResult235);
            			o.Emission = ( ( lerpResult239 * (i.vertexColor).rgb ) * _Emissive_Intensity );
            			float3 ase_worldPos = i.worldPos;
            			float3 ase_worldViewDir = normalize( UnityWorldSpaceViewDir( ase_worldPos ) );
            			float3 ase_worldNormal = i.worldNormal;
            			float fresnelNdotV167 = dot( ase_worldNormal, ase_worldViewDir );
            			float fresnelNode167 = ( i.uv_texcoord.z + _Fresnel_Scale * pow( 1.0 - fresnelNdotV167, _Fresnel_Power ) );
            			float4 ase_screenPos = float4( i.screenPos.xyz , i.screenPos.w + 0.00000000001 );
            			float4 ase_screenPosNorm = ase_screenPos / ase_screenPos.w;
            			ase_screenPosNorm.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_screenPosNorm.z : ase_screenPosNorm.z * 0.5 + 0.5;
            			float screenDepth137 = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, ase_screenPosNorm.xy ));
            			float distanceDepth137 = abs( ( screenDepth137 - LinearEyeDepth( ase_screenPosNorm.z ) ) / ( _DepthFade_Distance ) );
            			float clampResult136 = clamp( ( 1.0 - distanceDepth137 ) , 0.0 , 1.0 );
            			float2 appendResult216 = (float2(0.0 , 0.0));
            			float2 uvs_TexCoord215 = i.uv_texcoord;
            			uvs_TexCoord215.xy = i.uv_texcoord.xy * _Vector0 + _Mask_Offset;
            			float2 panner218 = ( global_speed178 * appendResult216 + uvs_TexCoord215.xy);
            			float2 uvs_TexCoord26 = i.uv_texcoord;
            			uvs_TexCoord26.xy = i.uv_texcoord.xy * _Noise_01_Scale;
            			float2 panner78 = ( global_speed178 * _Noise_01_Speed + uvs_TexCoord26.xy);
            			float2 uvs_TexCoord58 = i.uv_texcoord;
            			uvs_TexCoord58.xy = i.uv_texcoord.xy * _Noise_02_Scale;
            			float2 panner80 = ( global_speed178 * _Noise_02_Speed + uvs_TexCoord58.xy);
            			float clampResult169 = clamp( ( ( fresnelNode167 + clampResult136 ) * ( pow( ( saturate( ( ( tex2D( _TextureSample0, ( ( Distortion64 * _DistortionMask ) + panner218 ) ).r * _Mask_Power ) * _Mask_Multiply ) ) * ( tex2D( _Noise_01_Texture, ( Distortion64 + panner78 ) ).r * tex2D( _Noise_02_Texture, ( Distortion64 + panner80 ) ).r ) ) , _Opacity_Power ) * _Opacity_Boost ) ) , 0.0 , 1.0 );
            			float screenDepth261 = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, ase_screenPosNorm.xy ));
            			float distanceDepth261 = abs( ( screenDepth261 - LinearEyeDepth( ase_screenPosNorm.z ) ) / ( _Opacity_DepthFade_Intensity ) );
            			float clampResult262 = clamp( distanceDepth261 , 0.0 , 1.0 );
            			o.Alpha = ( i.vertexColor.a * saturate( ( ( clampResult169 - ( i.uv_texcoord.w + _Dissolve ) ) * clampResult262 ) ) );
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
