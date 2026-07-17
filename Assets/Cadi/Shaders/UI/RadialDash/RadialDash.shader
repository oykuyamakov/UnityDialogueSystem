Shader "UI/RadialDash"
{
    Properties
    {
        _Color ("Color", Color) = (1, 0.8, 0.2, 1)
        _BackgroundTex ("Mask Texture", 2D) = "white" {}
        _Speed ("Rotation Speed", Float) = 1
        _DashCount ("Dash Count", Float) = 24
        _DashFill ("Dash Fill", Range(0.05, 0.95)) = 0.45
        _ThicknessMin ("Thickness Min", Range(0,1)) = 0.32
        _ThicknessMax ("Thickness Max", Range(0,1)) = 0.42
        _Center ("Center", Vector) = (0.5, 0.5, 0, 0)
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
            float _Speed;
            float _DashCount;
            float _DashFill;
            float _ThicknessMin;
            float _ThicknessMax;
            float4 _Center;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 p = i.uv - _Center.xy;

                float angle = atan2(p.y, p.x);
                float angle01 = (angle + 3.14159265) / 6.2831853;

                float rotated = frac(angle01 + _Time.y * _Speed);

                float dashPhase = frac(rotated * _DashCount);
                float dashMask = step(dashPhase, _DashFill);

                float dist = length(p);
                float ringMask = step(_ThicknessMin, dist) * step(dist, _ThicknessMax);

                float alpha = dashMask * ringMask;

                fixed4 maskTex = tex2D(_BackgroundTex, i.uv);
                alpha *= maskTex.a;

                return fixed4(_Color.rgb, _Color.a * alpha) * i.color;
            }
            ENDCG
        }
    }
}