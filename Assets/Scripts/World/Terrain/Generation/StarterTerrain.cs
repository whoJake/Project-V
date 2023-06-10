using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Terrain/Starter")]
public class StarterTerrain : TerrainLayerGenerator
{
    private ComputeShader shader;

    public float platformDensity;
    public Vector2 platformRadiusRange;
    public int numOfPlatforms;
    public Vector2 depthRange;

    private float depth = -1;

    private bool layerInitialized = false;
    private ComputeBuffer platformBuffer;

    private void OnEnable() {
        shader = Resources.Load<ComputeShader>("Compute/Layers/StarterTerrain");
    }

    private void InitializeLayer(TerrainLayer layer, int seed) {
        Debug.Log("Initializing Starter Terrain Generator");

        CreatePlatformBuffer(layer, seed);
        shader.SetBuffer(0, "_PlatformBuffer", platformBuffer);
        shader.SetVector("_PlatformRadiusRange", platformRadiusRange);

        shader.SetVector("_LayerOrigin", layer.origin);
        shader.SetVector("_LayerSize", layer.GetBounds().size);

        shader.SetFloat("_VoxelScale", layer.handler.voxelScale);
        layerInitialized = true;
    }

    public override void ReleaseBuffers() {
        platformBuffer.Release();
    }

    public void CreatePlatformBuffer(TerrainLayer layer, int seed) {
        platformBuffer = new ComputeBuffer(numOfPlatforms, 12);
        Bounds bounds = layer.GetBounds();
        bounds.Expand(-100f);

        Vector3[] platformPositions = MathUtils.RandomPointsInBounds(numOfPlatforms, bounds);

        platformBuffer.SetData(platformPositions);
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
