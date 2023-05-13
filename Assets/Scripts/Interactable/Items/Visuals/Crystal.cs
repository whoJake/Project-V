using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class Crystal : MonoBehaviour
{
    public int sides;
    public float topratio;
    public float sheight;
    public float pheight;

    private List<int> topVertices;
    private List<int> bottomVertices;

    private void Start() {
        Mesh crystal = Generate(sides, topratio, sheight, pheight);
        MeshInfo temp = new MeshInfo(crystal.vertices, crystal.triangles);
        MeshMaths.AddDuplicateVertices(temp);
        crystal = temp.AsMesh();
        GetComponent<MeshFilter>().mesh = crystal;
    }

    //
    // Summery:
    //      Creates a mesh of a crystal with nsides and no duplicate vertices
    //      First half of mesh vertices are top vertices, last half are bottom vertices
    //
    // int nsides: number of sides on crystal
    // float topBottomRatio: radius of bottom half of crystal
    // float shaftHeight: height of the main shaft of crystal in mesh space
    // float pointHeight: height of crystal point above the shaft
    //
    public static Mesh Generate(int nsides, float topBottomRatio, float shaftHeight, float pointHeight) {
        Vector3[] vertices = new Vector3[2 * nsides + 2];
        int[] triangles = new int[3 * 4 * nsides];

        //Create vertices
        vertices[0] = new Vector3(0, (shaftHeight / 2) + pointHeight, 0); //Top point
        float angStep = (360 / nsides) * Mathf.Deg2Rad;

        //Top crystal radius
        for(int i = 0; i < nsides; i++) {
            vertices[i + 1] = new Vector3(Mathf.Sin(i * angStep), shaftHeight / 2, Mathf.Cos(i * angStep));
        }

        //Bottom crystal radius
        int voffset = nsides + 1;
        for(int i = 0; i < nsides; i++) {
            vertices[voffset + i] = new Vector3(Mathf.Sin(i * angStep) * topBottomRatio, -shaftHeight / 2, Mathf.Cos(i * angStep) * topBottomRatio);
        }

        vertices[^1] = new Vector3(0, -((shaftHeight / 2) + pointHeight), 0); //Bottom point

        //Create Triangles

        //Top triangles
        for(int i = 0; i < nsides; i++) {
            int start = i * 3;
            triangles[start] = 0;
            triangles[start + 1] = i + 1;
            triangles[start + 2] = i == nsides-1 ? 1 : i + 2; // This accounts for it wrapping back around
        }

        //Shaft
        int toffset = nsides * 3;
        for(int i = 0; i < nsides; i++) {
            int start = toffset + i * 6;
            //Down pointing tri
            triangles[start] = nsides + i + 1;
            triangles[start + 1] = i + 2;
            triangles[start + 2] = i + 1;
            //Up pointing tri
            triangles[start + 3] = i + 2;
            triangles[start + 4] = i == nsides - 1 ? 1 : nsides + i + 1; //Not entirely sure about these side cases but it works
            triangles[start + 5] = i == nsides - 1 ? nsides : nsides + i + 2; //Same here
        }

        //Bottom triangles
        toffset = nsides * 9;
        for(int i = 0; i < nsides; i++) {
            int start = toffset + i * 3;
            triangles[start] = 2 * nsides + 1; //Bottom point
            triangles[start + 1] = i == nsides - 1 ? nsides + 1 : nsides + i + 2;
            triangles[start + 2] = nsides + i + 1;
        }

        Mesh result = new Mesh();
        result.vertices = vertices;
        result.triangles = triangles;
        result.RecalculateBounds();
        result.RecalculateNormals();
        result.RecalculateTangents();
        return result;
    }
}

