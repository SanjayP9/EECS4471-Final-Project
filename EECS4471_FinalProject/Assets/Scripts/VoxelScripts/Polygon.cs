﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class Polygon : MonoBehaviour
{
    public Chunk[,,] Chunks { get; } = new Chunk[12,12,12];
    MeshFilter meshFilter;

    float timer = 0.0f;

    public GameObject ChunkPrefab;

    private Camera mainCam;

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
                        GetComponent<MeshRenderer>().material, (x > 6 && x < 8 && y > 6 && y < 8 && z > 6 && z < 8) ? (byte)1 : (byte)0);
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
        foreach (Chunk c in Chunks)
        {
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
    }

    void OnDrawGizmos()
    {
        Ray r = new Ray(Camera.main.transform.position, Camera.main.ScreenToWorldPoint(Input.mousePosition));

        if (Physics.Raycast(r, out RaycastHit hit))
        {
            Gizmos.DrawLine(r.origin, hit.point);
        }

        Debug.Log(Camera.main.ScreenToWorldPoint(Input.mousePosition, Camera.MonoOrStereoscopicEye.Right));
        Gizmos.DrawLine(r.origin, r.direction);

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