Shader "SH_Vefects_VFX_Dissolve"
{
    Properties
{
		_Noise_01_Texture("Noise_01_Texture", 2D) = "white" {}
		_Noise_02_Texture("Noise_02_Texture", 2D) = "white" {}
		_Mask_Texture("Mask_Texture", 2D) = "white" {}
		_MaskMove_Texture("MaskMove_Texture", 2D) = "white" {}
		_Color("Color", Color) = (1,1,1,0)
		_NoiseDistortion_Texture("NoiseDistortion_Texture", 2D) = "white" {}
		_Noise_01_Scale("Noise_01_Scale", Vector) = (0.8,0.8,0,0)
		_Noise_02_Scale("Noise_02_Scale", Vector) = (1,1,0,0)
		_CamOffSet("CamOffSet", Float) = 0
		_NoiseDistortion_Scale("NoiseDistortion_Scale", Vector) = (1,1,0,0)
		_Noise_01_Speed("Noise_01_Speed", Vector) = (0.5,0.5,0,0)
		_Noise_02_Speed("Noise_02_Speed", Vector) = (-0.2,0.4,0,0)
		_MaskMove_Scale("MaskMove_Scale", Vector) = (1,1,0,0)
		_Mask_Scale("Mask_Scale", Vector) = (1,1,0,0)
		_Mask_Offset("Mask_Offset", Vector) = (0,0,0,0)
		_NoiseDistortion_Speed("NoiseDistortion_Speed", Vector) = (0.2,0.25,0,0)
		_Mask_Multiply("Mask_Multiply", Float) = 1
		_MaskMove_Multiply("MaskMove_Multiply", Float) = 1
		_Noises_Multiply("Noises_Multiply", Float) = 1
		_Mask_Power("Mask_Power", Float) = 1
		_Noises_Power("Noises_Power", Float) = 1
		_MaskMove_Power("MaskMove_Power", Float) = 1
		_Distortion("Distortion", Float) = 1
		_DistortionMask("DistortionMask", Float) = 0
		_Opacity_Boost("Opacity_Boost", Float) = 5
		_Emissive("Emissive", Float) = 1
		_Global_Speed("Global_Speed", Float) = 1
		_Mask_Speed("Mask_Speed", Float) = 0
		_Dissolve("Dissolve", Float) = 0
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
            			float3 worldPos;
            			float4 vertexColor : COLOR;
            			float2 uv_texcoord;
            			float4 uv2_texcoord2;
            		};
            
            		uniform float _CamOffSet;
            		uniform float4 _Color;
            		uniform sampler2D _Mask_Texture;
            		uniform sampler2D _NoiseDistortion_Texture;
            		uniform float _Global_Speed;
            		uniform float2 _NoiseDistortion_Speed;
            		uniform float2 _NoiseDistortion_Scale;
            		uniform float _Distortion;
            		uniform float _DistortionMask;
            		uniform float _Mask_Speed;
            		uniform float2 _Mask_Scale;
            		uniform float2 _Mask_Offset;
            		uniform float _Mask_Power;
            		uniform float _Mask_Multiply;
            		uniform sampler2D _MaskMove_Texture;
            		uniform float2 _MaskMove_Scale;
            		uniform float _MaskMove_Power;
            		uniform float _MaskMove_Multiply;
            		uniform sampler2D _Noise_01_Texture;
            		uniform float2 _Noise_01_Speed;
            		uniform float2 _Noise_01_Scale;
            		uniform sampler2D _Noise_02_Texture;
            		uniform float2 _Noise_02_Speed;
            		uniform float2 _Noise_02_Scale;
            		uniform float _Noises_Power;
            		uniform float _Noises_Multiply;
            		uniform float _Emissive;
            		uniform float _Opacity_Boost;
            		uniform float _Dissolve;
            
            		void vertexDataFunc( inout appdata_full v, out Input o )
            		{
            			UNITY_INITIALIZE_OUTPUT( Input, o );
            			float3 ase_worldPos = mul( unity_ObjectToWorld, v.vertex );
            			v.vertex.xyz += ( ( ase_worldPos - _WorldSpaceCameraPos ) * ( ( _CamOffSet + v.texcoord3.xy.y ) * 0.01 ) );
            			v.vertex.w = 1;
            		}
            
            
            		void surf( Input i , inout ASESurfaceOutput o )
            		{
            			float global_speed200 = ( _Global_Speed * _Time.y );
            			float2 uv_TexCoord30 = i.uv_texcoord * _NoiseDistortion_Scale;
            			float2 panner79 = ( global_speed200 * _NoiseDistortion_Speed + uv_TexCoord30);
            			float Distortion64 = ( ( tex2D( _NoiseDistortion_Texture, panner79 ).r * 0.1 ) * _Distortion );
            			float2 appendResult264 = (float2(_Mask_Speed , 0.0));
            			float2 uv_TexCoord216 = i.uv_texcoord * _Mask_Scale + _Mask_Offset;
            			float2 panner266 = ( global_speed200 * appendResult264 + uv_TexCoord216);
            			float2 uv_TexCoord212 = i.uv_texcoord * _MaskMove_Scale;
            			float2 appendResult226 = (float2(i.uv2_texcoord2.z , i.uv2_texcoord2.w));
            			float clampResult224 = clamp( ( saturate( ( ( tex2D( _Mask_Texture, ( ( Distortion64 * _DistortionMask ) + panner266 ) ).r * _Mask_Power ) * _Mask_Multiply ) ) * saturate( ( ( tex2D( _MaskMove_Texture, ( uv_TexCoord212 + appendResult226 ) ).r * _MaskMove_Power ) * _MaskMove_Multiply ) ) ) , 0.0 , 1.0 );
            			float2 uv_TexCoord26 = i.uv_texcoord * _Noise_01_Scale;
            			float2 panner78 = ( global_speed200 * _Noise_01_Speed + uv_TexCoord26);
            			float2 uv_TexCoord58 = i.uv_texcoord * _Noise_02_Scale;
            			float2 panner80 = ( global_speed200 * _Noise_02_Speed + uv_TexCoord58);
            			float noise205 = saturate( ( ( ( tex2D( _Noise_01_Texture, ( Distortion64 + panner78 ) ).r * tex2D( _Noise_02_Texture, ( Distortion64 + panner80 ) ).r ) * _Noises_Power ) * _Noises_Multiply ) );
            			float temp_output_207_0 = ( clampResult224 * noise205 );
            			o.Emission = ( ( ( (_Color).rgb * (i.vertexColor).rgb ) * temp_output_207_0 ) * _Emissive );
            			float temp_output_237_0 = ( saturate( ( temp_output_207_0 * _Opacity_Boost ) ) - ( i.uv2_texcoord2.x + _Dissolve ) );
            			float clampResult238 = clamp( temp_output_237_0 , 0.0 , 1.0 );
            			o.Alpha = ( i.vertexColor.a * clampResult238 );
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
