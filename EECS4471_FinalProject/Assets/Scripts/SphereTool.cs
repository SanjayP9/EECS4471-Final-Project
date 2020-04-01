using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SphereTool : MonoBehaviour
{
    public bool AddMode = true;
    private Polygon polygon;

    private VRInputManager input;

    void Start()
    {
        polygon = GameObject.Find("Polygon").GetComponent<Polygon>();
        input = GameObject.Find("[CameraRig]").GetComponent<VRInputManager>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerStay(Collider other)
    {
        if (!input.IsLeftTriggerDown) return;

        Chunk c = other.GetComponent<Chunk>();
        if (c == null) return;

        float distance = GetComponent<SphereCollider>().radius * transform.lossyScale.x;
        for (int x = 0; x < Chunk.CHUNK_SIZE; x++)
        {
            for (int y = 0; y < Chunk.CHUNK_SIZE; y++)
            {
                for (int z = 0; z < Chunk.CHUNK_SIZE; z++)
                {
                    //Debug.DrawRay(transform.position, c.transform.position + c.Center[x][y][z]);

                    if (!((c.transform.position + c.Center[x][y][z] - transform.position).magnitude < distance))
                        continue;

                    c.Voxels[x][y][z] = (byte) (AddMode ? 1 : 0);
                    c.MakeDirty();

                    for (int a_x = x - 1; a_x <= x + 1; a_x++)
                    {
                        for (int a_y = y - 1; a_y <= y + 1; a_y++)
                        {
                            for (int a_z = z - 1; a_z <= z + 1; a_z++)
                            {
                                Voxel.Direction result = c.InBounds(a_x, a_y, a_z);

                                if ((result & Voxel.Direction.Top) == Voxel.Direction.Top)
                                {
                                    if (polygon.InBounds(c.X, c.Y + 1, c.Z))
                                    {
                                        polygon.Chunks[c.X, c.Y + 1, c.Z].MakeDirty();
                                    }
                                }

                                if ((result & Voxel.Direction.Bottom) == Voxel.Direction.Bottom)
                                {
                                    if (polygon.InBounds(c.X, c.Y - 1, c.Z))
                                    {
                                        polygon.Chunks[c.X, c.Y - 1, c.Z].MakeDirty();
                                    }
                                }

                                if ((result & Voxel.Direction.Left) == Voxel.Direction.Left)
                                {
                                    if (polygon.InBounds(c.X - 1, c.Y, c.Z))
                                    {
                                        polygon.Chunks[c.X - 1, c.Y, c.Z].MakeDirty();
                                    }
                                }

                                if ((result & Voxel.Direction.Right) == Voxel.Direction.Right)
                                {
                                    if (polygon.InBounds(c.X + 1, c.Y, c.Z))
                                    {
                                        polygon.Chunks[c.X + 1, c.Y, c.Z].MakeDirty();
                                    }
                                }

                                if ((result & Voxel.Direction.Forward) == Voxel.Direction.Forward)
                                {
                                    if (polygon.InBounds(c.X, c.Y, c.Z + 1))
                                    {
                                        polygon.Chunks[c.X, c.Y, c.Z + 1].MakeDirty();
                                    }
                                }

                                if ((result & Voxel.Direction.Back) == Voxel.Direction.Back)
                                {
                                    if (polygon.InBounds(c.X, c.Y, c.Z - 1))
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
    }
}
