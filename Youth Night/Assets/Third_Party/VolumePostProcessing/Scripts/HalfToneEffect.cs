using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace izzynab.CustomPostProcessingStack
{

    // Define the Volume Component for the custom post processing effect 
    [System.Serializable, VolumeComponentMenu("CustomPostProcess/HalfTone")]
    public class HalfToneEffect : VolumeComponent
    {
        [Tooltip("Controls dot color. Alpha channel controls blending between original scene color and this one.")]
        public ColorParameter _DotColor = new ColorParameter(new Color(0,0,0,0));
        [Tooltip("Controls background color. Alpha channel controls blending between original scene color and this one.")]
        public ColorParameter _BackgroundColor = new ColorParameter(new Color(1, 1, 1, 0));

        [Tooltip("Controls size of each dot.")]
        public ClampedFloatParameter _DotSize = new ClampedFloatParameter(6, 0, 10);
        [Tooltip("Controls size of each cell.")]
        public ClampedFloatParameter _CellSize = new ClampedFloatParameter(6, 0, 10);
        [Tooltip("Controls smoothness of dots.")]
        public ClampedFloatParameter _DotSmoothness = new ClampedFloatParameter(0.002f, 0, 0.1f);
    }

    // Define the renderer for the custom post processing effect
    [CustomPostProcess("HalfTone", CustomPostProcessInjectionPoint.AfterOpaque | CustomPostProcessInjectionPoint.BeforePostProcess)]
    public class HalfToneEffectRenderer : CustomPostProcessRenderer
    {
        // A variable to hold a reference to the corresponding volume component (you can define as many as you like)
        private HalfToneEffect m_VolumeComponent;

        // The postprocessing material (you can define as many as you like)
        private Material m_Material;

        // By default, the effect is visible in the scene view, but we can change that here.
        public override bool visibleInSceneView => true;

        static class ShaderIDs
        {
            internal readonly static int _DotSize = Shader.PropertyToID("_DotSize");
            internal readonly static int _CellSize = Shader.PropertyToID("_CellSize");
            internal readonly static int _DotSmoothness = Shader.PropertyToID("_DotSmoothness");
            internal readonly static int _BackgroundColor = Shader.PropertyToID("_BackgroundColor");
            internal readonly static int _DotColor = Shader.PropertyToID("_DotColor");
        }

        // Initialized is called only once before the first render call
        // so we use it to create our material and initialize variables
        public override void Initialize()
        {
            m_Material = CoreUtils.CreateEngineMaterial("Hidden/CustomPostProcess/HalfTone");
        }

        // Called for each camera/injection point pair on each frame. Return true if the effect should be rendered for this camera.
        public override bool Setup(ref RenderingData renderingData, CustomPostProcessInjectionPoint injectionPoint)
        {
            // Get the current volume stack
            var stack = VolumeManager.instance.stack;
            // Get the corresponding volume component
            m_VolumeComponent = stack.GetComponent<HalfToneEffect>();
            // if power value > 0, then we need to render this effect. 
            return m_VolumeComponent._DotColor.value.a > 0 || m_VolumeComponent._BackgroundColor.value.a > 0;
        }

        // The actual rendering execution is done here
        public override void Render(CommandBuffer cmd, RenderTargetIdentifier source, RenderTargetIdentifier destination, ref RenderingData renderingData, CustomPostProcessInjectionPoint injectionPoint)
        {
            // set material properties
            if (m_Material != null)
            {
                m_Material.SetFloat(ShaderIDs._DotSize, m_VolumeComponent._DotSize.value);
                m_Material.SetFloat(ShaderIDs._CellSize, m_VolumeComponent._CellSize.value);
                m_Material.SetFloat(ShaderIDs._DotSmoothness, m_VolumeComponent._DotSmoothness.value);
                m_Material.SetColor(ShaderIDs._BackgroundColor, m_VolumeComponent._BackgroundColor.value);
                m_Material.SetColor(ShaderIDs._DotColor, m_VolumeComponent._DotColor.value);
            }
            cmd.Blit(source, destination, m_Material, 0);
        }
    }

}