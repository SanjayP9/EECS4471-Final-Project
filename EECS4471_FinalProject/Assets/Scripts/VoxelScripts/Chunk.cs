using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Direction = Voxel.Direction;

public class Chunk
{
    public const int CHUNK_SIZE = 4;
    Voxel[,,] voxels = new Voxel[CHUNK_SIZE, CHUNK_SIZE, CHUNK_SIZE];

    private Mesh mesh;

    private const float VOXEL_SIZE = Voxel.VOXEL_SIZE;

    byte[,,] voxelArray = new byte[CHUNK_SIZE, CHUNK_SIZE, CHUNK_SIZE];
    List<Vector3> vertices;
    List<int> triangles;

    Transform transform;

    public Mesh GetMesh()
    {
        return mesh;
    }

    public Chunk(Transform transform, Material mat)
    {
        mesh = new Mesh();
        mesh.MarkDynamic();
        this.transform = transform;
        RecomputeMesh();
    }

    public void RecomputeMesh()
    {
        for (int x = 0; x < voxelArray.GetLength(0); x++)
        {
            for (int y = 0; y < voxelArray.GetLength(0); y++)
            {
                for (int z = 0; z < voxelArray.GetLength(0); z++)
                {
                    if (z % 2 == 0 || y % 2 == 0 || x % 2 == 0)
                    {
                        voxelArray[x, y, z] = 1;
                    }
                }
            }
        }

        vertices = new List<Vector3>();
        triangles = new List<int>();

        for (int i = 0, x = 0; x < voxelArray.GetLength(0); x++)
        {
            for (int y = 0; y < voxelArray.GetLength(0); y++)
            {
                for (int z = 0; z < voxelArray.GetLength(0); z++)
                {
                    if (voxelArray[x, y, z] == 1)
                    {
                        GenerateVoxel(x, y, z, new Vector3(transform.position.x + (x * VOXEL_SIZE), transform.position.y + (y * VOXEL_SIZE), transform.position.z + (z * VOXEL_SIZE)));
                    }
                }
            }
        }

        mesh.Clear();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
    }

    private void GenerateVoxel(int x, int y, int z, Vector3 pos)
    {
        int dir = 0;

        if (x + 1 >= voxelArray.GetLength(0) || voxelArray[x + 1, y, z] == 0)
        {
            GenerateFace(pos, Direction.Right);
        }

        if (x - 1 < 0 || voxelArray[x - 1, y, z] == 0)
        {
            GenerateFace(pos, Direction.Left);
        }

        if (y + 1 >= voxelArray.GetLength(0) || voxelArray[x, y + 1, z] == 0)
        {
            GenerateFace(pos, Direction.Top);
        }

        if (y - 1 < 0 || voxelArray[x, y - 1, z] == 0)
        {
            GenerateFace(pos, Direction.Bottom);
        }

        if (z + 1 >= voxelArray.GetLength(0) || voxelArray[x, y, z + 1] == 0)
        {
            GenerateFace(pos, Direction.Forward);
        }

        if (z - 1 < 0 || voxelArray[x, y, z - 1] == 0)
        {
            GenerateFace(pos, Direction.Back);
        }
    }

    private void GenerateFace(Vector3 pos, Direction dir)
    {
        switch (dir)
        {
            case Direction.Top:
                vertices.Add(new Vector3(pos.x, pos.y, pos.z + VOXEL_SIZE));
                vertices.Add(new Vector3(pos.x + VOXEL_SIZE, pos.y, pos.z + VOXEL_SIZE));
                vertices.Add(new Vector3(pos.x + VOXEL_SIZE, pos.y, pos.z));
                vertices.Add(new Vector3(pos.x, pos.y, pos.z));
                break;
            case Direction.Bottom:
                vertices.Add(new Vector3(pos.x, pos.y - VOXEL_SIZE, pos.z));
                vertices.Add(new Vector3(pos.x + VOXEL_SIZE, pos.y - VOXEL_SIZE, pos.z));
                vertices.Add(new Vector3(pos.x + VOXEL_SIZE, pos.y - VOXEL_SIZE, pos.z + VOXEL_SIZE));
                vertices.Add(new Vector3(pos.x, pos.y - VOXEL_SIZE, pos.z + VOXEL_SIZE));
                break;
            case Direction.Left:
                vertices.Add(new Vector3(pos.x, pos.y - VOXEL_SIZE, pos.z + VOXEL_SIZE));
                vertices.Add(new Vector3(pos.x, pos.y, pos.z + VOXEL_SIZE));
                vertices.Add(new Vector3(pos.x, pos.y, pos.z));
                vertices.Add(new Vector3(pos.x, pos.y - VOXEL_SIZE, pos.z));
                break;
            case Direction.Right:
                vertices.Add(new Vector3(pos.x + VOXEL_SIZE, pos.y - VOXEL_SIZE, pos.z));
                vertices.Add(new Vector3(pos.x + VOXEL_SIZE, pos.y, pos.z));
                vertices.Add(new Vector3(pos.x + VOXEL_SIZE, pos.y, pos.z + VOXEL_SIZE));
                vertices.Add(new Vector3(pos.x + VOXEL_SIZE, pos.y - VOXEL_SIZE, pos.z + VOXEL_SIZE));
                break;
            case Direction.Forward:
                vertices.Add(new Vector3(pos.x + VOXEL_SIZE, pos.y - VOXEL_SIZE, pos.z + VOXEL_SIZE));
                vertices.Add(new Vector3(pos.x + VOXEL_SIZE, pos.y, pos.z + VOXEL_SIZE));
                vertices.Add(new Vector3(pos.x, pos.y, pos.z + VOXEL_SIZE));
                vertices.Add(new Vector3(pos.x, pos.y - VOXEL_SIZE, pos.z + VOXEL_SIZE));
                break;
            case Direction.Back:
                vertices.Add(new Vector3(pos.x, pos.y - VOXEL_SIZE, pos.z));
                vertices.Add(new Vector3(pos.x, pos.y, pos.z));
                vertices.Add(new Vector3(pos.x + VOXEL_SIZE, pos.y, pos.z));
                vertices.Add(new Vector3(pos.x + VOXEL_SIZE, pos.y - VOXEL_SIZE, pos.z));
                break;
        }

        triangles.Add(vertices.Count - 4); // vertex 0
        triangles.Add(vertices.Count - 3); // vertex 1
        triangles.Add(vertices.Count - 2); // vertex 2

        triangles.Add(vertices.Count - 4); // vertex 0
        triangles.Add(vertices.Count - 2); // vertex 2
        triangles.Add(vertices.Count - 1); // vertex 3
    }
}
