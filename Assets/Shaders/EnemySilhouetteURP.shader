Shader "Custom/EnemySilhouetteURP"
{
    Properties
    {
        _Color("Color", Color) = (1, 0, 0, 1) // シルエットの色
    }
        SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalRenderPipeline"
            "RenderType" = "Transparent"
            "Queue" = "Transparent+100" // 描画順序を調整
        }

        Pass
        {
            Name "EnemySilhouettePass"
            Blend SrcAlpha OneMinusSrcAlpha // 透過設定
            Cull Front // 裏面を描画
            ZWrite Off // 深度書き込みを無効化
            ZTest Always // 常に描画（壁を無視）

            HLSLPROGRAM
            #pragma prefer_hlslcc gles
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _ALPHATEST_ON
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID // インスタンシング用のID
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID // インスタンシング用のIDを保持
            };

            UNITY_INSTANCING_BUFFER_START(PerObjectData)
                // 必要に応じてカスタムのインスタンシングデータを追加できます
            UNITY_INSTANCING_BUFFER_END(PerObjectData)

                // 頂点シェーダー
                Varyings vert(Attributes IN)
                {
                    Varyings OUT;
                    UNITY_SETUP_INSTANCE_ID(IN);

                    // 修正箇所: positionOS.xyz を使用
                    float3 positionWS = TransformObjectToWorld(IN.positionOS.xyz);
                    OUT.positionHCS = TransformWorldToHClip(positionWS);

                    UNITY_TRANSFER_INSTANCE_ID(IN, OUT); // インスタンシングIDを転送
                    return OUT;
                }

                half4 _Color;

                // フラグメントシェーダー
                half4 frag(Varyings IN) : SV_Target
                {
                    UNITY_SETUP_INSTANCE_ID(IN);
                    return _Color;
                }
                ENDHLSL
            }
    }
}
