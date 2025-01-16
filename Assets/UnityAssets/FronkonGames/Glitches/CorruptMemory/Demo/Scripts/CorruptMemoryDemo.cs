using System;
using UnityEngine;
using FronkonGames.Glitches.CorruptMemory;

#if UNITY_EDITOR
using UnityEditor;
[CustomEditor(typeof(CorruptMemoryDemo))]
public class CorruptMemoryWarning : Editor
{
  private GUIStyle Style => style ??= new GUIStyle(GUI.skin.GetStyle("HelpBox")) { richText = true, fontSize = 14, alignment = TextAnchor.MiddleCenter };
  private GUIStyle style;
  public override void OnInspectorGUI() =>
    EditorGUILayout.TextArea($"\nThis code is only for the demo\n\n<b>DO NOT USE</b> it in your projects\n\nIf you have any questions,\ncheck the <a href='{Constants.Support.Documentation}'>online help</a> or use the <a href='mailto:{Constants.Support.Email}'>support email</a>,\n<b>thanks!</b>\n", Style);
}
#endif

/// <summary> Glitches: Corrupt Memory demo. </summary>
/// <remarks>
/// This code is designed for a simple demo, not for production environments.
/// </remarks>
public class CorruptMemoryDemo : MonoBehaviour
{
  private CorruptMemory.Settings settings;

  private GUIStyle styleTitle;
  private GUIStyle styleLabel;
  private GUIStyle styleButton;

  private void ResetEffect()
  {
    settings.ResetDefaultValues();
  }

  private void Awake()
  {
    if (CorruptMemory.IsInRenderFeatures() == false)
    {
      Debug.LogWarning($"Effect '{Constants.Asset.Name}' not found. You must add it as a Render Feature.");
#if UNITY_EDITOR
      if (EditorUtility.DisplayDialog($"Effect '{Constants.Asset.Name}' not found", $"You must add '{Constants.Asset.Name}' as a Render Feature.", "Quit") == true)
        EditorApplication.isPlaying = false;
#endif
    }

    this.enabled = CorruptMemory.IsInRenderFeatures();
  }

  private void Start()
  {
    settings = CorruptMemory.GetSettings();
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
      GUILayout.BeginVertical("box", GUILayout.Width(450.0f), GUILayout.Height(Screen.height));
      {
        const float space = 10.0f;

        GUILayout.Space(space);

        GUILayout.Label("CORRUPT MEMORY DEMO", styleTitle);

        GUILayout.Space(space);

        settings.intensity = SliderField("Intensity", settings.intensity);

        GUILayout.Space(space * 2.0f);


        settings.complexity = SliderField("Complexity", settings.complexity, 0.0f, 15.0f);
        settings.density = SliderField("Density", settings.density);
        settings.speed = SliderField("Speed", settings.speed, 0.0f, 25.0f);

        GUILayout.Space(2.0f);

        settings.pixelation = SliderField("Pixelation", settings.pixelation);
        settings.pixelationThreshold = SliderField("  Threshold", settings.pixelationThreshold);

        GUILayout.Space(2.0f);

        float aberration = settings.aberration.x;
        settings.aberration = SliderField("Aberration", aberration) * Vector2.one;
        settings.aberrationThreshold = SliderField("  Threshold", settings.aberrationThreshold);

        GUILayout.Space(2.0f);

        settings.offset = SliderField("Offset", settings.offset);
        settings.offsetThreshold = SliderField("  Threshold", settings.offsetThreshold);
        settings.offsetScale = SliderField("  Scale", settings.offsetScale);

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

  private Vector3 Vector3Field(string label, Vector3 value, string x = "X", string y = "Y", string z = "Z", float min = 0.0f, float max = 1.0f)
  {
    GUILayout.Label(label, styleLabel);

    value.x = SliderField($"   {x}", value.x, min, max);
    value.y = SliderField($"   {y}", value.y, min, max);
    value.z = SliderField($"   {z}", value.z, min, max);

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
