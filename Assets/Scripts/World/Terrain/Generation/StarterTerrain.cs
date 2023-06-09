using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StarterTerrain : ScriptableObject, TerrainLayerGenerator
{
    private readonly ComputeShader shader = Resources.Load<ComputeShader>("Compute/Layers/StarterTerrain");

    [SerializeField]
    private float platformDensity;

    [SerializeField]
    public int numOfPlatforms;

    private bool layerInitialized = false;
    private ComputeBuffer platformBuffer;

    private void InitializeLayer(TerrainLayer layer) {
        Debug.Log("Initializing");
        CreatePlatformBuffer(layer);
        shader.SetBuffer(0, "_PlatformBuffer", platformBuffer);
        layerInitialized = true;
    }

    public void ReleaseBuffers() {
        platformBuffer.Release();
    }

    public void CreatePlatformBuffer(TerrainLayer layer) {
        platformBuffer = new ComputeBuffer(numOfPlatforms, 12);
        Bounds bounds = layer.GetBounds();
        bounds.Expand(-30f);

        Vector3[] platformPositions = MathUtils.RandomPointsInBounds(numOfPlatforms, bounds);

        platformBuffer.SetData(platformPositions);
    }

    public void Generate(ref RenderTexture target, TerrainChunk chunk) {
        if (!layerInitialized) InitializeLayer(chunk.layer);

        shader.SetTexture(0, "_Target", target);
        shader.SetVector("_ChunkOrigin", chunk.origin);
        shader.SetVector("_ChunkSize", chunk.GetBounds().size);

        shader.SetVector("_LayerOrigin", chunk.layer.origin);
        shader.SetVector("_LayerSize", chunk.layer.GetBounds().size);

        shader.SetFloat("_VoxelScale", chunk.handler.voxelScale);

        Vector3Int threads = RTUtils.CalculateThreadAmount(chunk.handler.textureDimensions, 8);
        shader.Dispatch(0, threads.x, threads.y, threads.z);
    }
}
