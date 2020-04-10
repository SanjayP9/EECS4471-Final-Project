using System.Collections.Generic;
using UnityEngine;
using Direction = Voxel.Direction;

public class Chunk : MonoBehaviour
{
    // The amount of voxels per dimension
    public const int CHUNK_SIZE = 8;
    private readonly int GRID_SIZE = CHUNK_SIZE * CHUNK_SIZE * CHUNK_SIZE;

    private Mesh mesh;          // Mesh for this chunk
    private Polygon polygon;    // Reference to the parent object

    // Mesh variables
    private Vector3[] vertices;
    private Vector3[] normals;
    private List<int> triangles;
    private Vector2[] uvs;
    private int vertexCount;

    // UV Coordinates for colours
    private Vector2[] colourCoordinates =
    {
        new Vector2(0f, 0.124f), 
        new Vector2(0.125f, 0.24f),
        new Vector2(0.26f, 0.374f), 
        new Vector2(0.385f, 0.49f), 
        new Vector2(0.51f, 0.624f),
        new Vector2(0.635f, 0.74f),
        new Vector2(0.76f, 0.874f), 
        new Vector2(0.885f, 1f)
    };

    // 3D array for storing our voxels
    public byte[][][] Voxels { get; } = new byte[CHUNK_SIZE][][];
    private int[] startingIndexes;
    public Vector3[][][] Center = new Vector3[CHUNK_SIZE][][];

    // Optimized Mesh (Unused)
    /*
    public int[] VoxelMasks { get; } = new int[CHUNK_SIZE * CHUNK_SIZE * CHUNK_SIZE];
    public int[] Faces { get; } = new int[CHUNK_SIZE * CHUNK_SIZE * CHUNK_SIZE];
    private int[] vertSum = new int[CHUNK_SIZE * CHUNK_SIZE * CHUNK_SIZE];
    private int[][][] voxelFaces = new int[CHUNK_SIZE][][];
    private int numVerts;
    private ComputeShader meshShader;
    private int handle;
    public const int TOP = 1;
    public const int BOTTOM = 2;
    public const int LEFT = 4;
    public const int RIGHT = 8;
    public const int FORWARD = 16;
    public const int BACK = 32;

    // Compute Buffers
    private ComputeBuffer maskBuffer, faceBuffer, vertBuffer;
    */

    // Returns true if the chunk does not have any visible mesh
    public bool Empty => vertexCount == 0;

    // The X,Y,Z coordinates in the polygon's 3D array of chunks
    public int X, Y, Z;

    /// <summary>
    /// Initializes the data structures required for the chunk
    /// </summary>
    public void Init(Polygon polygon, int a_x, int a_y, int a_z, Vector3 offset, Material mat, byte defaultVoxel)
    {
        //meshShader = polygon.MeshShader;
        mesh = new Mesh();
        mesh.MarkDynamic();
        this.polygon = polygon;

        for (int x = 0; x < CHUNK_SIZE; x++)
        {
            Voxels[x] = new byte[CHUNK_SIZE][];
            Center[x] = new Vector3[CHUNK_SIZE][];
            //voxelFaces[x] = new int[CHUNK_SIZE][];
            for (int y = 0; y < CHUNK_SIZE; y++)
            {
                Voxels[x][y] = new byte[CHUNK_SIZE];
                Center[x][y] = new Vector3[CHUNK_SIZE];
                //voxelFaces[x][y] = new int[CHUNK_SIZE];
                for (int z = 0; z < CHUNK_SIZE; z++)
                {
                    Voxels[x][y][z] = defaultVoxel;
                    Center[x][y][z].Set(
                        x * Voxel.VoxelSize + Voxel.VoxelSize / 2f,
                        y * Voxel.VoxelSize + Voxel.VoxelSize / -2f,
                        z * Voxel.VoxelSize + Voxel.VoxelSize / 2f);
                }
            }
        }

        vertices = new Vector3[GRID_SIZE * 24];
        normals = new Vector3[vertices.Length];
        uvs = new Vector2[vertices.Length];
        triangles = new List<int>(short.MaxValue);
        startingIndexes = new int[GRID_SIZE];

        GetComponent<MeshFilter>().mesh = mesh;
        GetComponent<MeshRenderer>().material = mat;
        gameObject.AddComponent<BoxCollider>();
        GetComponent<BoxCollider>().size = (Vector3.one * CHUNK_SIZE) * Voxel.VoxelSize;
        GetComponent<BoxCollider>().center = new Vector3(GetComponent<BoxCollider>().size.x / 2f, 0.03f, GetComponent<BoxCollider>().size.z / 2f);
        GetComponent<BoxCollider>().isTrigger = true;
        X = a_x;
        Y = a_y;
        Z = a_z;

        //handle = meshShader.FindKernel("CSMain");
    }

