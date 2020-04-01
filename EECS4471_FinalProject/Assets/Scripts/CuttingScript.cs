using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CuttingScript : MonoBehaviour
{
    private BoxCollider collider;

    private Polygon polygon;
    // Start is called before the first frame update
    void Start()
    {
        collider = GetComponent<BoxCollider>();
        polygon = GameObject.Find("Polygon").GetComponent<Polygon>();

    }

    // Update is called once per frame
    void Update()
    {
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.TransformPoint(0f, 0f, -0.5f), 
            transform.forward);
    }

    void OnTriggerStay(Collider other)
    {
        Chunk c = other.GetComponent<Chunk>();

        if (c == null) return;
        if (c.Empty) return;

        // Get hit location
        Vector3[] rays = new Vector3[4];
        rays[0] = transform.TransformPoint(0.5f, -0.5f, -0.5f);
        rays[1] = transform.TransformPoint(-0.5f, -0.5f, -0.5f);
        rays[2] = transform.TransformPoint(-0.5f, 0.5f, -0.5f);
        rays[3] = transform.TransformPoint(0.5f, 0.5f, -0.5f);

        foreach (Vector3 r in rays)
        {
            Vector3 current = r;
            float distance = 0f;

            while (distance < 0.16f)
            {
                int[] coordinates = c.VectorToCoord(current);

                if (coordinates[0] >= 0 && coordinates[1] >= 0 && coordinates[2] >= 0 &&
                    coordinates[0] < Chunk.CHUNK_SIZE && coordinates[1] < Chunk.CHUNK_SIZE && coordinates[2] < Chunk.CHUNK_SIZE)
                {
                    if (c.Voxels[coordinates[0]][coordinates[1]][coordinates[2]] == 1)
                    {
                        c.Voxels[coordinates[0]][coordinates[1]][coordinates[2]] = 0;
                        c.MakeDirty();

                        if (coordinates[0] + 1 == Chunk.CHUNK_SIZE)
                        {
                            if (polygon.InBounds(c.X + 1, c.Y, c.Z))
                            {
                                polygon.Chunks[c.X + 1, c.Y, c.Z].MakeDirty();
                            }
                        }

                        if (coordinates[0] - 1 < 0)
                        {
                            if (polygon.InBounds(c.X - 1, c.Y, c.Z))
                            {
                                if (!polygon.Chunks[c.X - 1, c.Y, c.Z].Empty)
                                {
                                    polygon.Chunks[c.X - 1, c.Y, c.Z].MakeDirty();
                                }
                            }
                        }

                        if (coordinates[1] + 1 == Chunk.CHUNK_SIZE)
                        {
                            if (polygon.InBounds(c.X, c.Y + 1, c.Z))
                            {
                                if (!polygon.Chunks[c.X, c.Y + 1, c.Z].Empty)
                                {
                                    polygon.Chunks[c.X, c.Y + 1, c.Z].MakeDirty();
                                }
                            }
                        }

                        if (coordinates[1] - 1 < 0)
                        {
                            if (polygon.InBounds(c.X, c.Y - 1, c.Z))
                            {
                                if (!polygon.Chunks[c.X, c.Y - 1, c.Z].Empty)
                                {
                                    polygon.Chunks[c.X, c.Y - 1, c.Z].MakeDirty();
                                }
                            }
                        }

                        if (coordinates[2] + 1 == Chunk.CHUNK_SIZE)
                        {
                            if (polygon.InBounds(c.X, c.Y, c.Z + 1))
                            {
                                if (!polygon.Chunks[c.X, c.Y, c.Z + 1].Empty)
                                {
                                    polygon.Chunks[c.X, c.Y, c.Z + 1].MakeDirty();
                                }
                            }
                        }

                        if (coordinates[2] - 1 < 0)
                        {
                            if (polygon.InBounds(c.X, c.Y, c.Z - 1))
                            {
                                if (!polygon.Chunks[c.X, c.Y, c.Z - 1].Empty)
                                {
                                    polygon.Chunks[c.X, c.Y, c.Z - 1].MakeDirty();
                                }
                            }
                        }
                    }
                }

                current += Voxel.VOXEL_SIZE * transform.forward;
                distance = (current - r).magnitude;
            }
        }
    }
}
