#ifndef __NOISE_ARGS__
#define __NOISE_ARGS__

struct NoiseArg{
    float3 scale;
    int octaves;
    float frequency;
    float persistance;
    float lacunarity;
};

StructuredBuffer<NoiseArg> _NoiseArgs;

#endif
