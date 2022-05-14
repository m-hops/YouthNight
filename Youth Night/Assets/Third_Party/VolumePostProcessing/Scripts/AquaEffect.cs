using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace izzynab.CustomPostProcessingStack
{

    // Define the Volume Component for the custom post processing effect 
    [System.Serializable, VolumeComponentMenu("CustomPostProcess/Aqua")]
    public class AquaEffect : VolumeComponent
    {
        [Tooltip("Size of the 'brush'. Keep this value as low as possible. The larger it is, the more expensive it becomes to render this effect.")]
        public ClampedIntParameter _KernelSize = new ClampedIntParameter(0, 0,20);
    }

    // Define the renderer for the custom post processing effect
    [CustomPostProcess("Aqua", CustomPostProcessInjectionPoint.AfterOpaque | CustomPostProcessInjectionPoint.BeforePostProcess | CustomPostProcessInjectionPoint.AfterPostProcess)]
    public class AquaEffectRenderer : CustomPostProcessRenderer
    {
        // A variable to hold a reference to the corresponding volume component (you can define as many as you like)
        private AquaEffect m_VolumeComponent;

        // The postprocessing material (you can define as many as you like)
        private Material m_Material;


        // By default, the effect is visible in the scene view, but we can change that here.
        public override bool visibleInSceneView => true;

        static class ShaderIDs
        {
            internal readonly static int _KernelSize = Shader.PropertyToID("_KernelSize");
        }

        // Initialized is called only once before the first render call
        // so we use it to create our material and initialize variables
        public override void Initialize()
        {
            m_Material = CoreUtils.CreateEngineMaterial("Hidden/CustomPostProcess/Aqua");
        }

        // Called for each camera/injection point pair on each frame. Return true if the effect should be rendered for this camera.
        public override bool Setup(ref RenderingData renderingData, CustomPostProcessInjectionPoint injectionPoint)
        {
            // Get the current volume stack
            var stack = VolumeManager.instance.stack;
            // Get the corresponding volume component
            m_VolumeComponent = stack.GetComponent<AquaEffect>();
            // if power value > 0, then we need to render this effect. 
            return m_VolumeComponent._KernelSize.value > 0;
        }

        // The actual rendering execution is done here
        public override void Render(CommandBuffer cmd, RenderTargetIdentifier source, RenderTargetIdentifier destination, ref RenderingData renderingData, CustomPostProcessInjectionPoint injectionPoint)
        {
            // set material properties
            if (m_Material != null)
            {
                m_Material.SetInt(ShaderIDs._KernelSize, m_VolumeComponent._KernelSize.value);
            }

            cmd.Blit(source, destination, m_Material, 0);
        }
    }

}