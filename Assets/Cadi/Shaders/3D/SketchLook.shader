Shader "Custom/SketchLook"
{
    Properties
    {
        _BaseColor ("Base Color", Color) = (1,1,1,1)
        _BaseAlpha ("Base Alpha", Range(0,1)) = 0.0

        _FresnelColor ("Edge Color", Color) = (0,0,0,1)
        _FresnelAlpha ("Edge Alpha", Range(0,1)) = 0.8

        // --- Silhouette (view-dependent rim) ---
        _FresnelWidth     ("Silhouette Width",     Range(0.001,1)) = 0.15
        _FresnelSharpness ("Silhouette Sharpness", Range(1,16))    = 4.0

        // --- Interior hard edges (screen-space normal gradient) ---
        _EdgeSensitivity  ("Interior Edge Sensitivity", Range(0,50)) = 8.0

        // --- Sketch breakup ---
        _FresnelDensity   ("Sketch Density",     Range(1,100000)) = 20.0
        _FresnelRandomness("Sketch Randomness",  Range(0,1))   = 0.5
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalRenderPipeline"
            "Queue"          = "Transparent"
            "RenderType"     = "Transparent"
        }

        Pass
        {
            Name "Forward"
            Tags { "LightMode" = "UniversalForward" }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Back

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 normalWS   : TEXCOORD0;   // raw (not normalized) — needed for ddx/ddy
                float3 positionWS : TEXCOORD1;
                float3 positionOS : TEXCOORD2;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
                float  _BaseAlpha;

                float4 _FresnelColor;
                float  _FresnelAlpha;

                float  _FresnelWidth;
                float  _FresnelSharpness;

                float  _EdgeSensitivity;

                float  _FresnelDensity;
                float  _FresnelRandomness;
            CBUFFER_END

            // ---- Sketch noise helpers ----

            float hash21(float2 p)
            {
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 34.45);
                return frac(p.x * p.y);
            }

            float noise2D(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                float a = hash21(i);
                float b = hash21(i + float2(1, 0));
                float c = hash21(i + float2(0, 1));
                float d = hash21(i + float2(1, 1));
                float2 u = f * f * (3.0 - 2.0 * f);
                return lerp(lerp(a, b, u.x), lerp(c, d, u.x), u.y);
            }

            float fbm(float2 p)
            {
                float v = 0, a = 0.5;
                v += noise2D(p) * a; p *= 2.02; a *= 0.5;
                v += noise2D(p) * a; p *= 2.03; a *= 0.5;
                v += noise2D(p) * a;
                return v;
            }

            // ---- Vertex ----

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);

                VertexPositionInputs posInputs    = GetVertexPositionInputs(IN.positionOS.xyz);
                VertexNormalInputs   normalInputs = GetVertexNormalInputs(IN.normalOS);

                OUT.positionCS = posInputs.positionCS;
                OUT.normalWS   = normalInputs.normalWS;  // NOT normalized here on purpose
                OUT.positionWS = posInputs.positionWS;
                OUT.positionOS = IN.positionOS.xyz;

                return OUT;
            }

            // ---- Fragment ----

            half4 frag(Varyings IN) : SV_Target
            {
                float3 n = normalize(IN.normalWS);
                float3 v = normalize(GetWorldSpaceViewDir(IN.positionWS));

                // --------------------------------------------------
                // 1. SILHOUETTE edges  (view-dependent rim)
                // --------------------------------------------------
                float rim     = 1.0 - saturate(dot(n, v));
                float rimEdge = pow(smoothstep(1.0 - _FresnelWidth, 1.0, rim), _FresnelSharpness);

                // --------------------------------------------------
                // 2a. INTERIOR hard edges — screen-space normal gradient
                //     Fires exactly at the boundary pixel between two
                //     triangles with different normals (hard edges, cube
                //     edges, any topology crease). Always 1-2px wide but
                //     precise.
                // --------------------------------------------------
                float3 dndx = ddx(IN.normalWS);
                float3 dndy = ddy(IN.normalWS);
                float  normalGrad = sqrt(dot(dndx, dndx) + dot(dndy, dndy));
                float  hardCrease = saturate(normalGrad * _EdgeSensitivity);

                // --------------------------------------------------
                // 2b. INTERIOR soft edges — geometric vs vertex normal
                //     cross(ddx(pos), ddy(pos)) gives the true face normal
                //     at this pixel. Where it disagrees with the interpolated
                //     vertex normal, we're near a crease on a smooth mesh.
                //     This spreads naturally over several pixels, giving
                //     thicker edges on curved / smooth-shaded geometry.
                // --------------------------------------------------
                float3 dpdx    = ddx(IN.positionWS);
                float3 dpdy    = ddy(IN.positionWS);
                float3 faceN   = normalize(cross(dpdx, dpdy));
                float  faceDot = saturate(abs(dot(n, faceN)));
                float  softCrease = saturate((1.0 - faceDot) * _EdgeSensitivity * 0.5);

                // Combined interior edge (best of both detectors)
                float creaseEdge = max(hardCrease, softCrease);

                // --------------------------------------------------
                // 3. Sketch / hatching breakup — applied to SILHOUETTE
                //    only. Interior edges skip breakup so they stay
                //    visible even when thin (1-2 px on a hard mesh).
                // --------------------------------------------------
                float2 pA = IN.positionOS.xy * _FresnelDensity;
                float2 pB = IN.positionOS.zy * (_FresnelDensity * 0.8);

                float organic = fbm(pA * 0.35) * 0.5 + fbm(pB * 0.35 + 13.1) * 0.5;
                organic = saturate(organic);

                float lineA = abs(frac(pA.x + pA.y * 0.35 + organic * 1.2) - 0.5);
                float lineB = abs(frac(pA.x * 0.6 - pA.y  + organic * 0.9) - 0.5);

                float widthA = lerp(0.18, 0.05, organic);
                float widthB = lerp(0.16, 0.04, 1.0 - organic);

                float sketchA = 1.0 - smoothstep(widthA, widthA + 0.03, lineA);
                float sketchB = 1.0 - smoothstep(widthB, widthB + 0.03, lineB);

                float sketch  = max(sketchA, sketchB);
                float breakup = lerp(1.0, saturate(sketch * (0.6 + organic)), _FresnelRandomness);

                // Silhouette: organic/sketchy via breakup
                // Interior:   clean and always visible (no breakup)
                float rimMask    = rimEdge * breakup;
                float edgeMask   = saturate(max(rimMask, creaseEdge));

                // --------------------------------------------------
                // 5. Output
                // --------------------------------------------------
                float3 finalColor = lerp(_BaseColor.rgb, _FresnelColor.rgb, edgeMask);
                float  finalAlpha = _BaseAlpha + edgeMask * _FresnelAlpha;

                return half4(finalColor, saturate(finalAlpha));
            }
            ENDHLSL
        }
    }
}