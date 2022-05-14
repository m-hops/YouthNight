Shader "Hidden/CustomPostProcess/VideoGlitch"
{

    SubShader
    {
        Pass
        {
            Cull Off ZWrite Off ZTest Always
            HLSLPROGRAM
            #pragma vertex FullScreenTrianglePostProcessVertexProgram
            #pragma fragment Fragment
            #include "VideoGlitch.hlsl"
            ENDHLSL
        }
        Pass
        {
            Cull Off ZWrite Off ZTest Always
            HLSLPROGRAM
            #define GLITCH_BASIC
            #pragma vertex FullScreenTrianglePostProcessVertexProgram
            #pragma fragment Fragment
            #include "VideoGlitch.hlsl"
            ENDHLSL
        }
        Pass
        {
            Cull Off ZWrite Off ZTest Always
            HLSLPROGRAM
            #define GLITCH_BLOCK
            #pragma vertex FullScreenTrianglePostProcessVertexProgram
            #pragma fragment Fragment
            #include "VideoGlitch.hlsl"
            ENDHLSL
        }
        Pass
        {
            Cull Off ZWrite Off ZTest Always
            HLSLPROGRAM
            #define GLITCH_BASIC
            #define GLITCH_BLOCK
            #pragma vertex FullScreenTrianglePostProcessVertexProgram
            #pragma fragment Fragment
            #include "VideoGlitch.hlsl"
            ENDHLSL
        }
    }
    Fallback Off
}
