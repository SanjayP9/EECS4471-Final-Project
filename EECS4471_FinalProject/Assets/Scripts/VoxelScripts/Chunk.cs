using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk
{
    public const int CHUNK_SIZE = 4;
    Voxel[,,] voxels = new Voxel[CHUNK_SIZE, CHUNK_SIZE, CHUNK_SIZE];

    List<Vector3> vertices = new List<Vector3>();
    List<int> triangles = new List<int>();

    private Mesh mesh;
    private MeshFilter meshFilter;


    public Mesh GetMesh()
    {
        return mesh;
    }

    public Chunk(Transform transform, Material mat)
    {
        // generate vertices based on voxel data
        // generate triangles based on vertices

        for (int x = 0; x < CHUNK_SIZE; x++)
        {
            for (int y = 0; y < CHUNK_SIZE; y++)
            {
                for (int z = 0; z < CHUNK_SIZE; z++)
                {
                    voxels[x, y, z] = new Voxel(false);
                    Mat


                }
            }
        }

        mesh = new Mesh();
        mesh.MarkDynamic();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();
    }
}
