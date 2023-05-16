using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainChunk
{
    private static bool computeInitialized = false;
    private static ComputeShader computeShader;

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
        //Create triangles array
        //?Remove duplicates
        //Give vertices and triangles to mesh filter and collider
    }

    private void ComputeDensity(RenderTexture densityTexture) {
        computeShader.SetTexture(0, "_DensityTexture", densityTexture);
        computeShader.SetInt("layer_index", layerIndex);
        computeShader.SetFloat("voxel_scale", voxelScale);

        computeShader.SetVector("chunk_origin", origin);

        int threadsX = Mathf.CeilToInt(textureDimensions.x / 8f);
        int threadsY = Mathf.CeilToInt(textureDimensions.y / 8f);
        int threadsZ = Mathf.CeilToInt(textureDimensions.z / 8f);
        computeShader.Dispatch(0, threadsX, threadsY, threadsZ);
    }

    public static void InitializeCompute(TerrainSettings settings) {
        if (computeInitialized) {
            Debug.Log("Terrain chunk noise generation compute shader has already been initialized");
            return;
        }

        computeShader = Resources.Load<ComputeShader>("Compute/Noise/ChunkNoiseGeneration");
        ComputeBuffer settingsBuffer = new ComputeBuffer(settings.layers.Length, TerrainLayerSettings.stride);
        settingsBuffer.SetData(settings.layersStruct);

        computeShader.SetBuffer(0, "_ChunkSettings", settingsBuffer);
        computeShader.SetInt("seed", settings.seed);

        computeInitialized = true;
    }
}
