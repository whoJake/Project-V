using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.EventSystems.EventTrigger;

public class TerrainLayer : MonoBehaviour
{
    public ActiveState state { get; private set; }
    private TerrainChunk[,,] loadedChunks;
    private Queue<ChunkGenRequest> generationQueue;

    struct ChunkGenRequest {
        public Vector3Int id;
        public Vector3 position;
        public ActiveState state;
    }

    public TerrainLayerGenerator generator;

    public TerrainHandler handler;
    public int id;
    public Vector3 oldOrigin;
    public Vector3 origin;

    public Bounds bounds { get; private set; }
    [SerializeField]
    private Vector3Int chunkCount;

    public bool generating;
    public bool generated;

    public TerrainLayer Initialize(int _id, Vector3 _origin, TerrainHandler _handler, TerrainLayerGenerator _generator, ActiveState _state) {
        id = _id;
        handler = _handler;
        generator = _generator;
        generating = false;
        generated = false;
        SetState(_state);
        generationQueue = new Queue<ChunkGenRequest>();

        chunkCount = new Vector3Int(Mathf.CeilToInt((float)handler.generatedArea.x / handler.voxelsPerAxis.x / handler.voxelScale),
                                    Mathf.CeilToInt(generator.depth / handler.voxelsPerAxis.x / handler.voxelScale),
                                    Mathf.CeilToInt((float)handler.generatedArea.y / handler.voxelsPerAxis.z / handler.voxelScale));
        {
            Vector3 centre = new Vector3(_origin.x,
                                         _origin.y - (handler.voxelsPerAxis.y * chunkCount.y * handler.voxelScale / 2f),
                                         _origin.z); ;
            Vector3 size = new Vector3(handler.voxelsPerAxis.x * chunkCount.x,
                                       handler.voxelsPerAxis.y * chunkCount.y,
                                       handler.voxelsPerAxis.z * chunkCount.z) * handler.voxelScale;
            bounds = new Bounds(centre, size);
        }

        oldOrigin = _origin;
        origin = _origin - new Vector3(bounds.extents.x, bounds.size.y, bounds.extents.z);
        loadedChunks = new TerrainChunk[chunkCount.x, chunkCount.y, chunkCount.z];

        return this;
    }

    private void Update() {
        if (handler.enableChunkLODs && generated) {
            UpdateChunkActivity();
        }
        ProcessQueue();
    }

