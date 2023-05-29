using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainLayer : MonoBehaviour
{
    private ActiveState state;
    public TerrainLayerSettings settings;
    private List<TerrainChunk> chunks;

    public TerrainHandler handler;
    public int id;
    public Vector3 origin { get { return settings.origin; } }

    private Vector3Int chunkCount;

    public bool generating;
    public bool generated;

    public TerrainLayer Initialize(int _id, TerrainHandler _handler, TerrainLayerSettings _settings, ActiveState _state) {
        id = _id;
        handler = _handler;
        settings = _settings;
        state = _state;
        generated = false;
        generating = false;

        chunks = new List<TerrainChunk>();

        chunkCount = new Vector3Int(Mathf.CeilToInt(handler.generatedArea.x / handler.voxelsPerAxis.x / handler.voxelScale),
                                    Mathf.FloorToInt(settings.depth / handler.voxelsPerAxis.y / handler.voxelScale),
                                    Mathf.CeilToInt(handler.generatedArea.y / handler.voxelsPerAxis.z / handler.voxelScale));
        StartCoroutine(Generate());
        return this;
    }

    public void SetState(ActiveState state) {
        if (this.state == state) return;

        this.state = state;
        foreach(TerrainChunk chunk in chunks) {
            if (!chunk) continue;

            chunk.SetState(state);
        }
    }

    public void DistributeEditRequest(ChunkEditRequest request) {
        if (state == ActiveState.Inactive) return;

        foreach(TerrainChunk chunk in chunks) {
            if (chunk == null) continue;

            if (chunk.GetBounds().Intersects(request.GetBounds())) {
                chunk.MakeEditRequest(request.Clone());
            }
        }
    }

    //
    // Summery:
    //     Generates the layer with the given amount of chunks
    //   
    // Parameters:
    //   chunkCount:
    //     number of chunks per axis to generate
    //
    public IEnumerator Generate() {
        Vector3Int halfChunkCount = Vector3Int.FloorToInt((Vector3)chunkCount / 2f);
        for(int x = 0; x < chunkCount.x; x++) {
            for(int y = 0; y < chunkCount.y; y++) {
                for(int z = 0; z < chunkCount.z; z++) {
                    Vector3Int chunkID = new Vector3Int(-halfChunkCount.x + x,
                                                    y,
                                                    -halfChunkCount.z + z);

                    Vector3 position = origin + new Vector3(handler.voxelsPerAxis.x * chunkID.x,
                                                          -(handler.voxelsPerAxis.y * chunkID.y + (handler.voxelsPerAxis.y / 2f)), //To account for y position being at the start not the centre
                                                            handler.voxelsPerAxis.z * chunkID.z)
                                                            * handler.voxelScale;

                    GameObject chunkGameObject = CreateChunkGameObject(chunkID.x + "," + (-chunkID.y) + "," + chunkID.z, position);
                    TerrainChunk chunk = chunkGameObject.AddComponent<TerrainChunk>().Initialize(this, position, state);
                    chunks.Add(chunk);
                    yield return null;
                }
            }
        }
        generated = true;
        generating = false;
        SetState(state);
        TerrainHandler.OnLayerGenerated?.Invoke(id);
    }

    //
    // Summery:
    //     Handles unloading the entirety of this layer
    //     When in unloaded state, the layer will have to either regenerate from noise or load from a file
    //
    public void Unload() {
        for(int i = 0; i < chunks.Count; i++) {
            TerrainChunk chunk = chunks[i];
            chunk.Unload();
            chunks.Remove(chunk);
        }
        GameObject.Destroy(this);
    }

    //
    // Summery:
    //     Creates a game object with everything needed to act as a terrain chunk
    //
    // Parameters:
    //   name:
    //     name of the game object
    //   position:
    //     position of the game object
    //
    private GameObject CreateChunkGameObject(string name, Vector3 position) {
        GameObject result = new GameObject(name);
        result.transform.parent = transform;
        result.transform.position = position;

        result.AddComponent<MeshFilter>();
        result.AddComponent<MeshRenderer>().material = handler.material;
        result.AddComponent<MeshCollider>();
        return result;
    }

    public Bounds GetBounds() {
        Vector3 centre = new Vector3(origin.x,
                                     origin.y - (handler.voxelsPerAxis.y * chunkCount.y * handler.voxelScale / 2f),
                                     origin.z); ;

        Vector3 size = new Vector3(handler.voxelsPerAxis.x * chunkCount.x,
                                   handler.voxelsPerAxis.y * chunkCount.y,
                                   handler.voxelsPerAxis.z * chunkCount.z) * handler.voxelScale;

        return new Bounds(centre, size);
    }

}
