using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace izzynab.CustomPostProcessingStack
{

    // Define the Volume Component for the custom post processing effect 
    [System.Serializable, VolumeComponentMenu("CustomPostProcess/Slice")]
    public class SliceEffect : VolumeComponent
    {
        [Tooltip("Direction of the slices.")]
        public ClampedFloatParameter _Direction = new ClampedFloatParameter(45, 0, 90);
        [Tooltip("Strength of the displacement.")]
        public ClampedFloatParameter _Displacement = new ClampedFloatParameter(0, 0, 200);
        [Tooltip("Number of rows.")]
        public ClampedFloatParameter _Rows = new ClampedFloatParameter(0, 0, 0.6f);
        public IntParameter _Seed = new IntParameter(0);
    }

    // Define the renderer for the custom post processing effect
    [CustomPostProcess("Slice", CustomPostProcessInjectionPoint.BeforePostProcess | CustomPostProcessInjectionPoint.AfterPostProcess)]
    public class SliceEffectRenderer : CustomPostProcessRenderer
    {
        // A variable to hold a reference to the corresponding volume component (you can define as many as you like)
        private SliceEffect m_VolumeComponent;

        // The postprocessing material (you can define as many as you like)
        private Material m_Material;

        // By default, the effect is visible in the scene view, but we can change that here.
        public override bool visibleInSceneView => true;

        static class ShaderIDs
        {
            internal readonly static int _MainTex = Shader.PropertyToID("_MainTex");
            internal readonly static int _Seed = Shader.PropertyToID("_Seed");
            internal readonly static int _Rows = Shader.PropertyToID("_Rows");
            internal readonly static int _Displacement = Shader.PropertyToID("_Displacement");
            internal readonly static int _Direction = Shader.PropertyToID("_Direction");
        }

        // Initialized is called only once before the first render call
        // so we use it to create our material and initialize variables
        public override void Initialize()
        {
            m_Material = CoreUtils.CreateEngineMaterial("Hidden/CustomPostProcess/Slice");
        }

        // Called for each camera/injection point pair on each frame. Return true if the effect should be rendered for this camera.
        public override bool Setup(ref RenderingData renderingData, CustomPostProcessInjectionPoint injectionPoint)
        {
            // Get the current volume stack
            var stack = VolumeManager.instance.stack;
            // Get the corresponding volume component
            m_VolumeComponent = stack.GetComponent<SliceEffect>();
            // if power value > 0, then we need to render this effect. 
            return m_VolumeComponent._Rows.value > 0;
        }

        // The actual rendering execution is done here
        public override void Render(CommandBuffer cmd, RenderTargetIdentifier source, RenderTargetIdentifier destination, ref RenderingData renderingData, CustomPostProcessInjectionPoint injectionPoint)
        {
            // set material properties
            if (m_Material != null)
            {
                var rad = (m_VolumeComponent._Direction.value-90) * Mathf.Deg2Rad;
                var dir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));

                var seed = (uint)Time.time;
                seed = (seed << 16) | (seed >> 16);

                m_Material.SetVector(ShaderIDs._Direction, dir);
                m_Material.SetFloat (ShaderIDs._Displacement, m_VolumeComponent._Displacement.value);
                m_Material.SetFloat (ShaderIDs._Rows, m_VolumeComponent._Rows.value);
                m_Material.SetInt   (ShaderIDs._Seed, (int)seed);
            }

            // set source texture
            cmd.SetGlobalTexture(ShaderIDs._MainTex, source);
            CoreUtils.DrawFullScreen(cmd, m_Material, destination);
        }
    }

}