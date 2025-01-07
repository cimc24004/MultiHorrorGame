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
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Unity.Profiling;

namespace FronkonGames.Glitches.CheapCamera
{
  ///------------------------------------------------------------------------------------------------------------------
  /// <summary> Render Pass. </summary>
  /// <remarks> Only available for Universal Render Pipeline. </remarks>
  ///------------------------------------------------------------------------------------------------------------------
  public sealed partial class CheapCamera
  {
    [DisallowMultipleRendererFeature]
    private sealed class RenderPass : ScriptableRenderPass
    {
      private readonly Settings settings;

      private RenderTargetIdentifier colorBuffer;
      private RenderTextureDescriptor renderTextureDescriptor;

#if UNITY_2022_1_OR_NEWER
      private RTHandle renderTextureHandle0;

      private readonly ProfilingSampler profilingSamples = new(Constants.Asset.AssemblyName);
      private ProfilingScope profilingScope;
#else
      private int renderTextureHandle0;
      
      private static readonly ProfilerMarker ProfilerMarker = new($"{Constants.Asset.AssemblyName}.Pass.Execute");
#endif
      private readonly Material material;

      private const string CommandBufferName = Constants.Asset.AssemblyName;

      private static class ShaderIDs
      {
        internal static readonly int Intensity = Shader.PropertyToID("_Intensity");

        internal static readonly int LensVignette = Shader.PropertyToID("_LensVignette");
        internal static readonly int LensChromaticAberration = Shader.PropertyToID("_LensChromaticAberration");
        internal static readonly int LensRadialDistortion = Shader.PropertyToID("_LensRadialDistortion");
        internal static readonly int LensTangentialDistortion = Shader.PropertyToID("_LensTangentialDistortion");
        internal static readonly int LensZoom = Shader.PropertyToID("_LensZoom");
        internal static readonly int SensorResolution = Shader.PropertyToID("_SensorResolution");
        internal static readonly int SensorPixelNoise = Shader.PropertyToID("_SensorPixelNoise");
        internal static readonly int SensorExposure = Shader.PropertyToID("_SensorExposure");
        internal static readonly int SensorGain = Shader.PropertyToID("_SensorGain");
        internal static readonly int SensorDynamicRange = Shader.PropertyToID("_SensorDynamicRange");
        internal static readonly int ProcessorFunctions = Shader.PropertyToID("_ProcessorFunctions");
        internal static readonly int ProcessorWhiteBalance = Shader.PropertyToID("_ProcessorWhiteBalance");

        internal static readonly int Brightness = Shader.PropertyToID("_Brightness");
        internal static readonly int Contrast = Shader.PropertyToID("_Contrast");
        internal static readonly int Gamma = Shader.PropertyToID("_Gamma");
        internal static readonly int Hue = Shader.PropertyToID("_Hue");
        internal static readonly int Saturation = Shader.PropertyToID("_Saturation");
      }

      private static class Keywords
      {
        internal static string ChromaticAberration = "_CHROMATIC_ABERRATION";
        internal static string BlurPass4 = "_BLUR_4";
        internal static string BlurPass3 = "_BLUR_3";
      }

      /// <summary> Render pass constructor. </summary>
      public RenderPass(Settings settings)
      {
        this.settings = settings;

        string shaderPath = $"Shaders/{Constants.Asset.ShaderName}_URP";
        Shader shader = Resources.Load<Shader>(shaderPath);
        if (shader != null)
        {
          if (shader.isSupported == true)
            material = CoreUtils.CreateEngineMaterial(shader);
          else
            Log.Warning($"'{shaderPath}.shader' not supported");
        }
      }

      /// <inheritdoc/>
      public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
      {
        renderTextureDescriptor = renderingData.cameraData.cameraTargetDescriptor;
        renderTextureDescriptor.depthBufferBits = 0;

#if UNITY_2022_1_OR_NEWER
        colorBuffer = renderingData.cameraData.renderer.cameraColorTargetHandle;

        RenderingUtils.ReAllocateIfNeeded(ref renderTextureHandle0, renderTextureDescriptor, settings.filterMode, TextureWrapMode.Clamp, false, 1, 0, $"_RTHandle0_{Constants.Asset.Name}");
#else
        colorBuffer = renderingData.cameraData.renderer.cameraColorTarget;

        renderTextureHandle0 = Shader.PropertyToID($"_RTHandle0_{Constants.Asset.Name}");
        cmd.GetTemporaryRT(renderTextureHandle0, renderTextureDescriptor.width, renderTextureDescriptor.height, renderTextureDescriptor.depthBufferBits, settings.filterMode, RenderTextureFormat.ARGB32);
#endif
      }

