using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Valve.VR;
using Mode = SphereTool.Mode;

public class VRInputManager : MonoBehaviour
{
    public SteamVR_Action_Boolean InteractWithUI;
    public SteamVR_Action_Boolean xButtonPress;
    public SteamVR_Action_Boolean aButtonPress;
    public SteamVR_Action_Boolean bButtonPress;

    public SteamVR_Action_Vibration handVibrate;

    public GameObject LeftHand, RightHand;
    private Ray leftRay, rightRay;

    private bool leftMenuShow;
    private bool rightMenuShow;

    public bool IsLeftTriggerDown;
    public bool IsRightTriggerDown;

    private bool spotlightOn;

    public GameObject SphereTool, CuttingTool, TranslateTool, TranslateControl, ScaleTool1, ScaleTool2, ProxyScale;
    public GameObject LeftHandCanvas, RightHandCanvas, ColourCanvas;
    public GameObject Flashlight;

    private Polygon polygon;

    private float origScaleDistance;
    private float scalingDistance;
    private float ogVoxelSize;
    private bool scaleMode;
    private bool translateMode;

    private GameObject[] buttons;

    private enum Functions
    {
        Cut,
        Add,
        Remove,
        Colour,
        Scale,
        Translate,
        Rotate
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

        leftRay = new Ray();
        rightRay = new Ray();

        polygon = GameObject.Find("Polygon").GetComponent<Polygon>();
        buttons = GameObject.FindGameObjectsWithTag("Button");
    }

    public void OnXButtonPress(SteamVR_Action_Boolean action, SteamVR_Input_Sources source)
    {
        leftMenuShow = !leftMenuShow;
        LeftHandCanvas.GetComponent<Canvas>().enabled = leftMenuShow;
        handVibrate.Execute(0f, 0.2f, 60f, 0.4f, source);
    }

    public void OnAButtonPress(SteamVR_Action_Boolean action, SteamVR_Input_Sources source)
    {
        rightMenuShow = !rightMenuShow;
        RightHandCanvas.GetComponent<Canvas>().enabled = rightMenuShow;
        handVibrate.Execute(0f, 0.2f, 60f, 0.4f, source);
    }

    public void OnBButtonPress(SteamVR_Action_Boolean action, SteamVR_Input_Sources source)
    {
        spotlightOn = !spotlightOn;
        Flashlight.GetComponent<Light>().intensity = spotlightOn ? 1 : 0;
        handVibrate.Execute(0f, 0.2f, 60f, 0.4f, source);
    }

    public void OnTriggerDown(SteamVR_Action_Boolean action, SteamVR_Input_Sources source)
    {
        // If left trigger is pressed and menu is showing the click buttons that are being pointed at
        if (source == SteamVR_Input_Sources.LeftHand)
            IsLeftTriggerDown = true;
        else
            IsRightTriggerDown = true;
    }

    public void OnTriggerUp(SteamVR_Action_Boolean action, SteamVR_Input_Sources source)
    {
        if (source == SteamVR_Input_Sources.LeftHand)
            IsLeftTriggerDown = false;
        else
            IsRightTriggerDown = false;
    }

    // Update is called once per frame
    void Update()
    {
        LeftHand.GetComponent<SphereCollider>().enabled = currFunction == Functions.Scale;
        
        RightHand.GetComponent<SphereCollider>().enabled = currFunction == Functions.Scale;
        RightHand.GetComponent<LineRenderer>().enabled = currFunction == Functions.Add || 
                                                         currFunction == Functions.Colour || leftMenuShow;
        
        LeftHand.GetComponent<LineRenderer>().enabled = rightMenuShow;

        LeftHandCanvas.SetActive(leftMenuShow);
        RightHandCanvas.SetActive(rightMenuShow);

        // Menu
        leftRay.origin = LeftHand.transform.position;
        rightRay.origin = RightHand.transform.position;

        leftRay.direction = -1f * LeftHand.transform.up;
        rightRay.direction = -1f * RightHand.transform.up;

        foreach (GameObject i in buttons)
            i.GetComponent<Button>().image.color = Color.white;

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
                                ProxyScale.transform.position = Camera.main.gameObject.transform.position + Camera.main.gameObject.transform.forward * 0.3f;
                                
                                ogVoxelSize = Voxel.VoxelSize;

                                ProxyScale.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);

                                ScaleTool1.transform.position = ProxyScale.transform.position + ProxyScale.transform.up * -0.05f + ProxyScale.transform.right * -0.05f + ProxyScale.transform.forward * 0.05f;
                                ScaleTool2.transform.position = ProxyScale.transform.position + ProxyScale.transform.up * 0.05f + ProxyScale.transform.right * 0.05f + ProxyScale.transform.forward * -0.05f;
                                origScaleDistance = Vector3.Distance(ScaleTool1.transform.position, ScaleTool2.transform.position);

