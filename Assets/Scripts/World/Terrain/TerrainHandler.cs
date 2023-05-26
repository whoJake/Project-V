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
        StartCoroutine(GenerateTerrain(activeSettings));
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
        //Find layer
        int playerLayerIndex = 0;
        for(int layer = 0; layer < terrainLayers.Length; layer++) {
            if (terrainLayers[layer] == null) break; //Likely reached the bottom of what has been generated
            if (terrainLayers[layer].state == ActiveState.Inactive) continue;
            //player is in layer
            if (player.transform.position.y <= terrainLayers[layer].origin.y &&
                player.transform.position.y >= terrainLayers[layer].origin.y - activeSettings.layers[layer].genDepth) {
                playerLayerIndex = layer;
                break;
            }
        }

        //Update all layers
        for(int i = 0; i < terrainLayers.Length; i++) {
            if (terrainLayers[i] == null) break; //Likely reached the bottom of what has been generated
            int dstFromPlayer = Mathf.Abs(playerLayerIndex - i);

            ActiveState changeToState;
            if (dstFromPlayer < 2) changeToState = ActiveState.Active;
            else if (dstFromPlayer < 7) changeToState = ActiveState.Static;
            else changeToState = ActiveState.Inactive;

            terrainLayers[i].state = changeToState;
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

    //Currently an IEnumerator, not perfect as means only maximum/minimum 1 chunk can be drawn per frame and its still not waiting on it
    IEnumerator GenerateTerrain(TerrainSettings settings) {
        terrainLayers = new TerrainLayer[settings.layers.Length];

        //Pre-pass for setting variables needed in settings
        Vector3 layerOrigin = generationStartPosition;
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
        layerOrigin = generationStartPosition;
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
