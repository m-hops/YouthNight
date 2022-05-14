using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace izzynab.CustomPostProcessingStack
{

    // Define the Volume Component for the custom post processing effect 
    [System.Serializable, VolumeComponentMenu("CustomPostProcess/Recolor")]
    public class RecolorEffect : VolumeComponent
    {
        #region Local enum and wrapper class
        [System.Serializable] public sealed class DitherTypeParameter : VolumeParameter<DitherType> { }

        #endregion

        #region Effect parameters

        [Tooltip("Recolor gradient value.")]
        public GradientParameter fillGradient = new GradientParameter();

        [Tooltip("Controls the blending between the original and the ascii.")]
        public ClampedFloatParameter fillOpacity = new ClampedFloatParameter(0, 0, 1);

        [Tooltip("Dither size used by shader.")]
        public DitherTypeParameter ditherType = new DitherTypeParameter { value = DitherType.Bayer4x4 };

        [Tooltip("Dithering amount.")]
        public ClampedFloatParameter ditherStrength = new ClampedFloatParameter(0, 0, 1);

        #endregion

    }

    // Define the renderer for the custom post processing effect
    [CustomPostProcess("Recolor", CustomPostProcessInjectionPoint.AfterOpaque | CustomPostProcessInjectionPoint.BeforePostProcess | CustomPostProcessInjectionPoint.AfterPostProcess)]
    public class RecolorEffectRenderer : CustomPostProcessRenderer
    {
        #region Dither texture generator

        static Texture2D GenerateDitherTexture(DitherType type)
        {
            if (type == DitherType.Bayer2x2)
            {
                var tex = new Texture2D(2, 2, TextureFormat.R8, false, true);
                tex.LoadRawTextureData(new byte[] { 0, 170, 255, 85 });
                tex.Apply();
                return tex;
            }

            if (type == DitherType.Bayer3x3)
            {
                var tex = new Texture2D(3, 3, TextureFormat.R8, false, true);
                tex.LoadRawTextureData(new byte[] {
                    0, 223, 95, 191, 159, 63, 127, 31, 255
                });
                tex.Apply();
                return tex;
            }

            if (type == DitherType.Bayer4x4)
            {
                var tex = new Texture2D(4, 4, TextureFormat.R8, false, true);
                tex.LoadRawTextureData(new byte[] {
                    0, 136, 34, 170, 204, 68, 238, 102,
                    51, 187, 17, 153, 255, 119, 221, 85
                });
                tex.Apply();
                return tex;
            }

            if (type == DitherType.Bayer8x8)
            {
                var tex = new Texture2D(8, 8, TextureFormat.R8, false, true);
                tex.LoadRawTextureData(new byte[] {
                    0, 194, 48, 242, 12, 206, 60, 255,
                    129, 64, 178, 113, 141, 76, 190, 125,
                    32, 226, 16, 210, 44, 238, 28, 222,
                    161, 97, 145, 80, 174, 109, 157, 93,
                    8, 202, 56, 250, 4, 198, 52, 246,
                    137, 72, 186, 121, 133, 68, 182, 117,
                    40, 234, 24, 218, 36, 230, 20, 214,
                    170, 105, 153, 89, 165, 101, 149, 85
                });
                tex.Apply();
                return tex;
            }

            return null;
        }

        #endregion

        // A variable to hold a reference to the corresponding volume component (you can define as many as you like)
        private RecolorEffect m_VolumeComponent;

        // The postprocessing material (you can define as many as you like)
        private Material m_Material;

        // By default, the effect is visible in the scene view, but we can change that here.
        public override bool visibleInSceneView => true;


        Gradient _cachedGradient;
        GradientColorKey[] _cachedColorKeys;

        DitherType _ditherType;
        Texture2D _ditherTexture;

        static class ShaderIDs
        {
            internal readonly static int _MainTex = Shader.PropertyToID("_MainTex");
            internal readonly static int _DitherStrength = Shader.PropertyToID("_DitherStrength");
            internal readonly static int _DitherTexture = Shader.PropertyToID("_DitherTexture");
            internal readonly static int _FillOpacity = Shader.PropertyToID("_FillOpacity");
        }

        // Initialized is called only once before the first render call
        // so we use it to create our material and initialize variables
        public override void Initialize()
        {
            m_Material = CoreUtils.CreateEngineMaterial("Hidden/CustomPostProcess/Recolor");
        }

        // Called for each camera/injection point pair on each frame. Return true if the effect should be rendered for this camera.
        public override bool Setup(ref RenderingData renderingData, CustomPostProcessInjectionPoint injectionPoint)
        {
            // Get the current volume stack
            var stack = VolumeManager.instance.stack;
            // Get the corresponding volume component
            m_VolumeComponent = stack.GetComponent<RecolorEffect>();
            // if power value > 0, then we need to render this effect. 
            return m_VolumeComponent.fillOpacity.value > 0 || m_VolumeComponent.ditherStrength.value > 0;
        }

        // The actual rendering execution is done here
        public override void Render(CommandBuffer cmd, RenderTargetIdentifier source, RenderTargetIdentifier destination, ref RenderingData renderingData, CustomPostProcessInjectionPoint injectionPoint)
        {
            if (_ditherType != m_VolumeComponent.ditherType.value || _ditherTexture == null)
            {
                CoreUtils.Destroy(_ditherTexture);
                _ditherType = m_VolumeComponent.ditherType.value;
                _ditherTexture = GenerateDitherTexture(_ditherType);
            }

#if UNITY_EDITOR
            // In Editor, the gradient will be modified without any hint,
            // so we have to copy the color keys every frame.
            if (true)
#else
            // In Player, we assume no one can modify gradients in profiles,
            // so we update the cache only when the reference was updated.
            if (_cachedGradient != m_VolumeComponent.fillGradient.value)
#endif
            {
                _cachedGradient = m_VolumeComponent.fillGradient.value;
                _cachedColorKeys = _cachedGradient.colorKeys;
            }

            // set material properties
            if (m_Material != null)
            {
                m_Material.SetFloat(ShaderIDs._FillOpacity, m_VolumeComponent.fillOpacity.value);
                GradientUtility.SetColorKeys(m_Material, _cachedColorKeys);

                m_Material.SetTexture(ShaderIDs._DitherTexture, _ditherTexture);
                m_Material.SetFloat(ShaderIDs._DitherStrength, m_VolumeComponent.ditherStrength.value);
            }

            int pass = 0;
            if (m_VolumeComponent.fillOpacity.value > 0 && _cachedColorKeys.Length > 4) pass += 2;
            if (m_VolumeComponent.fillGradient.value.mode == GradientMode.Blend) pass += 1;
            ///////////////////////////

            // set source texture
            cmd.SetGlobalTexture(ShaderIDs._MainTex, source);
            // draw a fullscreen triangle to the destination
            CoreUtils.DrawFullScreen(cmd, m_Material, destination, null, pass);
        }
    }

}