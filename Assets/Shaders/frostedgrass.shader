Shader "Custom/BlurURP"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _Blur("Blur Amount", Float) = 10
    }
        SubShader
        {
            Tags
            {
                "RenderType" = "Transparent"
                "Queue" = "Transparent"
                "RenderPipeline" = "UniversalPipeline"
            }

            Pass
            {
                Name "HorizontalBlur"
                Tags { "LightMode" = "UniversalForward" }

                HLSLPROGRAM

                #pragma vertex vert
                #pragma fragment frag
                #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

                struct Attributes
                {
                    float3 positionOS : POSITION;
                    float2 uv : TEXCOORD0;
                };

                struct Varyings
                {
                    float4 positionHCS : SV_POSITION;
                    float2 uv : TEXCOORD0;
                    float4 screenPos : TEXCOORD1;
                };

                float _Blur;

                Varyings vert(Attributes IN)
                {
                    Varyings OUT;
                    OUT.positionHCS = TransformObjectToHClip(IN.positionOS);
                    OUT.uv = IN.uv;
                    OUT.screenPos = ComputeScreenPos(OUT.positionHCS);
                    return OUT;
                }

                TEXTURE2D(_CameraOpaqueTexture);
                SAMPLER(sampler_CameraOpaqueTexture);

                half4 frag(Varyings IN) : SV_Target
                {
                    float2 uv = IN.screenPos.xy / IN.screenPos.w;
                    #if UNITY_UV_STARTS_AT_TOP
                        uv.y = 1.0 - uv.y;
                    #endif

                    float2 texelSize = float2(1.0 / _ScreenParams.x, 1.0 / _ScreenParams.y);

                    float blur = max(1, _Blur);
                    half4 col = half4(0, 0, 0, 0);
                    float weight_total = 0;

                    [loop]
                    for (float x = -blur; x <= blur; x += 1)
                    {
                        float distance_normalized = abs(x / blur);
                        float weight = exp(-0.5 * distance_normalized * distance_normalized * 5.0);
                        weight_total += weight;
                        float2 sampleUV = uv + float2(x * texelSize.x, 0);
                        col += SAMPLE_TEXTURE2D(_CameraOpaqueTexture, sampler_CameraOpaqueTexture, sampleUV) * weight;
                    }

                    col /= weight_total;
                    return col;
                }
                ENDHLSL
            }

            Pass
            {
                Name "VerticalBlur"
                Tags { "LightMode" = "UniversalForward" }

                HLSLPROGRAM

                #pragma vertex vert
                #pragma fragment frag
                #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

                struct Attributes
                {
                    float3 positionOS : POSITION;
                    float2 uv : TEXCOORD0;
                };

                struct Varyings
                {
                    float4 positionHCS : SV_POSITION;
                    float2 uv : TEXCOORD0;
                    float4 screenPos : TEXCOORD1;
                };

                float _Blur;

                Varyings vert(Attributes IN)
                {
                    Varyings OUT;
                    OUT.positionHCS = TransformObjectToHClip(IN.positionOS);
                    OUT.uv = IN.uv;
                    OUT.screenPos = ComputeScreenPos(OUT.positionHCS);
                    return OUT;
                }

                TEXTURE2D(_CameraOpaqueTexture);
                SAMPLER(sampler_CameraOpaqueTexture);

                half4 frag(Varyings IN) : SV_Target
                {
                    float2 uv = IN.screenPos.xy / IN.screenPos.w;
                    #if UNITY_UV_STARTS_AT_TOP
                        uv.y = 1.0 - uv.y;
                    #endif

                    float2 texelSize = float2(1.0 / _ScreenParams.x, 1.0 / _ScreenParams.y);

                    float blur = max(1, _Blur);
                    half4 col = half4(0, 0, 0, 0);
                    float weight_total = 0;

                    [loop]
                    for (float y = -blur; y <= blur; y += 1)
                    {
                        float distance_normalized = abs(y / blur);
                        float weight = exp(-0.5 * distance_normalized * distance_normalized * 5.0);
                        weight_total += weight;
                        float2 sampleUV = uv + float2(0, y * texelSize.y);
                        col += SAMPLE_TEXTURE2D(_CameraOpaqueTexture, sampler_CameraOpaqueTexture, sampleUV) * weight;
                    }

                    col /= weight_total;
                    return col;
                }
                ENDHLSL
            }
        }
}
