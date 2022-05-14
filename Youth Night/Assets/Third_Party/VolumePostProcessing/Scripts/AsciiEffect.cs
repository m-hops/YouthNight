using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace izzynab.CustomPostProcessingStack
{

    // Define the Volume Component for the custom post processing effect 
    [System.Serializable, VolumeComponentMenu("CustomPostProcess/Ascii")]
    public class AsciiEffect : VolumeComponent
    {
        [Tooltip("Tint of the Ascii characters")]
        public ColorParameter color = new ColorParameter(Color.blue);

        [Tooltip("Controls the blending between the original and the ascii")]
        public ClampedFloatParameter opacity = new ClampedFloatParameter(0, 0,1);

        [Tooltip("If checked, characters will be more dense")]
        public BoolParameter narrowerSpacing = new BoolParameter(true);
    }

    // Define the renderer for the custom post processing effect
    [CustomPostProcess("Ascii", CustomPostProcessInjectionPoint.AfterOpaque  | CustomPostProcessInjectionPoint.BeforePostProcess)]
    public class AsciiEffectRenderer : CustomPostProcessRenderer
    {
        // A variable to hold a reference to the corresponding volume component (you can define as many as you like)
        private AsciiEffect m_VolumeComponent;

        // The postprocessing material (you can define as many as you like)
        private Material m_Material;

        // The ids of the shader variables
        static class ShaderIDs
        {
            internal readonly static int Input = Shader.PropertyToID("_MainTex");
            internal readonly static int alpha = Shader.PropertyToID("_Alpha");
            internal readonly static int color = Shader.PropertyToID("_Color");
            internal readonly static int spacing = Shader.PropertyToID("_Spacing");
        }

        // By default, the effect is visible in the scene view, but we can change that here.
        public override bool visibleInSceneView => true;


        // Initialized is called only once before the first render call
        // so we use it to create our material and initialize variables
        public override void Initialize()
        {
            m_Material = CoreUtils.CreateEngineMaterial("Hidden/CustomPostProcess/Ascii");
        }

        // Called for each camera/injection point pair on each frame. Return true if the effect should be rendered for this camera.
        public override bool Setup(ref RenderingData renderingData, CustomPostProcessInjectionPoint injectionPoint)
        {
            // Get the current volume stack
            var stack = VolumeManager.instance.stack;
            // Get the corresponding volume component
            m_VolumeComponent = stack.GetComponent<AsciiEffect>();
            // if power value > 0, then we need to render this effect. 
            return m_VolumeComponent.opacity.value > 0;
        }

        // The actual rendering execution is done here
        public override void Render(CommandBuffer cmd, RenderTargetIdentifier source, RenderTargetIdentifier destination, ref RenderingData renderingData, CustomPostProcessInjectionPoint injectionPoint)
        {
            // set material properties
            if (m_Material != null)
            {
                m_Material.SetFloat(ShaderIDs.alpha, m_VolumeComponent.opacity.value);
                m_Material.SetColor(ShaderIDs.color, m_VolumeComponent.color.value);
                m_Material.SetFloat(ShaderIDs.spacing, m_VolumeComponent.narrowerSpacing.value ? 1.5f : 2f);
            }

            cmd.SetGlobalTexture(ShaderIDs.Input, source);
            CoreUtils.DrawFullScreen(cmd, m_Material, destination);
        }
    }

}