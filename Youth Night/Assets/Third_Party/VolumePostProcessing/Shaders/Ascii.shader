//https://github.com/keijiro/AsciiArtFx

Shader "Hidden/CustomPostProcess/Ascii"
{
    HLSLINCLUDE
    #include "../ShaderLibrary/Core.hlsl"

    TEXTURE2D_X(_MainTex);

    float4 _Color;
    float _Alpha;
    float _Spacing;

    float character(float n, float2 p) 
    {
    #ifdef UNITY_HALF_TEXEL_OFFSET
        float2 offs = float2(2.5f, 2.5f);
    #else
        float2 offs = float2(2, 2);
    #endif
        p = floor(p * float2(4, -4) + offs);
        if (clamp(p.x, 0, 4) == p.x && clamp(p.y, 0, 4) == p.y)
        {
            float c = fmod(n / exp2(p.x + 5 * p.y), 2);
            if (int(c) == 1) return 1;
        }   
        return 0;
    }

    float4 FragProgram (PostProcessVaryings input) : SV_Target
    {
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
        float2 uv = UnityStereoTransformScreenSpaceTex(input.texcoord);
        uint2 positionSS = uv * _ScreenSize.xy;
        
        float2 texelSize = (1 /  _ScreenSize.xy);
        float2 uv1 = input.texcoord.xy / texelSize;

         float4 c = LOAD_TEXTURE2D_X(_MainTex, floor(uv1/8) * 8);

        float gray = (c.r + c.g + c.b) / 3;

        float n =  65536;              // .

        if (gray > 0.2f) n = 65600;    // :
        if (gray > 0.3f) n = 332772;   // *
        if (gray > 0.4f) n = 15255086; // o
        if (gray > 0.5f) n = 23385164; // &
        if (gray > 0.6f) n = 15252014; // 8
        if (gray > 0.7f) n = 13199452; // @
        if (gray > 0.8f) n = 11512810; // #

        float2 p = fmod(uv1 / 4, _Spacing) -1;
        c *= character(n, p);

        float4 src = LOAD_TEXTURE2D_X(_MainTex, positionSS);

        return lerp(src, float4(c.rgb * _Color.rgb, _Color.a), _Alpha);
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
    Fallback Off
}