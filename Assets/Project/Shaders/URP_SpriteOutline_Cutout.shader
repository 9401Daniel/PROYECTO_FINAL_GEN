Shader "Custom/URP_SpriteOutline_Cutout"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _OutlineColor ("Outline Color", Color) = (0,0,0,1)
        _OutlineWidth ("Outline Width (texels)", Range(0, 8)) = 1
        _AlphaCutoff ("Alpha Cutoff", Range(0,1)) = 0.5
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "TransparentCutout"
            "Queue" = "AlphaTest"
            "RenderPipeline" = "UniversalPipeline"
        }

        Cull Off
        ZWrite On

        Pass
        {
            Name "SpriteOutlineCutout"
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            float4 _MainTex_TexelSize;

            CBUFFER_START(UnityPerMaterial)
                float4 _Color;
                float4 _OutlineColor;
                float _OutlineWidth;
                float _AlphaCutoff;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                OUT.color = IN.color * _Color;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                half4 c = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv) * IN.color;

                // Si el pixel actual es transparente, revisamos si algún vecino es opaco
                if (c.a < _AlphaCutoff)
                {
                    float2 texel = _MainTex_TexelSize.xy * _OutlineWidth;
                    float a = 0;
                    a += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv + float2( texel.x, 0)).a;
                    a += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv + float2(-texel.x, 0)).a;
                    a += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv + float2(0,  texel.y)).a;
                    a += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv + float2(0, -texel.y)).a;
                    a += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv + float2( texel.x,  texel.y)).a;
                    a += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv + float2(-texel.x, -texel.y)).a;
                    a += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv + float2( texel.x, -texel.y)).a;
                    a += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv + float2(-texel.x,  texel.y)).a;

                    if (a > _AlphaCutoff)
                    {
                        c = _OutlineColor;
                        c.a = 1;
                    }
                }

                // Cutout: descarta lo que sigue siendo transparente (ni cuerpo ni contorno)
                clip(c.a - _AlphaCutoff);

                return c;
            }
            ENDHLSL
        }
    }
}