      /// <inheritdoc/>
      public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
      {
        if (material == null ||
            renderingData.postProcessingEnabled == false ||
            settings.intensity <= 0.0f ||
            settings.affectSceneView == false && renderingData.cameraData.isSceneViewCamera == true)
          return;

        CommandBuffer cmd = CommandBufferPool.Get(CommandBufferName);

        if (settings.enableProfiling == true)
#if UNITY_2022_1_OR_NEWER
          profilingScope = new ProfilingScope(cmd, profilingSamples);
#else
          ProfilerMarker.Begin();
#endif

        material.shaderKeywords = null;
        material.SetFloat(ShaderIDs.Intensity, settings.intensity);

        material.SetVector(ShaderIDs.ProcessorWhiteBalance, settings.processorWhiteBalance);

        if (settings.lensChromaticAberration != Vector4.zero)
        {
          material.EnableKeyword(Keywords.ChromaticAberration);
          material.SetVector(ShaderIDs.LensChromaticAberration, settings.lensChromaticAberration);
        }

        material.SetVector(ShaderIDs.LensVignette, settings.lensVignette);
        material.SetVector(ShaderIDs.LensRadialDistortion, settings.lensRadialDistortion);
        material.SetVector(ShaderIDs.LensTangentialDistortion, settings.lensTangentialDistortion);
        material.SetFloat(ShaderIDs.LensZoom, settings.lensZoom);

        material.SetVector(ShaderIDs.SensorResolution, new Vector2(Screen.width * Mathf.Max(settings.sensorResolution, 0.01f),
                                                                   Screen.height * Mathf.Max(settings.sensorResolution, 0.01f)));
        switch (settings.sensorBlurQuality)
        {
          case Settings.BlurQuality.Medium: material.EnableKeyword(Keywords.BlurPass3); break;
          case Settings.BlurQuality.High:   material.EnableKeyword(Keywords.BlurPass4); break;
        }

        material.SetVector(ShaderIDs.SensorPixelNoise, settings.sensorPixelNoise);
        material.SetFloat(ShaderIDs.SensorExposure, settings.sensorExposure);
        material.SetFloat(ShaderIDs.SensorGain, settings.sensorGain);
        material.SetFloat(ShaderIDs.SensorDynamicRange, settings.sensorDynamicRange);

        material.SetVector(ShaderIDs.ProcessorFunctions, settings.processorFunctions);

        material.SetFloat(ShaderIDs.Brightness, settings.brightness);
        material.SetFloat(ShaderIDs.Contrast, settings.contrast);
        material.SetFloat(ShaderIDs.Gamma, 1.0f / settings.gamma);
        material.SetFloat(ShaderIDs.Hue, settings.hue);
        material.SetFloat(ShaderIDs.Saturation, settings.saturation);

#if UNITY_2022_1_OR_NEWER
        cmd.Blit(colorBuffer, renderTextureHandle0, material);
        cmd.Blit(renderTextureHandle0, colorBuffer);
#else
        Blit(cmd, colorBuffer, renderTextureHandle0, material);
        Blit(cmd, renderTextureHandle0, colorBuffer);
#endif

        if (settings.enableProfiling == true)
#if UNITY_2022_1_OR_NEWER
          profilingScope.Dispose();
#else
          ProfilerMarker.End();
#endif

        context.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);
      }

#if UNITY_2022_1_OR_NEWER
      private void Dispose()
      {
        renderTextureHandle0?.Release();
      }
#else
      /// <inheritdoc/>
      public override void FrameCleanup(CommandBuffer cmd)
      {
        if (renderTextureHandle0 != -1)
          cmd.ReleaseTemporaryRT(renderTextureHandle0);
      }
#endif
    }
  }
}
