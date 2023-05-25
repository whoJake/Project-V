using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainLayer
{
    private readonly GameObject gameObject;
    public readonly int id;
    public readonly Vector3 origin;
    private readonly TerrainHandler handler;

    public TerrainLayer(int _id, GameObject _gameObject, Vector3 _origin, TerrainHandler _handler) {
        gameObject = _gameObject;
        id = _id;
        origin = _origin;
        handler = _handler;
    }

    //
    // Summery:
    //   Generates the layer with the given amount of chunks
    //   
    // Parameters:
    //   chunkCount:
    //     number of chunks per axis to generate
    public void Generate(float depth) {
        Vector3Int voxelsPerAxis = new Vector3Int(handler.chunkSize.x - 1,
                                                handler.chunkSize.y - 1,
                                                handler.chunkSize.z - 1);

        //Constantly having to account for num of voxels per axis being 1 less than number of points per axis is annoying
        Vector3Int chunkCount = new Vector3Int(Mathf.CeilToInt(handler.generatedArea.x / voxelsPerAxis.x / handler.voxelScale),
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
                    chunk.Generate(handler.settings);
                }
            }
        }
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

}
