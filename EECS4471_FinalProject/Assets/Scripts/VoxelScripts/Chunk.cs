using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Direction = Voxel.Direction;

public class Chunk : MonoBehaviour
{
    public const int CHUNK_SIZE = 16;

    private Mesh mesh;
    private Polygon polygon;
    private Vector3 chunkOffset;

    private const float VOXEL_SIZE = Voxel.VOXEL_SIZE;
    private Dictionary<Vector3, int> indicesMap;
    private List<Vector3> vertices;
    private List<int> triangles;
    private List<Vector3> normals;

    public byte[,,] Voxels { get; } = new byte[CHUNK_SIZE, CHUNK_SIZE, CHUNK_SIZE];

    public Mesh GetMesh() { return mesh; }

    public int[] index;

    public bool Empty => vertices.Count == 0;

    public void Init(Polygon polygon, int a_x, int a_y, int a_z, Vector3 offset, Material mat, byte defaultVoxel)
    {
        mesh = new Mesh();
        mesh.MarkDynamic();
        chunkOffset = offset;
        this.polygon = polygon;

        index = new [] { a_x, a_y, a_z };

        for (int x = 0; x < Voxels.GetLength(0); x++)
        {
            for (int y = 0; y < Voxels.GetLength(1); y++)
            {
                for (int z = 0; z < Voxels.GetLength(2); z++)
                {
                    Voxels[x, y, z] = defaultVoxel;
                }
            }
        }

        vertices = new List<Vector3>();
        triangles = new List<int>();
        indicesMap = new Dictionary<Vector3, int>();
        normals = new List<Vector3>();

        GetComponent<MeshFilter>().mesh = mesh;
        GetComponent<MeshRenderer>().material = mat;
        gameObject.AddComponent<BoxCollider>();
        GetComponent<BoxCollider>().size = Vector3.one * CHUNK_SIZE / 200f;
        GetComponent<BoxCollider>().center = new Vector3(CHUNK_SIZE / 400f, 0.035f, CHUNK_SIZE / 400f);
        GetComponent<BoxCollider>().isTrigger = true;
    }

    public void RecomputeMesh()
    {
        vertices.Clear();
        triangles.Clear();
        indicesMap.Clear();
        normals.Clear();

        for (int x = 0; x < Voxels.GetLength(0); x++)
        {
            for (int y = 0; y < Voxels.GetLength(0); y++)
            {
                for (int z = 0; z < Voxels.GetLength(0); z++)
                {
                    if (Voxels[x, y, z] == 1)
                    {
                        GenerateVoxel(x, y, z, (new Vector3(x, y, z) * VOXEL_SIZE));
                    }
                }
            }
        }

        mesh.Clear();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.normals = normals.ToArray();
        //mesh.RecalculateBounds();
        //mesh.RecalculateNormals();
        transform.position = chunkOffset;
    }

    private void GenerateVoxel(int x, int y, int z, Vector3 pos)
    {
        if (x + 1 >= Voxels.GetLength(0))
        {
            if (!GetAdjacent(x, y, z, Direction.Right))
                GenerateFace(x, y, z, pos, Direction.Right);
        }
        else if (Voxels[x + 1, y, z] == 0) 
            GenerateFace(x, y, z, pos, Direction.Right);

        if (x - 1 < 0)
        {
            if (!GetAdjacent(x, y, z, Direction.Left))
                GenerateFace(x, y, z, pos, Direction.Left);
        }
        else if (Voxels[x - 1, y, z] == 0)
            GenerateFace(x, y, z, pos, Direction.Left);

        if (y + 1 >= Voxels.GetLength(1))
        {
            if (!GetAdjacent(x, y, z, Direction.Top))
                GenerateFace(x, y, z, pos, Direction.Top);
        }
        else if (Voxels[x, y + 1, z] == 0)
            GenerateFace(x, y, z, pos, Direction.Top);

        if (y - 1 < 0)
        {
            if (!GetAdjacent(x, y, z, Direction.Bottom))
                GenerateFace(x, y, z, pos, Direction.Bottom);
        }
        else if (Voxels[x, y - 1, z] == 0)
            GenerateFace(x, y, z, pos, Direction.Bottom);

        if (z + 1 >= Voxels.GetLength(2))
        {
            if (!GetAdjacent(x, y, z, Direction.Forward))
                GenerateFace(x, y, z, pos, Direction.Forward);
        }
        else if (Voxels[x, y, z + 1] == 0)
            GenerateFace(x, y, z, pos, Direction.Forward);

        if (z - 1 < 0)
        {
            if (!GetAdjacent(x, y, z, Direction.Back))
                GenerateFace(x, y, z, pos, Direction.Back);
        }
        else if (Voxels[x, y, z - 1] == 0)
            GenerateFace(x, y, z, pos, Direction.Back);
    }

