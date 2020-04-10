using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Valve.VR;
using Debug = UnityEngine.Debug;
using Mode = SphereTool.Mode;

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

    public GameObject SphereTool, CuttingTool, TranslateTool, TranslateControl, ScaleCanvas;
    public GameObject LeftHandCanvas, RightHandCanvas, ColourCanvas;
    public GameObject Flashlight;

    private Polygon polygon;


    private enum Functions
    {
        Cut,
        Add,
        Remove,
        Colour,
        Scale,
        Translate,
        Rotate,
        SpawnCube,
        SpawnSphere
    }

    private Functions currFunction = Functions.Add;

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

        //LeftHandCanvas.GetComponent<Canvas>().transform.localScale = Vector3.zero;
        //RightHandCanvas.GetComponent<Canvas>().transform.localScale = Vector3.zero;

        leftRay = new Ray();
        rightRay = new Ray();

        polygon = GameObject.Find("Polygon").GetComponent<Polygon>();
    }

    public void OnXButtonPress(SteamVR_Action_Boolean action, SteamVR_Input_Sources source)
    {
        leftMenuShow = !leftMenuShow;
        LeftHandCanvas.GetComponent<Canvas>().enabled = leftMenuShow;
        LeftHand.GetComponent<LineRenderer>().enabled = rightMenuShow;
    }

    public void OnAButtonPress(SteamVR_Action_Boolean action, SteamVR_Input_Sources source)
    {
        rightMenuShow = !rightMenuShow;
        RightHandCanvas.GetComponent<Canvas>().enabled = rightMenuShow;
        RightHand.GetComponent<LineRenderer>().enabled = leftMenuShow;
    }

    public void OnBButtonPress(SteamVR_Action_Boolean action, SteamVR_Input_Sources source)
    {
        spotlightOn = !spotlightOn;
        Flashlight.GetComponent<Light>().intensity = spotlightOn ? 1 : 0;
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

        if (Input.GetKeyDown(KeyCode.L))
        {
            if (currFunction == Functions.Add)
            {
                SphereTool.GetComponent<SphereTool>().SetMode(Mode.Add);
                currFunction = Functions.Remove;
            }
            else if (currFunction == Functions.Remove)
            {
                currFunction = Functions.Colour;
                SphereTool.GetComponent<SphereTool>().SetMode(Mode.Remove);

            }
            else if (currFunction == Functions.Colour)
            {
                currFunction = Functions.Add;
                SphereTool.GetComponent<SphereTool>().SetMode(Mode.Colour);
            }
        }


        if (leftMenuShow)
        {
            RightHand.GetComponent<LineRenderer>().SetPositions(new[] { RightHand.transform.position, RightHand.transform.position + RightHand.transform.up * -0.2f });

            if (Physics.Raycast(rightRay, out RaycastHit rightHit, 0.2f, (1 << 5)))
            {
                if (rightHit.collider.gameObject.CompareTag("Button"))
                {
                    rightHit.collider.gameObject.GetComponent<Button>().image.color = Color.grey;

                    if (IsRightTriggerDown)
                    {
                        switch (rightHit.collider.gameObject.GetComponentInChildren<Text>().text)
                        {
                            case "Cut":
                                currFunction = Functions.Cut;
                                break;
                            case "Add":
                                currFunction = Functions.Add;
                                SphereTool.GetComponent<SphereTool>().SetMode(Mode.Add);
                                break;
                            case "Remove":
                                currFunction = Functions.Remove;
                                SphereTool.GetComponent<SphereTool>().SetMode(Mode.Remove);
                                break;
                            case "Colour":
                                currFunction = Functions.Colour;
                                SphereTool.GetComponent<SphereTool>().SetMode(Mode.Colour);
                                break;
                            case "Scale":
                                currFunction = Functions.Scale;
                                break;
                            case "Translate":
                                currFunction = Functions.Translate;
                                TranslateControl.transform.position = Camera.main.gameObject.transform.position + Camera.main.gameObject.transform.forward * 0.2f;
                                break;
                            case "Rotate":
                                currFunction = Functions.Rotate;
                                break;
                            case "Black":
                                SphereTool.GetComponent<SphereTool>().SetColourMode(0);
                                break;
                            case "Blue":
                                SphereTool.GetComponent<SphereTool>().SetColourMode(1);
                                break;
                            case "Green":
                                SphereTool.GetComponent<SphereTool>().SetColourMode(2);
                                break;
                            case "Red":
                                SphereTool.GetComponent<SphereTool>().SetColourMode(3);
                                break;
                            case "Yellow":
                                SphereTool.GetComponent<SphereTool>().SetColourMode(4);
                                break;
                            case "Purple":
                                SphereTool.GetComponent<SphereTool>().SetColourMode(5);
                                break;
                            case "Turquoise":
                                SphereTool.GetComponent<SphereTool>().SetColourMode(6);
                                break;
                            case "White":
                                SphereTool.GetComponent<SphereTool>().SetColourMode(7);
                                break;
                        }

                        if (currFunction == Functions.Scale) {
                            switch (rightHit.collider.gameObject.GetComponentInChildren<Text>().text)
                            {
                                case "Scale +":
                                    Voxel.VoxelSize += 0.01f;
                                    polygon.RecomputeChunks(true);
                                    break;
                                case "Scale -":
                                    Voxel.VoxelSize -= 0.01f;
                                    polygon.RecomputeChunks(true);
                                    break;                              
                            }
                        }
                        IsRightTriggerDown = leftMenuShow = LeftHandCanvas.GetComponent<Canvas>().enabled =
                            RightHand.GetComponent<LineRenderer>().enabled = false;
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

                if (leftHit.collider.gameObject.CompareTag("Button"))
                {
                    leftHit.collider.gameObject.GetComponent<Button>().image.color = Color.grey;

                    if (IsLeftTriggerDown)
                    {
                        switch (leftHit.collider.gameObject.GetComponentInChildren<Text>().text)
                        {
                            case "Cube":
                                polygon.InitCube();
                                break;
                            case "Sphere":
                                polygon.InitSphere();
                                break;
                            case "Clear":
                                polygon.ClearChunks();
                                break;
                        }
                        IsLeftTriggerDown = rightMenuShow = RightHandCanvas.GetComponent<Canvas>().enabled =
                            LeftHand.GetComponent<LineRenderer>().enabled = false;
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
                break;
            case Functions.Remove:
                break;
            case Functions.Scale:                
                break;
            case Functions.Translate:
                // If colliding spheres and right trigger down then move control to match tool position then move polygon by delta value
                TranslateTool.SetActive(true);
                TranslateControl.SetActive(true);
                if (TranslateTool.GetComponent<SphereTranslate>().collideWithControl && IsRightTriggerDown)
                {
                    Vector3 prevPos = TranslateControl.transform.position;

                    TranslateControl.transform.position = TranslateTool.transform.position;

                    polygon.transform.position += TranslateTool.transform.position - prevPos;
                }
                break;
            case Functions.Rotate:
                break;
            case Functions.SpawnCube:
                break;
            case Functions.SpawnSphere:
                break;
        }

        ScaleCanvas.SetActive(currFunction == Functions.Scale);
        SphereTool.SetActive(currFunction == Functions.Add || currFunction == Functions.Remove || currFunction == Functions.Colour);
        CuttingTool.SetActive(currFunction == Functions.Cut);
        TranslateTool.SetActive(currFunction == Functions.Translate);
        TranslateControl.SetActive(currFunction == Functions.Translate);
        ColourCanvas.SetActive(currFunction == Functions.Colour);
    }
}
