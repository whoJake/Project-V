using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainHandler : MonoBehaviour
{
    public Camera mainCamera;
    public bool enableLayerLODs;
    public bool enableChunkLODs;

    private Dictionary<int, TerrainLayer> loadedLayers;

    [SerializeField]
    private TerrainSettings authoredSettings;
    public TerrainSettings settings;

    struct LayerGenRequest {
        public int id;
        public Vector3 origin;
        public ActiveState state;
    }
    private List<LayerGenRequest> generationQueue;

    public Vector2Int generatedArea; //Only an approximate area that will be generated. Should always generate slightly larger than this but problems exist !
    [SerializeField]
    private Vector3Int chunkSize;
    
    public Material grassMaterial;

    public Vector3Int voxelsPerAxis { get { return new Vector3Int(chunkSize.x - 1,
                                                                  chunkSize.y - 1,
                                                                  chunkSize.z - 1); } }

    public Vector3Int textureDimensions { get { return chunkSize + (Vector3Int.one * margin * 2); } }

    [Min(0)]
    public int margin;

    [Min(0)]
    public float voxelScale;

    public Material material;

    [SerializeField]
    private bool showLayerBounds;

    public bool enableGrass;

    [HideInInspector]
    public bool yieldOnChunk;
    private bool active = false;

    public static System.Action<int> OnLayerGenerated;

    private void Start() {
        if (!mainCamera)
            mainCamera = Camera.main;

        yieldOnChunk = true;
        Unload(); //Just incase we have loaded during editor sequence
        Initialize();
        active = true;
    }

    public void ForceGenerate() {
        yieldOnChunk = false;
        Unload(true);
        Initialize();

        Vector3 currentLayerOrigin = transform.position;
        for(int i = 0; i < settings.layers.Length; i++) {
            CreateLayer(i, currentLayerOrigin, ActiveState.Static_NoGrass).ForceGenerateAllChunks(ActiveState.Static_NoGrass, false);
            currentLayerOrigin += Vector3.down * settings.layers[i].depth;
        }
        yieldOnChunk = true;
    }

    private void Update() {
        if (!active)
            return;

        //Process queue has to be done first or the generated layer will get destroyed before it can be removed from the queue :/
        if (generationQueue.Count != 0) 
            ProcessQueue();

        UpdateLayerActivity();
    }

    private void Initialize() {
        loadedLayers = null;
        loadedLayers = new Dictionary<int, TerrainLayer>();
        generationQueue = null;
        generationQueue = new List<LayerGenRequest>();
        settings = Instantiate(authoredSettings);
        for(int i = 0; i < settings.layers.Length; i++) {
            settings.layers[i] = Instantiate(settings.layers[i]);
            settings.layers[i].name = authoredSettings.layers[i].name + " (Layer " + i + ")";
            settings.layers[i].SetDepth(voxelsPerAxis.y, voxelScale);
        }

        TerrainChunk.InitializeCompute();
    }

    public void Unload(bool fromEditor = false) {
        if (loadedLayers != null) {
            foreach (KeyValuePair<int, TerrainLayer> l in loadedLayers) {
                l.Value.Unload(fromEditor);
            }
        } else {
            //Fail safe to unload
            foreach (Transform child in transform) {
                if (child.gameObject.TryGetComponent(out TerrainLayer layer))
                    layer.Unload(fromEditor);
            }
        }

        loadedLayers = null;
        settings = null;
        generationQueue = null;
        TerrainChunk.ReleaseBuffers();
    }

    private void UpdateLayerActivity() {
        Vector3 currentLayerOrigin = transform.position;

        if (!enableLayerLODs) {
            //Spawn all layers
            for(int i = 0; i < settings.layers.Length; i++) {
                if (!loadedLayers.ContainsKey(i)) {
                    LayerGenRequest info = new LayerGenRequest { id = i, origin = currentLayerOrigin, state = ActiveState.Active };
                    if (!generationQueue.Contains(info)) {
                        generationQueue.Add(info);
                    }
                }
                currentLayerOrigin += Vector3.down * settings.layers[i].depth;
            }
            return;
        }

        float cameraLookDirection = (Vector3.Dot(Vector3.up, mainCamera.transform.forward) > 0) ? 1 : -1;
        for(int i = 0; i < settings.layers.Length; i++) {
            Vector2 layerHeightRange = new Vector3(currentLayerOrigin.y, currentLayerOrigin.y - settings.layers[i].depth);
            float cameraHeight = mainCamera.transform.position.y;

            float topDst = cameraHeight - layerHeightRange.x;
            float btmDst = layerHeightRange.y - cameraHeight;
            float dstFromLayer = Mathf.Max(topDst, btmDst);

            ActiveState layerTargetState;
            if (dstFromLayer < 100)
                layerTargetState = ActiveState.Active;
            else if (dstFromLayer < 500)
                layerTargetState = ActiveState.Static;
            else if (dstFromLayer < 800)
                layerTargetState = ActiveState.Static_NoGrass;
            else if (dstFromLayer < 900)
                layerTargetState = ActiveState.Inactive;
            else {
                if (loadedLayers.ContainsKey(i)) {
                    loadedLayers[i].Unload();
                    loadedLayers.Remove(i);
                }
                currentLayerOrigin += Vector3.down * settings.layers[i].depth;
                continue;
            }

            if (loadedLayers.ContainsKey(i)) {
                loadedLayers[i].SetState(layerTargetState);
            } else {
                LayerGenRequest info = new LayerGenRequest { id = i, origin = currentLayerOrigin, state = layerTargetState };
                if (!generationQueue.Contains(info)) {
                    generationQueue.Add(info);
                    Debug.Log("Layer " + i + " added to generation queue");
                }
            }

            currentLayerOrigin += Vector3.down * settings.layers[i].depth;
        }

    }

    private void ProcessQueue() {
        LayerGenRequest toProcess = generationQueue[0];
        if (!loadedLayers.ContainsKey(toProcess.id)) {
            CreateLayer(toProcess.id, toProcess.origin, toProcess.state).ForceGenerateAllChunks(ActiveState.Inactive, true);
            Debug.Log("Layer " + toProcess.id + " being generated");
        }
        TerrainLayer layer = loadedLayers[toProcess.id];
        if (layer.generated) {
            layer.SetState(toProcess.state);
            OnLayerGenerated?.Invoke(toProcess.id);
            generationQueue.RemoveAt(0);
        }
    }

    public void DistributeEditRequest(ChunkEditRequest request) {
        foreach(var item in loadedLayers) {
            TerrainLayer layer = item.Value;
            if (layer.bounds.Intersects(request.GetBounds())) {
                layer.DistributeEditRequest(request);
            }
        }
    }

    private TerrainLayer CreateLayer(int layerIndex, Vector3 origin, ActiveState createState) {
        if (loadedLayers.ContainsKey(layerIndex)) {
            Debug.Log("Layer already created");
            return loadedLayers[layerIndex];
        }

        GameObject layerGObj = new GameObject("Layer: " + layerIndex);
        layerGObj.transform.parent = transform;

        TerrainLayer layer = layerGObj.AddComponent<TerrainLayer>().Initialize(layerIndex, origin, this, settings.layers[layerIndex], createState);
        loadedLayers.Add(layerIndex, layer);
        return layer;
    }

    private void OnDrawGizmos() {
        if(loadedLayers != null && showLayerBounds) {
            for (int layer = 0; layer < loadedLayers.Count; layer++) {
                if (loadedLayers.ContainsKey(layer)) {
                    Bounds bounds = loadedLayers[layer].bounds;
                    Gizmos.DrawWireCube(bounds.center, bounds.size);
                }
            }
        }
    }
}

[System.Serializable]
//Enum used for most terrain classes to dictate its activity state
public enum ActiveState {
    None,
    Inactive, //Cannot be changed and game object is disabled
    Static_NoGrass,
    Static, //Edits can be added to its queue but it will not update the mesh, collider mesh is not set
    Active //Edits can be added and will be updated immediately
}
