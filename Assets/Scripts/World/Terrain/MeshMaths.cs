using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MeshMaths
{
    public class MeshInfo {
        public MeshInfo(Vector3[] _vertices, int[] _triangles) {
            vertices = _vertices;
            triangles = _triangles;
        }

        public Vector3[] vertices;
        public int[] triangles;
    }

    public struct Triangle {
        public Triangle(Vector3 _a, Vector3 _b, Vector3 _c) {
            a = _a;
            b = _b;
            c = _c;
        }

        public Vector3 a;
        public Vector3 b;
        public Vector3 c;
    }

    public static class MeshMaths {

        //
        // Summery:
        //      Subdivide a triangle with each side having divisions more points added to it
        //
        public static MeshInfo SubdivideTriangle(Triangle tri, int divisions) {
            Debug.Assert(divisions >= 1, "Cannot subdivide a triangle less than once");

            int maxDepth = divisions + 2;

            Vector3[] leftPoints = SubdivideEdge(tri.a, tri.b, maxDepth);
            Vector3[] rightPoints = SubdivideEdge(tri.a, tri.c, maxDepth);

            int verticesLength = (maxDepth * (maxDepth + 1)) / 2;
            Vector3[] vertices = new Vector3[verticesLength];
            vertices[0] = tri.a;
            int vertexIndexCount = 1;

            List<int> triangles = new List<int>();

            for (int depth = 1; depth < maxDepth; depth++) {
                //Add Vertices
                Vector3 left = leftPoints[depth];
                Vector3 right = rightPoints[depth];

                Vector3[] depthPoints = SubdivideEdge(left, right, depth + 1);
                for (int i = 0; i < depthPoints.Length; i++) {
                    vertices[vertexIndexCount] = depthPoints[i];
                    vertexIndexCount++;
                }

                //Add Triangles
                int leftIndex = (depth * (depth + 1)) / 2;
                int rightIndex = leftIndex + depth;

                int aboveLeftIndex = (depth - 1) * depth / 2;
                int aboveRightIndex = aboveLeftIndex + (depth - 1);

                //Upwards pointing triangles
                for (int i = aboveLeftIndex; i <= aboveRightIndex; i++) {
                    int a = i;
                    int b = depth + i;
                    int c = b + 1;

                    triangles.AddRange(new int[3] { a, b, c });
                }

                //Downwards pointing triangles
                for (int i = leftIndex + 1; i <= rightIndex - 1; i++) {
                    int a = i;
                    int c = i - depth - 1;
                    int b = c + 1;

                    triangles.AddRange(new int[3] { a, b, c });
                }
            }

            return new MeshInfo(vertices, triangles.ToArray());
        }

        //
        // Summery:
        //      Subdivide and return the interpolated points between a and b
        //
        public static Vector3[] SubdivideEdge(Vector3 a, Vector3 b, int vertsOnNewEdge) {
            Debug.Assert(vertsOnNewEdge >= 2, "Cannot subdivide an edge to have less than 2 vertices");

            Vector3[] result = new Vector3[vertsOnNewEdge];
            Vector3 a2b = b - a;
            Vector3 step = a2b / (vertsOnNewEdge - 1);

            for (int i = 0; i < vertsOnNewEdge; i++) {
                result[i] = a + (step * i);
            }
            return result;
        }

        //
        // Summery:
        //      Remove duplicate vertices from a MeshInfo whilst correctly maintaining the triangles
        //
        public static void RemoveDuplicateVertices(MeshInfo mesh) {
            Dictionary<Vector3, int> vertToIndex = new Dictionary<Vector3, int>();
            List<Vector3> vertices = new List<Vector3>();
            int[] triangles = new int[mesh.triangles.Length];

            for(int i = 0; i < mesh.triangles.Length; i++) {
                int vertexIndex = mesh.triangles[i];
                Vector3 vertex = mesh.vertices[vertexIndex];
                if (vertToIndex.ContainsKey(vertex)) {
                    triangles[i] = vertToIndex[vertex];
                } else {
                    vertexIndex = vertices.Count;
                    vertices.Add(vertex);
                    triangles[i] = vertexIndex;
                    vertToIndex.Add(vertex, vertexIndex);
                }
            }
            Debug.Log("Removed " + (mesh.vertices.Length - vertices.Count) + " vertices");
            mesh.vertices = vertices.ToArray();
            mesh.triangles = triangles;
        }
    }
}
