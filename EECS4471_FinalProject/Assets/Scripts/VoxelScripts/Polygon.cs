using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Polygon : MonoBehaviour
{
    Chunk[] chunks = new Chunk[1];
    MeshFilter meshFilter;

    float timer = 0.0f;

    // Start is called before the first frame update
    void Start()
    {
        meshFilter = GetComponent<MeshFilter>();
        chunks[0] = new Chunk(transform, GetComponent<MeshRenderer>().material);
        meshFilter.mesh = chunks[0].GetMesh();
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
    }
}
