using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainHandler : MonoBehaviour
{
    public Vector3 generationStartPosition { get { return transform.position; } }
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
        activeSettings = authorLayers ? authoredSettings : GenerateLayerSettings(randomGenCount);
        //Randomize seed
        activeSettings.seed = Random.Range(0, 1024);
        InitializeGeneration();
        //StartCoroutine(GenerateTerrain(activeSettings));
    }

    private void Update() {
        if (terrainLayers?[1] != null) {
            //Let player fall once the first layer is generated that isnt air
            player.isActive = terrainLayers[1].IsGenerated;
        }

        UpdateLayerActivity();
        
        foreach(TerrainLayer layer in terrainLayers) {
            layer?.Update();
        }
    }

    private void UpdateLayerActivity() {
        //Find layer that player is on
        int playerLayerIndex = 0;
        for(int layer = 0; layer < terrainLayers.Length; layer++) {
            TerrainLayerSettings layerSettings = activeSettings.layers[layer];
            //player is in layer
            if (player.transform.position.y <= layerSettings.origin.y &&
                player.transform.position.y >= layerSettings.origin.y - layerSettings.genDepth) {
                playerLayerIndex = layer;
                break;
            }
        }

        //Update all layers
        for(int i = 0; i < terrainLayers.Length; i++) {
            int dstFromPlayer = Mathf.Abs(playerLayerIndex - i);

            //Check generation step
            ActiveState calcState;
            if(dstFromPlayer < 2) {
                calcState = ActiveState.Active;
            }
            else if(dstFromPlayer < 7) {
                calcState = ActiveState.Static;
            } else {
                calcState = ActiveState.Inactive;
            }

            if (terrainLayers[i] == null && calcState == ActiveState.Inactive) continue;
            if (terrainLayers[i] == null) GenerateLayer(i, calcState);
            else terrainLayers[i].state = calcState;
        }
    }

    public void MakeEditRequest(ChunkEditRequest request) {
        foreach(TerrainLayer layer in terrainLayers) {
            if (layer == null) continue;
            if (layer.state == ActiveState.Inactive) continue;

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
    TerrainSettings GenerateLayerSettings(int count) {
        TerrainSettings result = ScriptableObject.CreateInstance<TerrainSettings>();
        result.layers = new TerrainLayerSettings[count + 1];
        result.layers[0] = Resources.Load<TerrainLayerSettings>("AIR");
        for(int i = 0; i < count; i++) {
            result.layers[i + 1] = TerrainLayerSettings.GetAllRandom();
        }
        return result;
    }

    void InitializeGeneration() {
        terrainLayers = new TerrainLayer[activeSettings.layers.Length];

        //Pre-pass for setting variables needed in settings
        Vector3 layerOrigin = generationStartPosition;
        for (int layer = 0; layer < activeSettings.layers.Length; layer++) {
            TerrainLayerSettings nLayerSettings = activeSettings.layers[layer];
            float voxelsPerY = chunkSize.y - 1;
            float chunksOnY = Mathf.FloorToInt(nLayerSettings.depth / voxelsPerY / voxelScale);
            float generatedDepth = chunksOnY * voxelsPerY * voxelScale;

            nLayerSettings.genDepth = generatedDepth;
            Debug.Log("Layer " + layer + " generating to a depth of " + generatedDepth);
            nLayerSettings.origin = layerOrigin;

            layerOrigin.y -= generatedDepth + (margin * voxelScale);
        }
        TerrainChunk.InitializeCompute(activeSettings);
    }

    void GenerateLayer(int layerIndex, ActiveState genState) {
        if (terrainLayers[layerIndex] != null) {
            Debug.Log("layer already generated");
            return;
        }

        GameObject parent = new GameObject("Layer: " + layerIndex);
        parent.transform.parent = transform;

        //Setup layer
        TerrainLayer layer = new TerrainLayer(layerIndex, parent, activeSettings.layers[layerIndex].origin, this);
        terrainLayers[layerIndex] = layer;
        layer.state = genState;
        StartCoroutine(layer.Generate(activeSettings.layers[layerIndex].depth));
    }

    //Currently an IEnumerator, not perfect as means only maximum/minimum 1 chunk can be drawn per frame and its still not waiting on it
    IEnumerator GenerateTerrain(TerrainSettings settings) {

        TerrainChunk.InitializeCompute(settings);
        Vector3 layerOrigin = generationStartPosition;
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

//Enum used for most terrain classes to dictate its activity state
public enum ActiveState {
    Inactive, //Cannot be changed and game object is disabled
    Static, //Edits can be added to its queue but it will not update the mesh, collider mesh is not set
    Active //Edits can be added and will be updated immediately
}
