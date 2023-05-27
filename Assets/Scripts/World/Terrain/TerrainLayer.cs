using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class TerrainLayer
{
    public ActiveState state;
    private readonly List<TerrainChunk> chunks;

    private readonly TerrainHandler handler;
    public readonly int id;
    private readonly Vector3 origin; //Layer origin is not the -/- corner but instead the top face centre of the layer
    private readonly GameObject targetGObj;

    private Vector3Int chunkCount;

    private bool isGenerated = false;
    public bool IsGenerated { get { return isGenerated; } }

    public TerrainLayer(TerrainHandler _handler, int _id, Vector3 _origin, GameObject _targetGObj) {
        handler = _handler;
        id = _id;
        origin = _origin;
        targetGObj = _targetGObj;

        chunks = new List<TerrainChunk>();
    }

    public void Update() {
        foreach(TerrainChunk chunk in chunks) {
            chunk.state = state;
        }

        bool gObjActive;
        switch (state) {
            case ActiveState.Inactive:
                gObjActive = false;
                break;

            case ActiveState.Static:
                gObjActive = true;
                break;

            case ActiveState.Active:
                gObjActive = true;

                foreach(TerrainChunk chunk in chunks) {
                    chunk?.Update();
                }
                break;

            default:
                gObjActive = false;
                Debug.Log("Layer activeState isn't set");
                break;
        }

        targetGObj.SetActive(gObjActive);
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
    public IEnumerator Generate(float depth, System.Action<int> callback) {
        //I hate having to do this calculation here but I like the idea of putting in the depth in this function :)
        chunkCount = new Vector3Int(Mathf.CeilToInt(handler.generatedArea.x / handler.voxelsPerAxis.x / handler.voxelScale),
                                    Mathf.FloorToInt(depth / handler.voxelsPerAxis.y / handler.voxelScale),
                                    Mathf.CeilToInt(handler.generatedArea.y / handler.voxelsPerAxis.z / handler.voxelScale));

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
                    TerrainChunk chunk = new TerrainChunk(this, position, handler.chunkSize, handler.margin, handler.voxelScale, chunkGameObject);

                    chunk.Generate(handler.activeSettings);
                    chunk.state = state;
                    chunks.Add(chunk);
                    yield return null;
                }
            }
        }
        isGenerated = true;
        callback?.Invoke(id);
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
        GameObject.Destroy(targetGObj);
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
        result.transform.parent = targetGObj.transform;
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
