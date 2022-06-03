using UnityEngine;
using UnityEditor;

namespace OccaSoftware.Buto.Editor
{
    public class ButoShaderGUI : ShaderGUI
    {
        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
        {
            Material t = materialEditor.target as Material;
            
            EditorGUI.BeginChangeCheck();

            
            EditorGUILayout.LabelField("Quality", EditorStyles.boldLabel);
            int _SampleCount = EditorGUILayout.IntSlider("Number of Samples", t.GetInt(P._SampleCount), 16, 64);
            bool _AnimateSamplePosition = EditorGUILayout.Toggle("Animate Sample Position", IntToBool(t.GetInt(P._AnimateSamplePosition)));
            float _MaxDistanceVolumetric = EditorGUILayout.Slider("Volumetric Fog Sampling Distance (m)", t.GetFloat(P._MaxDistanceVolumetric), 10, 128);
            float _MaxDistanceNonVolumetric = EditorGUILayout.Slider("Non-Volumetric Fog Sampling Distance (m)", t.GetFloat(P._MaxDistanceNonVolumetric), 128, 10000);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Characteristics", EditorStyles.boldLabel);
            float _FogDensity = EditorGUILayout.FloatField("Fog Density", t.GetFloat(P._FogDensity));
            float _Anisotropy = EditorGUILayout.Slider("Anisotropy", t.GetFloat(P._Anisotropy), -1, 1);
            float _LightIntensity = EditorGUILayout.FloatField("Light Intensity", t.GetFloat(P._LightIntensity));
            float _ShadowIntensity = EditorGUILayout.FloatField("Shadow Intensity", t.GetFloat(P._ShadowIntensity));

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Geometry", EditorStyles.boldLabel);
            float _BaseHeight = EditorGUILayout.FloatField("Base Height", t.GetFloat(P._BaseHeight));
            float _HeightFalloff = EditorGUILayout.Slider("Height Falloff", t.GetFloat(P._HeightFalloff), 0, 2);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Color", EditorStyles.boldLabel);
            Texture2D _ColorRamp = (Texture2D)EditorGUILayout.ObjectField("Color Ramp Texture", t.GetTexture(P._ColorRamp), typeof(Texture2D), true, GUILayout.Height(EditorGUIUtility.singleLineHeight));
            float _ColorRampInfluence = EditorGUILayout.Slider("Color Ramp Influence", t.GetFloat(P._ColorRampInfluence), 0, 1);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Volumetric Noise", EditorStyles.boldLabel);
            Texture3D _NoiseTexture = (Texture3D)EditorGUILayout.ObjectField("Noise Texture",  t.GetTexture(P._NoiseTexture), typeof(Texture3D), true, GUILayout.Height(EditorGUIUtility.singleLineHeight));
            float _NoiseTiling = EditorGUILayout.FloatField("Noise Tiling Domain (m^3)", t.GetFloat(P._NoiseTiling));
            Vector3 _NoiseWindSpeed = EditorGUILayout.Vector3Field("Noise Wind Speed (m/s)", t.GetVector(P._NoiseWindSpeed));
            float _NoiseIntensityMin = t.GetFloat(P._NoiseIntensityMin);
            float _NoiseIntensityMax = t.GetFloat(P._NoiseIntensityMax);
            EditorGUILayout.MinMaxSlider("Noise Intensity Min/Max", ref _NoiseIntensityMin, ref _NoiseIntensityMax, 0f, 5f);
            

            

            if (EditorGUI.EndChangeCheck())
            {
                _FogDensity = Mathf.Max(0, _FogDensity);
                _LightIntensity = Mathf.Max(0, _LightIntensity);
                _ShadowIntensity = Mathf.Max(0, _ShadowIntensity);


                t.SetInt(P._SampleCount, _SampleCount);
                t.SetInt(P._AnimateSamplePosition, BoolToInt(_AnimateSamplePosition));
                t.SetFloat(P._MaxDistanceVolumetric, _MaxDistanceVolumetric);
                t.SetFloat(P._MaxDistanceNonVolumetric, _MaxDistanceNonVolumetric);
                t.SetFloat(P._Anisotropy, _Anisotropy);
                t.SetFloat(P._BaseHeight, _BaseHeight);
                t.SetFloat(P._HeightFalloff, _HeightFalloff);
                t.SetFloat(P._FogDensity, _FogDensity);
                t.SetFloat(P._LightIntensity, _LightIntensity);
                t.SetFloat(P._ShadowIntensity, _ShadowIntensity);
                t.SetTexture(P._ColorRamp, _ColorRamp);
                t.SetFloat(P._ColorRampInfluence, _ColorRampInfluence);
                t.SetTexture(P._NoiseTexture, _NoiseTexture);
                t.SetFloat(P._NoiseTiling, _NoiseTiling);
                t.SetVector(P._NoiseWindSpeed, _NoiseWindSpeed);
                t.SetFloat(P._NoiseIntensityMin, _NoiseIntensityMin);
                t.SetFloat(P._NoiseIntensityMax, _NoiseIntensityMax);
            }
        }

        private bool IntToBool(int a)
        {
            return a == 0 ? false : true;
        }

        private int BoolToInt(bool a)
        {
            return a == false ? 0 : 1;
        }

        private static class P
        {
            public static int _SampleCount = Shader.PropertyToID("_SampleCount");
            public static int _AnimateSamplePosition = Shader.PropertyToID("_AnimateSamplePosition");
            public static int _MaxDistanceVolumetric = Shader.PropertyToID("_MaxDistanceVolumetric");
            public static int _MaxDistanceNonVolumetric = Shader.PropertyToID("_MaxDistanceNonVolumetric");
            public static int _Anisotropy = Shader.PropertyToID("_Anisotropy");
            public static int _BaseHeight = Shader.PropertyToID("_BaseHeight");
            public static int _HeightFalloff = Shader.PropertyToID("_HeightFalloff");
            public static int _FogDensity = Shader.PropertyToID("_FogDensity");
            public static int _LightIntensity = Shader.PropertyToID("_LightIntensity");
            public static int _ShadowIntensity = Shader.PropertyToID("_ShadowIntensity");
            public static int _ColorRamp = Shader.PropertyToID("_ColorRamp");
            public static int _ColorRampInfluence = Shader.PropertyToID("_ColorRampInfluence");
            public static int _NoiseTexture = Shader.PropertyToID("_NoiseTexture");
            public static int _NoiseTiling = Shader.PropertyToID("_NoiseTiling");
            public static int _NoiseWindSpeed = Shader.PropertyToID("_NoiseWindSpeed");
            public static int _NoiseIntensityMin = Shader.PropertyToID("_NoiseIntensityMin");
            public static int _NoiseIntensityMax = Shader.PropertyToID("_NoiseIntensityMax");
        }
    }
}
