using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Vector3 = UnityEngine.Vector3;

public class Polygon : MonoBehaviour
{
    public Chunk[,,] Chunks { get; } = new Chunk[15,15,15];

    public GameObject ChunkPrefab;
    public ComputeShader MeshShader;

    public GameObject BoundingBox;

    private HashSet<Chunk> chunksToUpdate = new HashSet<Chunk>();

    // Start is called before the first frame update
    void Start()
    {
        // Initializes a small cube with random colours
        for (int x = 0; x < Chunks.GetLength(0); x++)
        {
            for (int y = 0; y < Chunks.GetLength(1); y++)
            {
                for (int z = 0; z < Chunks.GetLength(2); z++)
                {
                    GameObject chunk = Instantiate(ChunkPrefab, transform);
                    Chunks[x, y, z] = chunk.GetComponent<Chunk>();
                    Chunks[x, y, z].Init(this, x, y, z,
                        transform.position + (new Vector3(x, y, z) * Chunk.CHUNK_SIZE * Voxel.VoxelSize),
                        GetComponent<MeshRenderer>().material, (x >= 9 && x <= 12 && y >= 3 && y <= 6 && z >= 9 && z <= 12) ? (byte) Random.Range(1, 7) : (byte)0);
                    chunk.layer = 8;
                }
            }
        }

        RecomputeChunks();
    }

    /// <summary>
    /// Allocates enough threads to saturate the number of CPU threads and
    /// computes the meshes of all the chunks inside the polygon.
    /// </summary>
    public void RecomputeChunks()
    {
        int threadSize = (Chunks.GetLength(0) * Chunks.GetLength(1) * Chunks.GetLength(2)) / SystemInfo.processorCount;
        Thread[] threads = new Thread[SystemInfo.processorCount];
        int counter = threadSize;

        // Start all threads
        for (int i = 0; i < threads.Length; i++)
        {
            int start = i * threadSize;
            int end = counter;

            threads[i] = new Thread(() => RecomputeChunkThread(start, end));
            threads[i].Start();

            counter += threadSize;
        }

        // Wait for all threads to finish before continuing
        foreach (Thread t in threads)
            t.Join();

        foreach (Chunk c in Chunks)
            c.SetMesh();

        transform.position = new Vector3(
            -Chunks.GetLength(0) * Voxel.VoxelSize * Chunk.CHUNK_SIZE / 2f,
            1.8f,
            -Chunks.GetLength(2) * Voxel.VoxelSize * Chunk.CHUNK_SIZE / 2f
            );

        BoundingBox.transform.localScale = new Vector3(
            Chunks.GetLength(0) * Voxel.VoxelSize * Chunk.CHUNK_SIZE,
            Chunks.GetLength(1) * Voxel.VoxelSize * Chunk.CHUNK_SIZE,
            Chunks.GetLength(2) * Voxel.VoxelSize * Chunk.CHUNK_SIZE
            );

        BoundingBox.transform.localPosition = new Vector3(
                BoundingBox.transform.localScale.x / 2f,
                BoundingBox.transform.localScale.y / 2f,
                BoundingBox.transform.localScale.z / 2f
            );
    }

    /// <summary>
    /// A method initialized by a thread to recompute the meshes of a set of chunks
    /// between start and end
    /// </summary>
    private void RecomputeChunkThread(int start, int end)
    {
        for (int i = start; i < end; i++)
        {
            int x = i / (Chunks.GetLength(1) * Chunks.GetLength(2));
            int y = (i / Chunks.GetLength(2)) % Chunks.GetLength(1);
            int z = i % Chunks.GetLength(2);
            
            Chunks[x,y,z].RecomputeMesh();
        }
    }

    /// <summary>
    /// Generates a cube within the polygon boundaries
    /// </summary>
    public void InitCube()
    {
        Voxel.VoxelSize = Voxel.STANDARD_VOXEL_SIZE;
        for (int x = 0; x < Chunks.GetLength(0); x++)
            for (int y = 0; y < Chunks.GetLength(1); y++)
                for (int z = 0; z < Chunks.GetLength(2); z++)
                    for (int c_x = 0; c_x < Chunk.CHUNK_SIZE; c_x++)
                        for (int c_y = 0; c_y < Chunk.CHUNK_SIZE; c_y++)
                            for (int c_z = 0; c_z < Chunk.CHUNK_SIZE; c_z++)
                            {
                                if (x >= 4 && x <= 8 &&
                                    y >= 4 && y <= 8 &&
                                    z >= 4 && z <= 8)
                                    Chunks[x, y, z].Voxels[c_x][c_y][c_z] = 1;
                                else
                                    Chunks[x, y, z].Voxels[c_x][c_y][c_z] = 0;
                            }

        RecomputeChunks();
    }

