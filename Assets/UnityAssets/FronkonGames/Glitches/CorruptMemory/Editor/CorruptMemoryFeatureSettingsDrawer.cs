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
using static FronkonGames.Glitches.CorruptMemory.Inspector;

namespace FronkonGames.Glitches.CorruptMemory.Editor
{
  /// <summary> Corrupt Memory inspector. </summary>
  [CustomPropertyDrawer(typeof(CorruptMemory.Settings))]
  public class CorruptMemoryFeatureSettingsDrawer : Drawer
  {
    private CorruptMemory.Settings settings;

    protected override void ResetValues() => settings?.ResetDefaultValues();

    protected override void InspectorGUI()
    {
      settings ??= GetSettings<CorruptMemory.Settings>();

      /////////////////////////////////////////////////
      // Common.
      /////////////////////////////////////////////////
      settings.intensity = Slider("Intensity", "Controls the intensity of the effect [0, 1]. Default 0.", settings.intensity, 0.0f, 1.0f, 1.0f);

      /////////////////////////////////////////////////
      // Corrupt Memory.
      /////////////////////////////////////////////////
      Separator();

      settings.complexity = Slider("Complexity", "Complexity of the pattern used [0, 15]. Default 5.", settings.complexity, 0.0f, 15.0f, 5.0f);
      settings.density = Slider("Density", "Density of pattern used [0, 1]. Default 0.2.", settings.density, 0.0f, 1.0f, 0.2f);
      settings.speed = Slider("Speed", "Rate of change of the pattern used [0, 25]. Default 10.", settings.speed, 0.0f, 25.0f, 10.0f);
      settings.scrolling = Vector2Field("Scrolling", "Displacement of the pattern used.", settings.scrolling, Vector2.zero);
      settings.zoom = Vector2Field("Zoom", "Zoom of the used pattern.", settings.zoom, CorruptMemory.Settings.DefaultZoom);

      settings.pixelation = Slider("Pixelation", "Intensity of pixelation effect [0, 1]. Default 0.25.", settings.pixelation, 0.0f, 1.0f, 0.25f);
      IndentLevel++;
      settings.pixelationThreshold = Slider("Threshold", "Filter to apply pixelation. The more quantity, the less patterns will be used [0, 1]. Default 0.9.", settings.pixelationThreshold, 0.0f, 1.0f, 0.9f);
      IndentLevel--;

      settings.aberration = Vector3Field("Aberration", "Intensity of the chromatic aberration in each color channel.", settings.aberration, Vector3.one * 10.0f);
      IndentLevel++;
      settings.aberrationThreshold = Slider("Threshold", "Intensity of pixelation effect [0, 1]. Default 0.25.", settings.aberrationThreshold, 0.0f, 1.0f, 0.9f);
      IndentLevel--;

      settings.offset = Slider("Offset", "Intensity of image displacement [0, 1]. Default 0.25.", settings.offset, 0.0f, 1.0f, 0.5f);
      IndentLevel++;
      settings.offsetThreshold = Slider("Threshold", "Filter to apply offset. The more quantity, the less patterns will be used [0, 1]. Default 0.9.", settings.offsetThreshold, 0.0f, 1.0f, 0.9f);
      settings.offsetScale = Slider("Scale", "Image displacement scale [0, 1]. Default 0.25.", settings.offsetScale, 0.0f, 1.0f, 0.5f);
      IndentLevel--;

      settings.debugView = Toggle("Debug view", "Enable render pass profiling", settings.debugView);

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
