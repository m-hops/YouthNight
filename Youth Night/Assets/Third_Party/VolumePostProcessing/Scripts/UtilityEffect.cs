//https://github.com/keijiro/Kino

using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace izzynab.CustomPostProcessingStack
{

    // Define the Volume Component for the custom post processing effect 
    [System.Serializable, VolumeComponentMenu("CustomPostProcess/Utility")]
    public class UtilityEffect : VolumeComponent
    {
        [Tooltip("Saturation value.")]
        public ClampedFloatParameter _Saturation = new ClampedFloatParameter(0, 0, 1);
        [Tooltip("Hue Shift value.")]
        public ClampedFloatParameter _HueShift = new ClampedFloatParameter(0, 0, 1);
        [Tooltip("Invert value.")]
        public ClampedFloatParameter _Invert = new ClampedFloatParameter(0, 0, 1);
        [Tooltip("Fade to color.")]
        public ColorParameter _FadeColor = new ColorParameter(Color.white);
    }

    // Define the renderer for the custom post processing effect
    [CustomPostProcess("Utility", CustomPostProcessInjectionPoint.BeforePostProcess | CustomPostProcessInjectionPoint.AfterPostProcess)]
    public class UtilityEffectRenderer : CustomPostProcessRenderer
    {
        // A variable to hold a reference to the corresponding volume component (you can define as many as you like)
        private UtilityEffect m_VolumeComponent;

        // The postprocessing material (you can define as many as you like)
        private Material m_Material;

        // By default, the effect is visible in the scene view, but we can change that here.
        public override bool visibleInSceneView => true;

        static class ShaderIDs
        {
            internal readonly static int _MainTex = Shader.PropertyToID("_MainTex");
            internal readonly static int _Saturation = Shader.PropertyToID("_Saturation");
            internal readonly static int _HueShift = Shader.PropertyToID("_HueShift");
            internal readonly static int _FadeColor = Shader.PropertyToID("_FadeColor");
            internal readonly static int _Invert = Shader.PropertyToID("_Invert");
        }

        // Initialized is called only once before the first render call
        // so we use it to create our material and initialize variables
        public override void Initialize()
        {
            m_Material = CoreUtils.CreateEngineMaterial("Hidden/CustomPostProcess/Utility");
        }

        // Called for each camera/injection point pair on each frame. Return true if the effect should be rendered for this camera.
        public override bool Setup(ref RenderingData renderingData, CustomPostProcessInjectionPoint injectionPoint)
        {
            // Get the current volume stack
            var stack = VolumeManager.instance.stack;
            // Get the corresponding volume component
            m_VolumeComponent = stack.GetComponent<UtilityEffect>();
            // if power value > 0, then we need to render this effect. 
            return m_VolumeComponent._Saturation.value > 0 || m_VolumeComponent._HueShift.value > 0 || m_VolumeComponent._Invert.value > 0;
        }

        // The actual rendering execution is done here
        public override void Render(CommandBuffer cmd, RenderTargetIdentifier source, RenderTargetIdentifier destination, ref RenderingData renderingData, CustomPostProcessInjectionPoint injectionPoint)
        {
            // set material properties
            if (m_Material != null)
            {
                m_Material.SetFloat(ShaderIDs._Saturation, m_VolumeComponent._Saturation.value);
                m_Material.SetFloat(ShaderIDs._HueShift, m_VolumeComponent._HueShift.value);
                m_Material.SetFloat(ShaderIDs._Invert, m_VolumeComponent._Invert.value);
                m_Material.SetColor(ShaderIDs._FadeColor, m_VolumeComponent._FadeColor.value);
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