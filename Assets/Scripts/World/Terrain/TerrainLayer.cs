using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainLayer : MonoBehaviour
{
    private ActiveState state;
    private List<TerrainChunk> chunks;

    public TerrainLayerGenerator generator;

    public TerrainHandler handler;
    public int id;
    public Vector3 origin;

    private Vector3Int chunkCount;

    public bool generating;
    public bool generated;

    public TerrainLayer Initialize(int _id, Vector3 _origin, TerrainHandler _handler, TerrainLayerGenerator _generator, ActiveState _state) {
        id = _id;
        origin = _origin;
        handler = _handler;
        generator = _generator;
        state = _state;
        generated = false;
        generating = false;

        chunks = new List<TerrainChunk>();

        float chunkHeight = handler.voxelsPerAxis.y * handler.voxelScale;
        chunkCount = new Vector3Int(Mathf.CeilToInt(handler.generatedArea.x / handler.voxelsPerAxis.x / handler.voxelScale),
                                    Mathf.FloorToInt(generator.GetDepth(chunkHeight) / handler.voxelsPerAxis.y / handler.voxelScale),
                                    Mathf.CeilToInt(handler.generatedArea.y / handler.voxelsPerAxis.z / handler.voxelScale));

        StartCoroutine(Generate());
        return this;
    }

    public void SetState(ActiveState state) {
        if (this.state == state) return;

        this.state = state;
        StartCoroutine(SetChunkStatePerFrame(state));
    }

    //Makes sure that only one chunk is updated per frame
    //Had to do this as updating like 150 meshes to be rendered in one frame caused huge lag because of PhysX rebaking of collision meshes so spreading them out helps this a bit
    private IEnumerator SetChunkStatePerFrame(ActiveState state) {
        for(int i = 0; i < chunks.Count; i++) {
            chunks[i].SetState(state);
            yield return null;
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
        Vector3Int halfChunkCount = Vector3Int.CeilToInt((Vector3)chunkCount / 2f);

        //Bit of a hacky way to fix centering issues when chunkCount doesnt allow for an exact centre chunk
        Vector3 originOffset = Vector3.zero;
        if (new Vector2(halfChunkCount.x, halfChunkCount.z) * 2 == new Vector2(chunkCount.x, chunkCount.z))
            originOffset = new Vector3(handler.voxelsPerAxis.x * handler.voxelScale / 2f, 0, handler.voxelsPerAxis.z * handler.voxelScale / 2f);

        for(int x = 0; x < chunkCount.x; x++) {
            for(int y = 0; y < chunkCount.y; y++) {
                for(int z = 0; z < chunkCount.z; z++) {
                    Vector3Int chunkID = new Vector3Int(-halfChunkCount.x + x,
                                                    y,
                                                    -halfChunkCount.z + z);

                    Vector3 position = origin + new Vector3(handler.voxelsPerAxis.x * chunkID.x,
                                                          -(handler.voxelsPerAxis.y * chunkID.y + (handler.voxelsPerAxis.y / 2f)), //To account for y position being at the start not the centre
                                                            handler.voxelsPerAxis.z * chunkID.z)
                                                            * handler.voxelScale
                                                            + originOffset;

                    GameObject chunkGameObject = CreateChunkGameObject(chunkID.x + "," + (-chunkID.y) + "," + chunkID.z, position);
                    TerrainChunk chunk = chunkGameObject.AddComponent<TerrainChunk>().Initialize(this, position, state);
                    chunks.Add(chunk);
                    yield return null;
                }
            }
        }
        generated = true;
        generating = false;
        generator.ReleaseBuffers();
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
