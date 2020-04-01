using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Valve.VR;
using Debug = UnityEngine.Debug;

public class VRInputManager : MonoBehaviour
{
    public SteamVR_Action_Boolean InteractWithUI;
    public SteamVR_Action_Boolean xButtonPress;
    public SteamVR_Action_Boolean aButtonPress;
    public SteamVR_Action_Boolean bButtonPress;

    public GameObject LeftHand, RightHand;
    private Ray leftRay, rightRay;

    private bool leftMenuShow = false;
    private bool rightMenuShow = false;

    public bool IsLeftTriggerDown = false;
    public bool IsRightTriggerDown = false;

    private bool spotlightOn = false;

    public GameObject SphereTool, CuttingTool;
    private Polygon polygon;

    private enum Functions
    {
        Cut,
        Add,
        Remove,
        Scale,
        Translate,
        Rotate,
        SpawnCube,
        SpawnSphere
    }

    private Functions currFunction = Functions.Cut;

    // Start is called before the first frame update
    void Start()
    {
        InteractWithUI.AddOnStateDownListener(OnTriggerDown, SteamVR_Input_Sources.LeftHand);
        InteractWithUI.AddOnStateDownListener(OnTriggerDown, SteamVR_Input_Sources.RightHand);

        xButtonPress.AddOnStateDownListener(OnXButtonPress, SteamVR_Input_Sources.LeftHand);
        aButtonPress.AddOnStateDownListener(OnAButtonPress, SteamVR_Input_Sources.RightHand);
        bButtonPress.AddOnStateDownListener(OnBButtonPress, SteamVR_Input_Sources.RightHand);

        InteractWithUI.AddOnStateUpListener(OnTriggerUp, SteamVR_Input_Sources.LeftHand);
        InteractWithUI.AddOnStateUpListener(OnTriggerUp, SteamVR_Input_Sources.RightHand);

        GameObject.FindGameObjectWithTag("LeftCanvas").GetComponent<Canvas>().transform.localScale = Vector3.zero;
        GameObject.FindGameObjectWithTag("RightCanvas").GetComponent<Canvas>().transform.localScale = Vector3.zero;

        leftRay = new Ray();
        rightRay = new Ray();

        polygon = GameObject.Find("Polygon").GetComponent<Polygon>();
    }

    public void OnXButtonPress(SteamVR_Action_Boolean action, SteamVR_Input_Sources source)
    {
        leftMenuShow = !leftMenuShow;
        GameObject.FindGameObjectWithTag("LeftCanvas").GetComponent<Canvas>().transform.localScale = (leftMenuShow) ? (new Vector3(0.001f, 0.001f, 0.001f)) : (Vector3.zero);

    }

    public void OnAButtonPress(SteamVR_Action_Boolean action, SteamVR_Input_Sources source)
    {
        rightMenuShow = !rightMenuShow;
        GameObject.FindGameObjectWithTag("RightCanvas").GetComponent<Canvas>().transform.localScale = (rightMenuShow) ? (new Vector3(0.001f, 0.001f, 0.001f)) : (Vector3.zero);
    }

    public void OnBButtonPress(SteamVR_Action_Boolean action, SteamVR_Input_Sources source)
    {
        spotlightOn = !spotlightOn;
        GameObject.FindWithTag("Flashlight").GetComponent<Light>().intensity = (spotlightOn) ? (1) : (0);
    }

    public void OnTriggerDown(SteamVR_Action_Boolean action, SteamVR_Input_Sources source)
    {
        //Debug.Log((source == SteamVR_Input_Sources.LeftHand ? "Left" : "Right") + " Trigger was pressed");

        // If left trigger is pressed and menu is showing the click buttons that are being pointed at
        if (source == SteamVR_Input_Sources.LeftHand)
        {
            IsLeftTriggerDown = true;
        }
        else
        {
            IsRightTriggerDown = true;
        }
    }

