using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrozenBase : MonoBehaviour
{
    [SerializeField]
    [Range(0f, 1f)] private float age;

    [SerializeField]
    private Gradient colorGradient;

    [SerializeField]
    [Min(0f)] private float baseWidth;

    [SerializeField]
    [Min(0f)] private float crystalHeight;

    public bool recalc;

    void Start()
    {
        Crystal crystal = new Crystal(transform.position, baseWidth, crystalHeight);
        GetComponent<MeshFilter>().sharedMesh = crystal.ToMesh();
    }

    class Crystal {
        private Vector3[] vertices;
        private int[] triangles;
        private bool hasChanged;
        private Mesh mesh;

        public Crystal(Vector3 basePosition, float baseWidth, float height) {
            //This crystal should be changed or choose between a set of mesh's
            vertices = new Vector3[4]{ basePosition + new Vector3(0, height, 0),
                                       basePosition + new Vector3(0, 0, baseWidth),
                                       basePosition + new Vector3(0.866f * baseWidth, 0, -0.5f * baseWidth),
                                       basePosition + new Vector3(-0.866f * baseWidth, 0, -0.5f * baseWidth)};
            triangles = new int[12] { 1, 2, 0,
                                      2, 3, 0,
                                      3, 1, 0,
                                      3, 2, 1 };
            hasChanged = true;

        }

        public void TranslateTop(Vector3 translation) {
            //Foreach vertex in top group
            //Translate by amount
            //Update mesh
        }

        public Mesh ToMesh() {
            if (!hasChanged) return mesh;
            else {
                MeshInfo info = new MeshInfo(vertices, triangles);
                MeshMaths.AddDuplicateVertices(info);
                mesh = info.AsMesh();
                hasChanged = false;
                return mesh;
            }
        }

    }

    private void OnValidate() {
        if (recalc) {
            recalc = false;
            Mesh mesh = GetComponent<MeshFilter>().sharedMesh;
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();
        }
    }

}
