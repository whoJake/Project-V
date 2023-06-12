using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Has to be abstract class instead of interface so that it actually shows up in the editor as a scriptable object
public abstract class TerrainLayerGenerator : ScriptableObject {
    public abstract void Generate(ref RenderTexture target, TerrainChunk chunk, int seed);
    public abstract float GetDepth(float chunkHeight);
    public abstract void ReleaseBuffers();
}

[System.Serializable]
public struct NoiseArgs {
    public Vector3 scale;
    public int octaves;
    public float frequency;
    public float persistance;
    public float lacunarity;

    public static int stride = sizeof(int) + sizeof(float) * 6;
}