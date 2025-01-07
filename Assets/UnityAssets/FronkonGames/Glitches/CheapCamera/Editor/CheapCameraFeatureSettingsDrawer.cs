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
using UnityEngine;
using UnityEditor;
using static FronkonGames.Glitches.CheapCamera.Inspector;

namespace FronkonGames.Glitches.CheapCamera.Editor
{
  /// <summary> Cheap Camera inspector. </summary>
  [CustomPropertyDrawer(typeof(CheapCamera.Settings))]
  public class CheapCameraFeatureSettingsDrawer : Drawer
  {
    private CheapCamera.Settings settings;

    protected override void ResetValues() => settings?.ResetDefaultValues();

    protected override void InspectorGUI()
    {
      settings ??= GetSettings<CheapCamera.Settings>();

      /////////////////////////////////////////////////
      // Common.
      /////////////////////////////////////////////////
      settings.intensity = Slider("Intensity", "Controls the intensity of the effect [0, 1]. Default 0.", settings.intensity, 0.0f, 1.0f, 1.0f);

      /////////////////////////////////////////////////
      // Cheap Camera.
      /////////////////////////////////////////////////
      Separator();

      Label("Lens");
      IndentLevel++;
      settings.lensVignette = Vector3Field("Vignette", "Vignette effect: center, middle, outer.", settings.lensVignette, CheapCamera.Settings.DefaultLensVignette);
      settings.lensRadialDistortion = Vector3Field("Radial distortion", "Radial distortion: center, middle, outer.", settings.lensRadialDistortion, CheapCamera.Settings.DefaultLensRadialDistortion);
      settings.lensChromaticAberration = Vector4Field("Chromatic aberration", "Chromatic aberration: center, middle, outer, noise.", settings.lensChromaticAberration, CheapCamera.Settings.DefaultLensChromaticAberration);
      settings.lensTangentialDistortion = Vector2Field("Tangential distortion", "Diagonal distortions.", settings.lensTangentialDistortion, Vector2.zero);
      settings.lensZoom = Slider("Zoom", "Zoom [0, 1]. Default 0.9.", settings.lensZoom, 0.0f, 1.0f, 0.9f);
      IndentLevel--;

      Label("Sensor");
      IndentLevel++;
      settings.sensorResolution = Slider("Resolution", "Resolution, the lower the resolution, the more pixelated [0, 1]. Default 0.3.", settings.sensorResolution, 0.01f, 1.0f, 0.3f);
      settings.sensorBlurQuality = (CheapCamera.Settings.BlurQuality)EnumPopup("Blur quality", "Quality of the blur effect. Default High.", settings.sensorBlurQuality, CheapCamera.Settings.BlurQuality.High);
      settings.sensorPixelNoise = Vector3Field("Pixel noise", "Static noise, dynamic noise, speed.", settings.sensorPixelNoise, CheapCamera.Settings.DefaultSensorPixelNoise);
      settings.sensorExposure = Slider("Exposure", "Exposure (prior to applying the noise) [0, 10]. Default 1.", settings.sensorExposure, 0.0f, 10.0f, 1.0f);
      settings.sensorGain = Slider("Gain", "Gain (after applying the noise) [0, 10]. Default 1.", settings.sensorGain, 0.0f, 10.0f, 1.0f);
      settings.sensorDynamicRange = Slider("Dynamic range", "Sensitivity of the sensor to color, the lower the sensitivity, the less colors [0, 1]. Default 0.3.", settings.sensorDynamicRange, 0.0f, 1.0f, 0.3f);
      IndentLevel--;

      Label("Processor");
      IndentLevel++;
      settings.processorFunctions = Vector3Field("Functions", "Edges, sharpen intensity, pixel size.", settings.processorFunctions, CheapCamera.Settings.DefaultProcessorFunctions);
      settings.processorWhiteBalance = Vector3Field("White balance", "White balance per color channel.", settings.processorWhiteBalance, Vector3.one);
      IndentLevel--;

      /////////////////////////////////////////////////
      // Color.
      /////////////////////////////////////////////////
      Separator();

      if (Foldout("Color") == true)
      {
        IndentLevel++;

        settings.brightness = Slider("Brightness", "Brightness [-1.0, 1.0]. Default 0.", settings.brightness, -1.0f, 1.0f, 0.0f);
        settings.contrast = Slider("Contrast", "Contrast [0.0, 10.0]. Default 1.", settings.contrast, 0.0f, 10.0f, 1.0f);
        settings.gamma = Slider("Gamma", "Gamma [0.1, 10.0]. Default 1.", settings.gamma, 0.01f, 10.0f, 1.0f);
        settings.hue = Slider("Hue", "The color wheel [0.0, 1.0]. Default 0.", settings.hue, 0.0f, 1.0f, 0.0f);
        settings.saturation = Slider("Saturation", "Intensity of a colors [0.0, 2.0]. Default 1.", settings.saturation, 0.0f, 2.0f, 1.0f);

        IndentLevel--;
      }

      /////////////////////////////////////////////////
      // Advanced.
      /////////////////////////////////////////////////
      Separator();

      if (Foldout("Advanced") == true)
      {
        IndentLevel++;

        settings.affectSceneView = Toggle("Affect the Scene View?", "Does it affect the Scene View?", settings.affectSceneView);
        settings.filterMode = (FilterMode)EnumPopup("Filter mode", "Filter mode. Default Bilinear.", settings.filterMode, FilterMode.Bilinear);
        settings.whenToInsert = (UnityEngine.Rendering.Universal.RenderPassEvent)EnumPopup("RenderPass event",
          "Render pass injection. Default BeforeRenderingPostProcessing.",
          settings.whenToInsert,
          UnityEngine.Rendering.Universal.RenderPassEvent.BeforeRenderingPostProcessing);
        settings.ignoreDeltaTimeScale = Toggle("Ignore delta time scale", "Set to true to ignore delta time scaling", settings.ignoreDeltaTimeScale);
        settings.enableProfiling = Toggle("Enable profiling", "Enable render pass profiling", settings.enableProfiling);

        IndentLevel--;
      }
    }
  }
}
