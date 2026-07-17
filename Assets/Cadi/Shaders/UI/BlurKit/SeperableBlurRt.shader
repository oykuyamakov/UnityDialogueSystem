Shader "UI/SeparableBlurRT"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _BlurSize ("Blur Size", Vector) = (2, 0, 0, 0)
    }

    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" }

        Cull Off
        ZWrite Off
        ZTest Always
        Blend One Zero

        Pass
        {
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            float4 _MainTex_TexelSize;
            float4 _BlurSize;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                output.positionHCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                float2 dir = _BlurSize.xy * _MainTex_TexelSize.xy;

                half4 color = 0;
                color += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv + dir * -4.0) * 0.05;
                color += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv + dir * -3.0) * 0.09;
                color += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv + dir * -2.0) * 0.12;
                color += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv + dir * -1.0) * 0.15;
                color += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv)             * 0.18;
                color += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv + dir *  1.0) * 0.15;
                color += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv + dir *  2.0) * 0.12;
                color += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv + dir *  3.0) * 0.09;
                color += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv + dir *  4.0) * 0.05;

                return color;
            }
            ENDHLSL
        }
    }
}