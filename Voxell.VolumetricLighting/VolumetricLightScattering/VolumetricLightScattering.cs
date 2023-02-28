using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Voxell.VolumetricLighting
{
    internal class VolumetricLightScattering : ScriptableRendererFeature
    {
        public Settings settings;
        private VolumetricLightScatteringPass _renderPass;

        private Material _occluderMaterial;
        private Material _radialBlurMaterial;

        [System.Serializable]
        public class Settings
        {
            [Header("Volumetric Properties")]
            [Range(0.1f, 1f)]
            public float resolutionScale = 0.5f;
            [Range(0.0f, 1.0f)]
            public float intensity = 1.0f;
            [Range(0.0f, 1.0f)]
            public float blurWidth = 0.85f;
            [Range(0.0f, 0.5f)]
            public float fadeRange = 0.2f;
            [Range(50, 200)]
            public uint numSamples = 100;

            [Header("Noise Properties")]
            public Vector2 noiseSpeed = new Vector2(0.5f, 0.5f);
            public float noiseScale = 1.0f;
            [Range(0.0f, 1.0f)]
            public float noiseStrength = 0.6f;
        }

        public override void Create()
        {
            this._occluderMaterial = new Material(Shader.Find("Hidden/Occluder"));
            this._radialBlurMaterial = new Material(Shader.Find("Hidden/RadialBlur"));
            this._renderPass = new VolumetricLightScatteringPass(
              this._occluderMaterial, this._radialBlurMaterial, this.settings
            );
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            renderer.EnqueuePass(_renderPass);
        }

        // public override void SetupRenderPasses(ScriptableRenderer renderer, in RenderingData renderingData)
        // {
        //     // Calling ConfigureInput with the ScriptableRenderPassInput.Color argument ensures
        //     // that the opaque texture is available to the Render Pass
        //     // _renderPass.ConfigureInput(ScriptableRenderPassInput.Color);
        //     this._renderPass.SetTarget(renderer.cameraColorTargetHandle);
        // }

        protected override void Dispose(bool disposing)
        {
            CoreUtils.Destroy(this._occluderMaterial);
            CoreUtils.Destroy(this._radialBlurMaterial);
        }
    }
}