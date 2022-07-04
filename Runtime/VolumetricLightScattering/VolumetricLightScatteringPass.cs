using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Voxell.VolumetricLighting
{
  internal class VolumetricLightScatteringPass : ScriptableRenderPass
  {
    private ProfilingSampler _profilingSampler = new ProfilingSampler("VolumetricLightScattering");

    private Material _occluderMaterial;
    private Material _radialBlurMaterial;

    private RTHandle _cameraColorTarget;
    private RTHandle _occluderTarget;

    private VolumetricLightScattering.Settings _settings;
    private readonly List<ShaderTagId> _shaderTagIdList = new List<ShaderTagId>
    {
      new ShaderTagId("UniversalForward"),
      new ShaderTagId("UniversalForwardOnly"),
      new ShaderTagId("LightweightForward"),
      new ShaderTagId("SRPDefaultUnlit")
    };
    private FilteringSettings _filteringSettings = new FilteringSettings(RenderQueueRange.opaque);

    public VolumetricLightScatteringPass(
      Material occludersMaterial, Material radialBlurMaterial,
      VolumetricLightScattering.Settings settings
    )
    {
      this._occluderMaterial = occludersMaterial;
      this._radialBlurMaterial = radialBlurMaterial;
      this._settings = settings;
      this.renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;

      this._occluderTarget = RTHandles.Alloc(
        (int)(Screen.width*_settings.resolutionScale), (int)(Screen.height*_settings.resolutionScale),
        depthBufferBits: DepthBits.None
      );
    }

    public void SetTarget(RTHandle colorHandle)
    {
      this._cameraColorTarget = colorHandle;
    }

    public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
    {
      // finish configuration
      ConfigureTarget(this._occluderTarget);
    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
      if (this._occluderMaterial == null || this._radialBlurMaterial == null) return; 
      if (RenderSettings.sun ==  null || !RenderSettings.sun.enabled) return;

      CommandBuffer cmd = CommandBufferPool.Get();
      using (new ProfilingScope(cmd, this._profilingSampler))
      {
        context.ExecuteCommandBuffer(cmd);
        cmd.Clear();

        Camera camera = renderingData.cameraData.camera;
        context.DrawSkybox(camera);

        DrawingSettings drawSettings = CreateDrawingSettings(
          this._shaderTagIdList, ref renderingData, SortingCriteria.CommonOpaque
        );
        drawSettings.overrideMaterial = _occluderMaterial;
        context.DrawRenderers(renderingData.cullResults, ref drawSettings, ref this._filteringSettings);

        // schedule it for execution and release it after the execution
        context.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);

        Vector3 sunDirectionWorldSpace = RenderSettings.sun.transform.forward;
        Vector3 cameraDirectionWorldSpace = camera.transform.forward;
        Vector3 cameraPositionWorldSpace = camera.transform.position;
        Vector3 sunPositionWorldSpace = cameraPositionWorldSpace + sunDirectionWorldSpace;
        Vector3 sunPositionViewportSpace = camera.WorldToViewportPoint(sunPositionWorldSpace);

        float dotProd = Vector3.Dot(-cameraDirectionWorldSpace, sunDirectionWorldSpace);
        dotProd -= Vector3.Dot(cameraDirectionWorldSpace, Vector3.down);
        float intensityFader = dotProd / _settings.fadeRange;
        intensityFader = Mathf.Clamp(intensityFader, 0.0f, 1.0f);

        Color sunColor = RenderSettings.sun.color;
        if (RenderSettings.sun.useColorTemperature)
          sunColor *= Mathf.CorrelatedColorTemperatureToRGB(RenderSettings.sun.colorTemperature);

        _radialBlurMaterial.SetColor("_Color", sunColor);
        _radialBlurMaterial.SetVector("_Center", sunPositionViewportSpace);
        _radialBlurMaterial.SetFloat("_BlurWidth", _settings.blurWidth);
        _radialBlurMaterial.SetFloat("_NumSamples", _settings.numSamples);
        _radialBlurMaterial.SetFloat("_Intensity", _settings.intensity * intensityFader);

        _radialBlurMaterial.SetVector("_NoiseSpeed", _settings.noiseSpeed);
        _radialBlurMaterial.SetFloat("_NoiseScale", _settings.noiseScale);
        _radialBlurMaterial.SetFloat("_NoiseStrength", _settings.noiseStrength);

        Blit(cmd, _occluderTarget, _cameraColorTarget, _radialBlurMaterial);
      }
    }

    public override void OnCameraCleanup(CommandBuffer cmd)
    {
      base.OnCameraCleanup(cmd);
      cmd.ReleaseTemporaryRT(Shader.PropertyToID(_occluderTarget.name));
    }
  }
}