    /// <summary>
    /// Iterates through every voxel in the chunk, generating quads that represent
    /// the faces of the voxel. The total number of vertices required for the chunk
    /// is updated.
    /// </summary>
    public void RecomputeMesh()
    {
        vertexCount = 0;

        for (int x = 0; x < CHUNK_SIZE; x++)
        {
            for (int y = 0; y < CHUNK_SIZE; y++)
            {
                for (int z = 0; z < CHUNK_SIZE; z++)
                {
                    Center[x][y][z].Set(
                        x * Voxel.VoxelSize + Voxel.VoxelSize / 2f, 
                        y * Voxel.VoxelSize + Voxel.VoxelSize / -2f, 
                        z * Voxel.VoxelSize + Voxel.VoxelSize / 2f);

                    if (Voxels[x][y][z] != 0)
                        GenerateVoxel(x, y, z, new Vector3(x, y, z) * Voxel.VoxelSize);
                }
            }
        }
    }

    /// <summary>
    /// Reassigns the mesh component with the vertices, normals, and uvs
    /// Done as a separate method because Unity does not support threads executing
    /// Unity component methods
    /// </summary>
    public void SetMesh()
    {
        GetComponent<MeshRenderer>().enabled = vertexCount > 0;
        mesh.Clear();

        if (vertexCount > 0)
        {
            mesh.SetVertices(vertices);

            triangles.Clear();
            for (int i = 0; i < vertexCount; i += 4)
            {
                triangles.Add(i);
                triangles.Add(i + 1);
                triangles.Add(i + 2);

                triangles.Add(i);
                triangles.Add(i + 2);
                triangles.Add(i + 3);
            }

            mesh.SetTriangles(triangles, 0);
            mesh.SetNormals(normals);
            mesh.SetUVs(0, uvs);
        }

        GetComponent<BoxCollider>().size = (Vector3.one * CHUNK_SIZE) * Voxel.VoxelSize;
        GetComponent<BoxCollider>().center.Set(
            GetComponent<BoxCollider>().size.x / 2f,
            GetComponent<BoxCollider>().size.y / 2f - Voxel.VoxelSize,
            GetComponent<BoxCollider>().size.z / 2f);
        transform.position = polygon.transform.position + new Vector3(X, Y, Z) * CHUNK_SIZE * Voxel.VoxelSize;
    }


    /// <summary>
    /// Modifies a voxel and checks adjacent chunks or voxels.
    /// If a voxel borders a chunk, the adjacent chunk must also be recomputed
    /// </summary>
    public void ModifyVoxel(int x, int y, int z, int newValue)
    {
        if (Voxels[x][y][z] == newValue) return;

        Voxels[x][y][z] = (byte) newValue;

        /*
        RecomputeVoxelNeighbour(x, y, z, x - 1, y, z, LEFT, RIGHT);
        RecomputeVoxelNeighbour(x, y, z, x + 1, y, z, RIGHT, LEFT);
        RecomputeVoxelNeighbour(x, y, z, x, y - 1, z, BOTTOM, TOP);
        RecomputeVoxelNeighbour(x, y, z, x, y + 1, z, TOP, BOTTOM);
        RecomputeVoxelNeighbour(x, y, z, x, y, z - 1, BACK, FORWARD);
        RecomputeVoxelNeighbour(x, y, z, x, y, z + 1, FORWARD, BACK);
        */

        if (x + 1 >= CHUNK_SIZE && X + 1 < polygon.Chunks.GetLength(0))
            polygon.EnqueueChunkToUpdate(polygon.Chunks[X + 1, Y, Z]);
        else if (x - 1 < 0 && X - 1 >= 0)
            polygon.EnqueueChunkToUpdate(polygon.Chunks[X - 1, Y, Z]);

        if (y + 1 >= CHUNK_SIZE && Y + 1 < polygon.Chunks.GetLength(1))
            polygon.EnqueueChunkToUpdate(polygon.Chunks[X, Y + 1, Z]);
        else if (y - 1 < 0 && Y - 1 >= 0)
            polygon.EnqueueChunkToUpdate(polygon.Chunks[X, Y - 1, Z]);

        if (z + 1 >= CHUNK_SIZE && Z + 1 < polygon.Chunks.GetLength(2))
            polygon.EnqueueChunkToUpdate(polygon.Chunks[X, Y, Z + 1]);
        else if (z - 1 < 0 && Z - 1 >= 0)
            polygon.EnqueueChunkToUpdate(polygon.Chunks[X, Y, Z - 1]);
    }

