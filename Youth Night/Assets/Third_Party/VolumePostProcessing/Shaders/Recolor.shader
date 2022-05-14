//https://github.com/keijiro/Kino

Shader "Hidden/CustomPostProcess/Recolor"
{
    HLSLINCLUDE
    #include "../ShaderLibrary/Core.hlsl"
    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"

    TEXTURE2D_X(_MainTex);

    float _FillOpacity;

    float4 _ColorKey0;
    float4 _ColorKey1;
    float4 _ColorKey2;
    float4 _ColorKey3;
    float4 _ColorKey4;
    float4 _ColorKey5;
    float4 _ColorKey6;
    float4 _ColorKey7;

    TEXTURE2D(_DitherTexture);
    float _DitherStrength;

    float4 FragProgram (PostProcessVaryings input) : SV_Target
    {
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
        float2 uv = UnityStereoTransformScreenSpaceTex(input.texcoord);
        uint2 positionSS = uv * _ScreenSize.xy;

        // Source color
        float4 c0 = LOAD_TEXTURE2D_X(_MainTex, positionSS);

        // Four sample points of the roberts cross operator
        // TL / BR / TR / BL
        uint2 uv0 = positionSS;
        uint2 uv1 = min(positionSS + uint2(1, 1), _ScreenSize.xy - 1);
        uint2 uv2 = uint2(uv1.x, uv0.y);
        uint2 uv3 = uint2(uv0.x, uv1.y);

        // Dithering
        uint tw, th;
        _DitherTexture.GetDimensions(tw, th);
        float dither = LOAD_TEXTURE2D(_DitherTexture, positionSS % uint2(tw, th)).x;
        dither = (dither - 0.5) * _DitherStrength;

        // Apply fill gradient.
        float3 fill = _ColorKey0.rgb;
        float lum = Luminance(c0.rgb) + dither;

    #ifdef RECOLOR_GRADIENT_LERP
        fill = lerp(fill, _ColorKey1.rgb, saturate((lum - _ColorKey0.w) / (_ColorKey1.w - _ColorKey0.w)));
        fill = lerp(fill, _ColorKey2.rgb, saturate((lum - _ColorKey1.w) / (_ColorKey2.w - _ColorKey1.w)));
        fill = lerp(fill, _ColorKey3.rgb, saturate((lum - _ColorKey2.w) / (_ColorKey3.w - _ColorKey2.w)));
        #ifdef RECOLOR_GRADIENT_EXT
        fill = lerp(fill, _ColorKey4.rgb, saturate((lum - _ColorKey3.w) / (_ColorKey4.w - _ColorKey3.w)));
        fill = lerp(fill, _ColorKey5.rgb, saturate((lum - _ColorKey4.w) / (_ColorKey5.w - _ColorKey4.w)));
        fill = lerp(fill, _ColorKey6.rgb, saturate((lum - _ColorKey5.w) / (_ColorKey6.w - _ColorKey5.w)));
        fill = lerp(fill, _ColorKey7.rgb, saturate((lum - _ColorKey6.w) / (_ColorKey7.w - _ColorKey6.w)));
        #endif
    #else
        fill = lum > _ColorKey0.w ? _ColorKey1.rgb : fill;
        fill = lum > _ColorKey1.w ? _ColorKey2.rgb : fill;
        fill = lum > _ColorKey2.w ? _ColorKey3.rgb : fill;
        #ifdef RECOLOR_GRADIENT_EXT
        fill = lum > _ColorKey3.w ? _ColorKey4.rgb : fill;
        fill = lum > _ColorKey4.w ? _ColorKey5.rgb : fill;
        fill = lum > _ColorKey5.w ? _ColorKey6.rgb : fill;
        fill = lum > _ColorKey6.w ? _ColorKey7.rgb : fill;
        #endif
    #endif

        float3 cb = lerp(c0.rgb, fill, _FillOpacity);
        return float4(cb, c0.a);
    }

    ENDHLSL
    SubShader
    {
        Cull Off ZWrite Off ZTest Always

        // 4 keys, fixed gradient
        Pass
        {
            HLSLPROGRAM
            #pragma vertex FullScreenTrianglePostProcessVertexProgram
            #pragma fragment FragProgram
            ENDHLSL
        }

        // 8 keys, fixed gradient
        Pass
        {
            HLSLPROGRAM
            #pragma vertex FullScreenTrianglePostProcessVertexProgram
            #pragma fragment FragProgram
            #define RECOLOR_GRADIENT_EXT
            ENDHLSL
        }

        // 4 keys, blend gradient
        Pass
        {
            HLSLPROGRAM
            #pragma vertex FullScreenTrianglePostProcessVertexProgram
            #pragma fragment FragProgram
            #define RECOLOR_GRADIENT_LERP
            ENDHLSL
        }


        // 8 keys, blend gradient
        Pass
        {
            HLSLPROGRAM
            #pragma vertex FullScreenTrianglePostProcessVertexProgram
            #pragma fragment FragProgram
            #define RECOLOR_GRADIENT_EXT
            #define RECOLOR_GRADIENT_LERP
            ENDHLSL
        }

    }
    Fallback Off
}
