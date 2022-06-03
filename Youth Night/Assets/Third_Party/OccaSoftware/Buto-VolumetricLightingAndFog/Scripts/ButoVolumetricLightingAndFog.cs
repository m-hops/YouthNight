using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace OccaSoftware.Buto
{
    public class ButoVolumetricLightingAndFog : ScriptableRendererFeature
    {
        class RenderFogPass : ScriptableRenderPass
        {
            private RenderTargetIdentifier source;
            private RenderTargetHandle fogRenderTarget;
            private RenderTargetHandle fogMergeTarget;
            private RenderTargetHandle depthDownscaleTarget;

            private ButoFogComponent butoFogComponent = null;
            private Material mergeMaterial = null;
            private Material depthDownscaleMaterial = null;
            private const string mergeShaderPath = "Shader Graphs/VolumetricFogMergeShader";
            private const string depthDownscaleShaderPath = "Shader Graphs/DepthDownscaleShader";
            private const string mergeInputTexId = "_FOG_MERGE_INPUT_TEX";
            private const string downscaleInputTexId = "_DOWNSCALED_DEPTH_TEX";

            Shader mergeShader;
            Shader depthShader; 

            void SetupMaterials()
            {
                mergeShader = Shader.Find(mergeShaderPath);
                if (mergeShader != null && mergeMaterial == null)
                {
                    mergeMaterial = CoreUtils.CreateEngineMaterial(mergeShaderPath);
                }

                depthShader = Shader.Find(depthDownscaleShaderPath);
                if (depthShader != null && depthDownscaleMaterial == null)
                {
                    depthDownscaleMaterial = CoreUtils.CreateEngineMaterial(depthShader);
                }
            }

            public RenderFogPass()
            {
                fogRenderTarget.Init("Fog Render Target");
                fogMergeTarget.Init("Fog Merge Target");
                depthDownscaleTarget.Init("Depth Downscale Target");

                SetupMaterials();
            }


            // This method is called before executing the render pass.
            // It can be used to configure render targets and their clear state. Also to create temporary render target textures.
            // When empty this render pass will render to the active camera render target.
            // You should never call CommandBuffer.SetRenderTarget. Instead call <c>ConfigureTarget</c> and <c>ConfigureClear</c>.
            // The render pipeline will ensure target setup and clearing happens in a performant manner.
            public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
            {
                if(butoFogComponent == null)
                {
                    butoFogComponent = FindObjectOfType<ButoFogComponent>();
                }

                RenderTextureDescriptor descriptor = renderingData.cameraData.cameraTargetDescriptor;
                descriptor.colorFormat = RenderTextureFormat.DefaultHDR;
                RenderTextureDescriptor descriptorHalfRes = descriptor;
                descriptorHalfRes.width /= 2;
                descriptorHalfRes.height /= 2;
                cmd.GetTemporaryRT(fogRenderTarget.id, descriptorHalfRes);
                cmd.GetTemporaryRT(depthDownscaleTarget.id, descriptorHalfRes);

                cmd.GetTemporaryRT(fogMergeTarget.id, descriptor);
            }

            bool IsValidRenderPass()
            {
                if (butoFogComponent == null)
                    return false;

                if (butoFogComponent.GetFogMaterial() == null || depthDownscaleMaterial == null || mergeMaterial == null)
                    return false;

                return true;
            }

            // Here you can implement the rendering logic.
            // Use <c>ScriptableRenderContext</c> to issue drawing commands or execute command buffers
            // https://docs.unity3d.com/ScriptReference/Rendering.ScriptableRenderContext.html
            // You don't have to call ScriptableRenderContext.submit, the render pipeline will call it at specific points in the pipeline.
            public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
            {
                if (!IsValidRenderPass())
                    return;

                source = renderingData.cameraData.renderer.cameraColorTarget;

                CommandBuffer cmd = CommandBufferPool.Get("FogRenderPass");

                Blit(cmd, source, fogRenderTarget.Identifier(), butoFogComponent.GetFogMaterial());
                cmd.SetGlobalTexture(mergeInputTexId, fogRenderTarget.Identifier());
                Blit(cmd, source, depthDownscaleTarget.Identifier(), depthDownscaleMaterial);
                cmd.SetGlobalTexture(downscaleInputTexId, depthDownscaleTarget.Identifier());

                Blit(cmd, source, fogMergeTarget.Identifier(), mergeMaterial);
                Blit(cmd, fogMergeTarget.Identifier(), source);

                context.ExecuteCommandBuffer(cmd);
                cmd.Clear();
                CommandBufferPool.Release(cmd);
            }

            // Cleanup any allocated resources that were created during the execution of this render pass.
            public override void OnCameraCleanup(CommandBuffer cmd)
            {
                cmd.ReleaseTemporaryRT(fogRenderTarget.id);
                cmd.ReleaseTemporaryRT(fogMergeTarget.id);
            }
        }

        RenderFogPass renderFogPass;


        /// <inheritdoc/>
        public override void Create()
        {
            renderFogPass = new RenderFogPass();
            renderFogPass.renderPassEvent = RenderPassEvent.BeforeRenderingTransparents;
        }

        // Here you can inject one or multiple render passes in the renderer.
        // This method is called when setting up the renderer once per-camera.
        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            renderer.EnqueuePass(renderFogPass);
        }
    }



}