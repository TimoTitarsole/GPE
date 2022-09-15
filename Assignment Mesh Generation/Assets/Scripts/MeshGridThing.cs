using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshGridThing : MonoBehaviour
{
    [SerializeField] private int gridSizeX, gridSizeY;

    private float loopI = 0;
    private Mesh mesh;

    private int[] tris;
    private Vector2[] uv;
    private Vector3[] verts;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.black;
        for (int i = 0; i < verts.Length; i++)
        {
            Gizmos.DrawSphere(verts[i], 0.1f);
        }
    }

    private void Start()
    {
        mesh = GetComponent<MeshFilter>().mesh;

        verts = new Vector3[(gridSizeX + 1) * (gridSizeY + 1)];
        uv = new Vector2[verts.Length];

        int trisAmount = (gridSizeX * gridSizeY * 6);
        tris = new int[trisAmount * 2];

        UpdateGrid();
    }

    private void Update()
    {
        UpdateGrid();
    }

    private void UpdateGrid()
    {
        for (int i = 0, y = 0; y <= gridSizeY; y++)
        {
            for (int x = 0; x <= gridSizeX; x++, i++)
            {
                float height = Mathf.PerlinNoise(x * .3f, (y + loopI) * .3f);

                verts[i] = new Vector3(x, height, y);
                uv[i] = new Vector2((float)x / gridSizeX, ((float)y / gridSizeY) + loopI * .01f);
            }
        }
        loopI += 0.01f;

        for (int tri = 0, vert = 0, y = 0; y < gridSizeY; y++, vert++)
        {
            for (int x = 0; x < gridSizeX; x++, tri = tri + 6, vert++)
            {
                tris[tri] = vert;
                tris[tri + 1] = vert + gridSizeX + 1;
                tris[tri + 2] = vert + 1;
                tris[tri + 3] = tris[tri + 2];
                tris[tri + 4] = tris[tri + 1];
                tris[tri + 5] = vert + gridSizeX + 2;
            }
        }

        mesh.vertices = verts;
        mesh.triangles = tris;
        mesh.uv = uv;
    }
}