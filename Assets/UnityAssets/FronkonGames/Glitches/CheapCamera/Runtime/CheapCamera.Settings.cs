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
using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace FronkonGames.Glitches.CheapCamera
{
  ///------------------------------------------------------------------------------------------------------------------
  /// <summary> Settings. </summary>
  /// <remarks> Only available for Universal Render Pipeline. </remarks>
  ///------------------------------------------------------------------------------------------------------------------
  public sealed partial class CheapCamera
  {
    /// <summary> Settings. </summary>
    [Serializable]
    public sealed class Settings
    {
      public Settings() => ResetDefaultValues();

      /////////////////////////////////////////////////////////////////////////////////////////////////////////////////
      #region Common settings.

      /// <summary> Controls the intensity of the effect [0, 1]. Default 1. </summary>
      /// <remarks> An effect with Intensity equal to 0 will not be executed. </remarks>
      public float intensity = 1.0f;

      #endregion
      /////////////////////////////////////////////////////////////////////////////////////////////////////////////////

      /////////////////////////////////////////////////////////////////////////////////////////////////////////////////
      #region Cheap Camera settings.

      /// <summary> Blur effect quality types. </summary>
      public enum BlurQuality
      {
        /// <summary> Low quality, 2 samples. </summary>
        Low,

        /// <summary> Medium quality, 3 samples. </summary>
        Medium,

        /// <summary> High quality, 4 samples. </summary>
        High,
      }

      /// <summary>
      /// Vignette effect. Negative values for darker and positive values for brighter.
      /// x: intensity in the center of the screen.
      /// y: intensity in the middle area of the screen.
      /// z: intensity in the outer area of the screen.
      /// </summary>
      public Vector3 lensVignette = DefaultLensVignette;

      /// <summary>
      /// Chromatic aberration.
      /// x: intensity in the center of the screen.
      /// y: intensity in the middle area of the screen.
      /// z: intensity in the outer area of the screen.
      /// w: noise.
      /// </summary>
      public Vector4 lensChromaticAberration = DefaultLensChromaticAberration;

      /// <summary>
      /// Radial distortion.
      /// x: intensity in the center of the screen.
      /// y: intensity in the middle area of the screen.
      /// z: intensity in the outer area of the screen.
      /// </summary>
      public Vector3 lensRadialDistortion = DefaultLensRadialDistortion;

      /// <summary> Diagonal distortions. </summary>
      public Vector2 lensTangentialDistortion = Vector2.zero;

      /// <summary> Zoom [0, 1]. Default 0.9. </summary>
      public float lensZoom = 0.9f;

      /// <summary> Resolution, the lower the resolution, the more pixelated [0, 1]. Default 0.3. </summary>
      public float sensorResolution = 0.3f;

      /// <summary> Quality of the blur effect. Default High. </summary>
      public BlurQuality sensorBlurQuality = BlurQuality.High;

      /// <summary>
      /// Noise.
      /// x: intensity of static noise.
      /// y: dynamic noise intensity.
      /// z: dynamic noise velocity.
      /// </summary>
      public Vector3 sensorPixelNoise = DefaultSensorPixelNoise;

      /// <summary> Exposure (prior to applying the noise) [0, 10]. Default 1. </summary>
      public float sensorExposure = 1.0f;

      /// <summary> Gain (after applying the noise) [0, 10]. Default 1. </summary>
      public float sensorGain = 1.0f;

      /// <summary> Sensitivity of the sensor to color, the lower the sensitivity, the less colors [0, 1]. Default 0.3. </summary>
      public float sensorDynamicRange = 0.3f;

      /// <summary>
      /// Processor functions.
      /// x: edges.
      /// y: sharpen intensity.
      /// z: pixel size.
      /// </summary>
      public Vector3 processorFunctions = DefaultProcessorFunctions;

      /// <summary> White balance per color channel. </summary>
      public Vector3 processorWhiteBalance = Vector3.one;

      public static Vector3 DefaultLensVignette = new(0.0f, 0.0f, -0.2f);
      public static Vector3 DefaultLensRadialDistortion = new(-0.2f, -0.05f, 0.0f);
      public static Vector4 DefaultLensChromaticAberration = new(0.02f, 0.0f, 0.0f, 0.1f);
      public static Vector3 DefaultSensorPixelNoise = new(0.1f, 0.05f, 0.5f);
      public static Vector3 DefaultProcessorFunctions = new(5.0f, 0.8f, 2.0f);

      #endregion
      /////////////////////////////////////////////////////////////////////////////////////////////////////////////////

      /////////////////////////////////////////////////////////////////////////////////////////////////////////////////
      #region Color settings.

      /// <summary> Brightness [-1, 1]. Default 0. </summary>
      public float brightness = 0.0f;

      /// <summary> Contrast [0, 10]. Default 1. </summary>
      public float contrast = 1.0f;

      /// <summary> Gamma [0.1, 10]. Default 1. </summary>
      public float gamma = 1.0f;

      /// <summary> The color wheel [0, 1]. Default 0. </summary>
      public float hue = 0.0f;

      /// <summary> Intensity of a colors [0, 2]. Default 1. </summary>
      public float saturation = 1.0f;

      #endregion
      /////////////////////////////////////////////////////////////////////////////////////////////////////////////////

      /////////////////////////////////////////////////////////////////////////////////////////////////////////////////
      #region Advanced settings.

      /// <summary> Does it affect the Scene View? </summary>
      public bool affectSceneView = false;

      /// <summary> Filter mode. Default Bilinear. </summary>
      public FilterMode filterMode = FilterMode.Bilinear;

      /// <summary> Render pass injection. Default BeforeRenderingPostProcessing. </summary>
      public RenderPassEvent whenToInsert = RenderPassEvent.BeforeRenderingPostProcessing;

      /// <summary> Set to true to ignore delta time scaling. </summary>
      public bool ignoreDeltaTimeScale = false;

      /// <summary> Enable render pass profiling. </summary>
      public bool enableProfiling = false;

      #endregion
      /////////////////////////////////////////////////////////////////////////////////////////////////////////////////

      /// <summary> Reset to default values. </summary>
      public void ResetDefaultValues()
      {
        intensity = 1.0f;

        lensVignette = DefaultLensVignette;
        lensChromaticAberration = DefaultLensChromaticAberration;
        lensRadialDistortion = DefaultLensRadialDistortion;
        lensTangentialDistortion = Vector2.zero;
        lensZoom = 0.9f;
        sensorResolution = 0.3f;
        sensorBlurQuality = BlurQuality.High;
        sensorPixelNoise = DefaultSensorPixelNoise;
        sensorExposure = 1.0f;
        sensorGain = 1.0f;
        sensorDynamicRange = 0.3f;
        processorFunctions = DefaultProcessorFunctions;
        processorWhiteBalance = Vector3.one;

        brightness = 0.0f;
        contrast = 1.0f;
        gamma = 1.0f;
        hue = 0.0f;
        saturation = 1.0f;

        affectSceneView = false;
        filterMode = FilterMode.Bilinear;
        whenToInsert = RenderPassEvent.BeforeRenderingPostProcessing;
        ignoreDeltaTimeScale = false;
        enableProfiling = false;
      }
    }
  }
}