    public void OnTriggerUp(SteamVR_Action_Boolean action, SteamVR_Input_Sources source)
    {
        //Debug.Log((source == SteamVR_Input_Sources.LeftHand ? "Left" : "Right") + " Trigger was released");

        if (source == SteamVR_Input_Sources.LeftHand)
        {
            IsLeftTriggerDown = false;
        }
        else
        {
            IsRightTriggerDown = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Menu
        leftRay.origin = LeftHand.transform.position;
        rightRay.origin = RightHand.transform.position;

        leftRay.direction = -1f * LeftHand.transform.up;
        rightRay.direction = -1f * RightHand.transform.up;

        if (leftMenuShow || rightMenuShow)
        {
            GameObject[] buttons = GameObject.FindGameObjectsWithTag("Button");

            foreach (GameObject i in buttons)
            {
                i.GetComponent<Button>().image.color = Color.white;
            }
        }

        LeftHand.GetComponent<LineRenderer>().enabled = rightMenuShow;
        RightHand.GetComponent<LineRenderer>().enabled = leftMenuShow;

        if (leftMenuShow)
        {
            RightHand.GetComponent<LineRenderer>().SetPositions(new[] { RightHand.transform.position, RightHand.transform.position + RightHand.transform.up * -0.2f });

            if (Physics.Raycast(rightRay, out RaycastHit rightHit, 0.2f, (1 << 5)))
            {
                if (rightHit.collider.gameObject.tag.Equals("Button"))
                {
                    rightHit.collider.gameObject.GetComponent<Button>().image.color = Color.grey;

                    if (IsRightTriggerDown)
                    {
                        switch(rightHit.collider.gameObject.GetComponentInChildren<Text>().text)
                        {
                            case "Cut":
                                currFunction = Functions.Cut;
                                break;
                            case "Add":
                                currFunction = Functions.Add;
                                break;
                            case "Remove":
                                currFunction = Functions.Remove;
                                break;
                            case "Scale":
                                currFunction = Functions.Scale;
                                break;
                            case "Translate":
                                currFunction = Functions.Translate;
                                break;
                            case "Rotate":
                                currFunction = Functions.Rotate;
                                break;
                        }

                        IsRightTriggerDown = false;
                    }
                }

                RightHand.GetComponent<LineRenderer>().SetPositions(new[] { RightHand.transform.position, rightHit.point });
            }
        }


        if (rightMenuShow)
        {
            LeftHand.GetComponent<LineRenderer>().SetPositions(new[] { LeftHand.transform.position, LeftHand.transform.position + LeftHand.transform.up * -0.2f });

            if (Physics.Raycast(leftRay, out RaycastHit leftHit, 0.2f, (1 << 5)))
            {

                if (leftHit.collider.gameObject.name.Contains("Button"))
                {
                    leftHit.collider.gameObject.GetComponent<Button>().image.color = Color.grey;

                    if (IsLeftTriggerDown)
                    {
                        switch (leftHit.collider.gameObject.GetComponentInChildren<Text>().text)
                        {
                            case "Cube":
                                currFunction = Functions.SpawnCube;
                                break;
                            case "Sphere":
                                currFunction = Functions.SpawnSphere;
                                break;
                        }

                        IsLeftTriggerDown = false;
                    }
                }

                LeftHand.GetComponent<LineRenderer>().SetPositions(new[] { LeftHand.transform.position, leftHit.point });
            }
        }
        //Menu

        // Switch tools
        switch (currFunction)
        {
            case Functions.Cut:
                break;
            case Functions.Add:
                SphereTool.GetComponent<SphereTool>().AddMode = true;
                break;
            case Functions.Remove:
                SphereTool.GetComponent<SphereTool>().AddMode = false;
                break;
            case Functions.Scale:
                break;
            case Functions.Translate:
                break;
            case Functions.Rotate:
                break;
            case Functions.SpawnCube:
                polygon.InitCube();
                break;
            case Functions.SpawnSphere:
                polygon.InitSphere();
                break;
        }

        SphereTool.SetActive(currFunction == Functions.Add || currFunction == Functions.Remove);
        CuttingTool.SetActive(currFunction == Functions.Cut);
    }
}
