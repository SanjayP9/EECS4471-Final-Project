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
        switch (gameObject.name)
        {
            case "ScaleSphereL":
                if (!LeftCollision)
                    LeftCollision = other.gameObject.name == "Controller (left)";
                break;
            case "ScaleSphereR":
                if (!RightCollision)
                    RightCollision = other.gameObject.name == "Controller (right)";
                break;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        switch (gameObject.name)
        {
            case "ScaleSphereL":
                if (other.gameObject.name == "Controller (left)")
                {
                    if (!LeftCollision) 
                        LeftCollision = true;

                    if (input.IsLeftTriggerDown && input.IsRightTriggerDown) 
                        transform.position = input.LeftHand.transform.position;
                }
                break;
            case "ScaleSphereR":
                if (other.gameObject.name == "Controller (right)")
                {
                    if (!RightCollision)
                        RightCollision = true;

                    if (input.IsRightTriggerDown && input.IsLeftTriggerDown)
                        transform.position = input.RightHand.transform.position;
                }
                break;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        switch (other.gameObject.name)
        {
            case "Controller (left)":
                LeftCollision = false;
                break;
            case "Controller (right)":
                RightCollision = false;
                break;
        }
    }
}
