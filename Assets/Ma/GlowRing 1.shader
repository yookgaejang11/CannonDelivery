Shader "Custom/GlowRing"
{
    Properties
    {
        _GlowColor ("Glow Color", Color) = (0, 0.8, 1, 1)
        _InnerRadius ("Inner Radius", Range(0, 1)) = 0.35
        _OuterRadius ("Outer Radius", Range(0, 1)) = 0.5
        _EdgeSoftness ("Edge Softness", Range(0.001, 0.1)) = 0.01
        _EmissionIntensity ("Emission Intensity", Range(0, 10)) = 3.0
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
        }

        Blend SrcAlpha One   // Additive 블렌딩 → 빛나는 효과
        ZWrite Off
        Cull Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv          : TEXCOORD0;
            };

            // Properties
            half4  _GlowColor;
            float  _InnerRadius;
            float  _OuterRadius;
            float  _EdgeSoftness;
            float  _EmissionIntensity;

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                // UV 중심 기준으로 거리 계산
                float2 centered = IN.uv - float2(0.5, 0.5);
                float dist = length(centered);

                // 링 마스크: 안쪽/바깥쪽 경계를 부드럽게 처리
                float outerMask = smoothstep(_OuterRadius, _OuterRadius - _EdgeSoftness, dist);
                float innerMask = smoothstep(_InnerRadius, _InnerRadius + _EdgeSoftness, dist);
                float ring = outerMask * innerMask;

                // 링 중심부로 갈수록 더 밝게 (그라디언트)
                float centerGlow = 1.0 - abs(dist - (_InnerRadius + _OuterRadius) * 0.5)
                                        / ((_OuterRadius - _InnerRadius) * 0.5);
                centerGlow = saturate(centerGlow);
                float finalRing = ring * (0.6 + 0.4 * centerGlow);

                // 최종 색상 (Emission 강도 적용)
                half4 color = _GlowColor * _EmissionIntensity;
                color.a = finalRing;

                return color;
            }
            ENDHLSL
        }
    }
}