    /// <summary>
    /// Returns true if the adjacent space in that direction is occupied
    /// </summary>
    public bool GetAdjacent(int x, int y, int z, Direction dir)
    {
        switch (dir)
        {
            case Direction.Top:
                if (index[1] + 1 < polygon.Chunks.GetLength(1))
                {
                    return polygon.Chunks[index[0], index[1] + 1, index[2]].Voxels[x, 0, z] == 1;
                }
                return false;
            case Direction.Bottom:
                if (index[1] - 1 >= 0)
                {
                    return polygon.Chunks[index[0], index[1] - 1, index[2]].Voxels[x, CHUNK_SIZE - 1, z] == 1;
                }
                return false;
            case Direction.Left:
                if (index[0] - 1 >= 0)
                {
                    return polygon.Chunks[index[0] - 1, index[1], index[2]].Voxels[CHUNK_SIZE - 1, y, z] == 1;
                }
                return false;
            case Direction.Right:
                if (index[0] + 1 < polygon.Chunks.GetLength(0))
                {
                    return polygon.Chunks[index[0] + 1, index[1], index[2]].Voxels[0, y, z] == 1;
                }
                return false;
            case Direction.Forward:
                if (index[2] + 1 < polygon.Chunks.GetLength(2))
                {
                    return polygon.Chunks[index[0], index[1], index[2] + 1].Voxels[x, y, 0] == 1;
                }
                return false;
            case Direction.Back:
                if (index[2] - 1 >= 0)
                {
                    return polygon.Chunks[index[0], index[1], index[2] - 1].Voxels[x, y, CHUNK_SIZE - 1] == 1;
                }
                return false;
        }

        return false;
    }

    private void AddToIndexMap(int x, int y, int z, Vector3[] vector, Direction dir)
    {
        foreach (Vector3 v in vector)
        {
            /*
            if (!indicesMap.ContainsKey(v))
            {
                indicesMap.Add(v, indicesMap.Count);
                vertices.Add(v);
            }
            */
            vertices.Add(v);
            normals.Add(DirToVector(dir));
        }

        
        triangles.Add(vertices.Count - 4); // vertex 0
        triangles.Add(vertices.Count - 3); // vertex 1
        triangles.Add(vertices.Count - 2); // vertex 2

        triangles.Add(vertices.Count - 4); // vertex 0
        triangles.Add(vertices.Count - 2); // vertex 2
        triangles.Add(vertices.Count - 1); // vertex 3
        

        /*
        triangles.Add(indicesMap[vector[0]]); // vertex 0
        triangles.Add(indicesMap[vector[1]]); // vertex 1
        triangles.Add(indicesMap[vector[2]]); // vertex 2

        triangles.Add(indicesMap[vector[0]]); // vertex 0
        triangles.Add(indicesMap[vector[2]]); // vertex 2
        triangles.Add(indicesMap[vector[3]]); // vertex 3
        */
    }

    private Vector3 DirToVector(Direction dir)
    {
        switch (dir)
        {
            case Direction.Top:
                return Vector3.up;
            case Direction.Bottom:
                return Vector3.down;
            case Direction.Left:
                return Vector3.left;
            case Direction.Right:
                return Vector3.right;
            case Direction.Forward:
                return Vector3.forward;
            case Direction.Back:
                return Vector3.back;
        }

        return Vector3.zero;
    }

