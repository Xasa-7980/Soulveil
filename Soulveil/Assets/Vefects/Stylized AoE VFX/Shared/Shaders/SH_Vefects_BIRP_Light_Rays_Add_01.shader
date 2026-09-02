Shader "Vefects/SH_Vefects_BIRP_Light_Rays_Add_01"
{
    Properties
{
		[Space(33)][Header(Main Texture)][Space(13)]_MainTexture("Main Texture", 2D) = "white" {}
		_Ray01UVScale("Ray 01 UV Scale", Float) = 0.5
		_Ray01UVSpeed("Ray 01 UV Speed", Vector) = (-0.12,0,0,0)
		_Ray02UVScale("Ray 02 UV Scale", Float) = 0.25
		_Ray02UVSpeed("Ray 02 UV Speed", Vector) = (0.07,0,0,0)
		_Emission("Emission", Float) = 1
		[Space(33)][Header(AR)][Space(13)]_Cull("Cull", Float) = 2
		_Src("Src", Float) = 1
		_Dst("Dst", Float) = 1
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
            			float4 vertexColor : COLOR;
            			float4 uv_texcoord;
            			float4 uv2_texcoord2;
            			float4 screenPos;
            		};
            
            		uniform float _ZWrite;
            		uniform float _ZTest;
            		uniform float _Cull;
            		uniform float _Src;
            		uniform float _Dst;
            		uniform sampler2D _MainTexture;
            		uniform float2 _Ray01UVSpeed;
            		uniform float _Ray01UVScale;
            		uniform float2 _Ray02UVSpeed;
            		uniform float _Ray02UVScale;
            		UNITY_DECLARE_DEPTH_TEXTURE( _CameraDepthTexture );
            		uniform float4 _CameraDepthTexture_TexelSize;
            		uniform float _Emission;
            
            
            		void surf( Input i , inout ASESurfaceOutput o )
            		{
            			float2 panner54 = ( 1.0 * _Time.y * _Ray01UVSpeed + ( i.uv_texcoord.xy * _Ray01UVScale ));
            			float random64 = i.uv2_texcoord2.x;
            			float2 panner58 = ( 1.0 * _Time.y * _Ray02UVSpeed + ( i.uv_texcoord.xy * _Ray02UVScale ));
            			float4 tex2DNode31 = tex2D( _MainTexture, i.uv_texcoord.xy );
            			float temp_output_70_0 = saturate( tex2DNode31.b );
            			float saferPower44 = abs( ( temp_output_70_0 * 0.5 ) );
            			float saferPower32 = abs( tex2DNode31.g );
            			float4 ase_screenPos = float4( i.screenPos.xyz , i.screenPos.w + 0.00000000001 );
            			float4 ase_screenPosNorm = ase_screenPos / ase_screenPos.w;
            			ase_screenPosNorm.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_screenPosNorm.z : ase_screenPosNorm.z * 0.5 + 0.5;
            			float screenDepth26 = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, ase_screenPosNorm.xy ));
            			float distanceDepth26 = saturate( ( screenDepth26 - LinearEyeDepth( ase_screenPosNorm.z ) ) / ( i.uv_texcoord.w ) );
            			float temp_output_18_0 = saturate( ( saturate( ( saturate( ( saturate( ( saturate( ( saturate( ( tex2D( _MainTexture, ( panner54 + random64 ) ).r * tex2D( _MainTexture, ( panner58 + random64 ) ).r ) ) * temp_output_70_0 ) ) + pow( saferPower44 , 2.0 ) ) ) * pow( saferPower32 , 2.0 ) ) ) * i.vertexColor.a ) ) * distanceDepth26 ) );
            			o.Emission = ( ( i.vertexColor * temp_output_18_0 ) * ( _Emission * i.uv_texcoord.z ) ).rgb;
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
