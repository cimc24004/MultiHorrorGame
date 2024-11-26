using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Sirenix.OdinInspector;
using DG.Tweening;
public class PostProcessController : SerializedMonoBehaviour
{
    public UniversalRendererData rendererData; // URP Renderer を指定
    public PostProcessData postProcessData;   // PostProcessData を指定
    private void Start()
    {
        this.ToggleGlitchFX(false);
        DOVirtual.DelayedCall(0.4f, () =>
        {
            this.ToggleGlitchFX(true);
        });
    }
    [Button]
    // ポストプロセス全体の有効化/無効化
    public void TogglePostProcessing(bool isActive)
    {
        if (rendererData != null)
        {
            rendererData.postProcessData = isActive ? postProcessData : null;

            Debug.Log("ポストプロセスを " + (isActive ? "有効化" : "無効化") + " しました");
        }
        else
        {
            Debug.LogError("RendererData が設定されていません");
        }
    }
    [Button]
    public void ToggleGlitchFX(bool isActive)
    {
        if (rendererData != null && rendererData.rendererFeatures != null)
        {
            foreach (var feature in rendererData.rendererFeatures)
            {
                Debug.Log("!?!?!?!??!?!?: : : : : " + feature.name);
                feature.SetActive(isActive);
            }
        }
    }
}