    private void GenerateFace(int x, int y, int z, Vector3 pos, Direction dir)
    {
        Vector3[] temp = new Vector3[4];
        switch (dir)
        {
            case Direction.Top:
                temp[0] = new Vector3(pos.x, pos.y, pos.z + VOXEL_SIZE);
                temp[1] = new Vector3(pos.x + VOXEL_SIZE, pos.y, pos.z + VOXEL_SIZE);
                temp[2] = new Vector3(pos.x + VOXEL_SIZE, pos.y, pos.z);
                temp[3] = new Vector3(pos.x, pos.y, pos.z);
                break;
            case Direction.Bottom:
                temp[0] = new Vector3(pos.x, pos.y - VOXEL_SIZE, pos.z);
                temp[1] = new Vector3(pos.x + VOXEL_SIZE, pos.y - VOXEL_SIZE, pos.z);
                temp[2] = new Vector3(pos.x + VOXEL_SIZE, pos.y - VOXEL_SIZE, pos.z + VOXEL_SIZE);
                temp[3] = new Vector3(pos.x, pos.y - VOXEL_SIZE, pos.z + VOXEL_SIZE);
                break;
            case Direction.Left:
                temp[0] = new Vector3(pos.x, pos.y - VOXEL_SIZE, pos.z + VOXEL_SIZE);
                temp[1] = new Vector3(pos.x, pos.y, pos.z + VOXEL_SIZE);
                temp[2] = new Vector3(pos.x, pos.y, pos.z);
                temp[3] = new Vector3(pos.x, pos.y - VOXEL_SIZE, pos.z);
                break;
            case Direction.Right:
                temp[0] = new Vector3(pos.x + VOXEL_SIZE, pos.y - VOXEL_SIZE, pos.z);
                temp[1] = new Vector3(pos.x + VOXEL_SIZE, pos.y, pos.z);
                temp[2] = new Vector3(pos.x + VOXEL_SIZE, pos.y, pos.z + VOXEL_SIZE);
                temp[3] = new Vector3(pos.x + VOXEL_SIZE, pos.y - VOXEL_SIZE, pos.z + VOXEL_SIZE);
                break;
            case Direction.Forward:
                temp[0] = new Vector3(pos.x + VOXEL_SIZE, pos.y - VOXEL_SIZE, pos.z + VOXEL_SIZE);
                temp[1] = new Vector3(pos.x + VOXEL_SIZE, pos.y, pos.z + VOXEL_SIZE);
                temp[2] = new Vector3(pos.x, pos.y, pos.z + VOXEL_SIZE);
                temp[3] = new Vector3(pos.x, pos.y - VOXEL_SIZE, pos.z + VOXEL_SIZE);
                break;
            case Direction.Back:
                temp[0] = new Vector3(pos.x, pos.y - VOXEL_SIZE, pos.z);
                temp[1] = new Vector3(pos.x, pos.y, pos.z);
                temp[2] = new Vector3(pos.x + VOXEL_SIZE, pos.y, pos.z);
                temp[3] = new Vector3(pos.x + VOXEL_SIZE, pos.y - VOXEL_SIZE, pos.z);
                break;
        }

        AddToIndexMap(x, y, z, temp, dir);
    }

    private Collider other;
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == "Cutter")
        {
            this.other = other;
        }
    }

    void OnTriggerExit(Collider other)
    {
        this.other = null;
    }

    void Update()
    {
        if (other != null)
        {
            // Get hit location
            Ray r = new Ray(other.transform.position - (other.transform.forward * 0.08f), other.transform.forward);

            if (Physics.Raycast(r, out RaycastHit hit, 0.16f, 1 << 8))
            {
                int[] coordinates = VectorToCoord(hit.point);
                Debug.Log(coordinates[0] + ", " + coordinates[1] + ", " + coordinates[2] );
            }
        }
    }

    int[] coords;
    private int[] VectorToCoord(Vector3 position)
    {
        Vector3 convertedVector = position - chunkOffset;
        Debug.Log("Position: " + position);
        convertedVector /= VOXEL_SIZE * Voxels.GetLength(0);

        coords = new [] {(int) convertedVector.x, (int) convertedVector.y, (int) convertedVector.z};


        return coords;
    }

    void OnDrawGizmos()
    {
        if (other != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(other.transform.position - (other.transform.forward * 0.08f), other.transform.position + (other.transform.forward * 0.08f));

            Ray r = new Ray(other.transform.position - (other.transform.forward * 0.08f), other.transform.forward);

            if (Physics.Raycast(r, out RaycastHit hit, 0.16f, 1 << 8))
            {
                Gizmos.DrawSphere(hit.point, 0.01f);
                Gizmos.color = Color.blue;
                Gizmos.DrawCube(new Vector3(
                    chunkOffset.x + (VOXEL_SIZE * coords[0]),
                    chunkOffset.y + (VOXEL_SIZE * coords[1]),
                    chunkOffset.z + (VOXEL_SIZE * coords[2])
                ), Vector3.one * 0.005f);


            }

            

            //Gizmos.DrawRay(r);
        }

        //Gizmos.color = Color.red;
        //Gizmos.DrawSphere(chunkOffset, 0.005f);
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