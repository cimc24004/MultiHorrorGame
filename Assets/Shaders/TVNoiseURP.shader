Shader "Custom/TVNoiseURP"
{
    Properties
    {
        // -------- マスク用 Sprite --------
        _SpriteTex        ("Sprite Texture", 2D) = "white" {}          // 形状マスクに使う Sprite
        _NoiseStrength    ("Noise Strength", Range(0,2)) = 1
        _ScanlineStrength ("Scanline Strength", Range(0,1)) = 0.4
        _Vignette         ("Vignette", Range(0,1)) = 0.3
        _Speed            ("Speed", Range(0.1,3)) = 1

        // -------- Emission --------
        [HDR]_EmissionTint("Emission Tint" , Color)        = (1,1,1,1) // ノイズ色
        _EmissionStrength ("Emission Strength", Range(0,10)) = 1

        // C# から入る SpriteRect
        [HideInInspector]_SpriteRect("SpriteRect", Vector) = (0,0,1,1)
    }

    SubShader
    {
        Tags { "RenderPipeline"="UniversalRenderPipeline" "RenderType"="Opaque" }

        Pass               // ─── 本描画 ───
        {
            Name  "StaticPass"
            Tags{ "LightMode"="SRPDefaultUnlit" }
            Blend SrcAlpha OneMinusSrcAlpha

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes { float4 positionOS : POSITION; float2 uv : TEXCOORD0; };
            struct Varyings   { float4 posHCS     : SV_POSITION; float2 uv : TEXCOORD0; };

            CBUFFER_START(UnityPerMaterial)
                float  _NoiseStrength;
                float  _ScanlineStrength;
                float  _Vignette;
                float  _Speed;
                float4 _SpriteRect;
                float4 _EmissionTint;
                float  _EmissionStrength;
            CBUFFER_END

            TEXTURE2D(_SpriteTex);   SAMPLER(sampler_SpriteTex);
            float4 _SpriteTex_ST;

            // 軽量ランダム
            float hash21(float2 p)
            {
                p = frac(p * float2(123.34,456.21));
                p += dot(p, p + 45.32);
                return frac(p.x * p.y);
            }

            Varyings vert (Attributes IN)
            {
                Varyings OUT;
                OUT.posHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv     = IN.uv;
                return OUT;
            }

            float4 frag (Varyings IN) : SV_Target
            {
                // --- ST 適用 → SpriteRect 切り出し
                float2 uvTile   = IN.uv * _SpriteTex_ST.xy + _SpriteTex_ST.zw;
                float2 spriteUV = uvTile * _SpriteRect.zw + _SpriteRect.xy;

                // --- ノイズ生成
                float t = _Time.y * _Speed;
                float n = hash21(uvTile * 640 + t * 60);                      // 基本ノイズ
                n += sin(uvTile.y * 1000) * 0.5 * _ScanlineStrength;          // 走査線
                n *= smoothstep(0.7, 0.0, length(uvTile - 0.5)                // ビネット
                                * (1.0 + _Vignette * 1.5));
                n = saturate((n - 0.5) * _NoiseStrength + 0.5);               // 強度調整

                // --- マスク (Sprite の RGB をそのまま使用。白=100%発光)
                float mask = SAMPLE_TEXTURE2D(_SpriteTex, sampler_SpriteTex, spriteUV).r;

                // --- 発光色 = ノイズ輝度 × マスク × Tint × Strength
                float3 emissive = n * mask * _EmissionTint.rgb * _EmissionStrength;

                return float4(emissive, 1);   // Unlit → emissive が最終色
            }
            ENDHLSL
        }
    }
    FallBack Off
}
