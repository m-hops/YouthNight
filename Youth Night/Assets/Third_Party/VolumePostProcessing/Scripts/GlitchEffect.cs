using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace izzynab.CustomPostProcessingStack
{

    // Define the Volume Component for the custom post processing effect 
    [System.Serializable, VolumeComponentMenu("CustomPostProcess/Glitch")]
    public class GlitchEffect : VolumeComponent
    {
        [Tooltip("Controls glitch animation speed.")]
        public ClampedFloatParameter _Speed = new ClampedFloatParameter(0, 0, 30);
        [Tooltip("Controls the size of the smallest block of the scene texture that can be affected by an glitch.")]
        public ClampedFloatParameter _BlockSize = new ClampedFloatParameter(0.03f, 0, 3);
        [Tooltip("Controls max rgb split value at X axis.")]
        public ClampedFloatParameter _MaxRGBSplitX = new ClampedFloatParameter(80, 0, 200);
        [Tooltip("Controls max rgb split value at Y axis.")]
        public ClampedIntParameter _MaxRGBSplitY = new ClampedIntParameter(150, 0,200);
    }

    // Define the renderer for the custom post processing effect
    [CustomPostProcess("Glitch", CustomPostProcessInjectionPoint.AfterOpaque | CustomPostProcessInjectionPoint.BeforePostProcess | CustomPostProcessInjectionPoint.AfterPostProcess)]
    public class GlitchEffectRenderer : CustomPostProcessRenderer
    {
        // A variable to hold a reference to the corresponding volume component (you can define as many as you like)
        private GlitchEffect m_VolumeComponent;

        // The postprocessing material (you can define as many as you like)
        private Material m_Material;

        // The ids of the shader variables
        static class ShaderIDs
        {
            internal readonly static int Input = Shader.PropertyToID("_MainTex");

            internal readonly static int _MaxRGBSplitX = Shader.PropertyToID("_MaxRGBSplitX");
            internal readonly static int _MaxRGBSplitY = Shader.PropertyToID("_MaxRGBSplitY");
            internal readonly static int _BlockSize = Shader.PropertyToID("_BlockSize");
            internal readonly static int _Speed = Shader.PropertyToID("_Speed");
        }

        // By default, the effect is visible in the scene view, but we can change that here.
        public override bool visibleInSceneView => true;


        // Initialized is called only once before the first render call
        // so we use it to create our material and initialize variables
        public override void Initialize()
        {
            m_Material = CoreUtils.CreateEngineMaterial("Hidden/CustomPostProcess/Glitch");
        }

        // Called for each camera/injection point pair on each frame. Return true if the effect should be rendered for this camera.
        public override bool Setup(ref RenderingData renderingData, CustomPostProcessInjectionPoint injectionPoint)
        {
            // Get the current volume stack
            var stack = VolumeManager.instance.stack;
            // Get the corresponding volume component
            m_VolumeComponent = stack.GetComponent<GlitchEffect>();
            // if power value > 0, then we need to render this effect. 
            return m_VolumeComponent._MaxRGBSplitX.value > 0 || m_VolumeComponent._MaxRGBSplitY.value > 0 || m_VolumeComponent._BlockSize.value > 0 || m_VolumeComponent._Speed.value > 0;
        }

        // The actual rendering execution is done here
        public override void Render(CommandBuffer cmd, RenderTargetIdentifier source, RenderTargetIdentifier destination, ref RenderingData renderingData, CustomPostProcessInjectionPoint injectionPoint)
        {
            // set material properties
            if (m_Material != null)
            {
                m_Material.SetFloat(ShaderIDs._Speed, m_VolumeComponent._Speed.value);
                m_Material.SetFloat(ShaderIDs._MaxRGBSplitX, m_VolumeComponent._MaxRGBSplitX.value);
                m_Material.SetFloat(ShaderIDs._MaxRGBSplitY, m_VolumeComponent._MaxRGBSplitY.value);
                m_Material.SetFloat(ShaderIDs._BlockSize, m_VolumeComponent._BlockSize.value);
            }

            // set source texture
            cmd.SetGlobalTexture(ShaderIDs.Input, source);
            // draw a fullscreen triangle to the destination
            CoreUtils.DrawFullScreen(cmd, m_Material, destination);
        }
    }

}