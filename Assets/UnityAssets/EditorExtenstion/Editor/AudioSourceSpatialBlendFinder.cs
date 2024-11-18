using UnityEngine;
using UnityEditor;

public class AudioSourceSpatialBlendFinder : EditorWindow
{
    [MenuItem("Tools/Find AudioSources with Spatial Blend")]
    public static void ShowWindow()
    {
        GetWindow<AudioSourceSpatialBlendFinder>("Spatial Blend Finder");
    }

    private float targetSpatialBlend = 1.0f;

    private void OnGUI()
    {
        GUILayout.Label("Spatial Blend Finder", EditorStyles.boldLabel);
        targetSpatialBlend = EditorGUILayout.Slider("Target Spatial Blend", targetSpatialBlend, 0f, 1f);

        if (GUILayout.Button("Search"))
        {
            FindAudioSourcesWithSpatialBlend(targetSpatialBlend);
        }
    }

    private void FindAudioSourcesWithSpatialBlend(float blendValue)
    {
        AudioSource[] audioSources = FindObjectsOfType<AudioSource>();

        foreach (AudioSource audioSource in audioSources)
        {
            if (Mathf.Approximately(audioSource.spatialBlend, blendValue))
            {
                Debug.Log($"Found AudioSource in GameObject: {audioSource.gameObject.name}", audioSource.gameObject);
            }
        }
    }
}