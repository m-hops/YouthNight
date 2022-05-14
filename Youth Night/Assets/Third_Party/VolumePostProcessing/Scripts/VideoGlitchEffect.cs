//https://github.com/keijiro/Kino

using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace izzynab.CustomPostProcessingStack
{

    // Define the Volume Component for the custom post processing effect 
    [System.Serializable, VolumeComponentMenu("CustomPostProcess/VideoGlitch")]
    public class VideoGlitchEffect : VolumeComponent
    {
        [Tooltip("Block noise effect.")]
        public ClampedFloatParameter block = new ClampedFloatParameter(0, 0, 1);
        [Tooltip("Color drift effect.")]
        public ClampedFloatParameter drift = new ClampedFloatParameter(0, 0, 1);
        [Tooltip("Scan line jitter effect.")]
        public ClampedFloatParameter jitter = new ClampedFloatParameter(0, 0, 1);
        [Tooltip("Vertical jump effect.")]
        public ClampedFloatParameter jump = new ClampedFloatParameter(0, 0, 1);
        [Tooltip("Horizontal shake effect.")]
        public ClampedFloatParameter shake = new ClampedFloatParameter(0, 0, 1);

    }

    // Define the renderer for the custom post processing effect
    [CustomPostProcess("VideoGlitch", CustomPostProcessInjectionPoint.BeforePostProcess | CustomPostProcessInjectionPoint.AfterPostProcess)]
    public class VideoGlitchEffectRenderer : CustomPostProcessRenderer
    {
        // A variable to hold a reference to the corresponding volume component (you can define as many as you like)
        private VideoGlitchEffect m_VolumeComponent;

        // The postprocessing material (you can define as many as you like)
        private Material m_Material;

        float _prevTime;
        float _jumpTime;

        float _blockTime;
        int _blockSeed1 = 71;
        int _blockSeed2 = 113;
        int _blockStride = 1;

        // The ids of the shader variables
        static class ShaderIDs
        {
            internal static readonly int BlockSeed1 = Shader.PropertyToID("_BlockSeed1");
            internal static readonly int BlockSeed2 = Shader.PropertyToID("_BlockSeed2");
            internal static readonly int BlockStrength = Shader.PropertyToID("_BlockStrength");
            internal static readonly int BlockStride = Shader.PropertyToID("_BlockStride");
            internal static readonly int Drift = Shader.PropertyToID("_Drift");
            internal static readonly int InputTexture = Shader.PropertyToID("_MainTex");
            internal static readonly int Jitter = Shader.PropertyToID("_Jitter");
            internal static readonly int Jump = Shader.PropertyToID("_Jump");
            internal static readonly int Seed = Shader.PropertyToID("_Seed");
            internal static readonly int Shake = Shader.PropertyToID("_Shake");
        }

        // By default, the effect is visible in the scene view, but we can change that here.
        public override bool visibleInSceneView => true;


        // Initialized is called only once before the first render call
        // so we use it to create our material and initialize variables
        public override void Initialize()
        {
            m_Material = CoreUtils.CreateEngineMaterial("Hidden/CustomPostProcess/VideoGlitch");
        }

        // Called for each camera/injection point pair on each frame. Return true if the effect should be rendered for this camera.
        public override bool Setup(ref RenderingData renderingData, CustomPostProcessInjectionPoint injectionPoint)
        {
            // Get the current volume stack
            var stack = VolumeManager.instance.stack;
            // Get the corresponding volume component
            m_VolumeComponent = stack.GetComponent<VideoGlitchEffect>();
            // if power value > 0, then we need to render this effect. 
            return m_VolumeComponent.block.value > 0 || m_VolumeComponent.drift.value > 0 || m_VolumeComponent.jitter.value > 0 || m_VolumeComponent.jump.value > 0 || m_VolumeComponent.shake.value > 0; ;
        }

        // The actual rendering execution is done here
        public override void Render(CommandBuffer cmd, RenderTargetIdentifier source, RenderTargetIdentifier destination, ref RenderingData renderingData, CustomPostProcessInjectionPoint injectionPoint)
        {
            if (m_Material == null) return;

            // Update the time parameters.
            var time = Time.time;
            var delta = time - _prevTime;
            _jumpTime += delta * m_VolumeComponent.jump.value * 11.3f;
            _prevTime = time;

            // Block parameters
            var block3 = m_VolumeComponent.block.value * m_VolumeComponent.block.value * m_VolumeComponent.block.value;

            // Shuffle block parameters every 1/30 seconds.
            _blockTime += delta * 60;
            if (_blockTime > 1)
            {
                if (Random.value < 0.09f) _blockSeed1 += 251;
                if (Random.value < 0.29f) _blockSeed2 += 373;
                if (Random.value < 0.25f) _blockStride = Random.Range(1, 32);
                _blockTime = 0;
            }

            // Drift parameters (time, displacement)
            var vdrift = new Vector2(
                time * 606.11f % (Mathf.PI * 2),
                m_VolumeComponent.drift.value * 0.04f
            );

            // Jitter parameters (threshold, displacement)
            var jv = m_VolumeComponent.jitter.value;
            var vjitter = new Vector3(
                Mathf.Max(0, 1.001f - jv * 1.2f),
                0.002f + jv * jv * jv * 0.05f
            );

            // Jump parameters (scroll, displacement)
            var vjump = new Vector2(_jumpTime, m_VolumeComponent.jump.value);

            // Invoke the shader.
            m_Material.SetInt(ShaderIDs.Seed, (int)(time * 10000));
            m_Material.SetFloat(ShaderIDs.BlockStrength, block3);
            m_Material.SetInt(ShaderIDs.BlockStride, _blockStride);
            m_Material.SetInt(ShaderIDs.BlockSeed1, _blockSeed1);
            m_Material.SetInt(ShaderIDs.BlockSeed2, _blockSeed2);
            m_Material.SetVector(ShaderIDs.Drift, vdrift);
            m_Material.SetVector(ShaderIDs.Jitter, vjitter);
            m_Material.SetVector(ShaderIDs.Jump, vjump);
            m_Material.SetFloat(ShaderIDs.Shake, m_VolumeComponent.shake.value * 0.2f);

            // Shader pass number
            var pass = 0;
            if (m_VolumeComponent.drift.value > 0 || m_VolumeComponent.jitter.value > 0 || m_VolumeComponent.jump.value > 0 || m_VolumeComponent.shake.value > 0) pass += 1;
            if (m_VolumeComponent.block.value > 0) pass += 2;

            // set source texture
            cmd.SetGlobalTexture(ShaderIDs.InputTexture, source);
            // draw a fullscreen triangle to the destination
            CoreUtils.DrawFullScreen(cmd, m_Material, destination, shaderPassId: pass);

        }
    }

}