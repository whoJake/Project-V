using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainHandler : MonoBehaviour
{

    [SerializeField]
    private Camera mainCamera;

    [SerializeField]
    private TerrainSettings authoredSettings;
    private TerrainLayer[] terrainLayers;
    public TerrainSettings activeSettings;

    [SerializeField]
    private bool authorLayers;
    [SerializeField]
    private int randomGenCount;

    public Vector2Int generatedArea; //Only an approximate area that will be generated. Should always generate slightly larger than this but problems exist !
    public Vector3Int chunkSize;

    public Vector3Int voxelsPerAxis { get {
            return new Vector3Int(chunkSize.x - 1,
                                  chunkSize.y - 1,
                                  chunkSize.z - 1); } }
    private Vector3 generationStartPosition { get { return transform.position; } }

    [Min(0)]
    public int margin;

    [Min(0)]
    public float voxelScale;

    public Material material;

    [SerializeField]
    private bool showLayerBounds;

    public static System.Action<int> OnLayerGenerated;

    private void Start() {
        activeSettings = authorLayers ? authoredSettings : GenerateLayerSettings(randomGenCount);
        activeSettings.seed = Random.Range(0, 1024);
        InitializeGeneration();
    }

    private void Update() {
        UpdateLayerActivity();
        
        foreach(TerrainLayer layer in terrainLayers) {
            layer?.Update();
        }
    }

    private void OnDisable() {
        TerrainChunk.ReleaseBuffers();
    }

    private void UpdateLayerActivity() {
        int playerCurLayerIndex = 0;
        for(int layer = 0; layer < terrainLayers.Length; layer++) {
            TerrainLayerSettings layerSettings = activeSettings.layers[layer];
            bool playerIsBetweenLayers = (mainCamera.transform.position.y <= layerSettings.origin.y) &&
                                         (mainCamera.transform.position.y >= layerSettings.origin.y - layerSettings.genDepth);

            if (playerIsBetweenLayers) {
                playerCurLayerIndex = layer;
                break;
            }
        }

        for(int i = 0; i < terrainLayers.Length; i++) {
            //Positive dstFromPlayer is above, Negative is below
            int dstFromPlayer = playerCurLayerIndex - i;

            //Check generation step
            ActiveState calcState;
            dstFromPlayer = Mathf.Abs(dstFromPlayer);
            if(dstFromPlayer < 2) {
                calcState = ActiveState.Active;
            }
            else if(dstFromPlayer < 4) {
                calcState = ActiveState.Static;
            } else if(dstFromPlayer < 8) {
                calcState = ActiveState.Inactive;
            } else {
                //Unload
                terrainLayers[i]?.Unload();
                terrainLayers[i] = null;
                calcState = ActiveState.Inactive;
            }

            if (terrainLayers[i] == null && calcState == ActiveState.Inactive) continue;
            if (terrainLayers[i] == null) GenerateLayer(i, calcState);
            else terrainLayers[i].state = calcState;
        }
    }

    public void DistributeEditRequest(ChunkEditRequest request) {
        foreach(TerrainLayer layer in terrainLayers) {
            if (layer == null) continue;
            if (layer.state == ActiveState.Inactive) continue;

            if (layer.GetBounds().Intersects(request.GetBounds())) {
                layer.DistributeEditRequest(request);
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

    private void InitializeGeneration() {
        terrainLayers = new TerrainLayer[activeSettings.layers.Length];

        Vector3 layerOrigin = generationStartPosition;
        for (int layer = 0; layer < activeSettings.layers.Length; layer++) {
            TerrainLayerSettings layerSettings = activeSettings.layers[layer];
            float chunksOnY = Mathf.FloorToInt(layerSettings.depth / voxelsPerAxis.y / voxelScale);
            float generatedDepth = chunksOnY * voxelsPerAxis.y * voxelScale;

            layerSettings.genDepth = generatedDepth;
            layerSettings.origin = layerOrigin;

            layerOrigin.y -= generatedDepth + (margin * voxelScale);
        }
        TerrainChunk.InitializeCompute(activeSettings);
    }

    private void GenerateLayer(int layerIndex, ActiveState genState) {
        if (terrainLayers[layerIndex] != null) {
            Debug.Log("layer already generated");
            return;
        }

        GameObject parent = new GameObject("Layer: " + layerIndex);
        parent.transform.parent = transform;

        TerrainLayer layer = new TerrainLayer(this, layerIndex, activeSettings.layers[layerIndex].origin, parent);
        terrainLayers[layerIndex] = layer;
        layer.state = genState;

        StartCoroutine(layer.Generate(activeSettings.layers[layerIndex].depth, OnLayerGenerated));
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
