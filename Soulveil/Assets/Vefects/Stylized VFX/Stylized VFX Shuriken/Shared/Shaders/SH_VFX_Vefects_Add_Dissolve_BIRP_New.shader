Shader "Vefects/SH_VFX_Vefects_Add_Dissolve_BIRP_New"
{
    Properties
{
		_Texture( "Texture", 2D ) = "white" {}
		[Space(33)][Header(Noise)][Space(13)] _Noise( "Noise", 2D ) = "white" {}
		_Emissive_Intensity( "Emissive Intensity", Float ) = 1
		_NoisePower( "Noise Power", Float ) = 1
		_NoiseScale( "Noise Scale", Vector ) = ( 1, 1, 0, 0 )
		_NoiseSpeed( "Noise Speed", Vector ) = ( 0, 0.2, 0, 0 )
		[Space(33)][Header(AR)][Space(13)] _Cull( "Cull", Float ) = 2
		_Src( "Src", Float ) = 1
		_Dst( "Dst", Float ) = 1
		_ZWrite( "ZWrite", Float ) = 0
		_ZTest( "ZTest", Float ) = 2
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
            			float2 uv_texcoord;
            			float4 uv2_texcoord2;
            		};
            
            		uniform float _Dst;
            		uniform float _ZWrite;
            		uniform float _Src;
            		uniform float _ZTest;
            		uniform float _Cull;
            		uniform sampler2D _Noise;
            		uniform float2 _NoiseScale;
            		uniform float2 _NoiseSpeed;
            		uniform float _NoisePower;
            		uniform sampler2D _Texture;
            		uniform float4 _Texture_ST;
            		uniform float _Emissive_Intensity;
            
            
            		void surf( Input i , inout ASESurfaceOutput o )
            		{
            			float2 uv_TexCoord109 = i.uv_texcoord * _NoiseScale + ( _Time.y * _NoiseSpeed );
            			float2 uv_Texture = i.uv_texcoord * _Texture_ST.xy + _Texture_ST.zw;
            			o.Emission = ( (i.vertexColor).rgb * ( i.vertexColor.a * saturate( ( saturate( ( saturate( pow( tex2D( _Noise, uv_TexCoord109 ).r , _NoisePower ) ) * tex2D( _Texture, uv_Texture ).r ) ) +  (0.0 + ( ( 1.0 - i.uv2_texcoord2.z ) - 0.0 ) * ( 1.0 - 0.0 ) / ( 1.0 - 0.0 ) ) ) ) ) * i.uv2_texcoord2.w * _Emissive_Intensity );
            			o.Alpha = 1;
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
