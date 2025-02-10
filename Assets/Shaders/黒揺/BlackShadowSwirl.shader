Shader "URP/BlackYuyo"
{
    Properties
    {
        // ざらざら感・歪み演出用のノイズテクスチャ
        _NoiseTex ("Noise Texture", 2D) = "white" {}
        // ノイズテクスチャのスクロール速度
        _ScrollSpeed ("Scroll Speed", Range(0,10)) = 1.0
        // ノイズの拡大率（大きくすると模様が大きく見える）
        _NoiseScale ("Noise Scale", Range(0.1,10)) = 1.0
        // 出力色。黒を基本にして、場合によって赤味などを混ぜると不気味さアップ
        _Tint ("Tint Color", Color) = (0,0,0,1)
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent" 
            "RenderType"="Transparent" 
            "UniversalRenderPipeline"="true"
        }
        LOD 100

        Pass
        {
            Name "BlackYuyoPass"
            Tags { "LightMode"="UniversalForward" }
            
            // 透過ブレンド設定。黒い影が重なるときに自然な見た目に
            Blend SrcAlpha OneMinusSrcAlpha
            Cull Off
            ZWrite Off

            HLSLPROGRAM
            // 必要なプラグマ
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            // TEXTURE2D/SAMPLER 宣言 (必ずペアで)
            TEXTURE2D(_NoiseTex);
            SAMPLER(sampler_NoiseTex);

            // プロパティの変数宣言
            float4 _NoiseTex_ST;
            float _ScrollSpeed;
            float _NoiseScale;
            float4 _Tint;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv     : TEXCOORD0;
            };

            v2f vert(appdata v)
            {
                v2f o;
                // オブジェクト空間からクリップ空間へ変換
                o.vertex = TransformObjectToHClip(v.vertex);
                // テクスチャの Tiling/Offset を反映 (TRANSFORM_TEX マクロ利用)
                o.uv = TRANSFORM_TEX(v.uv, _NoiseTex);
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                // 時間経過に応じたスクロールオフセットを計算
                float2 scrollOffset = _Time.y * _ScrollSpeed;
                // UV をノイズ用にスケール＆スクロール
                float2 noiseUV = i.uv * _NoiseScale + scrollOffset;
                
                // ノイズテクスチャをサンプリング（ここではグレースケールとして R チャンネルのみ利用）
                float noiseVal = SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, noiseUV).r;
                
                // ノイズ値に基づき、アルファ値を制御
                // ※noiseVal の値域や表現に合わせて調整してください
                float alpha = saturate(noiseVal);
                
                // 最終出力色は _Tint にアルファ値を掛け合わせたもの
                float4 col = _Tint;
                col.a *= alpha;
                return col;
            }
            ENDHLSL
        }
    }
}
