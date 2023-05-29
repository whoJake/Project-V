using System.Collections.Generic;
using UnityEngine;

public class TerrainChunk : MonoBehaviour
{
    public ActiveState state;
    private List<ChunkEditRequest> editRequests;
    private RenderTexture densityTexture;

    public TerrainLayer layer;
    public TerrainHandler handler { get { return layer.handler; } }

    private Vector3 centre;

    private MeshFilter targetFilter;
    private MeshCollider targetCollider;

    public Vector3 origin { get { return centre - ((Vector3)handler.textureDimensions * handler.voxelScale / 2f); } }

    private static bool shadersLoaded = false;
    private static ComputeShader computeDensityShader;
    private static ComputeShader computeVerticesShader;
    private static ComputeBuffer layerSettingsBuffer;

    private void Awake() {
        targetFilter = GetComponent<MeshFilter>();
        targetCollider = GetComponent<MeshCollider>();
    }

    public TerrainChunk Initialize(TerrainLayer _layer, Vector3 _centre, ActiveState _state) {
        layer = _layer;
        centre = _centre;
        state = _state;
        editRequests = new List<ChunkEditRequest>();

        return this;
    }

    private void Update() {
        bool gObjActive;
        Mesh colliderMesh;

        switch (state) {
            case ActiveState.Inactive:
                gObjActive = false;
                colliderMesh = null;
                break;

            case ActiveState.Static:
                gObjActive = true;
                colliderMesh = null;
                break;

            case ActiveState.Active:
                gObjActive = true;
                colliderMesh = targetFilter.mesh;
                if (editRequests.Count != 0) HandleEditRequests();
                break;

            default:
                Debug.Log("Chunk active state is not set. This should never happen");
                gObjActive = false;
                colliderMesh = null;
                break;
        }

        gameObject.SetActive(gObjActive);
        targetCollider.sharedMesh = colliderMesh;

    }

    public void MakeEditRequest(ChunkEditRequest request) {
        if (state == ActiveState.Inactive) return;

        editRequests.Add(request);
    }

    private void HandleEditRequests() {
        for(int i = editRequests.Count - 1; i >= 0; i--) {
            ChunkEditRequest request = editRequests[i];
            if (!request.InProgress) request.Process(densityTexture, this);
            editRequests.Remove(request);
        }
        UpdateMesh();
    }

    public void Generate(TerrainSettings settings) {
        if (!shadersLoaded) Debug.Log("Compute shader has not been initialized");

        densityTexture = RTUtils.Create3D_RFloat(handler.textureDimensions);
        ComputeDensity(densityTexture, settings);
        UpdateMesh();
    }

    public void UpdateMesh() {
        Vector3[] vertices = ComputeVertices(densityTexture);
        int[] triangles = new int[vertices.Length];
        for (int i = 0; i < triangles.Length; i++) {
            triangles[i] = i;
        }

        MeshInfo meshInfo = new MeshInfo(vertices, triangles);
        targetFilter.mesh = meshInfo.AsMesh();
    }

    //
    // Summery:
    //   Computes the the values of a density texture
    //
    // Parameters:
    //   densityTexture:
    //     empty texture to put density values in
    private void ComputeDensity(RenderTexture densityTexture, TerrainSettings settings) {
        computeDensityShader.SetTexture(0, "_DensityTexture", densityTexture);
        computeDensityShader.SetInt("layer_index", layer.id);
        computeDensityShader.SetFloat("voxel_scale", handler.voxelScale);

        computeDensityShader.SetVector("chunk_origin", origin);

        Vector3Int threads = RTUtils.CalculateThreadAmount(handler.textureDimensions, 8);
        computeDensityShader.Dispatch(0, threads.x, threads.y, threads.z);
    }

    //
    // Summery:
    //   Computes and returns the vertices returned by marching cubes compute shader
    //
    // Parameters:
    //   densityTexture:
    //     texture sent to the marching cubes shader to evaluate
    private Vector3[] ComputeVertices(RenderTexture densityTexture) {
        int maxCubes = handler.textureDimensions.x * handler.textureDimensions.y * handler.textureDimensions.z;
        int maxTris = maxCubes * 5;

        ComputeBuffer vertexBuffer = new ComputeBuffer(maxTris , sizeof(float) * 3 * 3, ComputeBufferType.Append);
        ComputeBuffer triangleCountBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.Raw);

        vertexBuffer.SetCounterValue(0);
        computeVerticesShader.SetTexture(0, "_DensityTexture", densityTexture);
        computeVerticesShader.SetBuffer(0, "_TriangleBuffer", vertexBuffer);
        computeVerticesShader.SetInts("texture_size", handler.textureDimensions.x, handler.textureDimensions.y, handler.textureDimensions.z);
        computeVerticesShader.SetFloat("voxel_scale", handler.voxelScale);
        computeVerticesShader.SetBool("interpolate", true);
        computeVerticesShader.SetFloat("threshold", 0.5f);

        Vector3Int threads = RTUtils.CalculateThreadAmount(handler.textureDimensions, 8);
        //Debug.Log((Vector3)threads + " threads dispatched for ComputeVertices");
        computeVerticesShader.Dispatch(0, threads.x, threads.y, threads.z);

        //Gets the number of times Append was called on the buffer (number of triangles added)
        //then uses that to read the buffer, annoying asf to deal with it like this
        int[] triangleCount = new int[1];
        triangleCountBuffer.SetData(triangleCount);
        ComputeBuffer.CopyCount(vertexBuffer, triangleCountBuffer, 0);
        triangleCountBuffer.GetData(triangleCount);

        int vertexCount = triangleCount[0] * 3;

        Vector3[] result = new Vector3[vertexCount];
        vertexBuffer.GetData(result);

        vertexBuffer.Release();
        triangleCountBuffer.Release();

        return result;
    }

    // 
    // Summery:
    //     Handles unloading all resources related to this chunk
    //     aswwell as releasing any associated textures or buffers
    //
    public void Unload() {
        densityTexture.Release();
        Destroy(this);
        //Dump current edit requests
        //Save chunk
    }

    public Bounds GetBounds() {
        return new Bounds(centre, (Vector3)handler.textureDimensions * handler.voxelScale);
    }

    //
    // Summery:
    //   Loads the resources needed to start compute
    public static void InitializeCompute(TerrainSettings settings) {
        if (shadersLoaded) {
            Debug.Log("Compute shaders have already been initialized");
            return;
        }

        //Could be big so we only set it once
        layerSettingsBuffer = new ComputeBuffer(settings.layers.Length, TerrainLayerSettings.stride);
        layerSettingsBuffer.SetData(settings.layersStruct);

        //Compute Density
        computeDensityShader = Resources.Load<ComputeShader>("Compute/Noise/ChunkNoiseGeneration");
        computeDensityShader.SetBuffer(0, "_ChunkSettings", layerSettingsBuffer);
        computeDensityShader.SetInt("seed", settings.seed);

        //Compute Vertices
        computeVerticesShader = Resources.Load<ComputeShader>("Compute/MCubes/MarchingCube");

        shadersLoaded = true;
    }


    public static void ReleaseBuffers() {
        layerSettingsBuffer.Release();
    }
}
