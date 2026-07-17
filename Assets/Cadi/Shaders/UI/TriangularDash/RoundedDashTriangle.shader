    Shader "UI/RoundedDashTriangle"
{
    Properties
    {
        _Color ("Color", Color) = (1, 0.8, 0.2, 1)
        _BackgroundTex ("Mask Texture", 2D) = "white" {}

        _Center ("Center", Vector) = (0.5, 0.5, 0, 0)
        _TriangleWidth ("Triangle Width", Range(0.05, 1.5)) = 0.8
        _Rotation ("Rotation (Radians)", Float) = 0

        _Thickness ("Border Thickness", Range(0.001, 0.1)) = 0.02
        _Softness ("Edge Softness", Range(0.0001, 0.05)) = 0.003

        _CornerRadius ("Triangle Corner Radius", Range(0.0, 0.2)) = 0.035

        _DashLength ("Dash Length", Range(0.001, 0.5)) = 0.08
        _GapLength ("Gap Length", Range(0.001, 0.5)) = 0.05
        _DashRoundness ("Dash Roundness", Range(0.0, 1.0)) = 1.0

        _Speed ("Speed", Float) = 0.5
        _OffsetOutward ("Offset Outward", Range(-0.1, 0.1)) = 0.0
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "RenderType"="Transparent"
            "IgnoreProjector"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            #define PI 3.14159265359
            #define TWO_PI 6.28318530718

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            sampler2D _BackgroundTex;
            float4 _Color;
            float4 _Center;

            float _TriangleWidth;
            float _Rotation;

            float _Thickness;
            float _Softness;
            float _CornerRadius;

            float _DashLength;
            float _GapLength;
            float _DashRoundness;

            float _Speed;
            float _OffsetOutward;

            struct CornerData
            {
                float2 trimPrev;
                float2 trimNext;
                float2 center;
                float radius;
                float arcLen;
                float startAngle;
                float deltaAngle;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color;
                return o;
            }

            float2 Rotate2D(float2 p, float angle)
            {
                float s = sin(angle);
                float c = cos(angle);

                return float2(
                    p.x * c - p.y * s,
                    p.x * s + p.y * c
                );
            }

            float ShortestAngleDelta(float from, float to)
            {
                float d = to - from;
                d = fmod(d + PI, TWO_PI);

                if (d < 0.0)
                {
                    d += TWO_PI;
                }

                return d - PI;
            }

            float2 ClosestPointOnSegment(float2 p, float2 a, float2 b, out float t)
            {
                float2 ab = b - a;
                float denom = dot(ab, ab);

                if (denom <= 1e-6)
                {
                    t = 0.0;
                    return a;
                }

                t = saturate(dot(p - a, ab) / denom);
                return a + ab * t;
            }

            float SdRect(float2 p, float2 halfSize)
            {
                float2 d = abs(p) - halfSize;
                return length(max(d, 0.0)) + min(max(d.x, d.y), 0.0);
            }

            float SdCapsuleHorizontal(float2 p, float halfSegmentLength, float radius)
            {
                float2 a = float2(-halfSegmentLength, 0.0);
                float2 b = float2(halfSegmentLength, 0.0);

                float2 pa = p - a;
                float2 ba = b - a;
                float h = saturate(dot(pa, ba) / dot(ba, ba));

                return length(pa - ba * h) - radius;
            }

            CornerData BuildCorner(float2 prev, float2 curr, float2 next, float radius)
            {
                CornerData c;

                float2 dirToPrev = normalize(prev - curr);
                float2 dirToNext = normalize(next - curr);

                float cosTheta = clamp(dot(dirToPrev, dirToNext), -1.0, 1.0);
                float theta = acos(cosTheta);
                float halfTheta = theta * 0.5;

                float trimDist = radius / max(tan(halfTheta), 1e-5);
                float centerDist = radius / max(sin(halfTheta), 1e-5);

                c.trimPrev = curr + dirToPrev * trimDist;
                c.trimNext = curr + dirToNext * trimDist;

                float2 bisector = normalize(dirToPrev + dirToNext);
                c.center = curr + bisector * centerDist;
                c.radius = radius;

                c.startAngle = atan2(c.trimPrev.y - c.center.y, c.trimPrev.x - c.center.x);
                float endAngle = atan2(c.trimNext.y - c.center.y, c.trimNext.x - c.center.x);
                c.deltaAngle = ShortestAngleDelta(c.startAngle, endAngle);
                c.arcLen = abs(c.deltaAngle) * radius;

                return c;
            }

            void EvalSegment(
                float2 p,
                float2 a,
                float2 b,
                float pathStart,
                inout float bestDist,
                inout float bestPathPos)
            {
                float t;
                float2 cp = ClosestPointOnSegment(p, a, b, t);
                float dist = length(p - cp);

                if (dist < bestDist)
                {
                    bestDist = dist;
                    bestPathPos = pathStart + t * length(b - a);
                }
            }

            void EvalArc(
                float2 p,
                CornerData c,
                float pathStart,
                inout float bestDist,
                inout float bestPathPos)
            {
                float angleP = atan2(p.y - c.center.y, p.x - c.center.x);
                float rel = 0.0;

                if (abs(c.deltaAngle) > 1e-6)
                {
                    rel = saturate(ShortestAngleDelta(c.startAngle, angleP) / c.deltaAngle);
                }

                float nearestAngle = c.startAngle + c.deltaAngle * rel;
                float2 nearestPoint = c.center + float2(cos(nearestAngle), sin(nearestAngle)) * c.radius;

                float dist = length(p - nearestPoint);

                if (dist < bestDist)
                {
                    bestDist = dist;
                    bestPathPos = pathStart + c.arcLen * rel;
                }
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 p = i.uv - _Center.xy;
                p = Rotate2D(p, _Rotation);

                // Base equilateral triangle
                float side = _TriangleWidth;
                float height = 0.8660254 * side;

                float2 v0 = float2(0.0, height * (2.0 / 3.0));
                float2 v1 = float2(-side * 0.5, -height * (1.0 / 3.0));
                float2 v2 = float2(side * 0.5, -height * (1.0 / 3.0));

                if (abs(_OffsetOutward) > 1e-6)
                {
                    float2 c = (v0 + v1 + v2) / 3.0;

                    v0 += normalize(v0 - c) * _OffsetOutward;
                    v1 += normalize(v1 - c) * _OffsetOutward;
                    v2 += normalize(v2 - c) * _OffsetOutward;
                }

                // Max usable corner radius for an equilateral triangle
                float maxCornerRadius = side * 0.28;
                float cornerRadius = clamp(_CornerRadius, 0.0, maxCornerRadius);

                CornerData c0 = BuildCorner(v2, v0, v1, cornerRadius);
                CornerData c1 = BuildCorner(v0, v1, v2, cornerRadius);
                CornerData c2 = BuildCorner(v1, v2, v0, cornerRadius);

                // Path order:
                // edge v0->v1, arc at v1, edge v1->v2, arc at v2, edge v2->v0, arc at v0
                float lenE0 = length(c1.trimPrev - c0.trimNext);
                float lenE1 = length(c2.trimPrev - c1.trimNext);
                float lenE2 = length(c0.trimPrev - c2.trimNext);

                float startE0 = 0.0;
                float startA1 = startE0 + lenE0;
                float startE1 = startA1 + c1.arcLen;
                float startA2 = startE1 + lenE1;
                float startE2 = startA2 + c2.arcLen;
                float startA0 = startE2 + lenE2;

                float perimeter = startA0 + c0.arcLen;

                float bestDist = 1e9;
                float pathPos = 0.0;

                EvalSegment(p, c0.trimNext, c1.trimPrev, startE0, bestDist, pathPos);
                EvalArc(p, c1, startA1, bestDist, pathPos);
                EvalSegment(p, c1.trimNext, c2.trimPrev, startE1, bestDist, pathPos);
                EvalArc(p, c2, startA2, bestDist, pathPos);
                EvalSegment(p, c2.trimNext, c0.trimPrev, startE2, bestDist, pathPos);
                EvalArc(p, c0, startA0, bestDist, pathPos);

                // Dash phase along whole rounded perimeter
                float period = max(_DashLength + _GapLength, 1e-5);
                float scroll = pathPos - (_Time.y * _Speed * period);

                // centered repeating coordinate: [-period/2, period/2]
                float localX = frac(scroll / period + 0.5) * period - period * 0.5;

                float halfThickness = _Thickness * 0.5;
                float halfDash = _DashLength * 0.5;

                // Flat-ended dash
                float rectSd = SdRect(
                    float2(localX, bestDist),
                    float2(halfDash, halfThickness)
                );

                // Round-ended dash
                float capsuleSd = SdCapsuleHorizontal(
                    float2(localX, bestDist),
                    halfDash,
                    halfThickness
                );

                float dashSd = lerp(rectSd, capsuleSd, _DashRoundness);
                float dashMask = 1.0 - smoothstep(0.0, _Softness, dashSd);

                fixed4 maskTex = tex2D(_BackgroundTex, i.uv);
                float alpha = dashMask * maskTex.a;

                return fixed4(_Color.rgb, _Color.a * alpha) * i.color;
            }
            ENDCG
        }
    }
}