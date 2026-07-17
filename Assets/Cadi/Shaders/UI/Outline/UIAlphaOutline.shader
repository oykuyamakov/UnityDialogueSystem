Shader "UI/AlphaOutline"
{
    Properties
    {
        [PerRendererData]_MainTex ("Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        _OutlineColor ("Outline Color", Color) = (1,1,1,1)
        _OutlineWidth ("Outline Width (px)", Range(0,30)) = 2
        _AlphaThreshold ("Alpha Threshold", Range(0,1)) = 0.1

        // 0 = Outward (draw on transparent area), 1 = Inward (draw on inside edge)
        [KeywordEnum(Outward, Inward)] _OutlineMode ("Outline Mode", Float) = 0

        // For inward mode: 0 = fully replace edge color, 1 = fully blend outline over base
        _OutlineBlend ("Inward Blend", Range(0,1)) = 1
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Stencil
        {
            Ref 1
            Comp Always
            Pass Keep
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            Name "UIOutline"
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            // for _OutlineMode keyword enum
            #pragma multi_compile _OUTLINEMODE_OUTWARD _OUTLINEMODE_INWARD

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float2 uv : TEXCOORD0;
                float4 worldPos : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            fixed4 _Color;

            fixed4 _OutlineColor;
            float _OutlineWidth;
            float _AlphaThreshold;
            float _OutlineBlend;

            float4 _ClipRect;

            v2f vert(appdata_t v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                o.worldPos = v.vertex;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.texcoord;
                o.color = v.color * _Color;
                return o;
            }

            fixed4 SampleTex(float2 uv)
            {
                return tex2D(_MainTex, uv);
            }

            // returns max neighbor alpha (8-tap)
            float NeighborMaxAlpha(float2 uv, float2 px)
            {
                float maxA = 0.0;
                maxA = max(maxA, SampleTex(uv + float2(px.x, 0)).a);
                maxA = max(maxA, SampleTex(uv + float2(-px.x, 0)).a);
                maxA = max(maxA, SampleTex(uv + float2(0, px.y)).a);
                maxA = max(maxA, SampleTex(uv + float2(0, -px.y)).a);

                maxA = max(maxA, SampleTex(uv + float2(px.x, px.y)).a);
                maxA = max(maxA, SampleTex(uv + float2(-px.x, px.y)).a);
                maxA = max(maxA, SampleTex(uv + float2(px.x, -px.y)).a);
                maxA = max(maxA, SampleTex(uv + float2(-px.x, -px.y)).a);
                return maxA;
            }

            // returns min neighbor alpha (8-tap) -> good for "is any neighbor outside?"
            float NeighborMinAlpha(float2 uv, float2 px)
            {
                float minA = 1.0;
                minA = min(minA, SampleTex(uv + float2(px.x, 0)).a);
                minA = min(minA, SampleTex(uv + float2(-px.x, 0)).a);
                minA = min(minA, SampleTex(uv + float2(0, px.y)).a);
                minA = min(minA, SampleTex(uv + float2(0, -px.y)).a);

                minA = min(minA, SampleTex(uv + float2(px.x, px.y)).a);
                minA = min(minA, SampleTex(uv + float2(-px.x, px.y)).a);
                minA = min(minA, SampleTex(uv + float2(px.x, -px.y)).a);
                minA = min(minA, SampleTex(uv + float2(-px.x, -px.y)).a);
                return minA;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 baseCol = SampleTex(i.uv) * i.color;

                // RectMask2D/Mask support
                float clipA = UnityGet2DClipping(i.worldPos.xy, _ClipRect);
                baseCol.a *= clipA;

                float a = baseCol.a;

                float2 px = _MainTex_TexelSize.xy * _OutlineWidth;

                #if defined(_OUTLINEMODE_OUTWARD)
                // OUTWARD: draw on transparent pixels near filled pixels
                if (a >= _AlphaThreshold)
                    return baseCol;

                float maxA = NeighborMaxAlpha(i.uv, px);

                if (maxA >= _AlphaThreshold)
                {
                    fixed4 ocol = _OutlineColor;
                    ocol.a *= clipA;
                    return ocol;
                }

                return 0;

                #else
                // INWARD: draw on filled pixels near transparent pixels
                // distance (in UV) to the nearest edge of the quad
                float d = min(min(i.uv.x, 1 - i.uv.x), min(i.uv.y, 1 - i.uv.y));


                float2 custSize = float2 (1.0/2048,1.0/1092); // default 
                float2 pCust = custSize * _OutlineWidth;
                
                // convert pixel width to UV-ish threshold (use max texel size)
                float w = max(pCust.x, pCust.y);

                if (d < w)
                {
                    fixed4 ocol = _OutlineColor;
                    ocol.a *= baseCol.a;
                    return lerp(baseCol, ocol, _OutlineBlend);
                }

                return baseCol;
                #endif
            }
            ENDCG
        }
    }
}