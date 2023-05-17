using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainChunk
{
    private static bool computeInitialized = false;
    private static ComputeShader computeDensityShader;
    private static ComputeShader computeVerticesShader;

    private readonly Vector3 centre; //Centre point of chunk
    private readonly Vector3Int voxelDimensions; //Number of voxels per each axis
    private readonly int margin;
    private readonly float voxelScale; //Size of each voxel in world space

    private readonly int layerIndex;

    private Vector3Int textureDimensions { get { return voxelDimensions + (Vector3Int.one * margin * 2); } }

    private Vector3 origin { get { return centre - ((Vector3)textureDimensions * voxelScale / 2f); } }

    private MeshFilter filter;
    private MeshCollider collider;

    public TerrainChunk(int _layerIndex, Vector3 _centre, Vector3Int _voxelDimensions, int _margin, float _voxelScale, GameObject owner) {
        layerIndex = _layerIndex;
        centre = _centre;
        voxelDimensions = _voxelDimensions;
        margin = _margin;
        voxelScale = _voxelScale;
        filter = owner.GetComponent<MeshFilter>();
        collider = owner.GetComponent<MeshCollider>();
    }

    public void Generate() {
        if (!computeInitialized) Debug.Log("Compute shader has not been initialized");

        //Initialize new texture
        RenderTexture densityTexture = RTUtils.Create3D_RFloat(textureDimensions);

        //Ask noise function for a texture
        ComputeDensity(densityTexture);

        //Run texture through marching cubes compute
        Vector3[] vertices = ComputeVertices(densityTexture);

        //Create triangles array
        int[] triangles = new int[vertices.Length];
        for (int i = 0; i < triangles.Length; i++) {
            triangles[i] = i;
        }

        MeshInfo meshInfo = new MeshInfo(vertices, triangles);

        //?Remove duplicates
        //MeshMaths.RemoveDuplicateVertices(meshInfo);

        //Give vertices and triangles to mesh filter and collider
        filter.mesh = meshInfo.AsMesh();
        
    }

    //
    // Summery:
    //   Computes the the values of a density texture
    //
    // Parameters:
    //   densityTexture:
    //     empty texture to put density values in
    private void ComputeDensity(RenderTexture densityTexture) {
        computeDensityShader.SetTexture(0, "_DensityTexture", densityTexture);
        computeDensityShader.SetInt("layer_index", layerIndex);
        computeDensityShader.SetFloat("voxel_scale", voxelScale);

        computeDensityShader.SetVector("chunk_origin", origin);

        Vector3Int threads = CalculateThreadAmount(textureDimensions, 8);
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
        Debug.Log("Max Cubes: " + maxCubes + "\nMax Triangles: " + maxTris);

        ComputeBuffer vertexBuffer = new ComputeBuffer(maxTris , sizeof(float) * 3 * 3, ComputeBufferType.Append);
        ComputeBuffer triangleCountBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.Raw);

        vertexBuffer.SetCounterValue(0);
        computeVerticesShader.SetTexture(0, "_DensityTexture", densityTexture);
        computeVerticesShader.SetBuffer(0, "_TriangleBuffer", vertexBuffer);
        computeVerticesShader.SetInts("texture_size", textureDimensions.x, textureDimensions.y, textureDimensions.z);
        computeVerticesShader.SetFloat("voxel_scale", voxelScale);
        computeVerticesShader.SetBool("interpolate", true);
        computeVerticesShader.SetFloat("threshold", -0.2f);

        Vector3Int threads = CalculateThreadAmount(textureDimensions, 8);
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
        densityTexture.Release(); //REMOVE WHEN IMPLEMENTING NEXT STEPS

        return result;
    }

    //
    // Summery:
    //   Loads the resources needed to start compute
    public static void InitializeCompute(TerrainSettings settings) {
        if (computeInitialized) {
            Debug.Log("Compute shaders have already been initialized");
            return;
        }

        //Compute Density
        computeDensityShader = Resources.Load<ComputeShader>("Compute/Noise/ChunkNoiseGeneration");
        ComputeBuffer settingsBuffer = new ComputeBuffer(settings.layers.Length, TerrainLayerSettings.stride); //Probably move this so that settingsBuffer can be flushed
        settingsBuffer.SetData(settings.layersStruct);

        computeDensityShader.SetBuffer(0, "_ChunkSettings", settingsBuffer);
        computeDensityShader.SetInt("seed", settings.seed);

        //Compute Vertices
        computeVerticesShader = Resources.Load<ComputeShader>("Compute/MCubes/MarchingCube");

        computeInitialized = true;
    }

    //
    // Summery:
    //   Calculates the number of threads to dispatch for a given texture size
    //
    // Parameters:
    //   size:
    //     dimensions of texture or buffer
    //   threadAmount:
    //     number of threads per block defined in the compute shader
    private Vector3Int CalculateThreadAmount(Vector3 size, int threadAmount) {
        return new Vector3Int {
            x = Mathf.CeilToInt(size.x / threadAmount),
            y = Mathf.CeilToInt(size.y / threadAmount),
            z = Mathf.CeilToInt(size.z / threadAmount)
        };
    }
}
