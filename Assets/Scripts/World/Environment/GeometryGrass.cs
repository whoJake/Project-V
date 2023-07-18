using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeometryGrass : MonoBehaviour
{
    public TerrainChunk chunk;
    [SerializeField] private float distanceCullingThreshold = 250f;
    [SerializeField] private int pointsPerTexel = 128;
    [Range(0f, 1f)]
    [SerializeField] private float jitterScale = 1f;

    private ComputeShader pointCreation;
    private static int bladeCount = 0;

    private void Update() {
        if (Vector3.Distance(chunk.handler.mainCamera.transform.position, transform.position) > distanceCullingThreshold * 1.5) return;
        if (pointCreation == null) pointCreation = Resources.Load<ComputeShader>("Compute/Environment/GrassPointCreation");
        if (!chunk.hasHeightMap || chunk.generatingHeightMap)
            return;

        //Create points
        pointCreation.SetTexture(0, "_HeightMap", chunk.data.heightMap);
        pointCreation.SetVector("_CameraWorldPosition", Camera.main.transform.position);
        pointCreation.SetFloat("_DistanceCullingThreshold", distanceCullingThreshold);
        pointCreation.SetVector("_ChunkOrigin", chunk.origin);
        pointCreation.SetVector("_ChunkSize", chunk.bounds.size);
        pointCreation.SetInts("_TextureSize", chunk.handler.textureDimensions.x, chunk.handler.textureDimensions.z);
        pointCreation.SetFloat("_JitterScale", jitterScale);
        pointCreation.SetInt("_PointsPerTexel", pointsPerTexel);
        pointCreation.SetFloat("_BladeHeight", 1.6f);

        Plane[] worldClipPlanes = GeometryUtility.CalculateFrustumPlanes(Camera.main);
        Vector4[] cameraWorldClipPlanes = new Vector4[6];
        for(int i = 0; i < 6; i++) {
            Vector3 n = worldClipPlanes[i].normal;
            float d = worldClipPlanes[i].distance;
            cameraWorldClipPlanes[i] = new Vector4(n.x, n.y, n.z, d);
        }
        pointCreation.SetVectorArray("_CameraWorldClipPlanes", cameraWorldClipPlanes);


        int maxPoints = chunk.handler.textureDimensions.x * chunk.handler.textureDimensions.z * pointsPerTexel;
        ComputeBuffer pointsBuffer = new ComputeBuffer(maxPoints, 3 * sizeof(float), ComputeBufferType.Append);
        ComputeBuffer pointCountBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.Raw);

        pointsBuffer.SetCounterValue(0);
        pointCreation.SetBuffer(0, "_GrassPoints", pointsBuffer);

        Vector3Int threads = RTUtils.CalculateThreadAmount(chunk.handler.textureDimensions - new Vector3Int(1, 1, 1), 8);
        pointCreation.Dispatch(0, threads.x, threads.z, 1);

        int[] pointCount = new int[1];
        pointCountBuffer.SetData(pointCount);
        ComputeBuffer.CopyCount(pointsBuffer, pointCountBuffer, 0);
        pointCountBuffer.GetData(pointCount);
        bladeCount += pointCount[0];

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

    private void LateUpdate() {
        if (bladeCount == 0)
            return;

        Debug.Log("Blades spawned this frame: " + bladeCount);
        bladeCount = 0;
    }

}
