using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainHandler : MonoBehaviour
{
    public PlayerMovement player;

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

    private void Update() {
        if (terrainLayers?[1] != null) {
            //Let player fall once the first layer is generated that isnt air
            player.isActive = terrainLayers[1].IsGenerated;
        }
        
        foreach(TerrainLayer layer in terrainLayers) {
            layer?.Update();
        }
    }

    public void MakeEditRequest(ChunkEditRequest request) {
        foreach(TerrainLayer layer in terrainLayers) {
            if (layer == null) continue;

            if (layer.GetBounds().Intersects(request.GetBounds())) {
                layer.MakeEditRequest(request);
            }
        }
    }

    //
    // Summery:
    //   Generate randomly generated layers
    //
    // Parameters:
    //   count:
    //     number of layers to be generated
    TerrainSettings GenerateLayers(int count) {
        TerrainSettings result = ScriptableObject.CreateInstance<TerrainSettings>();
        result.layers = new TerrainLayerSettings[count + 1];
        result.layers[0] = Resources.Load<TerrainLayerSettings>("AIR");
        for(int i = 0; i < count; i++) {
            result.layers[i + 1] = TerrainLayerSettings.GetAllRandom();
        }
        return result;
    }

    //Currently an IEnumerator, not perfect as means only maximum/minimum 1 chunk can be drawn per frame and its still not waiting on it
    IEnumerator GenerateTerrain(TerrainSettings settings) {
        terrainLayers = new TerrainLayer[settings.layers.Length];

        //Pre-pass for setting variables needed in settings
        Vector3 layerOrigin = Vector3.zero;
        for (int layer = 0; layer < settings.layers.Length; layer++) {
            TerrainLayerSettings nLayerSettings = settings.layers[layer];
            float voxelsPerY = chunkSize.y - 1;
            float chunksOnY = Mathf.FloorToInt(nLayerSettings.depth / voxelsPerY / voxelScale);
            float generatedDepth = chunksOnY * voxelsPerY * voxelScale;

            nLayerSettings.genDepth = generatedDepth;
            Debug.Log("Layer " + layer + " generating to a depth of " + generatedDepth);
            nLayerSettings.origin = layerOrigin;

            layerOrigin.y -= generatedDepth + (margin * voxelScale);
        }

        TerrainChunk.InitializeCompute(settings);
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

        //All layers have finished generating
        TerrainChunk.ReleaseBuffers();
    }

    private void OnDrawGizmos() {
        if(terrainLayers != null && showLayerBounds) {
            for(int layer = 0; layer < activeSettings.layers.Length; layer++) {
                Gizmos.DrawWireCube(activeSettings.layers[layer].origin - new Vector3(0, activeSettings.layers[layer].genDepth / 2f, 0), new Vector3(generatedArea.x, activeSettings.layers[layer].genDepth, generatedArea.y));
            }
        }
    }

}
