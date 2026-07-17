Shader "UI/SharpDashTriangle"
{
    Properties
    {
        _Color ("Color", Color) = (1, 0.8, 0.2, 1)
        _BackgroundTex ("Mask Texture", 2D) = "white" {}

        _Center ("Center", Vector) = (0.5, 0.5, 0, 0)
        _TriangleWidth ("Triangle Width", Range(0.05, 1.5)) = 0.8
        _Rotation ("Rotation (Radians)", Float) = 0

        _Thickness ("Border Thickness", Range(0.001, 0.1)) = 0.02
        _Softness ("Edge Softness", Range(0.0001, 0.05)) = 0.004

        _DashLength ("Dash Length", Range(0.001, 0.5)) = 0.08
        _GapLength ("Gap Length", Range(0.001, 0.5)) = 0.05
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

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0;
                float4 color  : COLOR;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv     : TEXCOORD0;
                float4 color  : COLOR;
            };

            sampler2D _BackgroundTex;
            float4 _Color;
            float4 _Center;

            float _TriangleWidth;
            float _Rotation;

            float _Thickness;
            float _Softness;

            float _DashLength;
            float _GapLength;
            float _Speed;
            float _OffsetOutward;

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

            float EdgeDistanceAndPerimeter(
                float2 p,
                float2 a, float2 b, float edgeStartDistance,
                out float edgeDistance,
                out float perimeterDistance)
            {
                float t;
                float2 cp = ClosestPointOnSegment(p, a, b, t);

                float edgeLen = length(b - a);
                edgeDistance = length(p - cp);
                perimeterDistance = edgeStartDistance + t * edgeLen;

                return edgeLen;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 p = i.uv - _Center.xy;
                p = Rotate2D(p, _Rotation);

                // Equilateral triangle centered around origin
                float side = _TriangleWidth;
                float height = 0.8660254 * side; // sqrt(3)/2 * side

                float2 v0 = float2(0.0,  height * (2.0 / 3.0));   // top
                float2 v1 = float2(-side * 0.5, -height * (1.0 / 3.0)); // bottom-left
                float2 v2 = float2( side * 0.5, -height * (1.0 / 3.0)); // bottom-right

                // Optional outward expansion
                if (abs(_OffsetOutward) > 1e-6)
                {
                    float2 c = (v0 + v1 + v2) / 3.0;

                    float2 d0 = normalize(v0 - c);
                    float2 d1 = normalize(v1 - c);
                    float2 d2 = normalize(v2 - c);

                    v0 += d0 * _OffsetOutward;
                    v1 += d1 * _OffsetOutward;
                    v2 += d2 * _OffsetOutward;
                }

                float d0;
                float d1;
                float d2;

                float s0;
                float s1;
                float s2;

                float len0 = EdgeDistanceAndPerimeter(p, v0, v1, 0.0, d0, s0);
                float len1 = EdgeDistanceAndPerimeter(p, v1, v2, len0, d1, s1);
                float len2 = EdgeDistanceAndPerimeter(p, v2, v0, len0 + len1, d2, s2);

                float perimeter = len0 + len1 + len2;

                float minDist = d0;
                float perimeterPos = s0;

                if (d1 < minDist)
                {
                    minDist = d1;
                    perimeterPos = s1;
                }

                if (d2 < minDist)
                {
                    minDist = d2;
                    perimeterPos = s2;
                }

                // Border alpha
                float borderAlpha = 1.0 - smoothstep(_Thickness, _Thickness + _Softness, minDist);

                // Marching ants pattern along perimeter
                float dashPeriod = max(_DashLength + _GapLength, 1e-5);
                float phase = frac((perimeterPos / dashPeriod) - (_Time.y * _Speed));

                float dashMask = 1.0 - step(_DashLength / dashPeriod, phase);

                float alpha = borderAlpha * dashMask;

                fixed4 maskTex = tex2D(_BackgroundTex, i.uv);
                alpha *= maskTex.a;

                return fixed4(_Color.rgb, _Color.a * alpha) * i.color;
            }
            ENDCG
        }
    }
}