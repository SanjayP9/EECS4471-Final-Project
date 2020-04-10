using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CuttingScript : MonoBehaviour
{
    private Polygon polygon;

    // Start is called before the first frame update
    void Start()
    {
        polygon = GameObject.Find("Polygon").GetComponent<Polygon>();
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
                        polygon.EnqueueChunkToUpdate(c);

                        if (coordinates[0] + 1 >= Chunk.CHUNK_SIZE && c.X + 1 < polygon.Chunks.GetLength(0))
                        {
                            polygon.EnqueueChunkToUpdate(polygon.Chunks[c.X + 1, c.Y, c.Z]);
                        }
                        else if (coordinates[0] - 1 < 0 && c.X - 1 >= 0)
                        {
                            polygon.EnqueueChunkToUpdate(polygon.Chunks[c.X - 1, c.Y, c.Z]);
                        }

                        if (coordinates[1] + 1 >= Chunk.CHUNK_SIZE && c.Y + 1 < polygon.Chunks.GetLength(1))
                        {
                            polygon.EnqueueChunkToUpdate(polygon.Chunks[c.X, c.Y + 1, c.Z]);
                        }
                        else if (coordinates[1] - 1 < 0 && c.Y - 1 >= 0)
                        {
                            polygon.EnqueueChunkToUpdate(polygon.Chunks[c.X, c.Y - 1, c.Z]);
                        }

                        if (coordinates[2] + 1 >= Chunk.CHUNK_SIZE && c.Z + 1 < polygon.Chunks.GetLength(2))
                        {
                            polygon.EnqueueChunkToUpdate(polygon.Chunks[c.X, c.Y, c.Z + 1]);
                        }
                        else if (coordinates[2] - 1 < 0 && c.Z - 1 >= 0)
                        {
                            polygon.EnqueueChunkToUpdate(polygon.Chunks[c.X, c.Y, c.Z - 1]);
                        }
                    }
                }

                current += Voxel.VoxelSize * transform.forward;
                distance = (current - r).magnitude;
            }
        }
    }
}
