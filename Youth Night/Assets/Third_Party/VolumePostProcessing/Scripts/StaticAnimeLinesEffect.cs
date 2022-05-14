using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace izzynab.CustomPostProcessingStack
{
    // Define the Volume Component for the custom post processing effect 
    [System.Serializable, VolumeComponentMenu("CustomPostProcess/StaticAnimeLines")]
    public class StaticAnimeLinesEffect : VolumeComponent
    {
        [Tooltip("Controls the opacity of the vignette.")]
        public ClampedFloatParameter Opacity = new ClampedFloatParameter(0, 0, 1);

        [Tooltip("Center of lines focus at X axis.")]
        public ClampedFloatParameter CenterX = new ClampedFloatParameter(0, -0.5f, 0.5f);

        [Tooltip("Center of lines focus at Y axis.")]
        public ClampedFloatParameter CenterY = new ClampedFloatParameter(0, -0.5f, 0.5f);

        [Tooltip("Focus amount of the lines in the center.")]
        public ClampedFloatParameter Central = new ClampedFloatParameter(0.0667f, 0.05f, 0.1f);

        [Tooltip("Controls Lines density.")]
        public ClampedFloatParameter Density = new ClampedFloatParameter(14.08f, 6, 20);

        [Tooltip("Controls Lines length towards center.")]
        public ClampedFloatParameter CentralLength = new ClampedFloatParameter(0.1556f, 0.1f, 0.3f);

        [Tooltip("Controls vignette distance from edge.")]
        public ClampedFloatParameter CentralEdge = new ClampedFloatParameter(0.8f, 0.3f, 0.8f);
    }

    // Define the renderer for the custom post processing effect
    [CustomPostProcess("Static Anime Lines", CustomPostProcessInjectionPoint.AfterPostProcess | CustomPostProcessInjectionPoint.BeforePostProcess)]
    public class StaticAnimeLinesEffectRenderer : CustomPostProcessRenderer
    {
        // A variable to hold a reference to the corresponding volume component (you can define as many as you like)
        private StaticAnimeLinesEffect m_VolumeComponent;

        // The postprocessing material (you can define as many as you like)
        private Material m_Material;

        // The ids of the shader variables
        static class ShaderIDs
        {
            internal readonly static int Input = Shader.PropertyToID("_MainTex");
            internal readonly static int CenterX = Shader.PropertyToID("_CenterX");
            internal readonly static int CenterY = Shader.PropertyToID("_CenterY");
            internal readonly static int Central = Shader.PropertyToID("_Central");
            internal readonly static int Line = Shader.PropertyToID("_Line");
            internal readonly static int CentralEdge = Shader.PropertyToID("_CentralEdge");
            internal readonly static int CentralLength = Shader.PropertyToID("_CentralLength");
            internal readonly static int Opacity = Shader.PropertyToID("_Opacity");
        }                                                             

        // By default, the effect is visible in the scene view, but we can change that here.
        public override bool visibleInSceneView => true;

        // Initialized is called only once before the first render call
        // so we use it to create our material
        public override void Initialize()
        {
            m_Material = CoreUtils.CreateEngineMaterial("Hidden/CustomPostProcess/StaticAnimeLines");
        }

        // Called for each camera/injection point pair on each frame. Return true if the effect should be rendered for this camera.
        public override bool Setup(ref RenderingData renderingData, CustomPostProcessInjectionPoint injectionPoint)
        {
            // Get the current volume stack
            var stack = VolumeManager.instance.stack;
            // Get the corresponding volume component
            m_VolumeComponent = stack.GetComponent<StaticAnimeLinesEffect>();
            // if split value > 0, then we need to render this effect. 
            return m_VolumeComponent.Opacity.value > 0;
        }

        // The actual rendering execution is done here
        public override void Render(CommandBuffer cmd, RenderTargetIdentifier source, RenderTargetIdentifier destination, ref RenderingData renderingData, CustomPostProcessInjectionPoint injectionPoint)
        {
            // set material properties
            if (m_Material != null)
            {
                float CenterX = m_VolumeComponent.CenterX.value + 0.5f;
                m_Material.SetFloat(ShaderIDs.CenterX, CenterX);

                float CenterY = m_VolumeComponent.CenterY.value + 0.5f;
                m_Material.SetFloat(ShaderIDs.CenterY, CenterY);

                float CentralEdge = m_VolumeComponent.CentralEdge.value;
                m_Material.SetFloat(ShaderIDs.CentralEdge, CentralEdge);

                float CentralLength = m_VolumeComponent.CentralLength.value;
                m_Material.SetFloat(ShaderIDs.CentralLength, CentralLength);

                float Line = m_VolumeComponent.Density.value;
                m_Material.SetFloat(ShaderIDs.Line, Line);

                float Central = m_VolumeComponent.Central.value;
                m_Material.SetFloat(ShaderIDs.Central, Central);

                float Opacity = m_VolumeComponent.Opacity.value;
                m_Material.SetFloat(ShaderIDs.Opacity, Opacity);
            }

            // set source texture
            cmd.SetGlobalTexture(ShaderIDs.Input, source);
            // draw a fullscreen triangle to the destination
            CoreUtils.DrawFullScreen(cmd, m_Material, destination);
        }
    }

}