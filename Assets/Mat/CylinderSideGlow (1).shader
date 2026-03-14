Shader "Custom/CylinderSideGlow"
{
    Properties
    {
        _GlowColor ("Glow Color", Color) = (0, 0.8, 1, 1)
        _EmissionIntensity ("Emission Intensity", Range(0, 10)) = 3.0
        _FadeStart ("Fade Start (높이)", Range(0, 1)) = 0.0
        _FadeEnd ("Fade End (높이)", Range(0, 1)) = 1.0
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
        }

        Blend SrcAlpha One
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
                float3 normalOS   : NORMAL;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv          : TEXCOORD0;
                float3 normalWS    : TEXCOORD1;
            };

            half4  _GlowColor;
            float  _EmissionIntensity;
            float  _FadeStart;
            float  _FadeEnd;

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                // 노말 Y값이 크면 위/아래 뚜껑 → discard로 완전 제거
                float isCapFace = abs(IN.normalWS.y);
                if (isCapFace > 0.5)
                    discard;

                // 위로 올라갈수록 알파 감소
                float height = IN.uv.y;
                float alpha = 1.0 - smoothstep(_FadeStart, _FadeEnd, height);

                half4 color = _GlowColor * _EmissionIntensity;
                color.a = alpha;

                return color;
            }
            ENDHLSL
        }
    }
}
