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

    struct GenInfo {
        public int id;
        public ActiveState state;
    }
    private List<GenInfo> generationQueue;

    [SerializeField]
    private bool authorLayers;
    [SerializeField]
    private int randomGenCount;

    public Vector2Int generatedArea; //Only an approximate area that will be generated. Should always generate slightly larger than this but problems exist !
    [SerializeField]
    private Vector3Int chunkSize;

    public Vector3Int voxelsPerAxis { get { return new Vector3Int(chunkSize.x - 1,
                                                                  chunkSize.y - 1,
                                                                  chunkSize.z - 1); } }

    public Vector3Int textureDimensions { get { return chunkSize + (Vector3Int.one * margin * 2); } }

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
        generationQueue = new List<GenInfo>();
        activeSettings = authorLayers ? authoredSettings : GenerateLayerSettings(randomGenCount);
        activeSettings.seed = Random.Range(0, 1024);
        InitializeGeneration();
    }

    private void Update() {
        //Process queue has to be done first or the generated layer will get destroyed before it can be removed from the queue :/
        if (generationQueue.Count != 0) ProcessQueue();

        UpdateLayerActivity();
    }

    private void ProcessQueue() {
        GenInfo toProcess = generationQueue[0];
        if (terrainLayers[toProcess.id] == null) {
            CreateLayer(toProcess.id, toProcess.state);
        }
        if (terrainLayers[toProcess.id].generating) return;
        if (terrainLayers[toProcess.id].generated) {
            generationQueue.RemoveAt(0);
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

        for (int i = 0; i < terrainLayers.Length; i++) {
            //Positive dstFromPlayer is above, Negative is below
            int dstFromPlayer = playerCurLayerIndex - i;
            TerrainLayer layer = terrainLayers[i];

            //Check generation step
            ActiveState calcState;
            dstFromPlayer = Mathf.Abs(dstFromPlayer);
            if (dstFromPlayer < 2) {
                calcState = ActiveState.Active;
            } else if (dstFromPlayer < 5) {
                calcState = ActiveState.Static;
            } else if (dstFromPlayer < 6) {
                calcState = ActiveState.Inactive;
            } else {
                //Unload
                if (layer && layer.generated) {
                    Destroy(layer.gameObject);
                }
                continue;
            }

            if (!layer) {
                GenInfo info = new GenInfo { id = i, state = calcState };
                if (!generationQueue.Contains(info)) {
                    generationQueue.Add(new GenInfo { id = i, state = calcState });
                }
            } else {
                terrainLayers[i].SetState(calcState);
            }
        }
    }

    public void DistributeEditRequest(ChunkEditRequest request) {
        foreach(TerrainLayer layer in terrainLayers) {
            if (layer == null) continue;

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

    private TerrainLayer CreateLayer(int layerIndex, ActiveState createState) {
        if (terrainLayers[layerIndex] != null) {
            Debug.Log("layer already created");
            return terrainLayers[layerIndex];
        }

        GameObject layerGObj = new GameObject("Layer: " + layerIndex);
        layerGObj.transform.parent = transform;
        TerrainLayer layer = layerGObj.AddComponent<TerrainLayer>().Initialize(layerIndex, this, activeSettings.layers[layerIndex], createState);

        terrainLayers[layerIndex] = layer;
        return layer;
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