    public void ModifyColour(int x, int y, int z, byte newColour)
    {
        Voxels[x][y][z] = (byte)(newColour + 1);

        polygon.EnqueueChunkToUpdate(this);
    }

    /// <summary>
    /// Converts a world space coordinate to a x,y,z index in the chunk's space
    /// </summary>
    public int[] VectorToCoord(Vector3 position)
    {
        Vector3 convertedVector = (position - transform.position) / Voxel.VoxelSize;

        return new[]
        {
            convertedVector.x < 0f ? -1 : Mathf.RoundToInt(convertedVector.x),
            convertedVector.y < 0f ? -1 : Mathf.RoundToInt(convertedVector.y),
            convertedVector.z < 0f ? -1 : Mathf.RoundToInt(convertedVector.z)
        };
    }

    /// <summary>
    /// Given a voxel's location, computes the quads and vertices
    /// </summary>
    private void GenerateVoxel(int x, int y, int z, Vector3 pos)
    {
        startingIndexes[SquashIndex(x,y,z)] = vertexCount;

        if (x + 1 >= CHUNK_SIZE)
        {
            if (X + 1 >= polygon.Chunks.GetLength(0))
                GenerateFace(x, y, z, pos, Direction.Right);
            else if (polygon.Chunks[X + 1, Y, Z].Voxels[0][y][z] == 0)
                GenerateFace(x, y, z, pos, Direction.Right);
        }
        else if (Voxels[x + 1][y][z] == 0) 
            GenerateFace(x, y, z, pos, Direction.Right);

        if (x - 1 < 0)
        {
            if (X - 1 < 0)
                GenerateFace(x, y, z, pos, Direction.Left);
            else if (polygon.Chunks[X - 1, Y, Z].Voxels[CHUNK_SIZE - 1][y][z] == 0)
                GenerateFace(x, y, z, pos, Direction.Left);
        }
        else if (Voxels[x - 1][y][z] == 0)
            GenerateFace(x, y, z, pos, Direction.Left);

        if (y + 1 >= CHUNK_SIZE)
        {
            if (Y + 1 >= polygon.Chunks.GetLength(1))
                GenerateFace(x, y, z, pos, Direction.Top);
            else if (polygon.Chunks[X, Y + 1, Z].Voxels[x][0][z] == 0)
                GenerateFace(x, y, z, pos, Direction.Top);
        }
        else if (Voxels[x][y + 1][z] == 0)
            GenerateFace(x, y, z, pos, Direction.Top);

        if (y - 1 < 0)
        {
            if (Y - 1 < 0)
                GenerateFace(x, y, z, pos, Direction.Bottom);
            else if (polygon.Chunks[X, Y - 1, Z].Voxels[x][CHUNK_SIZE - 1][z] == 0)
                GenerateFace(x, y, z, pos, Direction.Bottom);
        }
        else if (Voxels[x][y - 1][z] == 0)
            GenerateFace(x, y, z, pos, Direction.Bottom);

        if (z + 1 >= CHUNK_SIZE)
        {
            if (Z + 1 >= polygon.Chunks.GetLength(2))
                GenerateFace(x, y, z, pos, Direction.Forward);
            else if (polygon.Chunks[X, Y, Z + 1].Voxels[x][y][0] == 0)
                GenerateFace(x, y, z, pos, Direction.Forward);
        }
        else if (Voxels[x][y][z + 1] == 0)
            GenerateFace(x, y, z, pos, Direction.Forward);

        if (z - 1 < 0)
        {
            if (Z - 1 < 0)
                GenerateFace(x, y, z, pos, Direction.Back);
            else if (polygon.Chunks[X, Y, Z - 1].Voxels[x][y][CHUNK_SIZE - 1] == 0)
                GenerateFace(x, y, z, pos, Direction.Back);
        }
        else if (Voxels[x][y][z - 1] == 0)
            GenerateFace(x, y, z, pos, Direction.Back);
    }

