using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace izzynab.CustomPostProcessingStack
{
    // Define the Volume Component for the custom post processing effect 
    [System.Serializable, VolumeComponentMenu("CustomPostProcess/Analog")]
    public class AnalogEffect : VolumeComponent
    {
        [Tooltip("Controls amount of Scanline.")]
        public ClampedFloatParameter _Scanline = new ClampedFloatParameter(0, 0,1);
        [Tooltip("Controls amount of RGB split.")]
        public ClampedFloatParameter _FringeDelta = new ClampedFloatParameter(0, 0, 0.025f);
        [Tooltip("Controls amount of color bleed.")]
        public ClampedFloatParameter _BleedDelta = new ClampedFloatParameter(0, 0, 0.05f);
        [Tooltip("Controls amount of bleed taps.")]
        public ClampedIntParameter _BleedTaps = new ClampedIntParameter(0, 0,20);
    }

    // Define the renderer for the custom post processing effect
    [CustomPostProcess("Analog", CustomPostProcessInjectionPoint.BeforePostProcess | CustomPostProcessInjectionPoint.AfterPostProcess)]
    public class AnalogEffectRenderer : CustomPostProcessRenderer
    {
        // A variable to hold a reference to the corresponding volume component (you can define as many as you like)
        private AnalogEffect m_VolumeComponent;

        // The postprocessing material (you can define as many as you like)
        private Material m_Material;

        // The ids of the shader variables
        static class ShaderIDs
        {
            internal readonly static int Input = Shader.PropertyToID("_MainTex");

            internal readonly static int _Scanline = Shader.PropertyToID("_Scanline");
            internal readonly static int _FringeDelta = Shader.PropertyToID("_FringeDelta");
            internal readonly static int _BleedDelta = Shader.PropertyToID("_BleedDelta");
            internal readonly static int _BleedTaps = Shader.PropertyToID("_BleedTaps");
        }

        // By default, the effect is visible in the scene view, but we can change that here.
        public override bool visibleInSceneView => true;


        // Initialized is called only once before the first render call
        // so we use it to create our material and initialize variables
        public override void Initialize()
        {
            m_Material = CoreUtils.CreateEngineMaterial("Hidden/CustomPostProcess/Analog");
        }

        // Called for each camera/injection point pair on each frame. Return true if the effect should be rendered for this camera.
        public override bool Setup(ref RenderingData renderingData, CustomPostProcessInjectionPoint injectionPoint)
        {
            // Get the current volume stack
            var stack = VolumeManager.instance.stack;
            // Get the corresponding volume component
            m_VolumeComponent = stack.GetComponent<AnalogEffect>();
            // if power value > 0, then we need to render this effect. 
            return m_VolumeComponent._Scanline.value > 0 || m_VolumeComponent._FringeDelta.value > 0 || m_VolumeComponent._BleedDelta.value > 0 || m_VolumeComponent._BleedTaps.value > 0;
        }

        // The actual rendering execution is done here
        public override void Render(CommandBuffer cmd, RenderTargetIdentifier source, RenderTargetIdentifier destination, ref RenderingData renderingData, CustomPostProcessInjectionPoint injectionPoint)
        {
            // set material properties
            if (m_Material != null)
            {
                m_Material.SetInt(ShaderIDs._BleedTaps, m_VolumeComponent._BleedTaps.value);
                m_Material.SetFloat(ShaderIDs._BleedDelta, m_VolumeComponent._BleedDelta.value);
                m_Material.SetFloat(ShaderIDs._FringeDelta, m_VolumeComponent._FringeDelta.value);
                m_Material.SetFloat(ShaderIDs._Scanline, m_VolumeComponent._Scanline.value);
            }
            cmd.Blit(source, destination, m_Material, 0);
        }
    }

}