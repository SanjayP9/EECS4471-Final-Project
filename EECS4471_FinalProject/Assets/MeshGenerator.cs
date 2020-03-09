using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class MeshGenerator : MonoBehaviour
{
    Mesh mesh;
    Vector3[] vertices;
    int[] triangles;

    // Start is called before the first frame update
    void Start()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        mesh.MarkDynamic();

        int size = 10;
        vertices = new Vector3[8];
        triangles = new int[36];

        vertices[0] = new Vector3(transform.position.x - 0.5f, transform.position.y - 0.5f, transform.position.z - 0.5f);
        vertices[1] = new Vector3(transform.position.x + 0.5f, transform.position.y - 0.5f, transform.position.z - 0.5f);
        vertices[2] = new Vector3(transform.position.x - 0.5f, transform.position.y + 0.5f, transform.position.z - 0.5f);
        vertices[3] = new Vector3(transform.position.x + 0.5f, transform.position.y + 0.5f, transform.position.z - 0.5f);

        vertices[4] = new Vector3(transform.position.x - 0.5f, transform.position.y - 0.5f, transform.position.z + 0.5f);
        vertices[5] = new Vector3(transform.position.x + 0.5f, transform.position.y - 0.5f, transform.position.z + 0.5f);
        vertices[6] = new Vector3(transform.position.x - 0.5f, transform.position.y + 0.5f, transform.position.z + 0.5f);
        vertices[7] = new Vector3(transform.position.x + 0.5f, transform.position.y + 0.5f, transform.position.z + 0.5f);

        triangles[0] = 0;
        triangles[1] = 2;
        triangles[2] = 1;

        triangles[3] = 2;
        triangles[4] = 3;
        triangles[5] = 1;

        triangles[6] = 6;
        triangles[7] = 4;
        triangles[8] = 5;

        triangles[9] = 7;
        triangles[10] = 6;
        triangles[11] = 5;

        triangles[12] = 1;
        triangles[13] = 3;
        triangles[14] = 7;

        triangles[15] = 1;
        triangles[16] = 7;
        triangles[17] = 5;

        triangles[18] = 2;
        triangles[19] = 0;
        triangles[20] = 6;

        triangles[21] = 6;
        triangles[22] = 0;
        triangles[23] = 4;

        triangles[24] = 2;
        triangles[25] = 7;
        triangles[26] = 3;

        triangles[27] = 2;
        triangles[28] = 6;
        triangles[29] = 7;

        triangles[30] = 0;
        triangles[31] = 1;
        triangles[32] = 5;

        triangles[33] = 0;
        triangles[34] = 5;
        triangles[35] = 4;

        // 6 7
        // 4 5

        // 2 3
        // 0 1

        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnDrawGizmos()
    {
        if (vertices != null)
        {
            foreach (Vector3 vert in vertices)
            {
                Gizmos.DrawSphere(vert + transform.position, 0.01f);
            }
        }
    }
}