    /// <summary>
    /// Converts a direction into a vector
    /// </summary>
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

    /// <summary>
    /// Generates the associated quad for a particular direction and position
    /// </summary>
    private void GenerateFace(int x, int y, int z, Vector3 pos, Direction dir)
    {
        switch (dir)
        {
            case Direction.Top:
                vertices[vertexCount++].Set(pos.x, pos.y, pos.z + Voxel.VoxelSize);
                vertices[vertexCount++].Set(pos.x + Voxel.VoxelSize, pos.y, pos.z + Voxel.VoxelSize);
                vertices[vertexCount++].Set(pos.x + Voxel.VoxelSize, pos.y, pos.z);
                vertices[vertexCount++].Set(pos.x, pos.y, pos.z);
                break;
            case Direction.Bottom:
                vertices[vertexCount++].Set(pos.x, pos.y - Voxel.VoxelSize, pos.z);
                vertices[vertexCount++].Set(pos.x + Voxel.VoxelSize, pos.y - Voxel.VoxelSize, pos.z);
                vertices[vertexCount++].Set(pos.x + Voxel.VoxelSize, pos.y - Voxel.VoxelSize, pos.z + Voxel.VoxelSize);
                vertices[vertexCount++].Set(pos.x, pos.y - Voxel.VoxelSize, pos.z + Voxel.VoxelSize);
                break;
            case Direction.Left:
                vertices[vertexCount++].Set(pos.x, pos.y - Voxel.VoxelSize, pos.z + Voxel.VoxelSize);
                vertices[vertexCount++].Set(pos.x, pos.y, pos.z + Voxel.VoxelSize);
                vertices[vertexCount++].Set(pos.x, pos.y, pos.z);
                vertices[vertexCount++].Set(pos.x, pos.y - Voxel.VoxelSize, pos.z);
                break;
            case Direction.Right:
                vertices[vertexCount++].Set(pos.x + Voxel.VoxelSize, pos.y - Voxel.VoxelSize, pos.z);
                vertices[vertexCount++].Set(pos.x + Voxel.VoxelSize, pos.y, pos.z);
                vertices[vertexCount++].Set(pos.x + Voxel.VoxelSize, pos.y, pos.z + Voxel.VoxelSize);
                vertices[vertexCount++].Set(pos.x + Voxel.VoxelSize, pos.y - Voxel.VoxelSize, pos.z + Voxel.VoxelSize);
                break;
            case Direction.Forward:
                vertices[vertexCount++].Set(pos.x + Voxel.VoxelSize, pos.y - Voxel.VoxelSize, pos.z + Voxel.VoxelSize);
                vertices[vertexCount++].Set(pos.x + Voxel.VoxelSize, pos.y, pos.z + Voxel.VoxelSize);
                vertices[vertexCount++].Set(pos.x, pos.y, pos.z + Voxel.VoxelSize);
                vertices[vertexCount++].Set(pos.x, pos.y - Voxel.VoxelSize, pos.z + Voxel.VoxelSize);
                break;
            case Direction.Back:
                vertices[vertexCount++].Set(pos.x, pos.y - Voxel.VoxelSize, pos.z);
                vertices[vertexCount++].Set(pos.x, pos.y, pos.z);
                vertices[vertexCount++].Set(pos.x + Voxel.VoxelSize, pos.y, pos.z);
                vertices[vertexCount++].Set(pos.x + Voxel.VoxelSize, pos.y - Voxel.VoxelSize, pos.z);
                break;
        }

        for (int i = vertexCount - 4; i < vertexCount; i++)
        {
            Vector3 vDir = DirToVector(dir);
            normals[i].Set(vDir.x, vDir.y, vDir.z);
        }

        // Set UVs
        uvs[vertexCount - 4].Set(colourCoordinates[Voxels[x][y][z] - 1].x, 0f);
        uvs[vertexCount - 3].Set(colourCoordinates[Voxels[x][y][z] - 1].y, 0f);
        uvs[vertexCount - 2].Set(colourCoordinates[Voxels[x][y][z] - 1].x, 1f);
        uvs[vertexCount - 1].Set(colourCoordinates[Voxels[x][y][z] - 1].y, 1f);
    }

