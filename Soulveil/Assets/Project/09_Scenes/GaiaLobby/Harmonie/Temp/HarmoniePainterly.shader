Shader "HarmonieFX/Painterly"
{
    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType" = "Opaque"
        }

        Pass
        {
            Name "Painterly"

            ZWrite Off
            ZTest Always
            Cull Off

            HLSLPROGRAM

            #pragma target 3.0
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            float _Intensity;
            float _BrushBlend;
            float _BlurRadius;
            float _ColorSteps;

            float _OutlineStrength;
            float _OutlineThreshold;

            float _Saturation;
            float _Contrast;
            float _Warmth;
            float _Glow;

            float _PaperGrain;

            float _MotionStrength;
            float _MotionSpeed;

            float Hash21(float2 p)
            {
                p = frac(
                    p *
                    float2(
                        123.34,
                        345.45
                    )
                );

                p += dot(
                    p,
                    p + 34.345
                );

                return frac(
                    p.x *
                    p.y
                );
            }

            float Noise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);

                float a =
                    Hash21(i);

                float b =
                    Hash21(
                        i +
                        float2(1.0, 0.0)
                    );

                float c =
                    Hash21(
                        i +
                        float2(0.0, 1.0)
                    );

                float d =
                    Hash21(
                        i +
                        float2(1.0, 1.0)
                    );

                float2 u =
                    f *
                    f *
                    (3.0 - 2.0 * f);

                return lerp(
                    lerp(a, b, u.x),
                    lerp(c, d, u.x),
                    u.y
                );
            }

            float3 SampleColor(float2 uv)
            {
                return SAMPLE_TEXTURE2D_X(
                    _BlitTexture,
                    sampler_LinearClamp,
                    uv
                ).rgb;
            }

            float Luma(float3 c)
            {
                return dot(
                    c,
                    float3(
                        0.2126,
                        0.7152,
                        0.0722
                    )
                );
            }

            float3 SaturateColor(
                float3 color,
                float amount)
            {
                float l =
                    Luma(color);

                return lerp(
                    l.xxx,
                    color,
                    amount
                );
            }

            float3 ContrastColor(
                float3 color,
                float amount)
            {
                return
                    (color - 0.5) *
                    amount +
                    0.5;
            }

            float3 WarmColor(
                float3 color,
                float amount)
            {
                color.r +=
                    amount * 0.10;

                color.g +=
                    amount * 0.035;

                color.b -=
                    amount * 0.08;

                return color;
            }

            float3 PosterizeSoft(
                float3 color,
                float steps)
            {
                steps =
                    max(
                        steps,
                        2.0
                    );

                float3 scaled =
                    saturate(color) *
                    steps;

                float3 low =
                    floor(scaled) /
                    steps;

                float3 high =
                    ceil(scaled) /
                    steps;

                float3 f =
                    smoothstep(
                        0.20,
                        0.80,
                        frac(scaled)
                    );

                return lerp(
                    low,
                    high,
                    f
                );
            }

            float3 BrushFilter(
                float2 uv,
                float2 texel,
                float radius,
                float noiseValue)
            {
                float angle =
                    noiseValue *
                    6.2831853;

                float2 direction =
                    float2(
                        cos(angle),
                        sin(angle)
                    );

                float2 tangent =
                    float2(
                        -direction.y,
                        direction.x
                    );

                float2 a =
                    direction *
                    texel *
                    radius;

                float2 b =
                    tangent *
                    texel *
                    radius *
                    0.55;

                float3 result =
                    SampleColor(uv) *
                    3.0;

                result +=
                    SampleColor(uv + a);

                result +=
                    SampleColor(uv - a);

                result +=
                    SampleColor(uv + b);

                result +=
                    SampleColor(uv - b);

                result +=
                    SampleColor(
                        uv + a * 2.0
                    ) * 0.5;

                result +=
                    SampleColor(
                        uv - a * 2.0
                    ) * 0.5;

                return
                    result /
                    8.0;
            }

            float DetectEdge(
                float2 uv,
                float2 texel)
            {
                float left =
                    Luma(
                        SampleColor(
                            uv -
                            float2(
                                texel.x,
                                0.0
                            )
                        )
                    );

                float right =
                    Luma(
                        SampleColor(
                            uv +
                            float2(
                                texel.x,
                                0.0
                            )
                        )
                    );

                float down =
                    Luma(
                        SampleColor(
                            uv -
                            float2(
                                0.0,
                                texel.y
                            )
                        )
                    );

                float up =
                    Luma(
                        SampleColor(
                            uv +
                            float2(
                                0.0,
                                texel.y
                            )
                        )
                    );

                float gx =
                    right - left;

                float gy =
                    up - down;

                return sqrt(
                    gx * gx +
                    gy * gy
                );
            }

            half4 Frag(
                Varyings input)
                : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                float2 uv =
                    input.texcoord;

                // La textura que Blitter nos entrega tiene
                // el mismo tamaño que la cámara.
                float2 texel =
                    1.0 /
                    max(
                        _ScreenParams.xy,
                        float2(1.0, 1.0)
                    );

                float time =
                    _Time.y *
                    _MotionSpeed;

                float brushNoise =
                    Noise(
                        uv * 6.0 +
                        float2(
                            time * 0.035,
                            -time * 0.025
                        )
                    );

                float2 flow =
                    float2(
                        Noise(
                            uv * 11.0 +
                            time * 0.08
                        ),
                        Noise(
                            uv * 13.0 -
                            time * 0.06
                        )
                    ) -
                    0.5;

                float2 animatedUV =
                    uv +
                    flow *
                    texel *
                    5.0 *
                    _MotionStrength;

                float3 original =
                    SampleColor(uv);

                float3 moving =
                    SampleColor(
                        animatedUV
                    );

                float3 painted =
                    BrushFilter(
                        animatedUV,
                        texel,
                        _BlurRadius,
                        brushNoise
                    );

                painted =
                    lerp(
                        moving,
                        painted,
                        _BrushBlend
                    );

                painted =
                    PosterizeSoft(
                        painted,
                        _ColorSteps
                    );

                painted =
                    SaturateColor(
                        painted,
                        _Saturation
                    );

                painted =
                    ContrastColor(
                        painted,
                        _Contrast
                    );

                painted =
                    WarmColor(
                        painted,
                        _Warmth
                    );

                float brightness =
                    Luma(painted);

                float glowMask =
                    smoothstep(
                        0.50,
                        1.0,
                        brightness
                    );

                painted +=
                    painted *
                    glowMask *
                    _Glow *
                    0.40;

                float edge =
                    DetectEdge(
                        animatedUV,
                        texel
                    );

                float edgeMask =
                    smoothstep(
                        _OutlineThreshold,
                        max(
                            _OutlineThreshold +
                            0.0001,
                            _OutlineThreshold *
                            2.5
                        ),
                        edge
                    );

                painted *=
                    1.0 -
                    edgeMask *
                    _OutlineStrength *
                    0.45;

                float paper =
                    Noise(
                        uv *
                        _ScreenParams.xy /
                        6.0 +
                        float2(
                            time * 0.015,
                            0.0
                        )
                    );

                painted *=
                    1.0 +
                    (paper - 0.5) *
                    _PaperGrain;

                float3 result =
                    lerp(
                        original,
                        painted,
                        _Intensity
                    );

                return half4(
                    max(result, 0.0),
                    1.0
                );
            }

            ENDHLSL
        }
    }

    Fallback Off
}
