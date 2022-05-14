using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace izzynab.CustomPostProcessingStack
{
    // Define the Volume Component for the custom post processing effect 
    [System.Serializable, VolumeComponentMenu("CustomPostProcess/AnimeLines")]
    public class AnimeLinesEffect : VolumeComponent
    {
        [Tooltip("Controls the opacity of the vignette.")]
        public ClampedFloatParameter Opacity = new ClampedFloatParameter(0, 0, 1);
        [Tooltip("Color of the vignette.")]
        public ColorParameter _Color = new ColorParameter(new Color(0, 0, 0, 0));
        [Tooltip("Controls lines length.")]
        public ClampedFloatParameter _RadialScale = new ClampedFloatParameter(0.1f, 0, 10);
        [Tooltip("Controls lines amount.")]
        public ClampedFloatParameter _Tiling = new ClampedFloatParameter(200, 50, 400);
        [Tooltip("Controls lines animation speed.")]
        public ClampedFloatParameter _AnimationSpeed = new ClampedFloatParameter(3, 0, 9);
        [Tooltip("Controls lines rarity.")]
        public ClampedFloatParameter _Power = new ClampedFloatParameter(1, 1, 2);
        [Tooltip("Controls center mask scale.")]
        public ClampedFloatParameter _MaskScale = new ClampedFloatParameter(1, 0, 2);
        [Tooltip("Controls Lines length towards center.")]
        public ClampedFloatParameter _MaskPower = new ClampedFloatParameter(0.6f, 3f, 5);


    }

    // Define the renderer for the custom post processing effect
    [CustomPostProcess("Anime Lines", CustomPostProcessInjectionPoint.AfterPostProcess | CustomPostProcessInjectionPoint.BeforePostProcess)]
    public class AnimeLinesEffectRenderer : CustomPostProcessRenderer
    {
        // A variable to hold a reference to the corresponding volume component (you can define as many as you like)
        private AnimeLinesEffect m_VolumeComponent;

        // The postprocessing material (you can define as many as you like)
        private Material m_Material;                                                        

        // By default, the effect is visible in the scene view, but we can change that here.
        public override bool visibleInSceneView => true;

        static class ShaderIDs
        {
            internal readonly static int _Color = Shader.PropertyToID("_Color");
            internal readonly static int _SpeedLinesTiling = Shader.PropertyToID("_SpeedLinesTiling");
            internal readonly static int _SpeedLinesRemap = Shader.PropertyToID("_SpeedLinesRemap");
            internal readonly static int _SpeedLinesRadialScale = Shader.PropertyToID("_SpeedLinesRadialScale");
            internal readonly static int _SpeedLinesPower = Shader.PropertyToID("_SpeedLinesPower");
            internal readonly static int _SpeedLinesAnimation = Shader.PropertyToID("_SpeedLinesAnimation");
            internal readonly static int _MaskScale = Shader.PropertyToID("_MaskScale");
            internal readonly static int _MaskPower = Shader.PropertyToID("_MaskPower");
            internal readonly static int _MaskHardness = Shader.PropertyToID("_MaskHardness");
        }

        // Initialized is called only once before the first render call
        // so we use it to create our material
        public override void Initialize()
        {
            m_Material = CoreUtils.CreateEngineMaterial("Hidden/CustomPostProcess/AnimeLines");
        }

        // Called for each camera/injection point pair on each frame. Return true if the effect should be rendered for this camera.
        public override bool Setup(ref RenderingData renderingData, CustomPostProcessInjectionPoint injectionPoint)
        {
            // Get the current volume stack
            var stack = VolumeManager.instance.stack;
            // Get the corresponding volume component
            m_VolumeComponent = stack.GetComponent<AnimeLinesEffect>();
            // if split value > 0, then we need to render this effect. 
            return m_VolumeComponent.Opacity.value > 0;
        }

        // The actual rendering execution is done here
        public override void Render(CommandBuffer cmd, RenderTargetIdentifier source, RenderTargetIdentifier destination, ref RenderingData renderingData, CustomPostProcessInjectionPoint injectionPoint)
        {
            // set material properties
            if (m_Material != null)
            {
                m_Material.SetColor(ShaderIDs._Color, new Color(m_VolumeComponent._Color.value.r, m_VolumeComponent._Color.value.g, m_VolumeComponent._Color.value.b, m_VolumeComponent.Opacity.value));
                m_Material.SetFloat(ShaderIDs._MaskHardness, 0);
                m_Material.SetFloat(ShaderIDs._MaskPower, m_VolumeComponent._MaskPower.value);
                m_Material.SetFloat(ShaderIDs._MaskScale, m_VolumeComponent._MaskScale.value);
                m_Material.SetFloat(ShaderIDs._SpeedLinesAnimation, m_VolumeComponent._AnimationSpeed.value);
                m_Material.SetFloat(ShaderIDs._SpeedLinesPower, m_VolumeComponent._Power.value);
                m_Material.SetFloat(ShaderIDs._SpeedLinesRadialScale, m_VolumeComponent._RadialScale.value);
                m_Material.SetFloat(ShaderIDs._SpeedLinesRemap, 0.8f);
                m_Material.SetFloat(ShaderIDs._SpeedLinesTiling, m_VolumeComponent._Tiling.value);
            }

            cmd.Blit(source, destination, m_Material, 0);
        }
    }

}