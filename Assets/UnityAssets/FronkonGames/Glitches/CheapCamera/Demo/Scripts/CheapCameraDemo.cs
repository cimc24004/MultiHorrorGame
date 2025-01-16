using System;
using UnityEngine;
using FronkonGames.Glitches.CheapCamera;

#if UNITY_EDITOR
using UnityEditor;
[CustomEditor(typeof(CheapCameraDemo))]
public class CheapCameraWarning : Editor
{
  private GUIStyle Style => style ??= new GUIStyle(GUI.skin.GetStyle("HelpBox")) { richText = true, fontSize = 14, alignment = TextAnchor.MiddleCenter };
  private GUIStyle style;
  public override void OnInspectorGUI() =>
    EditorGUILayout.TextArea($"\nThis code is only for the demo\n\n<b>DO NOT USE</b> it in your projects\n\nIf you have any questions,\ncheck the <a href='{Constants.Support.Documentation}'>online help</a> or use the <a href='mailto:{Constants.Support.Email}'>support email</a>,\n<b>thanks!</b>\n", Style);
}
#endif

/// <summary> Glitches: Cheap Camera demo. </summary>
/// <remarks>
/// This code is designed for a simple demo, not for production environments.
/// </remarks>
public class CheapCameraDemo : MonoBehaviour
{
  private CheapCamera.Settings settings;

  private GUIStyle styleTitle;
  private GUIStyle styleLabel;
  private GUIStyle styleButton;

  private void ResetEffect()
  {
    settings.ResetDefaultValues();
  }

  private void Awake()
  {
    if (CheapCamera.IsInRenderFeatures() == false)
    {
      Debug.LogWarning($"Effect '{Constants.Asset.Name}' not found. You must add it as a Render Feature.");
#if UNITY_EDITOR
      if (EditorUtility.DisplayDialog($"Effect '{Constants.Asset.Name}' not found", $"You must add '{Constants.Asset.Name}' as a Render Feature.", "Quit") == true)
        EditorApplication.isPlaying = false;
#endif
    }

    this.enabled = CheapCamera.IsInRenderFeatures();
  }

  private void Start()
  {
    settings = CheapCamera.GetSettings();
    ResetEffect();
  }

  private void OnGUI()
  {
    styleTitle = new GUIStyle(GUI.skin.label)
    {
      alignment = TextAnchor.LowerCenter,
      fontSize = 32,
      fontStyle = FontStyle.Bold
    };

    styleLabel = new GUIStyle(GUI.skin.label)
    {
      alignment = TextAnchor.UpperLeft,
      fontSize = 24
    };

    styleButton = new GUIStyle(GUI.skin.button)
    {
      fontSize = 24
    };

    GUILayout.BeginHorizontal();
    {
      GUILayout.BeginVertical("box", GUILayout.Width(500.0f), GUILayout.Height(Screen.height));
      {
        const float space = 10.0f;

        GUILayout.Space(space);

        GUILayout.Label("CHEAP CAMERA DEMO", styleTitle);

        GUILayout.Space(space);

        settings.intensity = SliderField("Intensity", settings.intensity);

        settings.lensVignette = Vector3Field("Vignette", settings.lensVignette, -2.0f, 2.0f);
        settings.lensChromaticAberration = Vector4Field("Chromatic aberration", settings.lensChromaticAberration, -2.0f, 2.0f);
        settings.lensRadialDistortion = Vector3Field("Radial distortion", settings.lensRadialDistortion, -2.0f, 2.0f);
        settings.lensTangentialDistortion = Vector2Field("Tangential distortion", settings.lensTangentialDistortion, -2.0f, 2.0f);
        settings.lensZoom = SliderField("Zoom", settings.lensZoom);

        settings.sensorResolution = SliderField("Sensor resolution", settings.sensorResolution);
        settings.sensorPixelNoise = Vector3Field("Noise", settings.sensorPixelNoise, -2.0f, 2.0f);
        settings.sensorExposure = SliderField("Exposure", settings.sensorExposure, 0.0f, 10.0f);
        settings.sensorGain = SliderField("Gain", settings.sensorGain, 0.0f, 10.0f);
        settings.sensorDynamicRange = SliderField("Dynamic range", settings.sensorDynamicRange);

        settings.processorFunctions = Vector3Field("Processor functions", settings.processorFunctions, -2.0f, 2.0f);
        settings.processorWhiteBalance = Vector3Field("White balance", settings.processorWhiteBalance, -2.0f, 2.0f);

        GUILayout.FlexibleSpace();

        if (GUILayout.Button("RESET", styleButton) == true)
          ResetEffect();

        GUILayout.Space(4.0f);

        if (GUILayout.Button("ONLINE DOCUMENTATION", styleButton) == true)
          Application.OpenURL(Constants.Support.Documentation);

        GUILayout.Space(4.0f);

        if (GUILayout.Button("❤️ LEAVE A REVIEW ❤️", styleButton) == true)
          Application.OpenURL(Constants.Support.Store);

        GUILayout.Space(space * 2.0f);
      }
      GUILayout.EndVertical();

      GUILayout.FlexibleSpace();
    }
    GUILayout.EndHorizontal();
  }

