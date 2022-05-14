//https://github.com/yahiaetman/URPCustomPostProcessingStack

Shader "Hidden/CustomPostProcess/Glitch"
{
    HLSLINCLUDE
    #include "../ShaderLibrary/Core.hlsl"
    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

    TEXTURE2D_X(_MainTex);

    float _Speed;
    float _BlockSize;
    float _MaxRGBSplitX;
    float _MaxRGBSplitY;

    float FRandom(uint seed)
    {
	    return GenerateHashedRandomFloat(seed);
    }

    float randomNoise(float2 seed)
    {
	    return frac(sin(dot(seed * floor(_Time.y * _Speed), float2(17.13, 3.71))) * 43758.5453123);
    }

    float randomNoise(float seed)
    {
	    return randomNoise(float2(seed, 1.0));
    }

    float3 SampleTexture(float2 uv)
    {
	    return LOAD_TEXTURE2D_X(_MainTex, uv).rgb;
    }

    float4 FragmentProgram (PostProcessVaryings input) : SV_Target
    {
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

        float2 uv = UnityStereoTransformScreenSpaceTex(input.texcoord);
        uint2 positionSS = uv * _ScreenSize.xy;

        half2 block = randomNoise(floor(positionSS * _BlockSize));

	    float displaceNoise = pow(block.x, 20);
	    float splitRGBNoise = pow(randomNoise(7.2341), 17.0);
	    float offsetX = displaceNoise - splitRGBNoise * _MaxRGBSplitX;
	    float offsetY = displaceNoise - splitRGBNoise * _MaxRGBSplitY;

	    float noiseX = 0.05 * randomNoise(13.0);
	    float noiseY = 0.05 * randomNoise(7.0);
	    float2 offset = float2(offsetX * noiseX, offsetY * noiseY);

	    half3 colorR = SampleTexture(positionSS);
	    half3 colorG = SampleTexture(positionSS + offset);
	    half3 colorB = SampleTexture(positionSS - offset);

	    return half4(colorR.r, colorG.g, colorB.b, 1);
    }
    ENDHLSL



    SubShader
    {
        Cull Off ZWrite Off ZTest Always
        Pass
        {
            HLSLPROGRAM
            #pragma vertex FullScreenTrianglePostProcessVertexProgram
            #pragma fragment FragmentProgram
            ENDHLSL
        }
    }
    Fallback Off
}
