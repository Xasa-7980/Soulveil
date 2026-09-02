Shader "Vefects/SH_VFX_Fresnel_Bomb_BIRP"
{
    Properties
{
		[Space(33)][Header(Dissolve)][Space(13)]_DissolveTexture("Dissolve Texture", 2D) = "white" {}
		_DissolveUVScale("Dissolve UV Scale", Vector) = (1,1,0,0)
		_DissolveUVSpeed("Dissolve UV Speed", Vector) = (0,0.3,0,0)
		_DissolveInvert("Dissolve Invert", Range( 0 , 1)) = 0
		_DissolveEro("Dissolve Ero", Float) = 0
		_ColorIn("Color In", Color) = (1,1,1,0)
		_ColorExt("Color Ext", Color) = (0,0.6901961,1,0)
		_FrBias1("Fr Bias", Float) = 0
		_FrScale1("Fr Scale", Float) = 1
		_FrColorScale("Fr Color Scale", Float) = 1
		_FrColorBias("Fr Color Bias", Float) = 0
		_FrPower1("Fr Power", Float) = 1
		_FrColorPower("Fr Color Power", Float) = 1
		_CorePower("Core Power", Float) = 1
		_CoreIntensity("Core Intensity", Float) = 0.6
		_GlowIntensity("Glow Intensity", Float) = 1
		_AddDiss("Add Diss", Float) = -0.690476
		_CoreColorDifferent("Core Color Different", Float) = 0.5
		_Brightness("Brightness", Float) = 1
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

            #undef TRANSFORM_TEX
            		#define TRANSFORM_TEX(tex,name) float4(tex.xy * name##_ST.xy + name##_ST.zw, tex.z, tex.w)
            		struct Input
            		{
            			float4 vertexColor : COLOR;
            			float3 worldPos;
            			float3 worldNormal;
            			float4 uv_texcoord;
            		};
            
            		uniform float _Cull;
            		uniform float _Src;
            		uniform float _Dst;
            		uniform float _ZWrite;
            		uniform float _ZTest;
            		uniform float4 _ColorIn;
            		uniform float4 _ColorExt;
            		uniform float _FrColorBias;
            		uniform float _FrColorScale;
            		uniform float _FrColorPower;
            		uniform float _CoreColorDifferent;
            		uniform float _Brightness;
            		uniform sampler2D _DissolveTexture;
            		uniform float2 _DissolveUVSpeed;
            		uniform float2 _DissolveUVScale;
            		uniform float _DissolveInvert;
            		uniform float _DissolveEro;
            		uniform float _CorePower;
            		uniform float _CoreIntensity;
            		uniform float _GlowIntensity;
            		uniform float _FrBias1;
            		uniform float _FrScale1;
            		uniform float _FrPower1;
            		uniform float _AddDiss;
            
            
            		void surf( Input i , inout ASESurfaceOutput o )
            		{
            			float3 ase_worldPos = i.worldPos;
            			float3 ase_viewVectorWS = ( _WorldSpaceCameraPos.xyz - ase_worldPos );
            			float3 ase_viewDirWS = normalize( ase_viewVectorWS );
            			float3 ase_worldNormal = i.worldNormal;
            			float fresnelNdotV39 = dot( ase_worldNormal, ase_viewDirWS );
            			float fresnelNode39 = ( _FrColorBias + _FrColorScale * pow( 1.0 - fresnelNdotV39, _FrColorPower ) );
            			float temp_output_43_0 = saturate( fresnelNode39 );
            			float4 lerpResult38 = lerp( _ColorIn , _ColorExt , temp_output_43_0);
            			float4 lerpResult57 = lerp( i.vertexColor , lerpResult38 , _CoreColorDifferent);
            			o.Emission = ( lerpResult57 * _Brightness ).rgb;
            			float2 panner19 = ( 1.0 * _Time.y * _DissolveUVSpeed + ( i.uv_texcoord.xy * _DissolveUVScale ));
            			float4 tex2DNode16 = tex2D( _DissolveTexture, panner19 );
            			float4 lerpResult23 = lerp( tex2DNode16 , ( 1.0 - tex2DNode16 ) , _DissolveInvert);
            			float4 temp_output_30_0 = ( saturate( lerpResult23 ) + ( i.uv_texcoord.z + _DissolveEro ) );
            			float4 temp_cast_1 = (_CorePower).xxxx;
            			float4 temp_output_52_0 = saturate( ( ( pow( temp_output_30_0 , temp_cast_1 ) * _CoreIntensity ) + ( temp_output_30_0 * _GlowIntensity ) ) );
            			float fresnelNdotV62 = dot( ase_worldNormal, ase_viewDirWS );
            			float fresnelNode62 = ( _FrBias1 + _FrScale1 * pow( 1.0 - fresnelNdotV62, _FrPower1 ) );
            			float temp_output_78_0 = saturate( fresnelNode62 );
            			float4 temp_cast_2 = (temp_output_78_0).xxxx;
            			float4 temp_cast_3 = (temp_output_43_0).xxxx;
            			o.Alpha = saturate( ( ( i.vertexColor.a * saturate( step( temp_output_52_0 , temp_cast_2 ) ) ) + step( ( ( i.vertexColor.a + temp_output_52_0 ) + _AddDiss ) , temp_cast_3 ) ) ).r;
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
