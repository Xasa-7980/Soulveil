Shader "Vefects/SH_VFX_Bomb_BIRP"
{
    Properties
{
		_MainColor("Main Color", Color) = (0.3921569,0.3921569,0.3921569,1)
		_SpecularColor("Specular Color", Color) = (0.3921569,0.3921569,0.3921569,1)
		_Shininess("Shininess", Range( 0.01 , 1)) = 0.1
		[HDR]_Color2("Color 2", Color) = (0.6415094,0.1361695,0.1361695,0)
		[HDR]_Color0("Color 0", Color) = (0.2980392,0.4666663,1,0)
		[HDR]_Color1("Color 1", Color) = (0.2122642,0.8048544,1,0)
		_Shadow("Shadow", Color) = (0.1766198,0.1845427,0.254717,0)
		[Space(33)][Header(AR)][Space(13)]_Cull("Cull", Float) = 2
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}
    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalPipeline"
            "RenderType"="Opaque"
            "Queue"="Geometry"
        }
        Cull Back
        ZWrite On

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
            		struct Input
            		{
            			float2 uv_texcoord;
            			float4 vertexColor : COLOR;
            			float3 worldPos;
            			float3 worldNormal;
            			INTERNAL_DATA
            		};
            
            		uniform float _Cull;
            		uniform float4 _Color1;
            		uniform float4 _Color0;
            		uniform float4 _Color2;
            		uniform float4 _Shadow;
            		uniform float4 _SpecularColor;
            		uniform float _Shininess;
            		uniform float4 _MainColor;
            
            
            		void surf( Input i , inout ASESurfaceOutput o )
            		{
            			o.Normal = float3(0,0,1);
            			float4 lerpResult45 = lerp( _Color1 , _Color0 , abs( sin( ( i.uv_texcoord.x * 31.45 ) ) ));
            			float4 lerpResult22 = lerp( lerpResult45 , _Color2 , step( i.uv_texcoord.y , 0.5 ));
            			float4 temp_output_43_0_g5 = _SpecularColor;
            			float3 ase_worldPos = i.worldPos;
            			float3 ase_viewVectorWS = ( _WorldSpaceCameraPos.xyz - ase_worldPos );
            			float3 ase_viewDirWS = normalize( ase_viewVectorWS );
            			#if defined(LIGHTMAP_ON) && UNITY_VERSION < 560 //aseld
            			float3 ase_worldlightDir = 0;
            			#else //aseld
            			float3 ase_worldlightDir = normalize( UnityWorldSpaceLightDir( ase_worldPos ) );
            			#endif //aseld
            			float3 normalizeResult4_g6 = normalize( ( ase_viewDirWS + ase_worldlightDir ) );
            			float3 normalizeResult64_g5 = normalize( (WorldNormalVector( i , float3(0,0,1) )) );
            			float dotResult19_g5 = dot( normalizeResult4_g6 , normalizeResult64_g5 );
            			#if defined(LIGHTMAP_ON) && ( UNITY_VERSION < 560 || ( defined(LIGHTMAP_SHADOW_MIXING) && !defined(SHADOWS_SHADOWMASK) && defined(SHADOWS_SCREEN) ) )//aselc
            			float4 ase_lightColor = 0;
            			#else //aselc
            			float4 ase_lightColor = _LightColor0;
            			#endif //aselc
            			float4 temp_output_40_0_g5 = ( ase_lightColor * 1 );
            			float dotResult14_g5 = dot( normalizeResult64_g5 , ase_worldlightDir );
            			float4 temp_output_42_0_g5 = _MainColor;
            			float4 temp_cast_4 = (0.05).xxxx;
            			float4 lerpResult26 = lerp( ( lerpResult22 * _Shadow ) , ( float4( (i.vertexColor).rgb , 0.0 ) * lerpResult22 ) , saturate( ( 1.0 - step( ( ( float4( (temp_output_43_0_g5).rgb , 0.0 ) * (temp_output_43_0_g5).a * pow( max( dotResult19_g5 , 0.0 ) , ( _Shininess * 128.0 ) ) * temp_output_40_0_g5 ) + ( ( ( temp_output_40_0_g5 * max( dotResult14_g5 , 0.0 ) ) + float4( float3(0,0,0) , 0.0 ) ) * float4( (temp_output_42_0_g5).rgb , 0.0 ) ) ) , temp_cast_4 ) ) ));
            			o.Emission = ( lerpResult26 * 2.0 ).rgb;
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
