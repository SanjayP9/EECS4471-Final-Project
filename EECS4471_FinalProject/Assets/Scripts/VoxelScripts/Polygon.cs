using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class Polygon : MonoBehaviour
{
    public Chunk[,,] Chunks { get; } = new Chunk[6,6,6];
    MeshFilter meshFilter;

    float timer = 0.0f;

    public GameObject ChunkPrefab;
    public ComputeShader MeshShader;

    private Camera mainCam;

    private int x = 0, y = 0, z = 0;
    private int x_c = 0, y_c = 0, z_c = 0;

    // Start is called before the first frame update
    void Start()
    {
        mainCam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();

        meshFilter = GetComponent<MeshFilter>();

        for (int x = 0; x < Chunks.GetLength(0); x++)
        {
            for (int y = 0; y < Chunks.GetLength(1); y++)
            {
                for (int z = 0; z < Chunks.GetLength(2); z++)
                {
                    GameObject chunk = Instantiate(ChunkPrefab, transform);
                    Chunks[x, y, z] = chunk.GetComponent<Chunk>();
                    Chunks[x, y, z].Init(this, x, y, z,
                        transform.position + (new Vector3(x, y, z) * Chunk.CHUNK_SIZE * Voxel.VOXEL_SIZE),
                        GetComponent<MeshRenderer>().material, (x >= 1 && x <= 4 && y >= 1 && y <= 4 && z >= 1 && z <= 4) ? (byte)1 : (byte)0);
                        //GetComponent<MeshRenderer>().material, 1);
                    chunk.layer = 8;
                }
            }
        }

        foreach (Chunk c in Chunks)
        {
            c.RecomputeMesh();
        }
    }

    // Update is called once per frame
    void Update()
    {

        //timer += Time.deltaTime;


        if (timer >= 0.01f)
        {
            
            for (int i = 0; i < 10; i++)
            {
                Chunk c = Chunks[Random.Range(0, Chunks.GetLength(0)), Random.Range(0, Chunks.GetLength(1)),
                    Random.Range(0, Chunks.GetLength(2))];

                c.Voxels[Random.Range(0, c.Voxels.GetLength(0)), Random.Range(0, c.Voxels.GetLength(1)),
                    Random.Range(0, c.Voxels.GetLength(2))] = 1;

                c.RecomputeMesh();
                /*
                Vector3[] boundsCorners = new Vector3[8];

                boundsCorners[0] = c.GetComponent<MeshRenderer>().bounds.min;
                boundsCorners[1] = c.GetComponent<MeshRenderer>().bounds.max;
                boundsCorners[2] = new Vector3(boundsCorners[0].x, boundsCorners[0].y, boundsCorners[1].z);
                boundsCorners[3] = new Vector3(boundsCorners[0].x, boundsCorners[1].y, boundsCorners[0].z);
                boundsCorners[4] = new Vector3(boundsCorners[1].x, boundsCorners[0].y, boundsCorners[0].z);
                boundsCorners[5] = new Vector3(boundsCorners[0].x, boundsCorners[1].y, boundsCorners[1].z);
                boundsCorners[6] = new Vector3(boundsCorners[1].x, boundsCorners[0].y, boundsCorners[1].z);
                boundsCorners[7] = new Vector3(boundsCorners[1].x, boundsCorners[1].y, boundsCorners[0].z);

                foreach (Vector3 corner in boundsCorners)
                {
                    Physics.Raycast(new Ray(mainCam.transform.position, corner - mainCam.transform.position), out RaycastHit hit);

                    if (hit.transform != null)
                    {
                        c.GetComponent<MeshRenderer>().enabled = hit.transform.gameObject.GetComponent<Chunk>() == c;

                        if (c.GetComponent<MeshRenderer>().enabled)
                        {
                            break;
                        }
                    }
                }
                */
            }

            timer = 0f;
        }
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
