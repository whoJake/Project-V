using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainChunk
{
    public ActiveState state;
    private List<ChunkEditRequest> editRequests;
    private RenderTexture densityTexture;

    //Layer reference that chunk is a part of
    private readonly TerrainLayer layer;

    //Centre point of chunk
    private readonly Vector3 centre;
    //Number of voxels per axis
    private readonly Vector3Int voxelDimensions;
    private readonly int margin;
    public readonly float voxelScale;

    public Vector3Int textureDimensions { get { return voxelDimensions + (Vector3Int.one * margin * 2); } }
    public Vector3 origin { get { return centre - ((Vector3)textureDimensions * voxelScale / 2f); } }

    private static bool shadersLoaded = false;
    private static ComputeShader computeDensityShader;
    private static ComputeShader computeVerticesShader;

    private static ComputeBuffer layerSettingsBuffer;

    private GameObject target;
    private MeshFilter filter;
    private MeshCollider collider;

    public TerrainChunk(TerrainLayer _layer, Vector3 _centre, Vector3Int _voxelDimensions, int _margin, float _voxelScale, GameObject _target) {
        layer = _layer;
        centre = _centre;
        voxelDimensions = _voxelDimensions;
        margin = _margin;
        voxelScale = _voxelScale;

        target = _target;
        filter = target.GetComponent<MeshFilter>();
        collider = target.GetComponent<MeshCollider>();
        state = layer.state;
        editRequests = new List<ChunkEditRequest>();
    }

    public void Update() {
        switch (state) {
            case ActiveState.Inactive:
                if (target.activeSelf != false) target.SetActive(false);
                if (collider.sharedMesh != null) collider.sharedMesh = null;
                break;

            case ActiveState.Static:
                if (target.activeSelf != true) target.SetActive(true);
                if (collider.sharedMesh != null) collider.sharedMesh = null;
                break;

            case ActiveState.Active:
                if (target.activeSelf != true) target.SetActive(true);
                collider.sharedMesh = filter.mesh;
                if (editRequests.Count != 0) HandleEditRequests();
                break;

            default:
                Debug.Log("Chunk active state is not set. This should never happen");
                break;
        }
    }

    public void MakeEditRequest(ChunkEditRequest request) {
        if (state == ActiveState.Inactive) return;

        editRequests.Add(request);
        Debug.Log("Edit request made");
    }

    private void HandleEditRequests() {
        for(int i = editRequests.Count - 1; i >= 0; i--) {
            ChunkEditRequest request = editRequests[i];
            if (!request.InProgress) request.Process(densityTexture, this);
            editRequests.Remove(request);
        }
        Vector3[] resultVertices = ComputeVertices(densityTexture);
        UpdateMesh(resultVertices);
    }

    public void Generate(TerrainSettings settings) {
        if (!shadersLoaded) Debug.Log("Compute shader has not been initialized");

        //Initialize new texture
        densityTexture = RTUtils.Create3D_RFloat(textureDimensions);

        //Ask noise function for a texture
        ComputeDensity(densityTexture, settings);

        //Run texture through marching cubes compute
        Vector3[] vertices = ComputeVertices(densityTexture);

        UpdateMesh(vertices);
    }

    public void UpdateMesh(Vector3[] vertices) {
        //Create triangles array
        int[] triangles = new int[vertices.Length];
        for (int i = 0; i < triangles.Length; i++) {
            triangles[i] = i;
        }

        MeshInfo meshInfo = new MeshInfo(vertices, triangles);

        //?Remove duplicates
        //MeshMaths.RemoveDuplicateVertices(meshInfo);

        //Give vertices and triangles to mesh filter
        filter.mesh = meshInfo.AsMesh();
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
        computeDensityShader.SetFloat("voxel_scale", voxelScale);

        computeDensityShader.SetVector("chunk_origin", origin);

        Vector3Int threads = RTUtils.CalculateThreadAmount(textureDimensions, 8);
        Debug.Log((Vector3)threads + " threads dispatched for ComputeDensity");
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
        int maxCubes = textureDimensions.x * textureDimensions.y * textureDimensions.z;
        int maxTris = maxCubes * 5;
        //Debug.Log("Max Cubes: " + maxCubes + "\nMax Triangles: " + maxTris);

        ComputeBuffer vertexBuffer = new ComputeBuffer(maxTris , sizeof(float) * 3 * 3, ComputeBufferType.Append);
        ComputeBuffer triangleCountBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.Raw);

        vertexBuffer.SetCounterValue(0);
        computeVerticesShader.SetTexture(0, "_DensityTexture", densityTexture);
        computeVerticesShader.SetBuffer(0, "_TriangleBuffer", vertexBuffer);
        computeVerticesShader.SetInts("texture_size", textureDimensions.x, textureDimensions.y, textureDimensions.z);
        computeVerticesShader.SetFloat("voxel_scale", voxelScale);
        computeVerticesShader.SetBool("interpolate", true);
        computeVerticesShader.SetFloat("threshold", 0.5f);

        Vector3Int threads = RTUtils.CalculateThreadAmount(textureDimensions, 8);
        Debug.Log((Vector3)threads + " threads dispatched for ComputeVertices");
        computeVerticesShader.Dispatch(0, threads.x, threads.y, threads.z);

        //Gets the number of times Append was called on the buffer (number of triangles added)
        int[] triangleCount = new int[1];
        triangleCountBuffer.SetData(triangleCount);
        ComputeBuffer.CopyCount(vertexBuffer, triangleCountBuffer, 0);
        triangleCountBuffer.GetData(triangleCount);

        int vertexCount = triangleCount[0] * 3;

        Vector3[] result = new Vector3[vertexCount];
        vertexBuffer.GetData(result);

        vertexBuffer.Release();
        triangleCountBuffer.Release();
        //densityTexture.Release(); //REMOVE WHEN IMPLEMENTING NEXT STEPS

        return result;
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

    public Bounds GetBounds() {
        return new Bounds(centre, (Vector3)textureDimensions * voxelScale);
    }
}
