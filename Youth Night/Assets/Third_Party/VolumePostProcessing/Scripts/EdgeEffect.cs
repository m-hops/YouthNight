using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace izzynab.CustomPostProcessingStack
{
    [System.Serializable, VolumeComponentMenu("CustomPostProcess/Edge")]
    public class EdgeEffect : VolumeComponent
    {
        [Tooltip("Controls edge color strength. If equals 0, Edge effect won't work.")]
        public ClampedFloatParameter _EdgeColorStrength = new ClampedFloatParameter(0, 0, 1);
        [Tooltip("Controls edge color. Alpha channel controls blending between original scene colors, affected by lightness and staturation parametes, and this one.")]
        public ColorParameter _EdgeColor = new ColorParameter(Color.black);
        [Tooltip("Controls background color. Alpha channel controls blending between original scene color and this one.")]
        public ColorParameter _BackgroundColor = new ColorParameter(Color.white);
        [Tooltip("Controls cutoff value for edge detection.")]
        public ClampedFloatParameter _Cutoff = new ClampedFloatParameter(0, 0, 4);

        [Header("Scene affected edge color")]
        [Tooltip("Controls lightness of edge color based on the scene color.")]
        public ClampedFloatParameter _Lightness = new ClampedFloatParameter(1, 0, 4);
        [Tooltip("Controls saturation of edge color based on the scene color.")]
        public ClampedFloatParameter _Saturation = new ClampedFloatParameter(1, 0, 4);

    }

    // Define the renderer for the custom post processing effect
    [CustomPostProcess("Edge", CustomPostProcessInjectionPoint.AfterOpaque)]
    public class EdgeEffectRenderer : CustomPostProcessRenderer
    {
        // A variable to hold a reference to the corresponding volume component (you can define as many as you like)
        private EdgeEffect m_VolumeComponent;

        // The postprocessing material (you can define as many as you like)
        private Material m_Material;


        // By default, the effect is visible in the scene view, but we can change that here.
        public override bool visibleInSceneView => true;

        static class ShaderIDs
        {
            internal readonly static int _EdgeColorLightness = Shader.PropertyToID("_EdgeColorLightness");
            internal readonly static int _EdgeColor = Shader.PropertyToID("_EdgeColor");
            internal readonly static int _BackgroundColor = Shader.PropertyToID("_BackgroundColor");
            internal readonly static int _Cutoff = Shader.PropertyToID("_Cutoff");
            internal readonly static int _Lightness = Shader.PropertyToID("_Lightness");
            internal readonly static int _Saturation = Shader.PropertyToID("_Saturation");
        }


        // Initialized is called only once before the first render call
        // so we use it to create our material and initialize variables
        public override void Initialize()
        {
            m_Material = CoreUtils.CreateEngineMaterial("Hidden/CustomPostProcess/Neon");
        }

        // Called for each camera/injection point pair on each frame. Return true if the effect should be rendered for this camera.
        public override bool Setup(ref RenderingData renderingData, CustomPostProcessInjectionPoint injectionPoint)
        {
            // Get the current volume stack
            var stack = VolumeManager.instance.stack;
            // Get the corresponding volume component
            m_VolumeComponent = stack.GetComponent<EdgeEffect>();
            // if power value > 0, then we need to render this effect. 
            return m_VolumeComponent._EdgeColorStrength.value > 0;
        }

        // The actual rendering execution is done here
        public override void Render(CommandBuffer cmd, RenderTargetIdentifier source, RenderTargetIdentifier destination, ref RenderingData renderingData, CustomPostProcessInjectionPoint injectionPoint)
        {
            // set material properties
            if (m_Material != null)
            {
                m_Material.SetFloat(ShaderIDs._Saturation, m_VolumeComponent._Saturation.value);
                m_Material.SetFloat(ShaderIDs._Lightness, m_VolumeComponent._Lightness.value);
                m_Material.SetFloat(ShaderIDs._Cutoff, m_VolumeComponent._Cutoff.value);
                m_Material.SetColor(ShaderIDs._BackgroundColor, m_VolumeComponent._BackgroundColor.value);
                m_Material.SetColor(ShaderIDs._EdgeColor, m_VolumeComponent._EdgeColor.value);
                m_Material.SetFloat(ShaderIDs._EdgeColorLightness, m_VolumeComponent._EdgeColorStrength.value);
            }
            // Since we are using a shader graph, we cann't use CoreUtils.DrawFullScreen without modifying the vertex shader.
            // So we go with the easy route and use CommandBuffer.Blit instead. The same goes if you want to use legacy image effect shaders.
            // Note: don't forget to set pass to 0 (last argument in Blit) to make sure that extra passes are not drawn.
            cmd.Blit(source, destination, m_Material, 0);
        }
    }

}