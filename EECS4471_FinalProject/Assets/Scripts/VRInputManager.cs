using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Debug = UnityEngine.Debug;

public class VRInputManager : MonoBehaviour
{
    public SteamVR_Action_Boolean InteractWithUI;

    public GameObject LeftHand, RightHand;
    private Ray leftRay, rightRay;


    // Start is called before the first frame update
    void Start()
    {
        InteractWithUI.AddOnStateDownListener(OnTriggerDown, SteamVR_Input_Sources.LeftHand);
        InteractWithUI.AddOnStateDownListener(OnTriggerDown, SteamVR_Input_Sources.RightHand);

        InteractWithUI.AddOnStateUpListener(OnTriggerUp, SteamVR_Input_Sources.LeftHand);
        InteractWithUI.AddOnStateUpListener(OnTriggerUp, SteamVR_Input_Sources.RightHand);

        leftRay = new Ray();
        rightRay = new Ray();
    }

    public void OnTriggerDown(SteamVR_Action_Boolean action, SteamVR_Input_Sources source)
    {
        Debug.Log((source == SteamVR_Input_Sources.LeftHand ? "Left" : "Right") + " Trigger was pressed");
    }

    public void OnTriggerUp(SteamVR_Action_Boolean action, SteamVR_Input_Sources source)
    {
        Debug.Log((source == SteamVR_Input_Sources.LeftHand ? "Left" : "Right") + " Trigger was released");
    }

    // Update is called once per frame
    void Update()
    {
        leftRay.origin = LeftHand.transform.position;
        rightRay.origin = RightHand.transform.position;

        leftRay.direction = LeftHand.transform.forward;
        rightRay.direction = RightHand.transform.forward;

        if (Physics.Raycast(leftRay, out RaycastHit leftHit, 10f))
        {
            LeftHand.GetComponent<LineRenderer>().SetPositions(new []
            {
                LeftHand.transform.position,
                leftHit.point
            });
        }
        else
        {
            LeftHand.GetComponent<LineRenderer>().SetPositions(new []
            {
                LeftHand.transform.position,
                LeftHand.transform.position + LeftHand.transform.forward * 10f
            });
        }

        if (Physics.Raycast(rightRay, out RaycastHit rightHit, 10f))
        {
            RightHand.GetComponent<LineRenderer>().SetPositions(new []
            {
                RightHand.transform.position,
                rightHit.point
            });
        }
        else
        {
            RightHand.GetComponent<LineRenderer>().SetPositions(new []
            {
                RightHand.transform.position,
                RightHand.transform.position + RightHand.transform.forward * 10f
            });
        }
    }
}
