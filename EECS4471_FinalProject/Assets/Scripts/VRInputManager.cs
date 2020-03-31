using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Debug = UnityEngine.Debug;

public class VRInputManager : MonoBehaviour
{
    public SteamVR_Action_Boolean InteractWithUI;
    public SteamVR_Action_Boolean xButtonPress;
    public SteamVR_Action_Boolean aButtonPress;

    public GameObject LeftHand, RightHand;
    private Ray leftRay, rightRay;

    private bool leftMenuShow = false;
    private bool rightMenuShow = false;


    // Start is called before the first frame update
    void Start()
    {
        InteractWithUI.AddOnStateDownListener(OnTriggerDown, SteamVR_Input_Sources.LeftHand);
        InteractWithUI.AddOnStateDownListener(OnTriggerDown, SteamVR_Input_Sources.RightHand);

        xButtonPress.AddOnStateDownListener(OnXButtonPress, SteamVR_Input_Sources.LeftHand);
        aButtonPress.AddOnStateDownListener(OnAButtonPress, SteamVR_Input_Sources.RightHand);

        InteractWithUI.AddOnStateUpListener(OnTriggerUp, SteamVR_Input_Sources.LeftHand);
        InteractWithUI.AddOnStateUpListener(OnTriggerUp, SteamVR_Input_Sources.RightHand);

        GameObject.FindGameObjectWithTag("LeftCanvas").GetComponent<Canvas>().transform.localScale = new Vector3(0.001f, 0.001f, 0.001f);
        GameObject.FindGameObjectWithTag("RightCanvas").GetComponent<Canvas>().transform.localScale = new Vector3(0.001f, 0.001f, 0.001f);

        leftRay = new Ray();
        rightRay = new Ray();
    }

    public void OnXButtonPress(SteamVR_Action_Boolean action, SteamVR_Input_Sources source)
    {
        leftMenuShow = !leftMenuShow;
        Debug.Log("X was pressed " + leftMenuShow);
        GameObject.FindGameObjectWithTag("LeftCanvas").GetComponent<Canvas>().transform.localScale = (leftMenuShow)?(new Vector3(0.001f, 0.001f, 0.001f)):(Vector3.zero);

    }

    public void OnAButtonPress(SteamVR_Action_Boolean action, SteamVR_Input_Sources source)
    {
        rightMenuShow = !rightMenuShow;
        Debug.Log("A was pressed " + rightMenuShow);
        GameObject.FindGameObjectWithTag("RightCanvas").GetComponent<Canvas>().transform.localScale = (rightMenuShow) ? (new Vector3(0.001f, 0.001f, 0.001f)) : (Vector3.zero);
    }


    public void OnTriggerDown(SteamVR_Action_Boolean action, SteamVR_Input_Sources source)
    {
        Debug.Log((source == SteamVR_Input_Sources.LeftHand ? "Left" : "Right") + " Trigger was pressed");

        // If left trigger is pressed and menu is showing the click buttons that are being pointed at
        if (source == SteamVR_Input_Sources.LeftHand && leftMenuShow)
        {

        }
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
