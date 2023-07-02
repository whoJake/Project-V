using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Terrain/Destructable Zone", order = 3)]
public class DestructableZoneTerrain : TerrainLayerGenerator {
    private ComputeShader shader;

    [SerializeField]
    private float depth = 64f;

    [SerializeField]
    private Wall[] walls;
    private ComputeBuffer wallsBuffer;

    private static bool layerInitialized = false;

    private void OnEnable() {
        shader = Resources.Load<ComputeShader>("Compute/Layers/DestructableZoneTerrain");
    }

    private void InitializeLayer(TerrainLayer layer) {
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
        shader.SetVector("_LayerOrigin", layer.origin);
        shader.SetVector("_LayerSize", layer.GetBounds().size);
        shader.SetFloat("_VoxelScale", layer.handler.voxelScale);
        layerInitialized = true;
    }

    public override void Generate(ref RenderTexture target, TerrainChunk chunk, int seed) {
        if (!layerInitialized) InitializeLayer(chunk.layer);

        shader.SetTexture(0, "_Target", target);
        shader.SetVector("_ChunkOrigin", chunk.origin);
        shader.SetVector("_ChunkSize", chunk.GetBounds().size);

        Vector3Int threads = RTUtils.CalculateThreadAmount(chunk.handler.textureDimensions, 8);
        shader.Dispatch(0, threads.x, threads.y, threads.z);
    }

    public override float GetDepth(float chunkHeight) {
        depth = Mathf.Round(depth / chunkHeight) * chunkHeight;

        return depth;
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
