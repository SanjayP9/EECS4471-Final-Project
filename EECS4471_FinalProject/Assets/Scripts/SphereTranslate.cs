using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SphereTranslate : MonoBehaviour
{
    public bool collideWithControl = false;
    private VRInputManager input;

    // Start is called before the first frame update
    void Start()
    {
        input = GameObject.Find("[CameraRig]").GetComponent<VRInputManager>();

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerStay(Collider other)
    {
        if (!collideWithControl)
            collideWithControl = other.gameObject.name == "SphereControl";
    }

    private void OnTriggerExit(Collider other)
    {
        collideWithControl = false;
    }
}
