// https://github.com/keijiro/KinoBinary

Shader "Hidden/CustomPostProcess/Binary"
{
    HLSLINCLUDE
    #include "../ShaderLibrary/Core.hlsl"
    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"

    TEXTURE2D_X(_MainTex);
   
    half3 _Color0;
    half3 _Color1;
    half _Opacity;
    float _Lighterness;

    TEXTURE2D(_DitherTexture);

    float4 FragProgram (PostProcessVaryings input) : SV_Target
    {
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
        float2 uv = UnityStereoTransformScreenSpaceTex(input.texcoord);
        uint2 positionSS = uv * _ScreenSize.xy;

        float4 source = LOAD_TEXTURE2D_X(_MainTex, positionSS);

         // Dithering
        uint tw, th;
        _DitherTexture.GetDimensions(tw, th);
        float dither = LOAD_TEXTURE2D(_DitherTexture, positionSS % uint2(tw, th)).x;
        dither = (dither + _Lighterness) * _Opacity;

         // Relative luminance in linear RGB space
        half rlum = Luminance(source.rgb);

        // Blending
        half3 rgb = rlum < dither ? _Color0 : _Color1;
        return half4(lerp(source.rgb, rgb, _Opacity), source.a);
    }
    ENDHLSL

    SubShader
    {
        Cull Off ZWrite Off ZTest Always
        Pass
        {
            HLSLPROGRAM
            #pragma vertex FullScreenTrianglePostProcessVertexProgram
            #pragma fragment FragProgram
            ENDHLSL
        }
    }
}