  private void OnDestroy()
  {
    settings?.ResetDefaultValues();
  }

  private bool ToggleField(string label, bool value)
  {
    GUILayout.BeginHorizontal();
    {
      GUILayout.Label(label, styleLabel);

      value = GUILayout.Toggle(value, string.Empty);
    }
    GUILayout.EndHorizontal();

    return value;
  }

  private float SliderField(string label, float value, float min = 0.0f, float max = 1.0f)
  {
    GUILayout.BeginHorizontal();
    {
      GUILayout.Label(label, styleLabel);

      value = GUILayout.HorizontalSlider(value, min, max);
    }
    GUILayout.EndHorizontal();

    return value;
  }

  private int SliderField(string label, int value, int min, int max)
  {
    GUILayout.BeginHorizontal();
    {
      GUILayout.Label(label, styleLabel);

      value = (int)GUILayout.HorizontalSlider(value, min, max);
    }
    GUILayout.EndHorizontal();

    return value;
  }

  private Color ColorField(string label, Color value, bool alpha = true)
  {
    GUILayout.BeginHorizontal();
    {
      GUILayout.Label(label, styleLabel);

      float originalAlpha = value.a;

      UnityEngine.Color.RGBToHSV(value, out float h, out float s, out float v);
      h = GUILayout.HorizontalSlider(h, 0.0f, 1.0f);
      value = UnityEngine.Color.HSVToRGB(h, s, v);

      if (alpha == false)
        value.a = originalAlpha;
    }
    GUILayout.EndHorizontal();

    return value;
  }

  private Vector2 Vector2Field(string label, Vector2 value, float min, float max) => Vector2Field(label, value, "X", "Y", min, max);

  private Vector2 Vector2Field(string label, Vector2 value, string x = "X", string y = "Y", float min = 0.0f, float max = 1.0f)
  {
    GUILayout.Label(label, styleLabel);

    value.x = SliderField($"   {x}", value.x, min, max);
    value.y = SliderField($"   {y}", value.y, min, max);

    return value;
  }

  private Vector3 Vector3Field(string label, Vector3 value, float min, float max) => Vector3Field(label, value, "X", "Y", "Z", min, max);

  private Vector3 Vector3Field(string label, Vector3 value, string x = "X", string y = "Y", string z = "Z", float min = 0.0f, float max = 1.0f)
  {
    GUILayout.Label(label, styleLabel);

    value.x = SliderField($"   {x}", value.x, min, max);
    value.y = SliderField($"   {y}", value.y, min, max);
    value.z = SliderField($"   {z}", value.z, min, max);

    return value;
  }

  private Vector4 Vector4Field(string label, Vector4 value, float min, float max) => Vector4Field(label, value, "X", "Y", "Z", "W", min, max);

  private Vector4 Vector4Field(string label, Vector4 value, string x = "X", string y = "Y", string z = "Z", string w = "W", float min = 0.0f, float max = 1.0f)
  {
    GUILayout.Label(label, styleLabel);

    value.x = SliderField($"   {x}", value.x, min, max);
    value.y = SliderField($"   {y}", value.y, min, max);
    value.z = SliderField($"   {z}", value.z, min, max);
    value.w = SliderField($"   {w}", value.w, min, max);

    return value;
  }

  private T EnumField<T>(string label, T value) where T : Enum
  {
    string[] names = System.Enum.GetNames(typeof(T));
    Array values = System.Enum.GetValues(typeof(T));
    int index = Array.IndexOf(values, value);

    GUILayout.BeginHorizontal();
    {
      GUILayout.Label(label, styleLabel);

      if (GUILayout.Button("<", styleButton) == true)
        index = index > 0 ? index - 1 : values.Length - 1;

      GUILayout.Label(names[index], styleLabel);

      if (GUILayout.Button(">", styleButton) == true)
        index = index < values.Length - 1 ? index + 1 : 0;
    }
    GUILayout.EndHorizontal();

    return (T)(object)index;
  }
}
