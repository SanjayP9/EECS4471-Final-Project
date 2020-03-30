using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CuttingScript : MonoBehaviour
{
    private BoxCollider collider;
    // Start is called before the first frame update
    void Start()
    {
        collider = GetComponent<BoxCollider>();
    }

    // Update is called once per frame
    void Update()
    {
    }


    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Vector3 basePosition = transform.position - (transform.forward * 0.08f);
        Gizmos.DrawRay(new Ray(transform.position, transform.forward));
        Gizmos.DrawRay(new Ray(transform.TransformPoint(-0.05f, -0.05f, 0f), transform.forward));
        //Gizmos.DrawRay(new Ray(transform.position + new Vector3(0.005f, 0.005f, 0), transform.forward));
        //Gizmos.DrawRay(new Ray(transform.position + new Vector3(-0.005f, 0.005f, 0), transform.forward));

        //Gizmos.DrawRay(new Ray(collider.bounds.min, transform.forward));
        //Gizmos.DrawRay(new Ray(collider.bounds.min, transform.forward));
    }
}
