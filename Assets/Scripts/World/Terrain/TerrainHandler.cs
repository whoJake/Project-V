using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainHandler : MonoBehaviour
{
    public TerrainSettings settings;

    public Vector3Int numChunks;
    public Vector3Int chunkSize;
    public int margin;
    public float voxelScale;
    public Material material;

    void Start()
    {
        TerrainChunk.InitializeCompute(settings);
        TerrainLayer layer = new TerrainLayer(0, gameObject, Vector3.zero, this);
        layer.Generate(numChunks);
    }
}
