    #include "../ShaderLibrary/Core.hlsl"
    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"

    TEXTURE2D_X(_MainTex);

uint _Seed;

float _BlockStrength;
uint _BlockStride;
uint _BlockSeed1;
uint _BlockSeed2;

float2 _Drift;
float2 _Jitter;
float2 _Jump;
float _Shake;

float FRandom(uint seed)
{
    return GenerateHashedRandomFloat(seed);
}

float4 Fragment(PostProcessVaryings input) : SV_Target
{
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
    float2 uv = UnityStereoTransformScreenSpaceTex(input.texcoord);
    uint2 positionSS = uv * _ScreenSize.xy;

#if defined(GLITCH_BLOCK)

        //
        // Block glitch
        //

        uint block_size = 32;
        uint columns = _ScreenSize.x / block_size;

        // Block index
        uint2 block_xy = input.texcoord * _ScreenSize.xy / block_size;
        uint block = block_xy.y * columns + block_xy.x;

        // Segment index
        uint segment = block / _BlockStride;

        // Per-block random number
        float r1 = FRandom(block     + _BlockSeed1);
        float r3 = FRandom(block / 3 + _BlockSeed2);
        uint seed = (r1 + r3) < 1 ? _BlockSeed1 : _BlockSeed2;
        float rand = FRandom(segment + seed);

        // Block damage (offsetting)
        block += rand * 20000 * (rand < _BlockStrength);

        // Screen space position reconstruction
        uint2 ssp = uint2(block % columns, block / columns) * block_size;
        ssp += (uint2)(input.texcoord * _ScreenSize.xy) % block_size;

        // UV recalculation
        uv = frac((ssp + 0.5) / _ScreenSize.xy);

#endif

#if defined(GLITCH_BASIC)

        //
        // Basic glitch effects
        //

        // Texture space position
        float tx = uv.x;
        float ty = uv.y;

        // Jump
        ty = lerp(ty, frac(ty + _Jump.x), _Jump.y);

        // Screen space Y coordinate
        uint sy = ty * _ScreenSize.y;

        // Jitter
        float jitter = Hash(sy + _Seed) * 2 - 1;
        tx += jitter * (_Jitter.x < abs(jitter)) * _Jitter.y;

        // Shake
        tx = frac(tx + (Hash(_Seed) - 0.5) * _Shake);

        // Drift
        float drift = sin(ty * 2 + _Drift.x) * _Drift.y;

        // Source sample
        uint sx1 = (tx        ) * _ScreenSize.x;
        uint sx2 = (tx + drift) * _ScreenSize.x;
        float4 c1 = LOAD_TEXTURE2D_X(_MainTex, uint2(sx1, sy));
        float4 c2 = LOAD_TEXTURE2D_X(_MainTex, uint2(sx2, sy));
        float4 c = float4(c1.r, c2.g, c1.b, c1.a);

#else

    float4 c = LOAD_TEXTURE2D_X(_MainTex, uv * _ScreenSize.xy);

#endif

#if defined(GLITCH_BLOCK)

        // Block damage (color mixing)
        if (frac(rand * 1234) < _BlockStrength * 0.1)
        {
            float3 hsv = RgbToHsv(c.rgb);
            hsv = hsv * float3(-1, 1, 0) + float3(0.5, 0, 0.9);
            c.rgb = HsvToRgb(hsv);
        }

#endif

    return c;
}

