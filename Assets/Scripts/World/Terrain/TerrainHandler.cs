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
        GenerateLayer();
    }

    void GenerateLayer() {
        Vector3Int halfNumChunks = Vector3Int.FloorToInt((Vector3)numChunks / 2f);

        for (int x = 0; x < numChunks.x; x++) {
            for(int y = 0; y < numChunks.y; y++) {
                for(int z = 0; z < numChunks.z; z++) {
                    Vector3Int id = -halfNumChunks + new Vector3Int(x, y, z);
                    Vector3 position = new Vector3((chunkSize.x - 1) * id.x, (chunkSize.y - 1) * id.y, (chunkSize.z - 1) * id.z) * voxelScale;
                    GameObject go = InstantiateChunk(position, id, 0);
                    TerrainChunk chunk = new TerrainChunk(0, position, chunkSize, margin, voxelScale, go);
                    chunk.Generate();
                }
            }
        }
    }

    GameObject InstantiateChunk(Vector3 position, Vector3Int id, int layer) {
        GameObject result = new GameObject("L" + layer + " C" + id.x + "," + id.y + "," + id.z);
        result.transform.parent = transform;
        result.transform.position = position;
        result.AddComponent<MeshFilter>();
        result.AddComponent<MeshRenderer>().sharedMaterial = material;
        result.AddComponent<MeshCollider>();
        //ADD MATERIAL HERE
        return result;
    }
}
