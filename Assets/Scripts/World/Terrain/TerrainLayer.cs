using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class TerrainLayer : MonoBehaviour
{
    [SerializeField]
    private ActiveState state;
    private Dictionary<Vector3Int, TerrainChunk> loadedChunks;
    private List<ChunkGenRequest> generationQueue;

    struct ChunkGenRequest {
        public Vector3Int id;
        public Vector3 position;
        public ActiveState state;
    }

    public TerrainLayerGenerator generator;

    public TerrainHandler handler;
    public int id;
    public Vector3 origin;

    [SerializeField]
    private Vector3Int chunkCount;

    public bool generating;
    public bool generated;

    public TerrainLayer Initialize(int _id, Vector3 _origin, TerrainHandler _handler, TerrainLayerGenerator _generator, ActiveState _state) {
        id = _id;
        origin = _origin;
        handler = _handler;
        generator = _generator;
        SetState(_state);
        generating = false;
        generated = false;

        loadedChunks = new Dictionary<Vector3Int, TerrainChunk>();
        generationQueue = new List<ChunkGenRequest>();

        chunkCount = new Vector3Int(Mathf.CeilToInt((float)handler.generatedArea.x / handler.voxelsPerAxis.x / handler.voxelScale),
                                    Mathf.RoundToInt(generator.depth / handler.voxelsPerAxis.x / handler.voxelScale),
                                    Mathf.CeilToInt((float)handler.generatedArea.y / handler.voxelsPerAxis.z / handler.voxelScale));

        if (!handler.enableChunkLODs)
            FillGenerationQueue(state);
        else
            FillGenerationQueue(ActiveState.Inactive);

        return this;
    }

    private void Update() {
        if (!handler.enableChunkLODs) {
            foreach(var item in loadedChunks) {
                item.Value.SetState(state);
            }
        }

        if (handler.enableChunkLODs && generated) {
            UpdateChunkActivity();
            ForceProcessQueue();
        } else {
            if (generationQueue.Count != 0) {
                generating = true;
                ProcessQueue();
            } else {
                generated = true;
                generating = false;
                TerrainHandler.OnLayerGenerated?.Invoke(id);
            }
        }
    }

    private void FillGenerationQueue(ActiveState state) {
        Vector3Int halfChunkCount = Vector3Int.FloorToInt((Vector3)chunkCount / 2f);
        //Bit of a hacky way to fix centering issues when chunkCount doesnt allow for an exact centre chunk
        Vector3 originOffset = Vector3.zero;
        if (new Vector2(halfChunkCount.x, halfChunkCount.z) * 2 == new Vector2(chunkCount.x, chunkCount.z))
            originOffset = new Vector3(handler.voxelsPerAxis.x * handler.voxelScale / 2f, 0, handler.voxelsPerAxis.z * handler.voxelScale / 2f);

        //Spawn all chunks
        for (int z = 0; z < chunkCount.z; z++) {
            for (int y = 0; y < chunkCount.y; y++) {
                for (int x = 0; x < chunkCount.x; x++) {
                    Vector3Int id = new Vector3Int(-halfChunkCount.x + x,
                                                    y,
                                                   -halfChunkCount.z + z);
                    if (!loadedChunks.ContainsKey(id)) {

                        Vector3 position = origin + new Vector3(handler.voxelsPerAxis.x * id.x,
                                                              -(handler.voxelsPerAxis.y * id.y + (handler.voxelsPerAxis.y / 2f)), //To account for y position being at the start not the centre
                                                                handler.voxelsPerAxis.z * id.z)
                                                              * handler.voxelScale
                                                              + originOffset;

                        ChunkGenRequest info = new ChunkGenRequest { id = id, position = position, state = state };
                        if (!generationQueue.Contains(info)) {
                            generationQueue.Add(info);
                        }
                    }
                }
            }
        }
    }

    private void UpdateChunkActivity() {
        Vector3Int halfChunkCount = Vector3Int.FloorToInt((Vector3)chunkCount / 2f);
        //Bit of a hacky way to fix centering issues when chunkCount doesnt allow for an exact centre chunk
        Vector3 originOffset = Vector3.zero;
        if (new Vector2(halfChunkCount.x, halfChunkCount.z) * 2 == new Vector2(chunkCount.x, chunkCount.z))
            originOffset = new Vector3(handler.voxelsPerAxis.x * handler.voxelScale / 2f, 0, handler.voxelsPerAxis.z * handler.voxelScale / 2f);

        //Find chunkID player is in
        Bounds bounds = GetBounds();
        Vector3 relativeOriginCamPosition = handler.mainCamera.transform.position - (origin - new Vector3(bounds.extents.x, 0, bounds.extents.z));
        relativeOriginCamPosition.y *= -1;

        Vector3 chunkWorldSize = (Vector3)handler.voxelsPerAxis * handler.voxelScale;
        Vector3Int chunksFromOrigin = Vector3Int.FloorToInt(new Vector3(relativeOriginCamPosition.x / chunkWorldSize.x, relativeOriginCamPosition.y / chunkWorldSize.y, relativeOriginCamPosition.z / chunkWorldSize.z));

        for(int z = -4; z <= 4; z++) {
            for (int y = -4; y <= 4; y++) {
                for (int x = -4; x <= 4; x++) {
                    Vector3Int id = chunksFromOrigin - new Vector3Int(halfChunkCount.x + x,
                                                                     y,
                                                                     halfChunkCount.z + z);
                    if (x == 0 && y == 0 && z == 0)
                        Debug.Log("Player in Chunk: " + (Vector3)id);
                    if (id.x < -halfChunkCount.x || id.x >= -halfChunkCount.x + chunkCount.x) continue;
                    if (id.y < -halfChunkCount.y || id.y >= -halfChunkCount.y + chunkCount.y) continue;
                    if (id.z < -halfChunkCount.z || id.z >= -halfChunkCount.z + chunkCount.z) continue;

                    Vector3 position = origin + new Vector3(handler.voxelsPerAxis.x * id.x,
                                                  -(handler.voxelsPerAxis.y * id.y + (handler.voxelsPerAxis.y / 2f)), //To account for y position being at the start not the centre
                                                    handler.voxelsPerAxis.z * id.z)
                                                  * handler.voxelScale
                                                  + originOffset;

                    float manDst = Mathf.Abs(x) + Mathf.Abs(y) + Mathf.Abs(z);
                    ActiveState chunkTargetState;
                    if (manDst <= 2)
                        chunkTargetState = ActiveState.Active;
                    else
                        chunkTargetState = ActiveState.Inactive;

                    ChunkGenRequest info = new ChunkGenRequest { id = id, position = position, state = chunkTargetState };
                    if (!generationQueue.Contains(info))
                        generationQueue.Add(info);
                    
                }
            }
        }

    }

    public void ForceProcessQueue() {
        if (handler.enableChunkLODs) //Just as a safeguard against filling it twice
            FillGenerationQueue(ActiveState.Static_NoGrass);
        while(generationQueue.Count != 0) {
            ProcessQueue();
        }
    }

    private void ProcessQueue() {
        ChunkGenRequest toProcess = generationQueue[0];
        if (!loadedChunks.ContainsKey(toProcess.id)) {
            CreateChunk(toProcess.id, toProcess.position, toProcess.state);
        }
        TerrainChunk chunk = loadedChunks[toProcess.id];
        if (chunk.generated) {
            chunk.SetState(toProcess.state);
            generationQueue.RemoveAt(0);
        }
    }

    private TerrainChunk CreateChunk(Vector3Int id, Vector3 position, ActiveState createState) {
        if (loadedChunks.ContainsKey(id)) {
            Debug.Log("Chunk already created");
            return loadedChunks[id];
        }

        GameObject chunkGObj = new GameObject(id.x + ", " + (-id.y) + ", " + id.z);
        chunkGObj.transform.parent = transform;
        chunkGObj.transform.position = position;
        chunkGObj.tag = "Terrain";
        chunkGObj.layer = LayerMask.NameToLayer("Terrain");
        chunkGObj.AddComponent<MeshFilter>();
        chunkGObj.AddComponent<MeshRenderer>().material = handler.material;
        chunkGObj.AddComponent<MeshCollider>();

        TerrainChunk chunk = chunkGObj.AddComponent<TerrainChunk>().Initialize(this, position, state);
        loadedChunks.Add(id, chunk);
        return chunk;
    }

    public void SetState(ActiveState state) {
        if (this.state == state) return;

        this.state = state;
    }

    public void DistributeEditRequest(ChunkEditRequest request) {
        if (state == ActiveState.Inactive) return;

        foreach(var item in loadedChunks) {
            TerrainChunk chunk = item.Value;
            if (chunk.GetBounds().Intersects(request.GetBounds())) {
                chunk.MakeEditRequest(request.Clone());
            }
        }
    }

    //
    // Summery:
    //     Handles unloading the entirety of this layer
    //     When in unloaded state, the layer will have to either regenerate from noise or load from a file
    //
    public void Unload(bool fromEditor = false) {
        generator.ReleaseBuffers();

        if (loadedChunks != null) { 
            foreach(var item in loadedChunks) {
                TerrainChunk chunk = item.Value;
                chunk.Unload(fromEditor);
            }
            loadedChunks = null;
        } else {
            //Fail safe
            foreach(Transform child in transform) {
                if(child.TryGetComponent(out TerrainChunk chunk)) {
                    chunk.Unload(fromEditor);
                }
            }
        }
        if (fromEditor)
            DestroyImmediate(gameObject);
        else
            Destroy(gameObject);
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