    public void ForceGenerateAllChunks(ActiveState _state, bool inQueue) {
        loadedChunks = new TerrainChunk[chunkCount.x, chunkCount.y, chunkCount.z];

        Vector3Int halfChunkCount = Vector3Int.FloorToInt((Vector3)chunkCount / 2f);
        //Bit of a hacky way to fix centering issues when chunkCount doesnt allow for an exact centre chunk
        Vector3 originOffset = Vector3.zero;
        if (new Vector2(halfChunkCount.x, halfChunkCount.z) * 2 == new Vector2(chunkCount.x, chunkCount.z))
            originOffset = new Vector3(handler.voxelsPerAxis.x * handler.voxelScale / 2f, 0, handler.voxelsPerAxis.z * handler.voxelScale / 2f);

        //Spawn all chunks
        for (int z = 0; z < chunkCount.z; z++) {
            for (int y = 0; y < chunkCount.y; y++) {
                for (int x = 0; x < chunkCount.x; x++) {
                    Vector3Int id = new Vector3Int(x, y, z);

                    Vector3 position = origin + originOffset + new Vector3(handler.voxelsPerAxis.x * id.x,
                                                                            handler.voxelsPerAxis.y * id.y,
                                                                            handler.voxelsPerAxis.z * id.z)
                                                                          * handler.voxelScale;

                    if (inQueue) {
                        generationQueue.Enqueue(new ChunkGenRequest { id = id, position = position, state = _state });
                    } else {
                        CreateChunk(id, position, _state);
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
        Vector3 relativeOriginCamPosition = handler.mainCamera.transform.position - origin;
        Vector3 chunkWorldSize = (Vector3)handler.voxelsPerAxis * handler.voxelScale;
        Vector3Int chunksFromOrigin = Vector3Int.FloorToInt(new Vector3(relativeOriginCamPosition.x / chunkWorldSize.x, relativeOriginCamPosition.y / chunkWorldSize.y, relativeOriginCamPosition.z / chunkWorldSize.z));
       
        for (int z = -6; z <= 6; z++) {
            for (int y = -6; y <= 6; y++) {
                for (int x = -6; x <= 6; x++) {
                    Vector3Int id = chunksFromOrigin + new Vector3Int(x, y, z);

                    if (id.x < 0 || id.x >= chunkCount.x) continue;
                    if (id.y < 0 || id.y >= chunkCount.y) continue;
                    if (id.z < 0 || id.z >= chunkCount.z) continue;

                    Vector3 position = origin + originOffset + new Vector3(handler.voxelsPerAxis.x * id.x,
                                                                           handler.voxelsPerAxis.y * id.y,
                                                                           handler.voxelsPerAxis.z * id.z)
                                                                         * handler.voxelScale;

                    float manDst = Mathf.Abs(x) + Mathf.Abs(y) + Mathf.Abs(z);
                    ActiveState chunkTargetState;
                    if (manDst <= 3)
                        chunkTargetState = ActiveState.Active;
                    else if (manDst <= 4)
                        chunkTargetState = ActiveState.Static;
                    else if (manDst <= 5)
                        chunkTargetState = ActiveState.Static_NoGrass;
                    else
                        chunkTargetState = ActiveState.Inactive;

                    TerrainChunk target = loadedChunks[id.x, id.y, id.z];
                    if (target == null) {
                        ChunkGenRequest info = new ChunkGenRequest { id = id, position = position, state = chunkTargetState };
                        if (!generationQueue.Contains(info)) {
                            generationQueue.Enqueue(info);
                        }
                    } else {
                        loadedChunks[id.x, id.y, id.z].SetState(chunkTargetState);
                    }
                    
                }
            }
        }

    }

    private void ProcessQueue() {
        ChunkGenRequest request;
        if (!generationQueue.TryPeek(out request)) {
            generating = false;
            generated = true;
            return;
        }

        TerrainChunk chunk = loadedChunks[request.id.x, request.id.y, request.id.z];

        if(chunk == null) {
            generating = true;
            CreateChunk(request.id, request.position, request.state);
        }
        else if (chunk.generated) {
            chunk.SetState(request.state);
            generationQueue.Dequeue();
        }
    }

    private TerrainChunk CreateChunk(Vector3Int id, Vector3 position, ActiveState createState) {
        if (loadedChunks[id.x, id.y, id.z] != null) {
            Debug.Log("Chunk already created");
            return loadedChunks[id.x, id.y, id.z];
        }

        GameObject chunkGObj = new GameObject(id.x + ", " + id.y + ", " + id.z);
        chunkGObj.transform.parent = transform;
        chunkGObj.tag = "Terrain";
        chunkGObj.layer = LayerMask.NameToLayer("Terrain");
        chunkGObj.AddComponent<MeshFilter>();
        chunkGObj.AddComponent<MeshRenderer>().material = handler.material;
        chunkGObj.AddComponent<MeshCollider>();

        TerrainChunk chunk = chunkGObj.AddComponent<TerrainChunk>().Initialize(this, position, createState);
        loadedChunks[id.x, id.y, id.z] = chunk;
        return chunk;
    }

    public void SetState(ActiveState state) {
        if (this.state == state) return;

        this.state = state;
    }

    public void DistributeEditRequest(ChunkEditRequest request) {
        if (state == ActiveState.Inactive) return;

        foreach(TerrainChunk chunk in loadedChunks) {
            if (chunk.bounds.Intersects(request.GetBounds())) {
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
        generator.Reset();

        if (loadedChunks != null) { 
            foreach(TerrainChunk chunk in loadedChunks) {
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
}
