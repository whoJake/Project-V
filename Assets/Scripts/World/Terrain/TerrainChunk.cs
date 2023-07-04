using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class TerrainChunk : MonoBehaviour
{
    private ActiveState state;
    private List<ChunkEditRequest> editRequests;
    private RenderTexture densityTexture;

    public TerrainLayer layer;
    public TerrainHandler handler { get { return layer.handler; } }

    private Vector3 centre;

    private MeshFilter targetFilter;
    [SerializeField]
    private MeshCollider targetCollider;

    public Vector3 origin { get { return centre - ((Vector3)handler.textureDimensions * handler.voxelScale / 2f); } }

    public bool updatingMesh;
    public bool generating;
    public bool generated { get { return densityTexture != null; } }

    private static bool shadersLoaded = false;
    private static ComputeShader computeVerticesShader;

    public TerrainChunk Initialize(TerrainLayer _layer, Vector3 _centre, ActiveState _state) {
        targetFilter = GetComponent<MeshFilter>();
        targetCollider = GetComponent<MeshCollider>();

        layer = _layer;
        centre = _centre;
        state = _state;
        editRequests = new List<ChunkEditRequest>();

        Generate();

        return this;
    }

    private void Update() {
        bool rendererEnabled;
        Mesh colliderMesh;

        switch (state) {
            case ActiveState.Inactive:
                rendererEnabled = false;
                colliderMesh = null;
                break;

            case ActiveState.Static:
                rendererEnabled = true;
                colliderMesh = null;
                break;

            case ActiveState.Active:
                rendererEnabled = true;
                colliderMesh = targetFilter.mesh;
                if (editRequests.Count != 0) HandleEditRequests();
                break;

            default:
                Debug.Log("Chunk active state is not set. This should never happen");
                rendererEnabled = false;
                colliderMesh = null;
                break;
        }

        GetComponent<MeshRenderer>().enabled = rendererEnabled;
        targetCollider.sharedMesh = colliderMesh;
    }

    private void Generate() {
        if (!shadersLoaded) Debug.Log("Compute shader has not been initialized");

        generating = true;
        densityTexture = RTUtils.Create3D_RFloat(handler.textureDimensions);
        StartCoroutine(ComputeDensity(densityTexture));
    }

    public void UpdateMesh() {
        if (!generated) return;
        StartCoroutine(CalculateVerticesAndApplyToMesh());
    }

    //
    // Summery:
    //     Uses the current density texture to calculate the vertices using marching cubes algorithm and then applies these to the mesh
    //
    private IEnumerator CalculateVerticesAndApplyToMesh() {
        int maxCubes = handler.textureDimensions.x * handler.textureDimensions.y * handler.textureDimensions.z;
        int maxTris = maxCubes * 5;

        ComputeBuffer vertexBuffer = new ComputeBuffer(maxTris, sizeof(float) * 3 * 3, ComputeBufferType.Append);
        ComputeBuffer triangleCountBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.Raw);

        updatingMesh = true;

        vertexBuffer.SetCounterValue(0);
        computeVerticesShader.SetTexture(0, "_DensityTexture", densityTexture);
        computeVerticesShader.SetBuffer(0, "_TriangleBuffer", vertexBuffer);
        computeVerticesShader.SetInts("texture_size", handler.textureDimensions.x, handler.textureDimensions.y, handler.textureDimensions.z);
        computeVerticesShader.SetFloat("voxel_scale", handler.voxelScale);
        computeVerticesShader.SetBool("interpolate", true);
        computeVerticesShader.SetFloat("threshold", 0.5f);

        Vector3Int threads = RTUtils.CalculateThreadAmount(handler.textureDimensions, 8);
        computeVerticesShader.Dispatch(0, threads.x, threads.y, threads.z);

        if(handler.yieldOnChunk)
            yield return ComputeUtils.WaitForResource(vertexBuffer);

        //Gets the number of times Append was called on the buffer (number of triangles added)
        //then uses that to read the buffer, annoying asf to deal with it like this
        int[] triangleCount = new int[1];
        triangleCountBuffer.SetData(triangleCount);
        ComputeBuffer.CopyCount(vertexBuffer, triangleCountBuffer, 0);
        triangleCountBuffer.GetData(triangleCount);

        int vertexCount = triangleCount[0] * 3;

        Vector3[] vertices = new Vector3[vertexCount];
        vertexBuffer.GetData(vertices);

        vertexBuffer.Release();
        triangleCountBuffer.Release();

        //Build triangle array which is actually just an array of length vertices and value as its position in the array lul
        int[] triangles = new int[vertices.Length];
        for (int i = 0; i < triangles.Length; i++) {
            triangles[i] = i;
        }
        MeshInfo meshInfo = new MeshInfo(vertices, triangles);
        targetFilter.mesh = meshInfo.AsMesh();
        updatingMesh = false;
    }

    //
    // Summery:
    //   Computes the the values of a density texture
    //
    // Parameters:
    //   densityTexture:
    //     empty texture to put density values in
    private IEnumerator ComputeDensity(RenderTexture densityTexture) {
        layer.generator.Generate(ref densityTexture, this, handler.settings.seed);

        if(handler.yieldOnChunk)
            yield return ComputeUtils.WaitForResource(densityTexture);

        yield return CalculateVerticesAndApplyToMesh();

        generating = false;
    }

    //
    // Summery:
    //     Adds a chunk edit request to the queue of requests to be processed
    //
    public void MakeEditRequest(ChunkEditRequest request) {
        if (state == ActiveState.Inactive) return;

        editRequests.Add(request);
    }

    //
    // Summery:
    //     Processes all current requests at once then updates the mesh
    //
    private void HandleEditRequests() {
        for (int i = editRequests.Count - 1; i >= 0; i--) {
            ChunkEditRequest request = editRequests[i];
            if (!request.InProgress) request.Process(densityTexture, this);
            editRequests.Remove(request);
        }
        UpdateMesh();
    }

    // 
    // Summery:
    //     Handles unloading all resources related to this chunk
    //     aswwell as releasing any associated textures or buffers
    //
    public void Unload(bool fromEditor = false) {
        if(densityTexture)
            densityTexture.Release();

        if (fromEditor)
            DestroyImmediate(gameObject);
        else
            Destroy(gameObject);

        //Dump current edit requests
        //Save chunk
    }

    private void OnDestroy() {
        if(densityTexture)
            densityTexture.Release();
    }

    public void SetState(ActiveState state) {
        this.state = state;
    }

    public Bounds GetBounds() {
        return new Bounds(centre, (Vector3)handler.textureDimensions * handler.voxelScale);
    }

    //
    // Summery:
    //   Loads the resources needed to start compute
    //
    public static void InitializeCompute() {
        if (shadersLoaded)
            return;

        //Compute Vertices
        computeVerticesShader = Resources.Load<ComputeShader>("Compute/MCubes/MarchingCube");

        shadersLoaded = true;
    }

    public static void ReleaseBuffers() {
        //layerSettingsBuffer.Release();
    }
}
