Shader "Hidden/CustomPostProcess/StaticAnimeLines"
{
    HLSLINCLUDE
    #include "../ShaderLibrary/Core.hlsl"

    TEXTURE2D_X(_MainTex);

    float _CenterX;
    float _CenterY;
    float _Central;
    float _Line;
    float _CentralEdge;
    float _CentralLength;
    float _Opacity;

    float2 dir(float2 p)
    {
        p = p % 289;
        float x = frac((34 * ((34 * p.x + 1) * p.x % 289 + p.y) + 1) * ((34 * p.x + 1) * p.x % 289 + p.y) % 289 / 41) * 2 - 1;
        return normalize(float2(x - floor(x + 0.5), abs(x) - 0.5));
    }
    
    float gradientNoise(float2 p)
    {
        p *= 40;
        float2 ip = floor(p);
        float2 fp = frac(p);
        float d00 = dot(dir(ip), fp);
        float d01 = dot(dir(ip + float2(0, 1)), fp - float2(0, 1));
        float d10 = dot(dir(ip + float2(1, 0)), fp - float2(1, 0));
        float d11 = dot(dir(ip + float2(1, 1)), fp - float2(1, 1));
        fp = fp * fp * fp * (fp * (fp * 6 - 15) + 10);
        return lerp(lerp(d00, d01, fp.y), lerp(d10, d11, fp.y), fp.x);
    }
    
    float4 AnimeLines (PostProcessVaryings input) : SV_Target
    {
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
        float2 uv = UnityStereoTransformScreenSpaceTex(input.texcoord);
        uint2 positionSS = uv * _ScreenSize.xy;

         float4 color = LOAD_TEXTURE2D_X(_MainTex, positionSS);

        float2 polarCoordinates = float2(length(uv - float2(_CenterX, _CenterY)) * 2 * _Central, atan2(uv.x - _CenterX, uv.y - _CenterY) * 1.0/6.28 * _Line);
        float step = smoothstep(_CentralEdge, 0.86, (gradientNoise(polarCoordinates.y ) + 0.5)  + ((polarCoordinates.x - 0.1) * 0.9 / (_CentralLength - 0.1) * -1));

        if(step != 0) {
            color.a = 0;
        } 
        else {
            color.rgb = color.rgb * (1 - _Opacity);
        }
        return color;
    }

    ENDHLSL

    SubShader
    {
        Cull Off ZWrite Off ZTest Always
        

        Pass
        {
            HLSLPROGRAM
            #pragma vertex FullScreenTrianglePostProcessVertexProgram
            #pragma fragment AnimeLines
            ENDHLSL
        }
    }
    Fallback Off
}