using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Terrain/Destructable Zone", order = 3)]
public class DestructableZoneTerrain : TerrainLayerGenerator {
    private ComputeShader shader;

    [SerializeField]
    private float desiredDepth = 64f;

    [SerializeField]
    private Wall[] walls;
    private ComputeBuffer wallsBuffer;

    private bool layerInitialized = false;

    private void OnEnable() {
        //shader = Resources.Load<ComputeShader>("Compute/Layers/DestructableZoneTerrain");
    }

    public override void Reset() {
        wallsBuffer?.Release();
        shader = null;
        layerInitialized = false;
    }

    private void InitializeLayer(TerrainLayer layer) {
        shader = Resources.Load<ComputeShader>("Compute/Layers/DestructableZoneTerrain");

        wallsBuffer = new ComputeBuffer(walls.Length, Wall.stride);
        Wall[] wallArray = new Wall[walls.Length];

        /*
        for (int i = 0; i < walls.Length; i++) {
            wallArray[i] = new Wall {
                position = walls[i].position,
                bounds = walls[i].localScale
            };
            walls[i].gameObject.SetActive(false);
        }
        */
        wallsBuffer.SetData(walls);
        
        shader.SetBuffer(0, "_WallBuffer", wallsBuffer);
        shader.SetVector("_LayerOrigin", layer.oldOrigin);
        shader.SetVector("_LayerSize", layer.bounds.size);
        shader.SetFloat("_VoxelScale", layer.handler.voxelScale);
        layerInitialized = true;
    }

    public override void Generate(ref RenderTexture target, TerrainChunk chunk, int seed) {
        if (!layerInitialized) InitializeLayer(chunk.layer);

        shader.SetTexture(0, "_Target", target);
        shader.SetVector("_ChunkOrigin", chunk.origin);
        shader.SetVector("_ChunkSize", chunk.bounds.size);

        Vector3Int threads = RTUtils.CalculateThreadAmount(chunk.handler.textureDimensions, 8);
        shader.Dispatch(0, threads.x, threads.y, threads.z);
    }

    public override void SetDepth(float voxelsPerY, float voxelScale) {
        float chunkCount = Mathf.FloorToInt(desiredDepth / voxelsPerY / voxelScale);
        depth = chunkCount * voxelsPerY * voxelScale;
    }

    public override void ReleaseBuffers() {
        wallsBuffer?.Release();
    }
}

[System.Serializable]
public struct Wall {
    public static int stride = sizeof(float) * 6;
    public Vector3 position;
    public Vector3 bounds;
}
