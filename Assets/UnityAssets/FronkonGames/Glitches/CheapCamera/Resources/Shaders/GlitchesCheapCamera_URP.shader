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
Shader "Hidden/Fronkon Games/Glitches/Cheap Camera URP"
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
      Name "Fronkon Games Glitches Cheap Camera Pass"

      HLSLPROGRAM
      #pragma target 3.5
      #pragma vertex GlitchesVert
      #pragma fragment GlitchesFrag
      #pragma fragmentoption ARB_precision_hint_fastest
      #pragma exclude_renderers d3d9 d3d11_9x ps3 flash
      #pragma multi_compile _ _USE_DRAW_PROCEDURAL
      #pragma multi_compile _ _BLUR_4 _BLUR_3
      #pragma multi_compile _ _CHROMATIC_ABERRATION
      #pragma multi_compile _ GLITCHES_DEMO

      #include "Glitches.hlsl"

      float3 _LensVignette;
      float4 _LensChromaticAberration;
      float3 _LensRadialDistortion;
      float2 _LensTangentialDistortion;
      float _LensZoom;

      float2 _SensorResolution;
      float3 _SensorPixelNoise;
      float _SensorExposure;
      float _SensorGain;
      float _SensorDynamicRange;

      float3 _ProcessorFunctions;
      float3 _ProcessorWhiteBalance;

      float3 R2R4R6;

      float2 LensDistortion(float2 uv)
      {
        float a1 = 2.0 * uv.x * uv.y;
        float a2 = R2R4R6.x + 2.0 * uv.x * uv.x;
        float a3 = R2R4R6.x + 2.0 * uv.y * uv.y;

        float cdist = 1.0 + dot(_LensRadialDistortion, R2R4R6);

        float2 tangentalDist = float2(dot(_LensTangentialDistortion, float2(a1, a2)),
                                      dot(_LensTangentialDistortion, float2(a3, a1)));

        float2 distortAMT = (float2)cdist + tangentalDist;

        return uv * distortAMT * _LensZoom;
      }

      inline float2 RestoreUV(float2 uv)
      {
        return (uv * 0.5) + (float2)0.5;
      }

      float3 CheapLens(float2 uv)
      {
        float3 pixel;
        #if _CHROMATIC_ABERRATION
        float abr = dot(_LensChromaticAberration.xyz, R2R4R6) * lerp(1.0, Rand(uv), _LensChromaticAberration.w);
        pixel = float3(SAMPLE_MAIN(RestoreUV(LensDistortion(uv * (1.0 - abr * 3.0)))).r,
                       SAMPLE_MAIN(RestoreUV(LensDistortion(uv * (1.0 - abr * 2.0)))).g,
                       SAMPLE_MAIN(RestoreUV(LensDistortion(uv * (1.0 - abr * 1.0)))).b).rgb;
        #else
        pixel = SAMPLE_MAIN(RestoreUV(LensDistortion(uv))).rgb;
        #endif

        return pixel * (1.0 + dot(_LensVignette, R2R4R6));
      }      

      float3 CheapSensor(float2 uv)
      {
        float2 pixelated = uv;
        pixelated.x -= mod(uv.x, 2.0 / _SensorResolution.x);
        pixelated.y -= mod(uv.y, 2.0 / _SensorResolution.y);

        float3 sample = CheapLens(pixelated);
        sample *= _SensorExposure;
        sample += dot(_SensorPixelNoise.xy, float2(Rand(uv) - 0.5, Rand(uv + float2(0.0, _Time.y * _SensorPixelNoise.z)) - 0.5));
        sample *= _SensorGain;

        sample = clamp(sample, 0.0, 1.0);
        sample -= mod(sample, 1.0 / pow(2.0, 16.0 * _SensorDynamicRange));

        return sample;
      }

      half4 GlitchesFrag(const VertexOutput input) : SV_Target
      {
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
        const float2 uv = UnityStereoTransformScreenSpaceTex(input.uv).xy;

        const half4 color = SAMPLE_MAIN(uv);
        half4 pixel = color;

        float2 pixelSize = 2.0 / _SensorResolution * _ProcessorFunctions.z;
        float2 centerUV = (uv - (float2)0.5) * 2.0;

        float r2 = 1.0 - dot(centerUV, centerUV);
        float r4 = 1.0 - (r2 * r2);
        float r6 = 1.0 - (r4 * r2);
        R2R4R6 = float3(r2, r4, r6);

        pixel.rgb = CheapSensor(centerUV);

        half3 blurred = (half3)0.0;
        #if _BLUR_4
        blurred += CheapSensor(centerUV + pixelSize * float2(1.0, 1.0));
        blurred += CheapSensor(centerUV + pixelSize * float2(-1.0, 1.0));
        blurred += CheapSensor(centerUV + pixelSize * float2(-1.0, -1.0));
        blurred += CheapSensor(centerUV + pixelSize * float2(1.0, -1.0));
        blurred /= 4.0;
        #elif _BLUR_3
        blurred += CheapSensor(centerUV + pixelSize * float2(1.0, 0.0));
        blurred += CheapSensor(centerUV + pixelSize * float2(1.0, 1.0));
        blurred += CheapSensor(centerUV + pixelSize * float2(-1.0, 1.0));
        blurred /= 3.0;
        #else
        blurred += CheapSensor(centerUV + pixelSize * float2(1.0, -1.0));
        blurred += CheapSensor(centerUV + pixelSize * float2(1.0, 1.0));
        blurred /= 2.0;
        #endif

        half3 edges = pixel.rgb - blurred;
        pixel.rgb = lerp(pixel.rgb, pixel.rgb + edges, _ProcessorFunctions.x);

        float sharpenOrBlur = (1.0 - length(abs(edges))) * _ProcessorFunctions.y;
        pixel.rgb = lerp(pixel.rgb, blurred, sharpenOrBlur);

        pixel.rgb = ColorAdjust(pixel.rgb, _Contrast, _Brightness, _Hue, _Gamma, _Saturation);
        pixel.rgb /= _ProcessorWhiteBalance;

#if GLITCHES_DEMO
        pixel.rgb = PixelDemo(color.rgb, pixel.rgb, uv);
#endif
        return lerp(color, pixel, _Intensity);
      }

      ENDHLSL
    }
  }
  
  FallBack "Diffuse"
}