    /// <summary>
    /// Generates a sphere within the polygon boundaries
    /// </summary>
    public void InitSphere()
    {
        Voxel.VoxelSize = Voxel.STANDARD_VOXEL_SIZE;

        Vector3 center = transform.position + Vector3.one *
            Chunks.GetLength(0) * Chunk.CHUNK_SIZE * Voxel.STANDARD_VOXEL_SIZE / 2f;

        Vector3 vect = Vector3.zero;

        for (int x = 0; x < Chunks.GetLength(0); x++)
            for (int y = 0; y < Chunks.GetLength(1); y++)
                for (int z = 0; z < Chunks.GetLength(2); z++)
                    for (int c_x = 0; c_x < Chunk.CHUNK_SIZE; c_x++)
                        for (int c_y = 0; c_y < Chunk.CHUNK_SIZE; c_y++)
                            for (int c_z = 0; c_z < Chunk.CHUNK_SIZE; c_z++)
                            {
                                vect.Set(
                                    Chunks[x, y, z].transform.position.x + (c_x * Voxel.VoxelSize),
                                    Chunks[x, y, z].transform.position.y + (c_y * Voxel.VoxelSize),
                                    Chunks[x, y, z].transform.position.z + (c_z * Voxel.VoxelSize)
                                    );

                                Chunks[x, y, z].Voxels[c_x][c_y][c_z] =
                                    (vect - center).magnitude < 0.4f ? (byte) 8 : (byte) 0;
                            }

        RecomputeChunks();
    }

    /// <summary>
    /// Resets the entire modifiable chunk array to empty
    /// </summary>
    public void ClearChunks()
    {
        Voxel.VoxelSize = Voxel.STANDARD_VOXEL_SIZE;

        for (int x = 0; x < Chunks.GetLength(0); x++)
            for (int y = 0; y < Chunks.GetLength(1); y++)
                for (int z = 0; z < Chunks.GetLength(2); z++)
                    for (int c_x = 0; c_x < Chunk.CHUNK_SIZE; c_x++)
                        for (int c_y = 0; c_y < Chunk.CHUNK_SIZE; c_y++)
                            for (int c_z = 0; c_z < Chunk.CHUNK_SIZE; c_z++)
                                Chunks[x, y, z].Voxels[c_x][c_y][c_z] = 0;

        RecomputeChunks();
    }

    public void EnqueueChunkToUpdate(Chunk c)
    {
        if (!chunksToUpdate.Contains(c))
            chunksToUpdate.Add(c);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            Stopwatch timer = new Stopwatch();

            timer.Start();
            RecomputeChunks();
            timer.Stop();

            Debug.Log(timer.ElapsedMilliseconds + "ms");
        }

        if (Input.GetKeyDown(KeyCode.A))
            Voxel.VoxelSize -= 0.01f;
        else if (Input.GetKeyDown(KeyCode.D))
            Voxel.VoxelSize += 0.01f;

        if (Input.GetKey(KeyCode.S))
        {
            int c_x = Random.Range(0, Chunks.GetLength(0) - 1);
            int c_y = Random.Range(0, Chunks.GetLength(1) - 1);
            int c_z = Random.Range(0, Chunks.GetLength(2) - 1);

            int a_x = Random.Range(0, Chunk.CHUNK_SIZE - 1);
            int a_y = Random.Range(0, Chunk.CHUNK_SIZE - 1);
            int a_z = Random.Range(0, Chunk.CHUNK_SIZE - 1);

            if (Chunks[c_x, c_y, c_z].Voxels[a_x][a_y][a_z] != 0)
                Chunks[c_x, c_y, c_z].ModifyColour(a_x, a_y, a_z,  (byte)Random.Range(1, 7));
        }

        if (Input.GetKeyDown(KeyCode.R))
            InitSphere();

        if (Input.GetKeyDown(KeyCode.P))
            ClearChunks();

        if (chunksToUpdate.Count > 0)
        {
            foreach (Chunk c in chunksToUpdate)
            {
                c.RecomputeMesh();
                c.SetMesh();
            }

            chunksToUpdate.Clear();
        }
    }
}
