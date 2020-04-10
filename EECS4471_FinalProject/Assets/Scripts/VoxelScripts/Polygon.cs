using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Vector3 = UnityEngine.Vector3;

public class Polygon : MonoBehaviour
{
    public Chunk[,,] Chunks { get; } = new Chunk[20,20,20];

    public GameObject ChunkPrefab;
    public ComputeShader MeshShader;

    private HashSet<Chunk> chunksToUpdate = new HashSet<Chunk>();

    // Start is called before the first frame update
    void Start()
    {
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
                        GetComponent<MeshRenderer>().material, (x >= 1 && x <= 4 && y >= 1 && y <= 4 && z >= 1 && z <= 4) ? (byte)1 : (byte)0);
                    chunk.layer = 8;
                }
            }
        }

        RecomputeChunks(true);
    }

    public void RecomputeChunks(bool force)
    {
        int threadSize = (Chunks.GetLength(0) * Chunks.GetLength(1) * Chunks.GetLength(2)) / SystemInfo.processorCount;
        Thread[] threads = new Thread[SystemInfo.processorCount];
        int counter = threadSize;

        for (int i = 0; i < threads.Length; i++)
        {
            int start = i * threadSize;
            int end = counter;

            threads[i] = new Thread(() => RecomputeChunkThread(start, end));
            threads[i].Start();

            counter += threadSize;
        }

        foreach (Thread t in threads)
            t.Join();

        foreach (Chunk c in Chunks)
            c.SetMesh();
    }

    private void RecomputeChunkThread(int start, int end)
    {
        /*
                     * int zDirection = i % zLength;
            int yDirection = (i / zLength) % yLength;
            int xDirection = i / (yLength * zLength); 
         */
        for (int i = start; i < end; i++)
        {
            int x = i / (Chunks.GetLength(1) * Chunks.GetLength(2));
            int y = (i / Chunks.GetLength(2)) % Chunks.GetLength(1);
            int z = i % Chunks.GetLength(2);
            
            Chunks[x,y,z].RecomputeMesh();
            
            //GetFaces(x, y, z);
            //VoxelMasks[i] = GetFaces(x, y, z);
        }
    }

    public void InitCube()
    {
        Vector3 center = transform.position + 
                       (Vector3.one * (Voxel.STANDARD_VOXEL_SIZE * Chunk.CHUNK_SIZE / 2f));
        
        Voxel.VoxelSize = Voxel.STANDARD_VOXEL_SIZE;
        for (int x = 0; x < Chunks.GetLength(0); x++)
        {
            for (int y = 0; y < Chunks.GetLength(1); y++)
            {
                for (int z = 0; z < Chunks.GetLength(2); z++)
                {
                    for (int c_x = 0; c_x < Chunk.CHUNK_SIZE; c_x++)
                    {
                        for (int c_y = 0; c_y < Chunk.CHUNK_SIZE; c_y++)
                        {
                            for (int c_z = 0; c_z < Chunk.CHUNK_SIZE; c_z++)
                            {
                                if (x >= 4 && x <= 8 &&
                                    y >= 4 && y <= 8 &&
                                    z >= 4 && z <= 8)
                                {
                                    Chunks[x, y, z].Voxels[c_x][c_y][c_z] = 1;
                                }
                                else
                                {
                                    Chunks[x, y, z].Voxels[c_x][c_y][c_z] = 0;
                                }
                            }
                        }
                    }
                }
            }
        }

        RecomputeChunks(true);
    }

    public void InitSphere()
    {
        Vector3 center = transform.position + Vector3.one *
            Chunks.GetLength(0) * Chunk.CHUNK_SIZE * Voxel.STANDARD_VOXEL_SIZE / 2f;

        for (int x = 0; x < Chunks.GetLength(0); x++)
        {
            for (int y = 0; y < Chunks.GetLength(1); y++)
            {
                for (int z = 0; z < Chunks.GetLength(2); z++)
                {
                    for (int c_x = 0; c_x < Chunk.CHUNK_SIZE; c_x++)
                    {
                        for (int c_y = 0; c_y < Chunk.CHUNK_SIZE; c_y++)
                        {
                            for (int c_z = 0; c_z < Chunk.CHUNK_SIZE; c_z++)
                            {
                                Vector3 vect = Chunks[x, y, z].transform.position +
                                               (new Vector3(c_x,c_y,c_z) * Voxel.STANDARD_VOXEL_SIZE);

                                if ((vect - center).magnitude < 0.4f)
                                {
                                    Chunks[x, y, z].Voxels[c_x][c_y][c_z] = 8;
                                }
                                else
                                {
                                    Chunks[x, y, z].Voxels[c_x][c_y][c_z] = 0;
                                }
                            }
                        }
                    }
                }
            }
        }

        RecomputeChunks(true);
    }

    public void ClearChunks()
    {
        for (int x = 0; x < Chunks.GetLength(0); x++)
        {
            for (int y = 0; y < Chunks.GetLength(1); y++)
            {
                for (int z = 0; z < Chunks.GetLength(2); z++)
                {
                    for (int c_x = 0; c_x < Chunk.CHUNK_SIZE; c_x++)
                    {
                        for (int c_y = 0; c_y < Chunk.CHUNK_SIZE; c_y++)
                        {
                            for (int c_z = 0; c_z < Chunk.CHUNK_SIZE; c_z++)
                            {
                                Chunks[x, y, z].Voxels[c_x][c_y][c_z] = 0;
                            }
                        }
                    }
                }
            }
        }

        RecomputeChunks(true);
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
            RecomputeChunks(true);
            timer.Stop();

            Debug.Log(timer.ElapsedMilliseconds + "ms");

            timer.Restart();
            foreach (Chunk c in Chunks)
            {
                c.RecomputeMesh();
                c.SetMesh();
            }
            timer.Stop();

            Debug.Log(timer.ElapsedMilliseconds + "ms");
        }

        if (Input.GetKeyDown(KeyCode.A))
        {
            Voxel.VoxelSize -= 0.01f;
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            Voxel.VoxelSize += 0.01f;
        }

        if (Input.GetKey(KeyCode.S))
        {
            int c_x = Random.Range(0, Chunks.GetLength(0) - 1);
            int c_y = Random.Range(0, Chunks.GetLength(1) - 1);
            int c_z = Random.Range(0, Chunks.GetLength(2) - 1);

            int a_x = Random.Range(0, Chunk.CHUNK_SIZE - 1);
            int a_y = Random.Range(0, Chunk.CHUNK_SIZE - 1);
            int a_z = Random.Range(0, Chunk.CHUNK_SIZE - 1);

            if (Chunks[c_x, c_y, c_z].Voxels[a_x][a_y][a_z] != 0)
            {
                Chunks[c_x, c_y, c_z].ModifyColour(a_x, a_y, a_z,
                    (byte)Random.Range(1, 7)
                );
                //Chunks[c_x, c_y, c_z].SetUVs();
            }
            //Chunks[c_x, c_y, c_z].ModifyVoxel(a_x, a_y, a_z, 1);

            //EnqueueChunkToUpdate(Chunks[c_x, c_y, c_z]);
                //Chunks[c_x, c_y, c_z].SetUVs();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            InitSphere();
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            ClearChunks();
        }


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

    public bool InBounds(int x, int y, int z)
    {
        return x >= 0 && y >= 0 && z >= 0 &&
               x < Chunks.GetLength(0) &&
               y < Chunks.GetLength(1) &&
               z < Chunks.GetLength(2);
    }

    void OnDrawGizmos()
    {
        //Vector3 screenPos = Vector3.zero;
        //screenPos.x = 0.15f * Camera.main.pixelWidth + 0.7f * Input.mousePosition.x;
        //screenPos.y = 0.15f * Camera.main.pixelHeight + 0.7f * Input.mousePosition.y;
        //Ray r = Camera.main.ScreenPointToRay(screenPos, Camera.main.stereoActiveEye);

        /*
        if (Physics.Raycast(r, out RaycastHit hit))
        {
            //Gizmos.DrawLine(r.origin, hit.point);
        }
        */
        //Debug.Log(r.direction);
        //Gizmos.DrawLine(r.origin, r.origin + r.direction * 10f);
        /*
        foreach (Chunk c in Chunks)
        {
            Vector3[] boundsCorners = new Vector3[8];

            boundsCorners[0] = c.GetComponent<MeshRenderer>().bounds.min;
            boundsCorners[1] = c.GetComponent<MeshRenderer>().bounds.max;
            boundsCorners[2] = new Vector3(boundsCorners[0].x, boundsCorners[0].y, boundsCorners[1].z);
            boundsCorners[3] = new Vector3(boundsCorners[0].x, boundsCorners[1].y, boundsCorners[0].z);
            boundsCorners[4] = new Vector3(boundsCorners[1].x, boundsCorners[0].y, boundsCorners[0].z);
            boundsCorners[5] = new Vector3(boundsCorners[0].x, boundsCorners[1].y, boundsCorners[1].z);
            boundsCorners[6] = new Vector3(boundsCorners[1].x, boundsCorners[0].y, boundsCorners[1].z);
            boundsCorners[7] = new Vector3(boundsCorners[1].x, boundsCorners[1].y, boundsCorners[0].z);

            Gizmos.color = Color.red;
            foreach (Vector3 corner in boundsCorners)
            {
                
                Gizmos.DrawLine(mainCam.transform.position, corner);
            }
            //Gizmos.DrawLine(mainCam.transform.position, c.transform.position);
            //Gizmos.DrawLine(mainCam.transform.position, c.transform.position);
            //Gizmos.DrawLine(mainCam.transform.position, c.transform.position);
        }
        */
    }
}
