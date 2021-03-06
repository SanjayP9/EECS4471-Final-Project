using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class SphereTool : MonoBehaviour
{
    [Flags] public enum Mode
    {
        Add,
        Remove,
        Colour
    }

    private VRInputManager input;

    public Mode CurrentMode { get; private set; }

    public byte CurrentColour { get; private set; }

    void Start()
    {
        input = GameObject.Find("[CameraRig]").GetComponent<VRInputManager>();
    }

    void OnTriggerStay(Collider other)
    {
        if (!input.IsLeftTriggerDown) return;

        Chunk c = other.GetComponent<Chunk>();
        if (c == null) return;

        // Using the bounding box of the sphere collider, calculate the range of
        // indexes to raytrace against instead of the entire chunk
        int[] minCoord = c.VectorToCoord(GetComponent<SphereCollider>().bounds.min);
        int[] maxCoord = c.VectorToCoord(GetComponent<SphereCollider>().bounds.max);

        minCoord[0] = minCoord[0] < 0 ? 0 : minCoord[0];
        minCoord[1] = minCoord[1] < 0 ? 0 : minCoord[1];
        minCoord[2] = minCoord[2] < 0 ? 0 : minCoord[2];

        maxCoord[0] = maxCoord[0] >= Chunk.CHUNK_SIZE ? Chunk.CHUNK_SIZE : maxCoord[0];
        maxCoord[1] = maxCoord[1] >= Chunk.CHUNK_SIZE ? Chunk.CHUNK_SIZE : maxCoord[1];
        maxCoord[2] = maxCoord[2] >= Chunk.CHUNK_SIZE ? Chunk.CHUNK_SIZE : maxCoord[2];

        float distance = GetComponent<SphereCollider>().radius * transform.lossyScale.x;

        for (int x = minCoord[0]; x < maxCoord[0]; x++)
        {
            for (int y = minCoord[1]; y < maxCoord[1]; y++)
            {
                for (int z = minCoord[2]; z < maxCoord[2]; z++)
                {
                    switch (CurrentMode)
                    {
                        case Mode.Add:
                            if (c.Voxels[x][y][z] == CurrentColour + 1) continue;
                            break;
                        case Mode.Remove:
                            if (c.Voxels[x][y][z] == 0) continue;
                            break;
                        case Mode.Colour:
                            if (c.Voxels[x][y][z] - 1 == CurrentColour ||
                                c.Voxels[x][y][z] == 0) continue;
                            break;
                    }
                    
                    // If the voxel's center is not within the radius of the sphere, skip
                    if (!((c.transform.position + c.Center[x][y][z] - transform.position).magnitude < distance))
                        continue;

                    switch (CurrentMode)
                    {
                        case Mode.Add:
                        case Mode.Remove:
                            c.ModifyVoxel(x, y, z, CurrentMode == Mode.Add ? (CurrentColour + 1) : 0);
                            break;
                        case Mode.Colour:
                            c.ModifyColour(x, y, z, CurrentColour);
                            break;
                    }
                }
            }
        }

        input.handVibrate.Execute(0f, 0.1f, 60f, 0.2f, SteamVR_Input_Sources.LeftHand);
    }

    public void SetMode(Mode newMode)
    {
        CurrentMode = newMode;

        switch (CurrentMode)
        {
            case Mode.Add:
                GetComponent<MeshRenderer>().material.SetFloat("ToolMode", 0);
                break;
            case Mode.Remove:
                GetComponent<MeshRenderer>().material.SetFloat("ToolMode", 1);
                break;
            case Mode.Colour:
                GetComponent<MeshRenderer>().material.SetFloat("ToolMode", 2);
                break;
        }
    }

    public void SetColourMode(byte colour)
    {
        CurrentColour = colour;
    }
}
