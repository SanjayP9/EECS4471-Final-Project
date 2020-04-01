using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SphereTool : MonoBehaviour
{
    public bool AddMode = true;
    private Polygon polygon;

    void Start()
    {
        polygon = GameObject.Find("Polygon").GetComponent<Polygon>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerStay(Collider other)
    {
        Chunk c = other.GetComponent<Chunk>();

        float distance = GetComponent<SphereCollider>().radius * transform.localScale.x;
        if (c == null) return;
        if (c.Empty) return;
        for (int x = 0; x < Chunk.CHUNK_SIZE; x++)
        {
            for (int y = 0; y < Chunk.CHUNK_SIZE; y++)
            {
                for (int z = 0; z < Chunk.CHUNK_SIZE; z++)
                {
                    if (!((c.transform.position + c.Center[x][y][z] - transform.position).magnitude < distance))
                        continue;
                    c.Voxels[x][y][z] = (byte) (AddMode ? 1 : 0);
                    c.MakeDirty();

                    if (x + 1 == Chunk.CHUNK_SIZE)
                    {
                        if (polygon.InBounds(c.X + 1, c.Y, c.Z))
                        {
                            if (!polygon.Chunks[c.X + 1, c.Y, c.Z].Empty)
                            {
                                polygon.Chunks[c.X + 1, c.Y, c.Z].MakeDirty();
                            }
                        }
                    }

                    if (x - 1 < 0)
                    {
                        if (polygon.InBounds(c.X - 1, c.Y, c.Z))
                        {
                            if (!polygon.Chunks[c.X - 1, c.Y, c.Z].Empty)
                            {
                                polygon.Chunks[c.X - 1, c.Y, c.Z].MakeDirty();
                            }
                        }
                    }

                    if (y + 1 == Chunk.CHUNK_SIZE)
                    {
                        if (polygon.InBounds(c.X, c.Y + 1, c.Z))
                        {
                            if (!polygon.Chunks[c.X, c.Y + 1, c.Z].Empty)
                            {
                                polygon.Chunks[c.X, c.Y + 1, c.Z].MakeDirty();
                            }
                        }
                    }

                    if (y - 1 < 0)
                    {
                        if (polygon.InBounds(c.X, c.Y - 1, c.Z))
                        {
                            if (!polygon.Chunks[c.X, c.Y - 1, c.Z].Empty)
                            {
                                polygon.Chunks[c.X, c.Y - 1, c.Z].MakeDirty();
                            }
                        }
                    }

                    if (z + 1 == Chunk.CHUNK_SIZE)
                    {
                        if (polygon.InBounds(c.X, c.Y, c.Z + 1))
                        {
                            if (!polygon.Chunks[c.X, c.Y, c.Z + 1].Empty)
                            {
                                polygon.Chunks[c.X, c.Y, c.Z + 1].MakeDirty();
                            }
                        }
                    }

                    if (z - 1 < 0)
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
        }
    }
}
