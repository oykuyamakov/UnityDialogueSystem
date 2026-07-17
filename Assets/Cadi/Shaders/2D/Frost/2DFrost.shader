Shader "Custom/2DFrost"
{
       Properties
    {
        _FrostColor ("Frost Color", Color) = (0.85, 0.95, 1.0, 1.0)
        _Growth ("Growth", Range(0, 1)) = 0
        _EdgeSoftness ("Edge Softness", Range(0.001, 0.25)) = 0.08
        _NoiseScale ("Noise Scale", Range(1, 40)) = 12
        _NoiseStrength ("Noise Strength", Range(0, 0.5)) = 0.12
        _WarpStrength ("Warp Strength", Range(0, 0.25)) = 0.05
        _CrystalContrast ("Crystal Contrast", Range(0.5, 8)) = 3
        _InteriorTextureStrength ("Interior Texture Strength", Range(0, 2)) = 0.8
        _Opacity ("Opacity", Range(0, 1)) = 1
        _TintSource ("Tint Source", Range(0, 1)) = 0.35
        _Distortion ("Distortion", Range(0, 0.05)) = 0.008
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalPipeline"
            "Queue"="Transparent"
        }

        Pass
        {
            Name "FrostFullscreenPass"

            ZWrite Off
            ZTest Always
            Cull Off
            Blend One Zero

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

           #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            float4 _FrostColor;
            float _Growth;
            float _EdgeSoftness;
            float _NoiseScale;
            float _NoiseStrength;
            float _WarpStrength;
            float _CrystalContrast;
            float _InteriorTextureStrength;
            float _Opacity;
            float _TintSource;
            float _Distortion;

            float Hash21(float2 p)
            {
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 45.32);
                return frac(p.x * p.y);
            }

            float Noise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);

                float a = Hash21(i);
                float b = Hash21(i + float2(1, 0));
                float c = Hash21(i + float2(0, 1));
                float d = Hash21(i + float2(1, 1));

                float2 u = f * f * (3.0 - 2.0 * f);

                return lerp(lerp(a, b, u.x), lerp(c, d, u.x), u.y);
            }

            float FBM(float2 p)
            {
                float value = 0.0;
                float amplitude = 0.5;
                float frequency = 1.0;

                value += Noise(p * frequency) * amplitude;
                frequency *= 2.02;
                amplitude *= 0.5;

                value += Noise(p * frequency) * amplitude;
                frequency *= 2.03;
                amplitude *= 0.5;

                value += Noise(p * frequency) * amplitude;
                frequency *= 2.01;
                amplitude *= 0.5;

                value += Noise(p * frequency) * amplitude;
                frequency *= 2.04;
                amplitude *= 0.5;

                value += Noise(p * frequency) * amplitude;

                return value;
            }

            float CornerDistance(float2 uv)
            {
                float d0 = length(uv - float2(0.0, 0.0));
                float d1 = length(uv - float2(1.0, 0.0));
                float d2 = length(uv - float2(0.0, 1.0));
                float d3 = length(uv - float2(1.0, 1.0));

                return min(min(d0, d1), min(d2, d3));
            }

            float FrostMask(float2 uv)
            {
                float2 p = uv;

                float2 warpA = float2(
                    FBM(p * _NoiseScale + float2(13.1, 4.7)),
                    FBM(p * _NoiseScale + float2(2.3, 17.9))
                );

                p += (warpA - 0.5) * _WarpStrength;

                float cornerDist = CornerDistance(p);
                float normalizedDist = saturate(cornerDist / 0.70710678);
                float growthFront = 1.0 - _Growth;

                float n1 = FBM(p * _NoiseScale);
                float n2 = FBM(p * (_NoiseScale * 2.15) + 7.17);
                float n3 = FBM(float2(p.x * 24.0 - p.y * 18.0, p.y * 24.0 + p.x * 18.0));

                float edgeNoise = (n1 * 0.6 + n2 * 0.3 + n3 * 0.1) - 0.5;
                edgeNoise *= _NoiseStrength;

                float threshold = growthFront + edgeNoise;
                float mask = 1.0 - smoothstep(threshold - _EdgeSoftness, threshold + _EdgeSoftness, normalizedDist);

                return saturate(mask);
            }

            float FrostInteriorPattern(float2 uv)
            {
                float2 p = uv;

                float a = FBM(p * (_NoiseScale * 1.7));
                float b = FBM(float2(p.x * 18.0 + p.y * 7.0, p.y * 18.0 - p.x * 7.0));
                float c = FBM(float2(p.x * 30.0 - p.y * 22.0, p.y * 30.0 + p.x * 22.0));

                float crystal = a * 0.45 + b * 0.35 + c * 0.2;
                crystal = saturate(pow(crystal, _CrystalContrast));

                return crystal;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                float2 uv = input.texcoord;

                float mask = FrostMask(uv);
                float pattern = FrostInteriorPattern(uv);

                float2 distortedUV = uv + (pattern - 0.5) * _Distortion * mask;

                half4 source = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, distortedUV);

                float frostyBody = saturate(mask * lerp(1.0, pattern, _InteriorTextureStrength * 0.65));
                float edgeHighlight = saturate(mask * pattern * 1.35);

                half3 frostedSource = lerp(source.rgb, source.rgb * _FrostColor.rgb, _TintSource * mask);
                half3 frostColor = _FrostColor.rgb * (frostyBody + edgeHighlight * 0.35);

                half3 finalColor = lerp(source.rgb, frostedSource + frostColor * 0.35, mask * _Opacity);

                return half4(finalColor, 1.0);
            }
            ENDHLSL
        }
    }
}

//rework this for crispiness oyk