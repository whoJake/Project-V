using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainHandler : MonoBehaviour
{
    public TerrainSettings settings;

    void Start()
    {
        TerrainChunk chunk = new TerrainChunk(0, Vector3.zero, Vector3Int.one * 32, 0, 1, gameObject);
        TerrainChunk.InitializeCompute(settings);
        chunk.Generate();
    }
}
