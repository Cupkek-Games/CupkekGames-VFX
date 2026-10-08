using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

namespace CupkekGames.VFX
{
    /// <summary>
    /// <see cref="SceneSpotlight"/>'s dim: one full-screen triangle that darkens the camera's
    /// picture by the global <c>_CkgSpotlightDim</c>, except where the stencil marks the
    /// spotlit renderers (its material is <c>Hidden/CupkekGames/SpotlightDim</c>, with the
    /// mask's stencil reference). Queued only while the dim is above zero, for game cameras.
    /// Place it after the spotlight's stencil passes and before the effects' passes.
    /// </summary>
    public class SpotlightDimFeature : ScriptableRendererFeature
    {
        [Tooltip("A Hidden/CupkekGames/SpotlightDim material.")]
        [SerializeField] private Material _material;

        [Tooltip("When the dim draws: after the transparents, before the effects drawn over it.")]
        [SerializeField] private RenderPassEvent _event = RenderPassEvent.AfterRenderingTransparents;

        private DimPass _pass;

        public override void Create()
        {
            _pass = new DimPass();
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (_material == null) return;
            if (renderingData.cameraData.cameraType != CameraType.Game) return;
            if (Shader.GetGlobalFloat(SceneSpotlight.DimId) <= 0f) return;

            _pass.renderPassEvent = _event;
            _pass.Material = _material;
            renderer.EnqueuePass(_pass);
        }

        private sealed class DimPass : ScriptableRenderPass
        {
            public Material Material;

            private sealed class PassData
            {
                public Material Material;
            }

            public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
            {
                UniversalResourceData resources = frameData.Get<UniversalResourceData>();
                using (IRasterRenderGraphBuilder builder = renderGraph.AddRasterRenderPass("Spotlight Dim", out PassData data))
                {
                    data.Material = Material;
                    builder.SetRenderAttachment(resources.activeColorTexture, 0, AccessFlags.ReadWrite);
                    // The stencil keeps the spotlit renderers out of the dim.
                    builder.SetRenderAttachmentDepth(resources.activeDepthTexture, AccessFlags.Read);
                    builder.SetRenderFunc(static (PassData passData, RasterGraphContext context) =>
                    {
                        context.cmd.DrawProcedural(Matrix4x4.identity, passData.Material, 0, MeshTopology.Triangles, 3, 1);
                    });
                }
            }
        }
    }
}
