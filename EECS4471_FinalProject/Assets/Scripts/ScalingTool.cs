using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScalingTool : MonoBehaviour
{
    private VRInputManager input;
    public static bool LeftCollision;
    public static bool RightCollision;

    // Start is called before the first frame update
    void Start()
    {
        input = GameObject.Find("[CameraRig]").GetComponent<VRInputManager>();
        LeftCollision = false;
        RightCollision = false;
    }

    private void OnTriggerEnter(Collider other)
    {        
        if (gameObject.name == "ScaleSphereL")
        {
            if (!LeftCollision)
            {
                LeftCollision = other.gameObject.name == "Controller (left)";
            }
        }
        if (gameObject.name == "ScaleSphereR")
        {
            if (!RightCollision)
            {
                RightCollision = other.gameObject.name == "Controller (right)";
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (gameObject.name == "ScaleSphereL")
        {
            if (other.gameObject.name == "Controller (left)" && input.IsLeftTriggerDown && input.IsRightTriggerDown) {
                transform.position = input.LeftHand.transform.position;
            }
        }
        if (gameObject.name == "ScaleSphereR")
        {
            if (other.gameObject.name == "Controller (right)" && input.IsRightTriggerDown && input.IsLeftTriggerDown) {
                transform.position = input.RightHand.transform.position;
            }
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.name == "Controller (left)") {
            Debug.Log("EXIT LEFT");
            LeftCollision = false; 
        }
        if (other.gameObject.name == "Controller (right)") {
            Debug.Log("EXIT RIGHT");
            RightCollision = false; 
        }
    }
}
