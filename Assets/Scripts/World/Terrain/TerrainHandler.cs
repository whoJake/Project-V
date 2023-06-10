using System.Collections.Generic;
using UnityEngine;

public class TerrainHandler : MonoBehaviour
{

    [SerializeField]
    private Camera mainCamera;

    private Dictionary<int, TerrainLayer> loadedLayers;

    
    [SerializeField]
    private TerrainSettings authoredSettings;
    public TerrainSettings settings;

    struct LayerGenRequest {
        public int id;
        public ActiveState state;
    }
    private List<LayerGenRequest> generationQueue;

    //[SerializeField]
    //private int randomGenCount;

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
        generationQueue = new List<LayerGenRequest>();
        loadedLayers = new Dictionary<int, TerrainLayer>();
        settings = authoredSettings;
        settings.seed = Random.Range(0, 1024);

        TerrainChunk.InitializeCompute();
    }

    private void Update() {
        //Process queue has to be done first or the generated layer will get destroyed before it can be removed from the queue :/
        if (generationQueue.Count != 0) ProcessQueue();

        UpdateLayerActivity();
    }

    private void OnDisable() {
        TerrainChunk.ReleaseBuffers();
    }

    private void UpdateLayerActivity() {
        if (true) { 
        //if (!mainCamera) {
            for(int i = 0; i < settings.layers.Length; i++) {
                if (!loadedLayers.ContainsKey(i)) {
                    LayerGenRequest info = new LayerGenRequest { id = i, state = ActiveState.Active };
                    if (!generationQueue.Contains(info)) {
                        generationQueue.Add(info);
                    }
                }
            }
            return;
        }
    }

    private void ProcessQueue() {
        LayerGenRequest toProcess = generationQueue[0];
        if (!loadedLayers.ContainsKey(toProcess.id)) {
            CreateLayer(toProcess.id, toProcess.state);
        }
        if (loadedLayers[toProcess.id].generating) return;
        if (loadedLayers[toProcess.id].generated) generationQueue.RemoveAt(0);
    }

    public void DistributeEditRequest(ChunkEditRequest request) {
        foreach(var item in loadedLayers) {
            TerrainLayer layer = item.Value;
            if (layer.GetBounds().Intersects(request.GetBounds())) {
                layer.DistributeEditRequest(request);
            }
        }
    }

    private TerrainLayer CreateLayer(int layerIndex, ActiveState createState) {
        if (loadedLayers.ContainsKey(layerIndex)) {
            Debug.Log("Layer already created");
            return loadedLayers[layerIndex];
        }

        GameObject layerGObj = new GameObject("Layer: " + layerIndex);
        layerGObj.transform.parent = transform;

        Vector3 layerOrigin = generationStartPosition;
        if (layerIndex-1 >= 0 && loadedLayers[layerIndex-1] != null) {
            layerOrigin = loadedLayers[layerIndex - 1].origin + Vector3.down * loadedLayers[layerIndex - 1].generator.GetDepth(0);
        }

        TerrainLayer layer = layerGObj.AddComponent<TerrainLayer>().Initialize(layerIndex, layerOrigin, this, settings.layers[layerIndex].Clone(), createState);

        loadedLayers.Add(layerIndex, layer);
        return layer;
    }

    //
    // Summery:
    //   Generate randomly generated layers
    //
    // Parameters:
    //   count:
    //     number of layers to be generated
    /*
    TerrainSettings GenerateLayerSettings(int count) {
        TerrainSettings result = ScriptableObject.CreateInstance<TerrainSettings>();
        result.layers = new TerrainLayerSettings[count + 1];
        result.layers[0] = Resources.Load<TerrainLayerSettings>("Layers/AIR");

        for(int i = 0; i < count; i++) {
            result.layers[i + 1] = TerrainLayerSettings.GetAllRandom();
        }
        return result;
    }
    */

    private void OnDrawGizmos() {
        if(loadedLayers != null && showLayerBounds) {
            for(int layer = 0; layer < loadedLayers.Count; layer++) {
                Gizmos.DrawWireCube(loadedLayers[layer].origin - new Vector3(0, loadedLayers[layer].generator.GetDepth(0) / 2f, 0), new Vector3(generatedArea.x, loadedLayers[layer].generator.GetDepth(0), generatedArea.y));
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