                                break;
                            case "Translate":
                                currFunction = Functions.Translate;
                                TranslateControl.transform.position = Camera.main.gameObject.transform.position + Camera.main.gameObject.transform.forward * 0.4f;
                                break;
                            case "Rotate":
                                currFunction = Functions.Rotate;
                                break;
                        }

                        if (currFunction == Functions.Scale) {
                            switch (rightHit.collider.gameObject.GetComponentInChildren<Text>().text)
                            {
                                case "Scale +":
                                    Voxel.VoxelSize += 0.01f;
                                    polygon.RecomputeChunks();
                                    break;
                                case "Scale -":
                                    Voxel.VoxelSize -= 0.01f;
                                    polygon.RecomputeChunks();
                                    break;                              
                            }
                        }

                        IsRightTriggerDown = leftMenuShow = LeftHandCanvas.GetComponent<Canvas>().enabled = false;
                    }
                }

                RightHand.GetComponent<LineRenderer>().SetPositions(new[] { RightHand.transform.position, rightHit.point });
            }
        }
        else
            RightHand.GetComponent<LineRenderer>().SetPositions(new[] { Vector3.zero, Vector3.zero });

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
            case Functions.Scale:
                if (ScalingTool.LeftCollision && IsLeftTriggerDown && ScalingTool.RightCollision && IsRightTriggerDown)
                    scaleMode = true;

                scalingDistance = Vector3.Distance(ScaleTool1.transform.position, ScaleTool2.transform.position) - origScaleDistance;
                ProxyScale.transform.localScale = new Vector3(0.1f + scalingDistance / 10f,
                    0.1f + scalingDistance / 10f, 0.1f + scalingDistance / 10f);

                if (scaleMode && !IsLeftTriggerDown && !IsRightTriggerDown)
                {
                    Voxel.VoxelSize = ogVoxelSize + scalingDistance / 10f;
                    ogVoxelSize = Voxel.VoxelSize;
                    polygon.RecomputeChunks();
                    scaleMode = false;
                    ProxyScale.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);

                    ScaleTool1.transform.position = ProxyScale.transform.position + ProxyScale.transform.up * -0.05f +
                                                    ProxyScale.transform.right * -0.05f +
                                                    ProxyScale.transform.forward * 0.05f;
                    
                    ScaleTool2.transform.position = ProxyScale.transform.position + ProxyScale.transform.up * 0.05f +
                                                    ProxyScale.transform.right * 0.05f +
                                                    ProxyScale.transform.forward * -0.05f;
                }
                break;
            case Functions.Translate:
                // If colliding spheres and right trigger down then move control to match tool position then move polygon by delta value
                TranslateTool.SetActive(true);
                TranslateControl.SetActive(true);
                if (TranslateTool.GetComponent<SphereTranslate>().collideWithControl)
                {
                    if (IsRightTriggerDown)
                    {
                        Vector3 prevPos = TranslateControl.transform.position;

                        TranslateControl.transform.position = TranslateTool.transform.position;

                        polygon.transform.position += TranslateTool.transform.position - prevPos;
                        translateMode = true;
                    }
                    else if (translateMode)
                    {
                        TranslateControl.transform.position = Camera.main.gameObject.transform.position + Camera.main.gameObject.transform.forward * 0.4f;
                        translateMode = false;
                    }
                }

                break;
            case Functions.Colour:
            case Functions.Add:
                if (Physics.Raycast(rightRay, out RaycastHit rightHit, 0.2f, (1 << 5)))
                {
                    if (rightHit.collider.gameObject.CompareTag("Button"))
                    {
                        rightHit.collider.gameObject.GetComponent<Button>().image.color = Color.grey;

                        if (IsRightTriggerDown)
                        {
                            switch (rightHit.collider.gameObject.GetComponentInChildren<Text>().text)
                            {
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
                        }
                    }

                    RightHand.GetComponent<LineRenderer>().SetPositions(new[] { RightHand.transform.position, rightHit.point });
                }
                else
                    RightHand.GetComponent<LineRenderer>().SetPositions(new [] { Vector3.zero, Vector3.zero });

                break;
        }

        ScaleTool1.SetActive(currFunction == Functions.Scale);
        ScaleTool2.SetActive(currFunction == Functions.Scale);
        ProxyScale.SetActive(currFunction == Functions.Scale);
        SphereTool.SetActive(currFunction == Functions.Add || currFunction == Functions.Remove || currFunction == Functions.Colour);
        CuttingTool.SetActive(currFunction == Functions.Cut);
        TranslateTool.SetActive(currFunction == Functions.Translate);
        TranslateControl.SetActive(currFunction == Functions.Translate);
        ColourCanvas.SetActive(currFunction == Functions.Colour || currFunction == Functions.Add);
    }
}
