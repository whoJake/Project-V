using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeometryGrass : MonoBehaviour
{
    [SerializeField] private int points;

    void Start()
    {
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
    }
}
