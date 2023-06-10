using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

[CreateAssetMenu(menuName = "Terrain/Starter")]
public class StarterTerrain : TerrainLayerGenerator
{
    private ComputeShader shader;

    public float platformDensity;
    public Vector2 platformRadiusRange;
    public int numOfPlatforms;
    public Vector2 depthRange;

    public Vector2 chasmRadiusRange;

    public NoiseArgs[] noiseArgs;

    private float depth = -1;

    private bool layerInitialized = false;
    private ComputeBuffer platformBuffer;
    private ComputeBuffer noiseArgsBuffer;

    private void OnEnable() {
        shader = Resources.Load<ComputeShader>("Compute/Layers/StarterTerrain");
    }

    private void InitializeLayer(TerrainLayer layer, int seed) {
        Debug.Log("Initializing Starter Terrain Generator");
        Random.InitState(seed);

        CreatePlatformBuffer(layer);
        CreateNoiseArgsBuffer();
        shader.SetBuffer(0, "_PlatformBuffer", platformBuffer);
        shader.SetBuffer(0, "_NoiseArgs", noiseArgsBuffer);
        shader.SetVector("_PlatformRadiusRange", platformRadiusRange);

        shader.SetVector("_LayerOrigin", layer.origin);
        shader.SetVector("_LayerSize", layer.GetBounds().size);

        shader.SetFloat("_ChasmRadius", Random.Range(chasmRadiusRange.x, chasmRadiusRange.y));

        shader.SetFloat("_VoxelScale", layer.handler.voxelScale);
        layerInitialized = true;
    }

    public override void ReleaseBuffers() {
        platformBuffer.Release();
        noiseArgsBuffer.Release();
    }

    public void CreatePlatformBuffer(TerrainLayer layer) {
        platformBuffer = new ComputeBuffer(numOfPlatforms, 12);
        Bounds bounds = layer.GetBounds();
        bounds.Expand(-platformRadiusRange.y * 1.25f);

        Vector3[] platformPositions = MathUtils.RandomPointsInBounds(numOfPlatforms, bounds);

        platformBuffer.SetData(platformPositions);
    }

    public void CreateNoiseArgsBuffer() {
        noiseArgsBuffer = new ComputeBuffer(noiseArgs.Length, NoiseArgs.stride);
        noiseArgsBuffer.SetData(noiseArgs);
    }

    public override void Generate(ref RenderTexture target, TerrainChunk chunk, int seed) {
        if (!layerInitialized) InitializeLayer(chunk.layer, seed);

        shader.SetTexture(0, "_Target", target);
        shader.SetFloat("_Seed", seed);
        shader.SetVector("_ChunkOrigin", chunk.origin);
        shader.SetVector("_ChunkSize", chunk.GetBounds().size);

        Vector3Int threads = RTUtils.CalculateThreadAmount(chunk.handler.textureDimensions, 8);
        shader.Dispatch(0, threads.x, threads.y, threads.z);
    }

    public override float GetDepth(float chunkHeight) {
        if (depth == -1.0) {
            depth = Random.Range(depthRange.x, depthRange.y);
            depth = Mathf.Round(depth / chunkHeight) * chunkHeight;
        }

        return depth;
    }
}

