//https://github.com/keijiro/Kino

Shader "Hidden/CustomPostProcess/Slice"
{
    HLSLINCLUDE
    #include "../ShaderLibrary/Core.hlsl"

    TEXTURE2D_X(_MainTex);

    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
    //#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderVariablesFunctions.hlsl"

    float2 _Direction;
    float _Displacement;
    float _Rows;
    uint _Seed;

    float4 FragProgram (PostProcessVaryings input) : SV_Target
    {
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
        float2 uv = UnityStereoTransformScreenSpaceTex(input.texcoord);
         uv = uv * _ScreenSize.xy;

        const float aspect = (float)_ScreenSize.x / _ScreenSize.y;
        const float inv_aspect = (float)_ScreenSize.y / _ScreenSize.x;

        const float2 axis1 = _Direction;
        const float2 axis2 = float2(-axis1.y, axis1.x);

        //float2 uv = input.texcoord;
        float param = dot(uv - 0.5, axis2 * float2(aspect, 1));
        uint seed = _Seed + (uint)((param + 10) * _Rows + 0.5);
        float delta = Hash(seed) - 0.5;

        uv += axis1 * delta * _Displacement * float2(inv_aspect, 1);

        //uv = ClampAndScaleUVForBilinear(uv);
        return LOAD_TEXTURE2D_X(_MainTex, uv);
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
