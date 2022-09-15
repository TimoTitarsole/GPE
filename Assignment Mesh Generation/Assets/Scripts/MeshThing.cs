using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshThing : MonoBehaviour
{
    private Mesh mesh;

    private int nVerts = 3;

    private int[] tris;

    private Vector2[] uv;

    private Vector3[] verts;

    private void Start()
    {
        mesh = GetComponent<MeshFilter>().mesh;

        verts = new Vector3[nVerts];
        verts[0] = new Vector3(0, 0, 0);
        verts[1] = new Vector3(0, 10, 0);
        verts[2] = new Vector3(5, 5, 0);

        tris = new int[3];
        tris[0] = 0;
        tris[1] = 1;
        tris[2] = 2;
        //tris[3] = 0;
        //tris[4] = 2;
        //tris[5] = 1;

        uv = new Vector2[3];
        uv[0] = new Vector2(0, 0);
        uv[1] = new Vector2(0, 1);
        uv[2] = new Vector2(1, 0.5f);

        mesh.vertices = verts;
        mesh.triangles = tris;
        mesh.uv = uv;
    }

    private void Update()
    {
    }
}