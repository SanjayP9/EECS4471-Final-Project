using System;
using UnityEngine;

public class Voxel
{
    public static float VoxelSize = 0.01f;
    public const float STANDARD_VOXEL_SIZE = 0.01f;

    [Flags] public enum Direction
    {
        Top = 1,
        Bottom = 2,
        Left = 4,
        Right = 8,
        Forward = 16,
        Back = 32
    }
}