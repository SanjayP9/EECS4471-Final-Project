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

    public byte[][][] Voxels { get; } = new byte[CHUNK_SIZE][][];
    private int[] voxelIndexes = new int[CHUNK_SIZE * CHUNK_SIZE * CHUNK_SIZE];

    public Mesh GetMesh() { return mesh; }

    public int[] index;

    public bool Empty => vertices.Count == 0;

    private bool dirty = false;

    public int X, Y, Z;

    private ComputeShader meshShader;
    private int handle;

    const int TOP = 1;
    const int BOTTOM = 2;
    const int LEFT = 4;
    const int RIGHT = 8;
    const int FORWARD = 16;
    const int BACK = 32;

    private float timer = 0f;

    public void Init(Polygon polygon, int a_x, int a_y, int a_z, Vector3 offset, Material mat, byte defaultVoxel)
    {
        meshShader = polygon.MeshShader;
        mesh = new Mesh();
        mesh.MarkDynamic();
        chunkOffset = offset;
        this.polygon = polygon;

        index = new [] { a_x, a_y, a_z };

        for (int x = 0; x < CHUNK_SIZE; x++)
        {
            Voxels[x] = new byte[CHUNK_SIZE][];
            for (int y = 0; y < CHUNK_SIZE; y++)
            {
                Voxels[x][y] = new byte[CHUNK_SIZE];
                for (int z = 0; z < CHUNK_SIZE; z++)
                {
                    Voxels[x][y][z] = defaultVoxel;
                    
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
        X = a_x;
        Y = a_y;
        Z = a_z;

        //handle = meshShader.FindKernel("CSMain");
    }

    public void RecomputeMesh()
    {
        vertices.Clear();
        triangles.Clear();
        indicesMap.Clear();
        normals.Clear();

        //int[] voxelMasks = new int[CHUNK_SIZE * CHUNK_SIZE * CHUNK_SIZE];
        //int[] faceArray = new int[CHUNK_SIZE * CHUNK_SIZE * CHUNK_SIZE];
        int numVerts = 0, numTriangles = 0;
        for (int x = 0; x < CHUNK_SIZE; x++)
        {
            for (int y = 0; y < CHUNK_SIZE; y++)
            {
                for (int z = 0; z < CHUNK_SIZE; z++)
                {
                    /*
                    voxelMasks[x + CHUNK_SIZE * (y + CHUNK_SIZE * z)] = GetFaces(x, y, z);
                    faceArray[x + CHUNK_SIZE * (y + CHUNK_SIZE * z)] = faces;
                    numVerts += faces * 4;
                    numTriangles += faces * 6; 
                    */

                    if (Voxels[x][y][z] == 1)
                    {
                        GenerateVoxel(x, y, z, (new Vector3(x, y, z) * VOXEL_SIZE));
                        voxelIndexes[x * CHUNK_SIZE * CHUNK_SIZE + y * CHUNK_SIZE + z] = vertices.Count;
                    }
                    else
                    {
                        voxelIndexes[x * CHUNK_SIZE * CHUNK_SIZE + y * CHUNK_SIZE + z] = 0;
                    }
                }
            }
        }
        RebuildTriangleArray();

        mesh.Clear();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.normals = normals.ToArray();
        transform.position = chunkOffset;
    }

    private void RebuildTriangleArray()
    {
        triangles.Clear();
        for (int j = 0; j < vertices.Count; j += 4)
        {
            triangles.Add(j); // vertex 0
            triangles.Add(j + 1); // vertex 1
            triangles.Add(j + 2); // vertex 2

            triangles.Add(j); // vertex 0
            triangles.Add(j + 2); // vertex 2
            triangles.Add(j + 3); // vertex 3
        }
    }

    private int GetNextIndex(int x, int y, int z)
    {
        z++;
        bool carry = false;
        if (z == CHUNK_SIZE)
        {
            z = 0;
            carry = true;
        }

        if (carry)
        {
            y++;
            carry = y == CHUNK_SIZE;
            y = carry ? 0 : y;
        }

        if (carry)
        {
            x++;
            if (x == CHUNK_SIZE) return -1;
        }

        return x * CHUNK_SIZE * CHUNK_SIZE + y * CHUNK_SIZE + z;
    }

    public void ModifyVoxel(int x, int y, int z)
    {
        /*
        vertices.Clear();
        triangles.Clear();
        indicesMap.Clear();
        normals.Clear();
        */

        // Remove old voxel faces
        int i = x * CHUNK_SIZE * CHUNK_SIZE + y * CHUNK_SIZE + z;
        int startingIndex = voxelIndexes[i];


        int end = GetNextIndex(x, y, z);

        if (end == -1)
        {
            end = vertices.Count - voxelIndexes[i - 1];
        }
        else
        {
            end = voxelIndexes[end];
        }

        for (int j = startingIndex; j < end; j++)
        {
            vertices.RemoveAt(j);
        }

        if (end - startingIndex > 0)
        {
            GenerateVoxel(x, y, z, new Vector3(x, y, z) * VOXEL_SIZE);
        }

        // Fix voxel indexes
        if (i != voxelIndexes.Length - 1)
        {
            int diff = faces - (end - startingIndex);
            if (diff != 0)
            {
                for (int k = i + 1; k < voxelIndexes.Length; k++)
                {
                    voxelIndexes[k] += faces;
                }
            }
        }

        // Rebuild triangle array
        RebuildTriangleArray();

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.normals = normals.ToArray();

        //mesh.RecalculateBounds();
        mesh.RecalculateNormals();

        /*
        if (numVerts > 0)
        {
            ComputeBuffer vertexBuffer = new ComputeBuffer(numVerts, 3 * sizeof(float));
            ComputeBuffer normalBuffer = new ComputeBuffer(numVerts, 3 * sizeof(float));
            ComputeBuffer triangleBuffer = new ComputeBuffer(numTriangles, sizeof(int));

            meshShader.SetInts("voxelMasks", voxelMasks);
            meshShader.SetInts("faceArray", faceArray);

            meshShader.SetBuffer(handle, "vertices", vertexBuffer);
            meshShader.SetBuffer(handle, "normals", normalBuffer);
            meshShader.SetBuffer(handle, "triangles", triangleBuffer);

            meshShader.Dispatch(handle, CHUNK_SIZE, CHUNK_SIZE, CHUNK_SIZE);

            Vector3[] verts = new Vector3[numVerts];
            Vector3[] norms = new Vector3[numVerts];
            int[] tris = new int[numTriangles];

            vertexBuffer.GetData(verts);
            normalBuffer.GetData(norms);
            triangleBuffer.GetData(tris);

            mesh.Clear();
            mesh.vertices = verts;
            mesh.normals = norms;
            mesh.triangles = tris;
        }
        */
    }

    private byte faces;
    private int GetFaces(int x, int y, int z)
    {
        int result = 0;
        faces = 0;

        if (x + 1 >= CHUNK_SIZE)
        {
            if (!GetAdjacent(x, y, z, Direction.Right))
            {
                result |= RIGHT;
                faces++;
            }

        }
        else if (Voxels[x + 1][y][z] == 0)
        {
            result |= RIGHT;
            faces++;
        }

        if (x - 1 < 0)
        {
            if (!GetAdjacent(x, y, z, Direction.Left))
            {
                result |= LEFT;
                faces++;
            }
        }
        else if (Voxels[x - 1][y][z] == 0)
        {
            result |= LEFT;
            faces++;
        }

        if (y + 1 >= CHUNK_SIZE)
        {
            if (!GetAdjacent(x, y, z, Direction.Top))
            {
                result |= TOP;
                faces++;
            }
        }
        else if (Voxels[x][y + 1][z] == 0)
        {
            result |= TOP;
            faces++;
        }

        if (y - 1 < 0)
        {
            if (!GetAdjacent(x, y, z, Direction.Bottom))
            {
                result |= BOTTOM;
                faces++;
            }
        }
        else if (Voxels[x][y - 1][z] == 0)
        {
            result |= BOTTOM;
            faces++;
        }

        if (z + 1 >= CHUNK_SIZE)
        {
            if (!GetAdjacent(x, y, z, Direction.Forward))
            {
                result |= FORWARD;
                faces++;
            }
        }
        else if (Voxels[x][y][z + 1] == 0)
        {
            result |= FORWARD;
            faces++;
        }

        if (z - 1 < 0)
        {
            if (!GetAdjacent(x, y, z, Direction.Back))
            {
                result |= BACK;
                faces++;
            }
        }
        else if (Voxels[x][y][z - 1] == 0)
        {
            result |= BACK;
            faces++;
        }

        return result;
    }

    private void GenerateVoxel(int x, int y, int z, Vector3 pos)
    {
        faces = 0;

        if (x + 1 >= CHUNK_SIZE)
        {
            if (!GetAdjacent(x, y, z, Direction.Right))
                GenerateFace(x, y, z, pos, Direction.Right);
        }
        else if (Voxels[x + 1][y][z] == 0) 
            GenerateFace(x, y, z, pos, Direction.Right);

        if (x - 1 < 0)
        {
            if (!GetAdjacent(x, y, z, Direction.Left))
                GenerateFace(x, y, z, pos, Direction.Left);
        }
        else if (Voxels[x - 1][y][z] == 0)
            GenerateFace(x, y, z, pos, Direction.Left);

        if (y + 1 >= CHUNK_SIZE)
        {
            if (!GetAdjacent(x, y, z, Direction.Top))
                GenerateFace(x, y, z, pos, Direction.Top);
        }
        else if (Voxels[x][y + 1][z] == 0)
            GenerateFace(x, y, z, pos, Direction.Top);

        if (y - 1 < 0)
        {
            if (!GetAdjacent(x, y, z, Direction.Bottom))
                GenerateFace(x, y, z, pos, Direction.Bottom);
        }
        else if (Voxels[x][y - 1][z] == 0)
            GenerateFace(x, y, z, pos, Direction.Bottom);

        if (z + 1 >= CHUNK_SIZE)
        {
            if (!GetAdjacent(x, y, z, Direction.Forward))
                GenerateFace(x, y, z, pos, Direction.Forward);
        }
        else if (Voxels[x][y][z + 1] == 0)
            GenerateFace(x, y, z, pos, Direction.Forward);

        if (z - 1 < 0)
        {
            if (!GetAdjacent(x, y, z, Direction.Back))
                GenerateFace(x, y, z, pos, Direction.Back);
        }
        else if (Voxels[x][y][z - 1] == 0)
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
                if (index[1] + 1 < polygon.Chunks.GetLength(0))
                {
                    return polygon.Chunks[index[0], index[1] + 1, index[2]].Voxels[x][0][z] == 1;
                }
                return false;
            case Direction.Bottom:
                if (index[1] - 1 >= 0)
                {
                    return polygon.Chunks[index[0], index[1] - 1, index[2]].Voxels[x][CHUNK_SIZE - 1][z] == 1;
                }
                return false;
            case Direction.Left:
                if (index[0] - 1 >= 0)
                {
                    return polygon.Chunks[index[0] - 1, index[1], index[2]].Voxels[CHUNK_SIZE - 1][y][z] == 1;
                }
                return false;
            case Direction.Right:
                if (index[0] + 1 < polygon.Chunks.GetLength(1))
                {
                    return polygon.Chunks[index[0] + 1, index[1], index[2]].Voxels[0][y][z] == 1;
                }
                return false;
            case Direction.Forward:
                if (index[2] + 1 < polygon.Chunks.GetLength(2))
                {
                    return polygon.Chunks[index[0], index[1], index[2] + 1].Voxels[x][y][0] == 1;
                }
                return false;
            case Direction.Back:
                if (index[2] - 1 >= 0)
                {
                    return polygon.Chunks[index[0], index[1], index[2] - 1].Voxels[x][y][CHUNK_SIZE - 1] == 1;
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

        /*
        triangles.Add(vertices.Count - 4); // vertex 0
        triangles.Add(vertices.Count - 3); // vertex 1
        triangles.Add(vertices.Count - 2); // vertex 2

        triangles.Add(vertices.Count - 4); // vertex 0
        triangles.Add(vertices.Count - 2); // vertex 2
        triangles.Add(vertices.Count - 1); // vertex 3
        */

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

        faces++;
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
        if (other != null && !Empty)
        {
            // Get hit location
            Ray[] rays = new Ray[4];
            rays[0] = new Ray(other.transform.TransformPoint(0.5f, -0.5f, -0.5f), other.transform.forward);
            rays[1] = new Ray(other.transform.TransformPoint(-0.5f, -0.5f, -0.5f), other.transform.forward);
            rays[2] = new Ray(other.transform.TransformPoint(-0.5f, 0.5f, -0.5f), other.transform.forward);
            rays[3] = new Ray(other.transform.TransformPoint(0.5f, 0.5f, -0.5f), other.transform.forward);

            foreach (Ray r in rays)
            {
                Vector3 current = r.origin;
                float distance = (current - r.origin).magnitude;

                while (distance < 0.256f)
                {
                    int[] coordinates = VectorToCoord(current);

                    if (coordinates[0] >= 0 && coordinates[1] >= 0 && coordinates[2] >= 0 &&
                        coordinates[0] < CHUNK_SIZE && coordinates[1] < CHUNK_SIZE && coordinates[2] < CHUNK_SIZE)
                    {
                        if (Voxels[coords[0]][coords[1]][coords[2]] == 1)
                        {
                            Voxels[coords[0]][coords[1]][coords[2]] = 0;
                            ModifyVoxel(coords[0], coords[1], coords[2]);
                            
                            dirty = true;
                        }

                        //Debug.Log(X + ", " + Y + ", " + Z + " -> " + coordinates[0] + ", " + coordinates[1] + ", " + coordinates[2]);
                    }
                    else
                    {
                        /*
                        int chunk_x = X, chunk_y = Y, chunk_z = Z;
                        if (coordinates[0] < 0)
                        {
                            coordinates[0] = CHUNK_SIZE + coordinates[0];
                            chunk_x--;
                        }
                        else if (coordinates[0] > CHUNK_SIZE - 1)
                        {
                            coordinates[0] = coordinates[0] - CHUNK_SIZE;
                            chunk_x++;
                        }

                        if (coordinates[1] < 0)
                        {
                            coordinates[1] = CHUNK_SIZE + coordinates[1];
                            chunk_y--;
                        }
                        else if (coordinates[1] > CHUNK_SIZE - 1)
                        {
                            coordinates[1] = coordinates[1] - CHUNK_SIZE;
                            chunk_y++;
                        }

                        if (coordinates[2] < 0)
                        {
                            coordinates[2] = CHUNK_SIZE + coordinates[2];
                            chunk_z--;
                        }
                        else if (coordinates[2] > CHUNK_SIZE - 1)
                        {
                            coordinates[2] = coordinates[2] - CHUNK_SIZE;
                            chunk_z++;
                        }

                        if (polygon.InBounds(chunk_x, chunk_y, chunk_z))
                        {
                            //Debug.Log(coordinates[0] + ", " + coordinates[1] + ", " + coordinates[2]);
                            if (polygon.Chunks[chunk_x, chunk_y, chunk_z].Voxels[coordinates[0], coordinates[1], coordinates[2]] == 1)
                            {
                                polygon.Chunks[chunk_x, chunk_y, chunk_z].Voxels[coordinates[0], coordinates[1],
                                    coordinates[2]] = 0;

                                polygon.Chunks[chunk_x, chunk_y, chunk_z].MakeDirty();
                            }
                        }
                        */
                    }

                    distance += VOXEL_SIZE;
                    current += VOXEL_SIZE * other.transform.forward;
                }
            }
        }

        if (dirty)
        {
            //RecomputeMesh();
            dirty = false;
        }

        chunkOffset = transform.position;
        GetComponent<BoxCollider>().enabled = !Empty;
    }

    public void MakeDirty() {  dirty = true;  }

    private int[] coords = new int[3];
    private int[] VectorToCoord(Vector3 position)
    {
        Vector3 convertedVector = position - transform.position;
        convertedVector /= VOXEL_SIZE;

        coords[0] = convertedVector.x < 0f ? -1 : (int) convertedVector.x;
        coords[1] = convertedVector.y < 0f ? -1 : (int) convertedVector.y;
        coords[2] = convertedVector.z < 0f ? -1 : (int) convertedVector.z;

        return coords;
    }

    void OnDrawGizmos()
    {
        if (other != null && !Empty)
        {
            Gizmos.color = Color.red;
            //Gizmos.DrawLine(other.transform.position - (other.transform.forward * 0.08f), other.transform.position + (other.transform.forward * 0.08f));

            Gizmos.DrawRay(other.transform.TransformPoint(0f, 0f, -1f), other.transform.forward);
        }
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