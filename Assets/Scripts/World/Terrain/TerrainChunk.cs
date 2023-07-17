using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainChunk : MonoBehaviour
{
    [System.Serializable]
    public struct ChunkData {
        public RenderTexture densityTexture;
        public RenderTexture heightMap;
    }

    private bool stateChanged;
    [SerializeField]
    private ActiveState state;
    private List<ChunkEditRequest> editRequests;
    public ChunkData data;

    public TerrainLayer layer;
    public TerrainHandler handler { get { return layer.handler; } }

    public Vector3 centre;

    private MeshFilter targetFilter;
    [SerializeField]
    private MeshCollider targetCollider;

    public Vector3 origin { get { return centre - ((Vector3)handler.textureDimensions * handler.voxelScale / 2f); } }

    public bool updatingMesh;
    public bool generating;
    public bool generated { get { return data.densityTexture != null; } }

    public bool generatingHeightMap = false;
    public bool generatedHeightMap = false;
    public bool hasHeightMap = false;

    public bool showBounds = false;
    static int gid = 0;
    public int id;

    private bool updateGrass;

    private static bool shadersLoaded = false;
    private static ComputeShader computeVerticesShader;

    public TerrainChunk Initialize(TerrainLayer _layer, Vector3 _centre, ActiveState _state) {
        targetFilter = GetComponent<MeshFilter>();
        targetCollider = GetComponent<MeshCollider>();

        layer = _layer;
        centre = _centre;
        state = _state;
        editRequests = new List<ChunkEditRequest>();
        id = gid;
        gid++;

        Generate();
        if (handler.enableGrass)
            GenerateGrassObject();

        return this;
    }

    private void GenerateGrassObject() {
        GameObject grass = new GameObject("Grass", typeof(MeshFilter), typeof(MeshRenderer), typeof(GeometryGrass));
        grass.transform.parent = transform;
        grass.transform.localPosition = Vector3.zero;
        grass.GetComponent<MeshRenderer>().material = handler.grassMaterial;
        grass.GetComponent<GeometryGrass>().chunk = this;
    }

    private void Update() {
        if(handler.enableGrass)
            transform.GetChild(0).gameObject.SetActive(updateGrass);

        if (state == ActiveState.Active && editRequests.Count != 0) HandleEditRequests();
    }

    private void Generate() {
        if (!shadersLoaded) Debug.Log("Compute shader has not been initialized");

        generating = true;
        data.densityTexture = RTUtils.Create3D_R8(handler.textureDimensions);
        StartCoroutine(ComputeDensity(data.densityTexture));
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
        computeVerticesShader.SetTexture(0, "_DensityTexture", data.densityTexture);
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

        ActiveState temp = state;
        state = ActiveState.Inactive;
        SetState(temp);

        if(handler.enableGrass)
            StartCoroutine(ComputeHeightMap(targetFilter.sharedMesh.vertices, targetFilter.sharedMesh.normals));
    }

    private IEnumerator ComputeHeightMap(Vector3[] positions, Vector3[] normals) {
        generatingHeightMap = true;
        Texture2D cpuHeightMap = new Texture2D(handler.textureDimensions.x, handler.textureDimensions.z, TextureFormat.R8, false);
        //Initialize
        for(int x = 0; x < handler.textureDimensions.x; x++) {
            for(int y = 0; y < handler.textureDimensions.z; y++) {
                cpuHeightMap.SetPixel(x, y, new Color(0, 0, 0, 0));
            }
        }

        for(int i = 0; i < positions.Length; i++) {
            if (i % 25 == 0)
                if(handler.yieldOnChunk) yield return null; //Wait a frame between every 25 calculations ig?

            if (Vector3.Dot(Vector3.up, normals[i]) <= 0.6)
                continue;

            Bounds bounds = GetBounds();
            Vector2Int texturePos = new Vector2Int(
                Mathf.FloorToInt(((positions[i].x + bounds.extents.x) / bounds.size.x) * handler.textureDimensions.x),
                Mathf.FloorToInt(((positions[i].z + bounds.extents.z) / bounds.size.z) * handler.textureDimensions.z)
                );

            float currentHeight = cpuHeightMap.GetPixel(texturePos.x, texturePos.y).r;
            float height = positions[i].y;
            float chunkNormalizedHeight = (height + bounds.extents.y) / bounds.size.y;

            //Debug.Log("CHUNKID: " + id + "\nSample Point: " + texturePos + "\nHeight :" + height + "\nChunkNormalizedHeight :" + chunkNormalizedHeight + "\nCurrentPixelHeight: " + currentHeight);

            //Debug.Log(currentHeight + ": currentHeight");
            //Debug.Log(currentHeight + ": chunkNormalizedHeight");
            float val = Mathf.Max(currentHeight, chunkNormalizedHeight);
            
            cpuHeightMap.SetPixel(texturePos.x, texturePos.y, new Color(val, 0, 0, 0));
            hasHeightMap = true;
        }

        if (hasHeightMap) {
            cpuHeightMap.Apply();
            data.heightMap = RTUtils.Create2D_R8(new Vector2Int(handler.textureDimensions.x, handler.textureDimensions.z));
            Graphics.Blit(cpuHeightMap, data.heightMap);
        }
        generatingHeightMap = false;
        generatedHeightMap = true;
        yield break;
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
            if (!request.InProgress) request.Process(data.densityTexture, this);
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
        if(data.densityTexture)
            data.densityTexture.Release();

        if (fromEditor)
            DestroyImmediate(gameObject);
        else
            Destroy(gameObject);

        //Dump current edit requests
        //Save chunk
    }

    private void OnDestroy() {
        if(data.densityTexture)
            data.densityTexture.Release();
    }

    public void SetState(ActiveState state) {
        if (this.state == state)
            return;

        this.state = state;

        bool rendererEnabled;
        Mesh colliderMesh;

        switch (state) {
            case ActiveState.Inactive:
                rendererEnabled = false;
                colliderMesh = null;
                updateGrass = false;
                break;

            case ActiveState.Static_NoGrass:
                rendererEnabled = true;
                colliderMesh = null;
                updateGrass = false;
                break;

            case ActiveState.Static:
                rendererEnabled = true;
                colliderMesh = null;
                updateGrass = true;
                break;

            case ActiveState.Active:
                rendererEnabled = true;
                colliderMesh = targetFilter.sharedMesh;
                updateGrass = true;
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
        gid = 0;
        shadersLoaded = true;
    }

    public static void ReleaseBuffers() {
        gid = 0;
    }

    private void OnDrawGizmosSelected() {
        if (!showBounds)
            return;

        Bounds bounds = GetBounds();
        Gizmos.DrawWireCube(transform.position, bounds.size);
    }

}
