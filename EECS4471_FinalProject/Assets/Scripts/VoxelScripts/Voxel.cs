using UnityEngine;

public class Voxel
{
    public const float VOXEL_SIZE = 0.5f;

    public bool Empty;
	private Mesh mesh;

	public Voxel(bool empty)
	{
		Empty = empty;
	}

	public Mesh GetMesh(Vector3 position)
	{
		if (mesh == null)
		{
			mesh = new Mesh();
			mesh.MarkDynamic();

			Vector3[] vertices = new Vector3[8];
			int[] triangles = new int[36];

            vertices[0] = new Vector3(position.x - VOXEL_SIZE, position.y - VOXEL_SIZE, position.z - VOXEL_SIZE);
            vertices[1] = new Vector3(position.x + VOXEL_SIZE, position.y - VOXEL_SIZE, position.z - VOXEL_SIZE);
            vertices[2] = new Vector3(position.x - VOXEL_SIZE, position.y + VOXEL_SIZE, position.z - VOXEL_SIZE);
            vertices[3] = new Vector3(position.x + VOXEL_SIZE, position.y + VOXEL_SIZE, position.z - VOXEL_SIZE);

            vertices[4] = new Vector3(position.x - VOXEL_SIZE, position.y - VOXEL_SIZE, position.z + VOXEL_SIZE);
            vertices[5] = new Vector3(position.x + VOXEL_SIZE, position.y - VOXEL_SIZE, position.z + VOXEL_SIZE);
            vertices[6] = new Vector3(position.x - VOXEL_SIZE, position.y + VOXEL_SIZE, position.z + VOXEL_SIZE);
            vertices[7] = new Vector3(position.x + VOXEL_SIZE, position.y + VOXEL_SIZE, position.z + VOXEL_SIZE);

            triangles[0] = 0;
            triangles[1] = 2;
            triangles[2] = 1;

            triangles[3] = 2;
            triangles[4] = 3;
            triangles[5] = 1;

            triangles[6] = 6;
            triangles[7] = 4;
            triangles[8] = 5;

            triangles[9] = 7;
            triangles[10] = 6;
            triangles[11] = 5;

            triangles[12] = 1;
            triangles[13] = 3;
            triangles[14] = 7;

            triangles[15] = 1;
            triangles[16] = 7;
            triangles[17] = 5;

            triangles[18] = 2;
            triangles[19] = 0;
            triangles[20] = 6;

            triangles[21] = 6;
            triangles[22] = 0;
            triangles[23] = 4;

            triangles[24] = 2;
            triangles[25] = 7;
            triangles[26] = 3;

            triangles[27] = 2;
            triangles[28] = 6;
            triangles[29] = 7;

            triangles[30] = 0;
            triangles[31] = 1;
            triangles[32] = 5;

            triangles[33] = 0;
            triangles[34] = 5;
            triangles[35] = 4;

            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
		}

		return mesh;
	}
}
