using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StarterTerrain : ScriptableObject, TerrainLayerGenerator
{
    private readonly ComputeShader shader = Resources.Load<ComputeShader>("Compute/Layers/StarterTerrain");

    [SerializeField]
    private float platformDensity;

    public void Generate(ref RenderTexture target, TerrainChunk chunk) {
        shader.SetTexture(0, "_Target", target);

        Vector3Int threads = RTUtils.CalculateThreadAmount(chunk.handler.textureDimensions, 8);
        shader.Dispatch(0, threads.x, threads.y, threads.z);
    }
}
