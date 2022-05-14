////https://github.com/keijiro/Kino

Shader "Hidden/CustomPostProcess/Utility"
{
    HLSLINCLUDE
    #include "../ShaderLibrary/Core.hlsl"

    TEXTURE2D_X(_MainTex);

    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"

    float4 _FadeColor;
    float _HueShift;
    float _Invert;
    float _Saturation;

     float4 FragProgram (PostProcessVaryings input) : SV_Target
    {
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

        float2 uv = UnityStereoTransformScreenSpaceTex(input.texcoord);
        int2 positionSS = uv * _ScreenSize.xy;

        float4 c = LOAD_TEXTURE2D_X(_MainTex, positionSS);
        float3 rgb = c.rgb;

        // Saturation
        rgb = max(0, lerp(Luminance(rgb), rgb, _Saturation));

        // Linear -> sRGB
        rgb = LinearToSRGB(rgb);

        // Hue shift
        float3 hsv = RgbToHsv(rgb);
        hsv.x = frac(hsv.x + _HueShift);
        rgb = HsvToRgb(hsv);

        // Invert
        rgb = lerp(rgb, 1 - rgb, _Invert);

        // Fade
        rgb = lerp(rgb, _FadeColor.rgb, _FadeColor.a);

        // sRGB -> Linear
        c.rgb = SRGBToLinear(rgb);

        return c;
    }

    ENDHLSL

    SubShader
    {
        Pass
        {
            Cull Off ZWrite Off ZTest Always
            HLSLPROGRAM
            #pragma vertex FullScreenTrianglePostProcessVertexProgram
            #pragma fragment FragProgram
            ENDHLSL
        }
    }
    Fallback Off
}
