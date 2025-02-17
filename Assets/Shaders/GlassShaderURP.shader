Shader "Custom/GlassShaderURP"
{
    Properties
    {
        _Color("Tint Color", Color) = (1,1,1,0.5)
        _RefractionStrength("Refraction Strength", Range(0, 0.2)) = 0.05
        _NormalMap("Normal Map", 2D) = "bump" {}
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        Pass
        {
            Name "UniversalForward"
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog

            // URP用のコアライブラリのインクルード
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            // Fog.hlsl はお使いのバージョンに存在しないため削除
            //#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Fog.hlsl"

            // Fog 関連のマクロが定義されていない場合は、ダミー定義を追加
            #ifndef FOG_COORDS
                #define FOG_COORDS(coord)
            #endif

            #ifndef TRANSFER_FOG
                #define TRANSFER_FOG(v, pos)
            #endif

            #ifndef APPLY_FOG
                #define APPLY_FOG(fogCoord, col)
            #endif

            // 頂点入力構造体
            struct Attributes
            {
                float4 vertex   : POSITION;
                float2 uv       : TEXCOORD0;
                float3 normal   : NORMAL;
            };

            // 頂点→フラグメント間のデータ
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv         : TEXCOORD0;
                float3 worldPos   : TEXCOORD1;
                float3 worldNormal: TEXCOORD2;
                FOG_COORDS(3)   // ダミー定義の場合は何も展開されません
            };

            // マテリアルプロパティ
            CBUFFER_START(UnityPerMaterial)
                float4 _Color;
                float _RefractionStrength;
            CBUFFER_END

            // プロパティテクスチャ
            TEXTURE2D(_NormalMap);
            SAMPLER(sampler_NormalMap);

            // カメラの不透明テクスチャ（Opaque Texture を有効にしてください）
            TEXTURE2D(_CameraOpaqueTexture);
            SAMPLER(sampler_CameraOpaqueTexture);

            // 頂点シェーダー
            Varyings vert (Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.vertex);
                output.uv = input.uv;
                output.worldPos = TransformObjectToWorld(input.vertex);
                output.worldNormal = TransformObjectToWorldNormal(input.normal);
                TRANSFER_FOG(output, output.positionCS);
                return output;
            }

            // フラグメントシェーダー
            half4 frag(Varyings input) : SV_Target
            {
                // カメラからの視線計算（ワールド空間）
                float3 viewDir = normalize(_WorldSpaceCameraPos - input.worldPos);

                // 法線マップによる細かい凹凸の取得
                float3 normalTex = UnpackNormal(SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, input.uv));
                float3 normal = normalize(input.worldNormal + normalTex * 0.5);

                // フレネル効果
                float fresnel = pow(1.0 - saturate(dot(viewDir, normal)), 3.0);

                // 画面空間の UV 座標計算
                float4 clipPos = input.positionCS;
                float2 screenUV = clipPos.xy / clipPos.w;
                screenUV = screenUV * 0.5 + 0.5;

                // 法線に基づく UV オフセット（屈折効果）
                float2 offsetUV = screenUV + normal.xy * _RefractionStrength;

                // 背景（不透明テクスチャ）のサンプリング
                half4 refracted = SAMPLE_TEXTURE2D(_CameraOpaqueTexture, sampler_CameraOpaqueTexture, offsetUV);

                // Tint Color との合成
                half4 color = refracted * _Color;
                color.a = _Color.a;

                APPLY_FOG(input.fogCoord, color);
                return color;
            }
            ENDHLSL
        }
    }
    FallBack "Universal Forward"
}
