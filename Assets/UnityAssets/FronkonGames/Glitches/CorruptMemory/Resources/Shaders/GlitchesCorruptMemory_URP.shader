////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Martin Bustos @FronkonGames <fronkongames@gmail.com>. All rights reserved.
//
// THIS FILE CAN NOT BE HOSTED IN PUBLIC REPOSITORIES.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
// COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
// OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
Shader "Hidden/Fronkon Games/Glitches/Corrupt Memory URP"
{
  Properties
  {
    _MainTex("Main Texture", 2D) = "white" {}
  }

  SubShader
  {
    Tags
    {
      "RenderType" = "Opaque"
      "RenderPipeline" = "UniversalPipeline"
    }
    LOD 100
    ZTest Always ZWrite Off Cull Off

    Pass
    {
      Name "Fronkon Games Glitches Corrupt Memory Pass"

      HLSLPROGRAM
      #pragma target 3.5
      #pragma vertex GlitchesVert
      #pragma fragment GlitchesFrag
      #pragma fragmentoption ARB_precision_hint_fastest
      #pragma exclude_renderers d3d9 d3d11_9x ps3 flash
      #pragma multi_compile _ _USE_DRAW_PROCEDURAL
      #pragma multi_compile _ DEBUG_VIEW

      #include "Glitches.hlsl"

      float _Complexity;
      float _Density;
      float _Speed;
      float2 _Scrolling;
      float2 _Zoom;
      float3 _Aberration;
      float _AberrationThreshold;
      float _Pixelation;
      float _PixelationThreshold;
      float _Offset;
      float _OffsetScale;
      float _OffsetThreshold;

      inline float4 Hash42(float2 p)
      {
        p += 1e4;
        half4 p4 = frac(p.xyxy * float4(0.1031, 0.1030, 0.0973, 0.1099));
        p4 += dot(p4, p4.wzxy + 33.33);
        
        return frac((p4.xxyz + p4.yzzw) * p4.zywx);
      }

      half4 GlitchesFrag(const VertexOutput input) : SV_Target
      {
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
        float2 uv = UnityStereoTransformScreenSpaceTex(input.uv).xy;

        const half4 color = SAMPLE_MAIN(uv);
        half4 pixel = color;

        float t = _Time.y * _Speed;

        float2 uv2 = uv;
        uv2.x *= _ScreenParams.x / _ScreenParams.y;
        uv2 *= _Zoom;
        uv2 += _Scrolling + (_Scrolling * t);

        float s = 1.0;
        float f = 0.95;

        t += 1e2;

        half4 corruption = 0.0;

        UNITY_LOOP
        for (float i = 1.0; i <= _Complexity; ++i)
        {
          float4 h = Hash42(floor(uv2 + i));
          float2 p = frac(uv2 + i) - (floor((t * s) / 0.2) * 0.2);

          uv2 += s * p * h.z;
          s = -s * 1.2;

          UNITY_BRANCH
          if (h.w < _Density)
            corruption = h;
        }

        UNITY_BRANCH
        if (_Pixelation > 0.0)
        {
          float pixelation = lerp(256.0, 8.0, _Pixelation);

          UNITY_BRANCH
          if (Luminance(corruption) > _PixelationThreshold)
            uv = floor(uv * pixelation) / pixelation;
        }

        float2 offset = float2(cos(corruption.x * 6.283), sin(corruption.y * 6.283)) * corruption.z;
        float fade = sin(frac(t) * PI);
        float2 scale = _OffsetScale / _ScreenParams.xy;
        float threshold = step(_OffsetThreshold, corruption.z);

        uv += _Offset * offset * threshold * fade * scale;

        pixel.r = SAMPLE_MAIN(uv + float2(0.0, step(_AberrationThreshold, corruption.r) * _Aberration.x) * _MainTex_TexelSize.xy).r;
        pixel.g = SAMPLE_MAIN(uv + float2(step(_AberrationThreshold, corruption.g) * _Aberration.y, 0.0) * _MainTex_TexelSize.xy).g;
        pixel.b = SAMPLE_MAIN(uv + float2(0.0, -step(_AberrationThreshold, corruption.b) * _Aberration.z) * _MainTex_TexelSize.xy).b;

        pixel.rgb = ColorAdjust(pixel.rgb, _Contrast, _Brightness, _Hue, _Gamma, _Saturation);

        #if DEBUG_VIEW
        pixel.rgb = Luminance(corruption);
        #endif
        
        return lerp(color, pixel, _Intensity);
      }

      ENDHLSL
    }
  }
  
  FallBack "Diffuse"
}
