using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainHandler : MonoBehaviour
{
    public TerrainSettings authoredSettings;
    private TerrainLayer[] terrainLayers;

    public TerrainSettings activeSettings;

    public bool authorLayers;
    public int randomGenCount;

    public Vector2Int generatedArea; //Approximate area that will be generated
    public Vector3Int chunkSize;
    public int margin;
    public float voxelScale;
    public Material material;

    public bool showLayerBounds;

    void Start()
    {
        activeSettings = authorLayers ? authoredSettings : GenerateLayers(randomGenCount);
        //Randomize seed
        activeSettings.seed = Random.Range(0, 1024);
        StartCoroutine(GenerateTerrain(activeSettings));
    }

    TerrainSettings GenerateLayers(int count) {
        TerrainSettings result = ScriptableObject.CreateInstance<TerrainSettings>();
        result.layers = new TerrainLayerSettings[count + 1];
        result.layers[0] = Resources.Load<TerrainLayerSettings>("AIR");
        for(int i = 0; i < count; i++) {
            result.layers[i + 1] = TerrainLayerSettings.GetRandom();
        }
        return result;
    }

    IEnumerator GenerateTerrain(TerrainSettings settings) {
        terrainLayers = new TerrainLayer[settings.layers.Length];

        //Pre-pass for setting variables needed in settings
        Vector3 layerOrigin = Vector3.zero;
        for (int layer = 0; layer < settings.layers.Length; layer++) {
            TerrainLayerSettings nLayerSettings = settings.layers[layer];
            float generatedDepth = Mathf.FloorToInt(nLayerSettings.depth / (chunkSize.y - 1)) * (chunkSize.y - 1);// * voxelScale;
            nLayerSettings.genDepth = generatedDepth;
            nLayerSettings.origin = layerOrigin;
            layerOrigin.y -= generatedDepth + (margin * voxelScale);
        }

        TerrainChunk.InitializeCompute();
        layerOrigin = Vector3.zero;
        for(int layer = 0; layer < settings.layers.Length; layer++) {
            //Make GameObject parent for layer
            GameObject parent = new GameObject("Layer: " + layer);
            parent.transform.parent = transform;

            //Setup layer
            TerrainLayerSettings layerSettings = settings.layers[layer];
            TerrainLayer nLayer = new TerrainLayer(layer, parent, layerOrigin, this);
            terrainLayers[layer] = nLayer;

            //Generate layer
            yield return nLayer.Generate(layerSettings.depth);
            layerOrigin.y -= layerSettings.genDepth + (margin * voxelScale);
        }
    }

    private void OnDrawGizmos() {
        if(terrainLayers != null && showLayerBounds) {
            for(int layer = 0; layer < activeSettings.layers.Length; layer++) {
                Gizmos.DrawWireCube(activeSettings.layers[layer].origin - new Vector3(0, activeSettings.layers[layer].genDepth / 2f, 0), new Vector3(generatedArea.x, activeSettings.layers[layer].genDepth, generatedArea.y));
            }
        }
    }

}
