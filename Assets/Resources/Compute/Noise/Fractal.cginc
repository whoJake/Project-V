#include "/Simplex.cginc"

/**
** Returns value between +/- fnoise_max(persistance, octaves)
**/
float fnoise(float3 pos, int octaves, float _frequency, float persistance, float lacunarity){
	if (octaves == 0) return 0;
	float amplitude = 1;
	float frequency = _frequency;

	float value = 0;

	for(int i = 0; i < octaves; i++){
		float noise = snoise(pos * frequency);
		value += noise * amplitude;

		amplitude *= persistance;
		frequency *= lacunarity;
	}

	return value;
}

float fnoise_max(float persistance, int octaves){
	float max = 0;
	float amp = 1;
	for(int i = 0; i < octaves; i++){
		max += amp;
		amp *= persistance;
	}
	return max;
}

float fnoise01(float3 pos, int octaves, float freq, float persistance, float lacunarity) {
	if (octaves == 0) return 0.5;
	float raw = fnoise(pos, octaves, freq, persistance, lacunarity);
	float rang = fnoise_max(persistance, octaves);
	return ((raw / rang) + 1) / 2;
}

float3 warp_coords(float3 pos, int octaves, float frequency, float persistance, float lacunarity, float strength){
    //Arbitruary offsets
    float3 dOffset1 = float3(1.9, 0.9, 5.2);
    float3 dOffset2 = float3(6.1, -2.8, 3.9);
    float3 dOffset3 = float3(-3.8, -5.1, 6.7);

    float wx = fnoise(pos + dOffset1, octaves, frequency, persistance, lacunarity);
    float wy = fnoise(pos + dOffset2, octaves, frequency, persistance, lacunarity);
    float wz = fnoise(pos + dOffset2, octaves, frequency, persistance, lacunarity);

    return (pos + (float3(wx, wy, wz) * strength));
}