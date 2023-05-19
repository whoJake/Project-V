using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainHandler : MonoBehaviour
{
    public TerrainSettings settings;
    private TerrainLayer[] terrainLayers;

    public Vector2Int generatedArea; //Approximate area that will be generated
    public Vector3Int chunkSize;
    public int margin;
    public float voxelScale;
    public Material material;

    void Start()
    {
        GenerateTerrain();
    }

    void GenerateTerrain() {
        terrainLayers = new TerrainLayer[settings.layers.Length];

        TerrainChunk.InitializeCompute();
        Vector3 layerOrigin = Vector3.zero;
        for(int layer = 0; layer < settings.layers.Length; layer++) {
            //Make GameObject parent for layer
            GameObject parent = new GameObject("Layer: " + layer);
            parent.transform.parent = transform;

            //Setup layer
            TerrainLayerSettings layerSettings = settings.layers[layer];
            TerrainLayer nLayer = new TerrainLayer(layer, parent, layerOrigin, this);
            terrainLayers[layer] = nLayer;

            //Generate layer
            nLayer.Generate(layerSettings.depth);
            //Another hack for corrrecting chunk height in voxels
            int yChunksNeededForLayer = Mathf.FloorToInt(layerSettings.depth / (chunkSize.y - 1f));

            layerOrigin.y -= (layerSettings.depth - yChunksNeededForLayer);
        }
    }
}
