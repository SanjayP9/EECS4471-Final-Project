using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Direction = Voxel.Direction;

public class Chunk
{
    public const int CHUNK_SIZE = 8;

    private Mesh mesh;

    private const float VOXEL_SIZE = Voxel.VOXEL_SIZE;

    byte[,,] voxelArray = new byte[CHUNK_SIZE, CHUNK_SIZE, CHUNK_SIZE];
    Dictionary<Vector3, int> indicesMap;
    List<Vector3> vertices;
    List<int> triangles;

    Transform transform;

    public byte[,,] Voxels()
    {
        return voxelArray;
    }

    public Mesh GetMesh()
    {
        return mesh;
    }

    public Chunk(Transform transform, Material mat)
    {
        mesh = new Mesh();
        mesh.MarkDynamic();
        this.transform = transform;

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

        RecomputeMesh();
    }

    public void RecomputeMesh()
    {
        vertices = new List<Vector3>();
        triangles = new List<int>();
        indicesMap = new Dictionary<Vector3, int>();

        for (int x = 0; x < voxelArray.GetLength(0); x++)
        {
            for (int y = 0; y < voxelArray.GetLength(0); y++)
            {
                for (int z = 0; z < voxelArray.GetLength(0); z++)
                {
                    if (voxelArray[x, y, z] == 1)
                    {
                        GenerateVoxel(x, y, z, new Vector3(x * VOXEL_SIZE, y * VOXEL_SIZE, z * VOXEL_SIZE));
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

    private void AddToIndexMap(Vector3[] vector)
    {
        foreach (Vector3 temp in vector)
        {
            if (!indicesMap.ContainsKey(temp))
            {
                indicesMap.Add(temp, indicesMap.Count);
                vertices.Add(temp);
            }
        }
    }

    private void GenerateFace(Vector3 pos, Direction dir)
    {
        Vector3[] temp = new Vector3[4];
        switch (dir)
        {
            case Direction.Top:
                temp[0] = new Vector3(pos.x, pos.y, pos.z + VOXEL_SIZE);
                temp[1] = new Vector3(pos.x + VOXEL_SIZE, pos.y, pos.z + VOXEL_SIZE);
                temp[2] = new Vector3(pos.x + VOXEL_SIZE, pos.y, pos.z);
                temp[3] = new Vector3(pos.x, pos.y, pos.z);
                AddToIndexMap(temp);
                break;
            case Direction.Bottom:
                temp[0] = new Vector3(pos.x, pos.y - VOXEL_SIZE, pos.z);
                temp[1] = new Vector3(pos.x + VOXEL_SIZE, pos.y - VOXEL_SIZE, pos.z);
                temp[2] = new Vector3(pos.x + VOXEL_SIZE, pos.y - VOXEL_SIZE, pos.z + VOXEL_SIZE);
                temp[3] = new Vector3(pos.x, pos.y - VOXEL_SIZE, pos.z + VOXEL_SIZE);
                AddToIndexMap(temp);
                break;
            case Direction.Left:
                temp[0] = new Vector3(pos.x, pos.y - VOXEL_SIZE, pos.z + VOXEL_SIZE);
                temp[1] = new Vector3(pos.x, pos.y, pos.z + VOXEL_SIZE);
                temp[2] = new Vector3(pos.x, pos.y, pos.z);
                temp[3] = new Vector3(pos.x, pos.y - VOXEL_SIZE, pos.z);
                AddToIndexMap(temp);
                break;
            case Direction.Right:
                temp[0] = new Vector3(pos.x + VOXEL_SIZE, pos.y - VOXEL_SIZE, pos.z);
                temp[1] = new Vector3(pos.x + VOXEL_SIZE, pos.y, pos.z);
                temp[2] = new Vector3(pos.x + VOXEL_SIZE, pos.y, pos.z + VOXEL_SIZE);
                temp[3] = new Vector3(pos.x + VOXEL_SIZE, pos.y - VOXEL_SIZE, pos.z + VOXEL_SIZE);
                AddToIndexMap(temp);
                break;
            case Direction.Forward:
                temp[0] = new Vector3(pos.x + VOXEL_SIZE, pos.y - VOXEL_SIZE, pos.z + VOXEL_SIZE);
                temp[1] = new Vector3(pos.x + VOXEL_SIZE, pos.y, pos.z + VOXEL_SIZE);
                temp[2] = new Vector3(pos.x, pos.y, pos.z + VOXEL_SIZE);
                temp[3] = new Vector3(pos.x, pos.y - VOXEL_SIZE, pos.z + VOXEL_SIZE);
                AddToIndexMap(temp);
                break;
            case Direction.Back:
                temp[0] = new Vector3(pos.x, pos.y - VOXEL_SIZE, pos.z);
                temp[1] = new Vector3(pos.x, pos.y, pos.z);
                temp[2] = new Vector3(pos.x + VOXEL_SIZE, pos.y, pos.z);
                temp[3] = new Vector3(pos.x + VOXEL_SIZE, pos.y - VOXEL_SIZE, pos.z);
                AddToIndexMap(temp);
                break;
        }

        triangles.Add(indicesMap[temp[0]]); // vertex 0
        triangles.Add(indicesMap[temp[1]]); // vertex 1
        triangles.Add(indicesMap[temp[2]]); // vertex 2

        triangles.Add(indicesMap[temp[0]]); // vertex 0
        triangles.Add(indicesMap[temp[2]]); // vertex 2
        triangles.Add(indicesMap[temp[3]]); // vertex 3
    }

    /*
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
    */
}