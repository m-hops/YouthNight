using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace izzynab.CustomPostProcessingStack
{

    // Define the Volume Component for the custom post processing effect 
    [System.Serializable, VolumeComponentMenu("CustomPostProcess/Sharpen")]
    public class SharpenEffect : VolumeComponent
    {
        [Tooltip("Controls sharpen intensity.")]
        public ClampedFloatParameter _Intensity = new ClampedFloatParameter(0, 0, 1);
    }

    // Define the renderer for the custom post processing effect
    [CustomPostProcess("Sharpen", CustomPostProcessInjectionPoint.BeforePostProcess | CustomPostProcessInjectionPoint.AfterPostProcess)]
    public class SharpenEffectRenderer : CustomPostProcessRenderer
    {
        // A variable to hold a reference to the corresponding volume component (you can define as many as you like)
        private SharpenEffect m_VolumeComponent;

        // The postprocessing material (you can define as many as you like)
        private Material m_Material;

        // By default, the effect is visible in the scene view, but we can change that here.
        public override bool visibleInSceneView => true;

        static class ShaderIDs
        {
            internal readonly static int _Intensity = Shader.PropertyToID("_Intensity");
            internal readonly static int _MainTex = Shader.PropertyToID("_MainTex");
        }

        // Initialized is called only once before the first render call
        // so we use it to create our material and initialize variables
        public override void Initialize()
        {
            m_Material = CoreUtils.CreateEngineMaterial("Hidden/CustomPostProcess/Sharpen");
        }

        // Called for each camera/injection point pair on each frame. Return true if the effect should be rendered for this camera.
        public override bool Setup(ref RenderingData renderingData, CustomPostProcessInjectionPoint injectionPoint)
        {
            // Get the current volume stack
            var stack = VolumeManager.instance.stack;
            // Get the corresponding volume component
            m_VolumeComponent = stack.GetComponent<SharpenEffect>();
            // if power value > 0, then we need to render this effect. 
            return m_VolumeComponent._Intensity.value > 0;
        }

        // The actual rendering execution is done here
        public override void Render(CommandBuffer cmd, RenderTargetIdentifier source, RenderTargetIdentifier destination, ref RenderingData renderingData, CustomPostProcessInjectionPoint injectionPoint)
        {
            // set material properties
            if (m_Material != null)
            {
                m_Material.SetFloat(ShaderIDs._Intensity, m_VolumeComponent._Intensity.value);
            }
            // Since we are using a shader graph, we cann't use CoreUtils.DrawFullScreen without modifying the vertex shader.
            // So we go with the easy route and use CommandBuffer.Blit instead. The same goes if you want to use legacy image effect shaders.
            // Note: don't forget to set pass to 0 (last argument in Blit) to make sure that extra passes are not drawn.
            //cmd.Blit(source, destination, m_Material, 0);


            // set source texture
            cmd.SetGlobalTexture(ShaderIDs._MainTex, source);
            // draw a fullscreen triangle to the destination
            CoreUtils.DrawFullScreen(cmd, m_Material, destination);
        }
    }

}