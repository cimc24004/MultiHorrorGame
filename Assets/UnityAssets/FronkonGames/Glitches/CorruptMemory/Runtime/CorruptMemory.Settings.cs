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

namespace FronkonGames.Glitches.CorruptMemory
{
  ///------------------------------------------------------------------------------------------------------------------
  /// <summary> Settings. </summary>
  /// <remarks> Only available for Universal Render Pipeline. </remarks>
  ///------------------------------------------------------------------------------------------------------------------
  public sealed partial class CorruptMemory
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
      #region Corrupt Memory settings.

      /// <summary> Complexity of the pattern used [0, 15]. Default 5. </summary>
      public float complexity = 5.0f;

      /// <summary> Density of pattern used [0, 1]. Default 0.2. </summary>
      public float density = 0.2f;

      /// <summary> Rate of change of the pattern used [0, 25]. Default 10. </summary>
      public float speed = 10.0f;

      /// <summary> Displacement of the pattern used. </summary>
      public Vector2 scrolling = Vector2.zero;

      /// <summary> Zoom of the used pattern. </summary>
      public Vector2 zoom = DefaultZoom;

      /// <summary> Intensity of the chromatic aberration in each color channel. </summary>
      public Vector3 aberration = Vector3.one * 10.0f;

      /// <summary> Filter to apply chromatic aberration. The more quantity, the less patterns will be used [0, 1]. Default 0.9. </summary>
      public float aberrationThreshold = 0.9f;

      /// <summary> Intensity of pixelation effect [0, 1]. Default 0.25. </summary>
      public float pixelation = 0.25f;

      /// <summary> Filter to apply pixelation. The more quantity, the less patterns will be used [0, 1]. Default 0.9. </summary>
      public float pixelationThreshold = 0.9f;

      /// <summary> Intensity of image displacement [0, 1]. Default 0.25. </summary>
      public float offset = 0.25f;

      /// <summary> Image displacement scale [0, 1]. Default 0.25. </summary>
      public float offsetScale = 0.5f;

      /// <summary> Filter to apply offset. The more quantity, the less patterns will be used [0, 1]. Default 0.9. </summary>
      public float offsetThreshold = 0.9f;

      /// <summary> Activate to see the pattern used. </summary>
      public bool debugView = false;

      public static Vector2 DefaultZoom = new(11.0f, 7.0f);

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

        density = 0.2f;
        complexity = 5.0f;
        aberration = Vector3.one * 10.0f;
        aberrationThreshold = 0.9f;
        speed = 10.0f;
        zoom = DefaultZoom;
        scrolling = Vector2.zero;
        pixelation = 0.25f;
        pixelationThreshold = 0.9f;
        offset = 0.25f;
        offsetScale = 0.5f;
        offsetThreshold = 0.9f;
        debugView = false;

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