    public static int SquashIndex(int x, int y, int z)
    {
        return z + CHUNK_SIZE * (y + CHUNK_SIZE * x);
    }

    #region Optimized Mesh Code (Unused)
    /*


    private int GetFaces(int x, int y, int z)
    {
        byte faces = 0;
        Faces[z + CHUNK_SIZE * (y + CHUNK_SIZE * x)] = 0;
        if (Voxels[x][y][z] == 0) return 0;

        int result = 0;

        if (x + 1 >= CHUNK_SIZE && X + 1 < polygon.Chunks.GetLength(0))
        {
            if (polygon.Chunks[X + 1, Y, Z].Voxels[0][y][z] == 0)
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

        if (x - 1 < 0 && X - 1 >= 0)
        {
            if (polygon.Chunks[X - 1, Y, Z].Voxels[CHUNK_SIZE - 1][y][z] == 0)
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

        if (y + 1 >= CHUNK_SIZE && Y + 1 < polygon.Chunks.GetLength(1))
        {
            if (polygon.Chunks[X, Y + 1, Z].Voxels[x][0][z] == 0)
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

        if (y - 1 < 0 && Y - 1 >= 0)
        {
            if (polygon.Chunks[X, Y - 1, Z].Voxels[x][CHUNK_SIZE - 1][z] == 0)
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

        if (z + 1 >= CHUNK_SIZE && Z + 1 < polygon.Chunks.GetLength(2))
        {
            if (polygon.Chunks[X, Y, Z + 1].Voxels[x][y][0] == 0)
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

        if (z - 1 < 0 && Z - 1 >= 0)
        {
            if (polygon.Chunks[X, Y, Z - 1].Voxels[x][y][CHUNK_SIZE - 1] == 0)
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

        Faces[z + CHUNK_SIZE * (y + CHUNK_SIZE * x)] = faces;

        return result;
    }


    private void RecomputeVoxelNeighbour(int x, int y, int z, int n_x, int n_y, int n_z, int dir, int n_dir)
    {
        int c_x = X, c_y = Y, c_z = Z;

        if (n_x < 0)
        {
            c_x--;
            n_x = CHUNK_SIZE - 1;
        }
        else if (n_x >= CHUNK_SIZE)
        {
            c_x++;
            n_x = 0;
        }

        if (n_y < 0)
        {
            c_y--;
            n_y = CHUNK_SIZE - 1;
        }
        else if (n_y >= CHUNK_SIZE)
        {
            c_y++;
            n_y = 0;
        }

        if (n_z < 0)
        {
            c_z--;
            n_z = CHUNK_SIZE - 1;
        }
        else if (n_z >= CHUNK_SIZE)
        {
            c_z++;
            n_z = 0;
        }

        if (c_x < 0 || c_y < 0 || c_z < 0 ||
            c_x >= polygon.Chunks.GetLength(0) ||
            c_y >= polygon.Chunks.GetLength(1) ||
            c_z >= polygon.Chunks.GetLength(2)) return;

        // Cases
        // 0 -> 1, 1
        // 0 -> 1, 0
        // 1 -> 0, 1
        // 1 -> 0, 0

        int squashedIndex = SquashIndex(x, y, z);
        int squashedNeighbour = SquashIndex(n_x, n_y, n_z);
        if (Voxels[x][y][z] == 1)
        {
            if (polygon.Chunks[c_x, c_y, c_z].Voxels[n_x][n_y][n_z] == 1)
            {
                // Both sides are filled, remove faces between them

                if ((VoxelMasks[squashedIndex] & dir) == dir)
                {
                    VoxelMasks[squashedIndex] &= ~dir;
                    Faces[squashedIndex]--;
                }

                if ((polygon.Chunks[c_x, c_y, c_z].VoxelMasks[squashedNeighbour] & n_dir) == n_dir)
                {
                    polygon.Chunks[c_x, c_y, c_z].VoxelMasks[squashedNeighbour] &= ~n_dir;

                    polygon.Chunks[c_x, c_y, c_z].Faces[squashedNeighbour]--;

                    polygon.EnqueueChunkToUpdate(polygon.Chunks[c_x, c_y, c_z]);
                }
            }
            else
            {
                // Voxel is filled, but neighbour is not, add face to current voxel
                if ((VoxelMasks[squashedIndex] & dir) == dir) return;

                VoxelMasks[squashedIndex] |= dir;
                Faces[squashedIndex]++;
            }
        }
        else
        {
            if (polygon.Chunks[c_x, c_y, c_z].Voxels[n_x][n_y][n_z] == 1)
            {
                // Only neighbour is filled, add a face to that voxel
                polygon.Chunks[c_x, c_y, c_z].VoxelMasks[squashedNeighbour] |= n_dir;
                polygon.Chunks[c_x, c_y, c_z].Faces[squashedNeighbour]++;

                polygon.EnqueueChunkToUpdate(polygon.Chunks[c_x, c_y, c_z]);
            }
            else
            {
                // Neither are filled, reset their faces
                VoxelMasks[squashedIndex] =
                    polygon.Chunks[c_x, c_y, c_z].VoxelMasks[squashedNeighbour] = 0;

                Faces[squashedIndex] =
                    polygon.Chunks[c_x, c_y, c_z].Faces[squashedNeighbour] = 0;

                polygon.EnqueueChunkToUpdate(polygon.Chunks[c_x, c_y, c_z]);
            }
        }
    }


    public void RecomputeMeshOptimized()
    {
        // Recompute the total number of vertices required to render the chunk
        
        numVerts = numTriangles = 0;
        for (int x = 0; x < CHUNK_SIZE; x++)
        {
            for (int y = 0; y < CHUNK_SIZE; y++)
            {
                for (int z = 0; z < CHUNK_SIZE; z++)
                {
                    int index = z + CHUNK_SIZE * (y + CHUNK_SIZE * x);
                    numVerts += Faces[index] * 4;
                    numTriangles += Faces[index] * 6;
                    vertSum[index] = numVerts;
                }
            }
        }

        numTriangles = numVerts / 4 * 6;
        

        mesh.Clear();
        transform.position = polygon.transform.position + new Vector3(X, Y, Z) * CHUNK_SIZE * Voxel.VoxelSize;

        if (numVerts == 0) return;

        
        maskBuffer = new ComputeBuffer(CHUNK_SIZE * CHUNK_SIZE * CHUNK_SIZE, sizeof(int));
        faceBuffer = new ComputeBuffer(CHUNK_SIZE * CHUNK_SIZE * CHUNK_SIZE, sizeof(int));
        vertBuffer = new ComputeBuffer(CHUNK_SIZE * CHUNK_SIZE * CHUNK_SIZE, sizeof(int));

        maskBuffer.SetData(VoxelMasks);
        faceBuffer.SetData(Faces);
        vertBuffer.SetData(vertSum);

        ComputeBuffer verticesBuffer = new ComputeBuffer(numVerts, 12);
        ComputeBuffer triangleBuffer = new ComputeBuffer(numTriangles, sizeof(int));
        ComputeBuffer normalBuffer = new ComputeBuffer(numVerts, 12);

        polygon.MeshShader.SetBuffer(handle, "voxelMasks", maskBuffer);
        polygon.MeshShader.SetBuffer(handle, "faceArray", faceBuffer);
        polygon.MeshShader.SetBuffer(handle, "vertSum", vertBuffer);

        polygon.MeshShader.SetBuffer(handle, "vertices", verticesBuffer);
        polygon.MeshShader.SetBuffer(handle, "triangles", triangleBuffer);
        polygon.MeshShader.SetBuffer(handle, "normals", normalBuffer);

        polygon.MeshShader.SetFloat("voxelSize", Voxel.VoxelSize);

        polygon.MeshShader.Dispatch(handle, CHUNK_SIZE / 8, CHUNK_SIZE / 8 , CHUNK_SIZE / 8);

        Vector3[] verts = new Vector3[numVerts];
        int[] triangs = new int[numTriangles];
        Vector3[] norms = new Vector3[numVerts];

        verticesBuffer.GetData(verts);
        triangleBuffer.GetData(triangs);
        //normalBuffer.GetData(norms);
        
        maskBuffer.Dispose();
        faceBuffer.Dispose();
        vertBuffer.Dispose();

        verticesBuffer.Dispose();
        triangleBuffer.Dispose();
        normalBuffer.Dispose();
        

        //Vector3[] verts = new Vector3[numVerts];
        //int[] triangs = new int[numTriangles];
        //Vector3[] norms = new Vector3[numVerts];

        /*
        int id;
        for (int x = 0; x < CHUNK_SIZE; x++)
        {
            for (int y = 0; y < CHUNK_SIZE; y++)
            {
                for (int z = 0; z < CHUNK_SIZE; z++)
                {
                    id = z + CHUNK_SIZE * (y + CHUNK_SIZE * x);;
                    GenerateVoxel(verts, norms, triangs, x * Voxel.VoxelSize, y * Voxel.VoxelSize, z * Voxel.VoxelSize, 
                        vertSum[id] - (Faces[id] * 4), 
                        VoxelMasks[id]);
                }
            }
        }

        mesh.vertices = verts;
        mesh.triangles = triangs;
        mesh.normals = norms;
        
    }

    private void GenerateTriangles(int[] triangs, int tri, int vertex)
    {
        triangs[tri] = vertex;
        triangs[tri + 1] = vertex + 1;
        triangs[tri + 2] = vertex + 2;

        triangs[tri + 3] = vertex;
        triangs[tri + 4] = vertex + 2;
        triangs[tri + 5] = vertex + 3;
    }

    private void GenerateNormals(List<Vector3> norms, int offset, int direction)
    {
        switch (direction)
        {
            case TOP:
                norms[offset++].Set(0, 1, 0);
                norms[offset++].Set(0, 1, 0);
                norms[offset++].Set(0, 1, 0);
                norms[offset++].Set(0, 1, 0);
                break;
            case BOTTOM:
                norms[offset++].Set(0, -1, 0);
                norms[offset++].Set(0, -1, 0);
                norms[offset++].Set(0, -1, 0);
                norms[offset++].Set(0, -1, 0);
                break;
            case LEFT:
                norms[offset++].Set(-1, 0, 0);
                norms[offset++].Set(-1, 0, 0);
                norms[offset++].Set(-1, 0, 0);
                norms[offset++].Set(-1, 0, 0);
                break;
            case RIGHT:
                norms[offset++].Set(1, 0, 0);
                norms[offset++].Set(1, 0, 0);
                norms[offset++].Set(1, 0, 0);
                norms[offset++].Set(1, 0, 0);
                break;
            case FORWARD:
                norms[offset++].Set(0, 0, 1);
                norms[offset++].Set(0, 0, 1);
                norms[offset++].Set(0, 0, 1);
                norms[offset++].Set(0, 0, 1);
                break;
            case BACK:
                norms[offset++].Set(0, 0, -1);
                norms[offset++].Set(0, 0, -1);
                norms[offset++].Set(0, 0, -1);
                norms[offset++].Set(0, 0, -1);
                break;
        }
    }

    int GenerateVoxel(List<Vector3> verts, List<Vector3> norms, int[] triangs, float x, float y, float z, int vertexOffset, int mask)
    {
        int triangleIndex = vertexOffset / 4 * 6;

        if ((mask & TOP) == TOP)
        {
            GenerateNormals(norms, vertexOffset, TOP);
            GenerateTriangles(triangs, triangleIndex, vertexOffset);
            triangleIndex += 6;
            verts[vertexOffset++].Set(x, y, z + Voxel.VoxelSize);
            verts[vertexOffset++].Set(x + Voxel.VoxelSize, y, z + Voxel.VoxelSize);
            verts[vertexOffset++].Set(x + Voxel.VoxelSize, y, z);
            verts[vertexOffset++].Set(x, y, z);
        }

        if ((mask & BOTTOM) == BOTTOM)
        {
            GenerateNormals(norms, vertexOffset, BOTTOM);
            GenerateTriangles(triangs, triangleIndex, vertexOffset);
            triangleIndex += 6;
            verts[vertexOffset++].Set(x, y - Voxel.VoxelSize, z);
            verts[vertexOffset++].Set(x + Voxel.VoxelSize, y - Voxel.VoxelSize, z);
            verts[vertexOffset++].Set(x + Voxel.VoxelSize, y - Voxel.VoxelSize, z + Voxel.VoxelSize);
            verts[vertexOffset++].Set(x, y - Voxel.VoxelSize, z + Voxel.VoxelSize);
        }

        if ((mask & LEFT) == LEFT)
        {
            GenerateNormals(norms, vertexOffset, LEFT);
            GenerateTriangles(triangs, triangleIndex, vertexOffset);
            triangleIndex += 6;
            verts[vertexOffset++].Set(x, y - Voxel.VoxelSize, z + Voxel.VoxelSize);
            verts[vertexOffset++].Set(x, y, z + Voxel.VoxelSize);
            verts[vertexOffset++].Set(x, y, z);
            verts[vertexOffset++].Set(x, y - Voxel.VoxelSize, z);
        }

        if ((mask & RIGHT) == RIGHT)
        {
            GenerateNormals(norms, vertexOffset, RIGHT);
            GenerateTriangles(triangs, triangleIndex, vertexOffset);
            triangleIndex += 6;
            verts[vertexOffset++].Set(x + Voxel.VoxelSize, y - Voxel.VoxelSize, z);
            verts[vertexOffset++].Set(x + Voxel.VoxelSize, y, z);
            verts[vertexOffset++].Set(x + Voxel.VoxelSize, y, z + Voxel.VoxelSize);
            verts[vertexOffset++].Set(x + Voxel.VoxelSize, y - Voxel.VoxelSize, z + Voxel.VoxelSize);
        }

        if ((mask & FORWARD) == FORWARD)
        {
            GenerateNormals(norms, vertexOffset, FORWARD);
            GenerateTriangles(triangs, triangleIndex, vertexOffset);
            triangleIndex += 6;
            verts[vertexOffset++].Set(x + Voxel.VoxelSize, y - Voxel.VoxelSize, z + Voxel.VoxelSize);
            verts[vertexOffset++].Set(x + Voxel.VoxelSize, y, z + Voxel.VoxelSize);
            verts[vertexOffset++].Set(x, y, z + Voxel.VoxelSize);
            verts[vertexOffset++].Set(x, y - Voxel.VoxelSize, z + Voxel.VoxelSize);
        }

        if ((mask & BACK) == BACK)
        {
            GenerateNormals(norms, vertexOffset, BACK);
            GenerateTriangles(triangs, triangleIndex, vertexOffset);
            verts[vertexOffset++].Set(x, y - Voxel.VoxelSize, z);
            verts[vertexOffset++].Set(x, y, z);
            verts[vertexOffset++].Set(x + Voxel.VoxelSize, y, z);
            verts[vertexOffset++].Set(x + Voxel.VoxelSize, y - Voxel.VoxelSize, z);
        }

        return vertexOffset;
    }

    /// <summary>
    /// Checks every voxel and determines how many faces is required for that specific voxel
    /// </summary>
    public void RecomputeFaces()
    {
        for (int x = 0; x < CHUNK_SIZE; x++)
        {
            for (int y = 0; y < CHUNK_SIZE; y++)
            {
                for (int z = 0; z < CHUNK_SIZE; z++)
                {
                    VoxelMasks[z + CHUNK_SIZE * (y + CHUNK_SIZE * x)] = GetFaces(x, y, z);

                    Center[x][y][z].Set(x * Voxel.VoxelSize, y * Voxel.VoxelSize, z * Voxel.VoxelSize);
                    Center[x][y][z] += Voxel.VoxelSize / 2f * new Vector3(1, -1, 1);
                }
            }
        }
    }
    */
    #endregion

}