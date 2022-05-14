using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections.Generic;
using UnityEditor;

namespace izzynab.CustomPostProcessingStack
{
    // Define the Volume Component for the custom post processing effect 
    [System.Serializable, VolumeComponentMenu("CustomPostProcess/Binary")]
    public class BinaryEffect : VolumeComponent
    {
        #region Effect parameters
        [Tooltip("Color representing darker parts of the scene.")]
        public ColorParameter DarkerColor = new ColorParameter(Color.black);
        [Tooltip("Color representing lighter parts of the scene.")]
        public ColorParameter LighterColor = new ColorParameter(Color.white);

        [Tooltip("Controls light offset for calculating threshold between light and dark color.")]
        public ClampedFloatParameter _LuminanceOffset = new ClampedFloatParameter(0, -1, 1);

        [Tooltip("Controls the blending between the original and the binary.")]
        public ClampedFloatParameter opacity = new ClampedFloatParameter(0, 0, 1);

        [Tooltip("Dither size used by shader.")]
        public DitherTypeParameter ditherType = new DitherTypeParameter { value = DitherType.Bayer4x4 };

        #endregion

        #region Local enum and wrapper class
        [System.Serializable] public sealed class DitherTypeParameter : VolumeParameter<DitherType> { }

        #endregion
    }

    // Define the renderer for the custom post processing effect
    [CustomPostProcess("Binary", CustomPostProcessInjectionPoint.AfterOpaque | CustomPostProcessInjectionPoint.BeforePostProcess)]
    public class BinaryEffectRenderer : CustomPostProcessRenderer
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
        private BinaryEffect m_VolumeComponent;

        // The postprocessing material (you can define as many as you like)
        private Material m_Material;

        DitherType _ditherType;
        Texture2D _ditherTexture;

        // The ids of the shader variables
        static class ShaderIDs
        {
            internal readonly static int Input = Shader.PropertyToID("_MainTex");
            internal readonly static int Texture = Shader.PropertyToID("_DitherTexture");

            internal readonly static int color1 = Shader.PropertyToID("_Color0");
            internal readonly static int color2 = Shader.PropertyToID("_Color1");
            internal readonly static int opacity = Shader.PropertyToID("_Opacity");
            internal readonly static int ditherTexture = Shader.PropertyToID("_DitherTexture");
            internal readonly static int _LuminanceOffset = Shader.PropertyToID("_Lighterness");
        }

        // By default, the effect is visible in the scene view, but we can change that here.
        public override bool visibleInSceneView => true;

        // Initialized is called only once before the first render call
        // so we use it to create our material and initialize variables
        public override void Initialize()
        {
             m_Material = CoreUtils.CreateEngineMaterial("Hidden/CustomPostProcess/Binary");
        }

        // Called for each camera/injection point pair on each frame. Return true if the effect should be rendered for this camera.
        public override bool Setup(ref RenderingData renderingData, CustomPostProcessInjectionPoint injectionPoint)
        {
            // Get the current volume stack
            var stack = VolumeManager.instance.stack;
            // Get the corresponding volume component
            m_VolumeComponent = stack.GetComponent<BinaryEffect>();
            // if power value > 0, then we need to render this effect. 
            return m_VolumeComponent.opacity.value > 0;
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

            // set material properties
            if (m_Material != null)
            {
                m_Material.SetFloat(ShaderIDs.opacity, m_VolumeComponent.opacity.value);
                m_Material.SetColor(ShaderIDs.color1, m_VolumeComponent.DarkerColor.value);
                m_Material.SetColor(ShaderIDs.color2, m_VolumeComponent.LighterColor.value);
                m_Material.SetFloat(ShaderIDs._LuminanceOffset, m_VolumeComponent._LuminanceOffset.value);
                m_Material.SetTexture(ShaderIDs.ditherTexture, _ditherTexture);
            }

            cmd.SetGlobalTexture(ShaderIDs.Input, source);
            CoreUtils.DrawFullScreen(cmd, m_Material, destination);
        }
    }

}