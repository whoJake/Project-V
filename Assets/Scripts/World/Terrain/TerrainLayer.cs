using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainLayer
{
    private List<TerrainChunk> chunks;

    private readonly GameObject gameObject;
    public readonly int id;
    public readonly Vector3 origin;
    private readonly TerrainHandler handler;

    private Vector3Int voxelsPerAxis;
    private Vector3Int chunkCount;

    private bool isGenerated = false;
    public bool IsGenerated { get { return isGenerated; } }

    public TerrainLayer(int _id, GameObject _gameObject, Vector3 _origin, TerrainHandler _handler) {
        gameObject = _gameObject;
        id = _id;
        origin = _origin;
        handler = _handler;
        chunks = new List<TerrainChunk>();
    }

    public void Update() {
        foreach(TerrainChunk chunk in chunks) {
            chunk?.Update();
        }
    }

    public void MakeEditRequest(ChunkEditRequest request) {
        foreach(TerrainChunk chunk in chunks) {
            if (chunk == null) continue;

            if (chunk.GetBounds().Intersects(request.GetBounds())) {
                chunk.MakeEditRequest(request.Clone());
            }
        }
    }

    //
    // Summery:
    //   Generates the layer with the given amount of chunks
    //   
    // Parameters:
    //   chunkCount:
    //     number of chunks per axis to generate
    public IEnumerator Generate(float depth) {
        voxelsPerAxis = new Vector3Int(handler.chunkSize.x - 1,
                                       handler.chunkSize.y - 1,
                                       handler.chunkSize.z - 1);

        //Constantly having to account for num of voxels per axis being 1 less than number of points per axis is annoying
        chunkCount = new Vector3Int(Mathf.CeilToInt(handler.generatedArea.x / voxelsPerAxis.x / handler.voxelScale),
                                    Mathf.FloorToInt(depth / voxelsPerAxis.y / handler.voxelScale),
                                    Mathf.CeilToInt(handler.generatedArea.y / voxelsPerAxis.z / handler.voxelScale));

        Debug.Log(chunkCount + " chunks dispatched to generate");

        Vector3Int halfChunkCount = Vector3Int.FloorToInt((Vector3)chunkCount / 2f);
        for(int x = 0; x < chunkCount.x; x++) {
            for(int y = 0; y < chunkCount.y; y++) {
                for(int z = 0; z < chunkCount.z; z++) {
                    //Layer origin is at the top centre of the layer
                    Vector3Int cid = new Vector3Int(-halfChunkCount.x + x,
                                                    y,
                                                    -halfChunkCount.z + z);


                    Vector3 position = origin + new Vector3(voxelsPerAxis.x * cid.x,
                                                          -(voxelsPerAxis.y * cid.y + (voxelsPerAxis.y / 2f)), //To account for y position being at the start not the centre
                                                            voxelsPerAxis.z * cid.z)
                                                            * handler.voxelScale;

                    GameObject chunkGameObject = CreateChunkGameObject(cid.x + "," + (-cid.y) + "," + cid.z, position);
                    TerrainChunk chunk = new TerrainChunk(this, position, handler.chunkSize, handler.margin, handler.voxelScale, chunkGameObject);
                    //Probably add this to some array or list
                    chunk.Generate(handler.activeSettings);
                    chunks.Add(chunk);
                    yield return null;
                }
            }
        }
        isGenerated = true;
    }

    //
    // Summery:
    //   Creates a game object with everything needed to act as a terrain chunk
    //
    // Parameters:
    //   name:
    //     name of the game object
    //   position:
    //     position of the game object
    private GameObject CreateChunkGameObject(string name, Vector3 position) {
        GameObject result = new GameObject(name);
        result.transform.parent = gameObject.transform;
        result.transform.position = position;
        result.AddComponent<MeshFilter>();
        result.AddComponent<MeshRenderer>().material = handler.material;
        result.AddComponent<MeshCollider>();
        return result;
    }

    public Bounds GetBounds() {
        Vector3 centre = new Vector3(origin.x, origin.y - (voxelsPerAxis.y * chunkCount.y * handler.voxelScale / 2f), origin.z); ;
        Vector3 size = new Vector3(voxelsPerAxis.x * chunkCount.x, voxelsPerAxis.y * chunkCount.y, voxelsPerAxis.z * chunkCount.z) * handler.voxelScale;

        return new Bounds(centre, size);
    }

}
