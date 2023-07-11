using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeometryGrass : MonoBehaviour
{
    public TerrainChunk chunk;
    [SerializeField] private float distanceCullingThreshold = 250f;
    [SerializeField] private int pointsPerTexel = 32;
    [Range(0f, 1f)]
    [SerializeField] private float jitterScale = 1f;

    private ComputeShader pointCreation;

    void Start()
    {
        /*
        BoxCollider collider = GetComponent<BoxCollider>();
        Bounds bounds = collider.bounds;
        Destroy(collider);
        Vector3[] vertices = new Vector3[points];
        int[] indices = new int[points];

        for(int i = 0; i < points; i++) {
            float rx = Random.Range(-bounds.extents.x, bounds.extents.x);
            float rz = Random.Range(-bounds.extents.z, bounds.extents.z);

            Vector3 point = transform.position + new Vector3(rx, 0, rz);
            vertices[i] = point;
            indices[i] = i;
        }

        Mesh mesh = new Mesh();
        mesh.SetVertices(vertices);
        mesh.SetIndices(indices, MeshTopology.Points, 0);
        GetComponent<MeshFilter>().mesh = mesh;
        */
    }

    private void Update() {
        if (pointCreation == null) pointCreation = Resources.Load<ComputeShader>("Compute/Environment/GrassPointCreation");
        if (!chunk.hasHeightMap || chunk.generatingHeightMap)
            return;

        //Create points
        pointCreation.SetTexture(0, "_HeightMap", chunk.data.heightMap);
        pointCreation.SetVector("_CameraWorldPosition", Camera.main.transform.position);
        pointCreation.SetFloat("_DistanceCullingThreshold", distanceCullingThreshold);
        pointCreation.SetVector("_ChunkOrigin", chunk.origin);
        pointCreation.SetVector("_ChunkSize", chunk.GetBounds().size);
        pointCreation.SetInts("_TextureSize", chunk.handler.textureDimensions.x, chunk.handler.textureDimensions.z);
        pointCreation.SetFloat("_JitterScale", jitterScale);
        pointCreation.SetInt("_PointsPerTexel", pointsPerTexel);

        int maxPoints = chunk.handler.textureDimensions.x * chunk.handler.textureDimensions.z * pointsPerTexel;
        ComputeBuffer pointsBuffer = new ComputeBuffer(maxPoints, 3 * sizeof(float), ComputeBufferType.Append);
        ComputeBuffer pointCountBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.Raw);

        pointsBuffer.SetCounterValue(0);
        pointCreation.SetBuffer(0, "_GrassPoints", pointsBuffer);

        Vector3Int threads = RTUtils.CalculateThreadAmount(chunk.handler.textureDimensions, 8);
        pointCreation.Dispatch(0, threads.x, threads.z, 1);

        int[] pointCount = new int[1];
        pointCountBuffer.SetData(pointCount);
        ComputeBuffer.CopyCount(pointsBuffer, pointCountBuffer, 0);
        pointCountBuffer.GetData(pointCount);

        Vector3[] grassPoints = new Vector3[pointCount[0]];
        pointsBuffer.GetData(grassPoints);

        pointsBuffer.Release();
        pointCountBuffer.Release();

        
        int[] indicies = new int[pointCount[0]];
        for (int i = 0; i < pointCount[0]; i++) {
            indicies[i] = i;
        }

        Mesh mesh = new Mesh();
        mesh.SetVertices(grassPoints);
        mesh.SetIndices(indicies, MeshTopology.Points, 0);
        GetComponent<MeshFilter>().mesh = mesh;
    }

}
