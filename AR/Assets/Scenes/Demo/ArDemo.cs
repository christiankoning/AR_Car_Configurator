using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ArDemo : MonoBehaviour
{
    // AR Camera + Tracking
    private Camera myCamera;
    public GameObject placementIndicator;
    private Pose placementPose;
    static List<ARRaycastHit> s_Hits = new List<ARRaycastHit>();
    private bool placementPoseIsValid = false;
    private bool IsSpawned = false;

    // Objects to spawn
    public GameObject objectToPlace;
    public GameObject AudiA4;
    public GameObject BMWi8;
    private GameObject spawnedObject;

    // UI
    public GameObject canvas;
    public GameObject ShowSelected;
    public GameObject FirstParent;
    private GameObject currentSelected;
    public GameObject scalingPanel;
    public Slider scaling;
    private bool hasScaled = false;
    public GameObject loadingScreen;
    public GameObject ColorPanel;
    public GameObject ModificationsPanel;
    private UI colorUI;
    private UI ModificationUI;

    // Change Color + Auto Rotate
    public Material Carpaint;
    private bool IsRotating = false;

    // Rotation Left + Right
    private bool IsRightPressed;
    private bool IsLeftPressed;

    // Rim Position + Type + Placement
    private Transform RFrontR;
    private Transform RFrontL;
    private Transform RBackR;
    private Transform RBackL;
    private GameObject ChangeRim;
    private GameObject frontright;
    private GameObject frontleft;
    private GameObject backright;
    private GameObject backleft;
    public GameObject[] RimPages;
    public GameObject Arcan;
    public GameObject Argon;
    public GameObject Astral;
    public GameObject Atom;
    public Button NextRimPage;
    public Button PrevRimPage;
    private int MinNumber = 0;
    private int MaxNumber = 1;
    private int CurrentNumber;
    private bool IsPlaced = false;

    // Car Brand + Type
    public GameObject ChangeCarPanel;
    public GameObject CarBrandsPanel;
    public GameObject AudiPanel;
    public GameObject BMWPanel;
    public GameObject Back;

    // Rim color material
    public Material RimOutside;
    public Material RimPlating;

    // UI Switch between Carpaint and Rim Colors
    public Button CarpaintButton;
    public Button RimColorButton;
    public GameObject[] ColorOptions;
    private int CurrentColor = 0;

    //Hints
    private bool moveDevice = true;
    private bool tapToPlace = false;
    public GameObject moveDeviceObject;
    public GameObject tapToPlaceObject;

    // References
    private ARSessionOrigin arOrigin;
    public ARPlaneManager arplanemanager;
    ARPlaneManager planemanager
    {
        get { return arplanemanager; }
        set { arplanemanager = value; }
    }
    [SerializeField] private ARRaycastManager raycastManager;
    bool PlanesFound()
    {
        if(planemanager == null)
        {
            return false;
        }
       return planemanager.trackables.count > 0;
    }

    void Awake()
    {
        // Searches for the object with the raycast manager component
        raycastManager = GetComponent<ARRaycastManager>();
        colorUI = ColorPanel.GetComponent<UI>();
        ModificationUI = ModificationsPanel.GetComponent<UI>();

    }
    void Start()
    {
        // Searches for the object containing the ARSessionOrigin/ARRaycastManager
        arOrigin = FindObjectOfType<ARSessionOrigin>();

        //Rim
        ChangeRim = Arcan;
        PickRimColor(0);
        
        //Sets the framerate to 60
        Application.targetFrameRate = 60;

        loadingScreen.SetActive(true);
        Loading();
    }
    void Update()
    {
        // AR Tracking + Spawning
        UpdatePlacementPose();
        UpdatePlacementIndicator();
        CheckForTouches();

        // Auto Rotation
        ARotate();

        // Rotation left + right
        CheckLeftRotation();
        CheckRightRotation();

        // Rims
        UpdateRimPosition();
        CheckRimPage(CurrentNumber);

        // Change between Carpaint and Rim Colors
        CheckColorOptions(CurrentColor);

        //Animations Checker
        CheckAnimations();

        //Checks Scaling once the object is spawned
        CheckScale();
    }
    //------------------------------------------------------------------ AR Tracking + Spawning -----------------------------------------------------------------------------------------//
    private void CheckForTouches()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (raycastManager.Raycast(touch.position, s_Hits, TrackableType.Planes) && placementPoseIsValid)
            {
                Pose hitPose = s_Hits[0].pose;
                if (spawnedObject == null)
                {
                    spawnedObject = Instantiate(objectToPlace, placementPose.position, placementPose.rotation);
                    IsSpawned = true;
                    hasScaled = false;
                    CheckAnimations();
                    scalingPanel.SetActive(true);

                    //For Debugging purposes. This requires that the ARPlaneManager contains a debug prefab
                    arplanemanager.enabled = false;
                    //foreach(var plane in arplanemanager.trackables)
                    //{
                       //plane.gameObject.SetActive(false);
                    //}
                }
            }
        }
    }
    private void UpdatePlacementIndicator()
    {
        // if there is no object placed yet and in worldspace there is a plane detected, this will show the placement indicator
        if (placementPoseIsValid && !IsSpawned)
        {
            placementIndicator.SetActive(true);
            placementIndicator.transform.SetPositionAndRotation(placementPose.position, placementPose.rotation);
        }
        else
        {
            placementIndicator.SetActive(false);
        }
    }
    private void UpdatePlacementPose()
    {
        Vector3 screenCenter = Camera.current.ViewportToScreenPoint(new Vector3(0.5f, 0.5f));
        var hits = new List<ARRaycastHit>();
        if (raycastManager.Raycast(screenCenter, hits, TrackableType.PlaneWithinPolygon))
        {
            placementPoseIsValid = true;
            placementPose = hits[0].pose;

            var cameraForward = Camera.main.transform.forward;
            var cameraBearing = new Vector3(cameraForward.x, 0, cameraForward.z).normalized;
            placementPose.rotation = Quaternion.LookRotation(cameraBearing);
        }
        else
        {
            placementPoseIsValid = false;
        }
    }
    //------------------------------------------------------------------ AutoRotate + Change Color --------------------------------------------------------------------------------------//
    public void AutoRotate()
    {
        // if auto rotate gets clicked. if the object is not rotating, this will activate the rotation.
        if (IsRotating == false)
        {
            IsRotating = true;
        }
        // if this button is clicked and the object is already rotating, then this will turn the auto rotate off.
        else
        {
            IsRotating = false;
        }
    }
    void ARotate()
    {
        // waits until the auto rotate button is pressed and will only activate when the object is not rotating yet
        if (IsRotating == true)
        {
            spawnedObject.transform.Rotate(Vector3.up);
        }
    }
    public void ChangeColor(int Color)
    {
        // this function contains more than 1 button. every button has its own number. the color depends on what button/number you click
        switch (Color)
        {
            case 0: // white
                Carpaint.color = new Color32(255, 255, 255, 255);
                break;
            case 1: // black
                Carpaint.color = new Color32(0, 0, 0, 255);
                break;
            case 2: // grey
                Carpaint.color = new Color32(87, 86, 86, 255);
                break;
            case 3: // blue
                Carpaint.color = new Color32(7, 19, 106, 255);
                break;
            case 4: // red
                Carpaint.color = new Color32(212, 45, 45, 255);
                break;
        }
    }
    //------------------------------------------------------------------ Rotation left + right ------------------------------------------------------------------------------------------//
    void CheckRightRotation()
    {
        // This checks if the button to rotate the model to the right is being pressed/held down and if thats the case it will rotate the model
        if (IsRightPressed)
        {
            spawnedObject.transform.Rotate(Vector3.down);
            IsRotating = false;
        }
    }
    void CheckLeftRotation()
    {
        // This checks if the button to rotate the model to the left is being pressed/held down and if thats the case it will rotate the model
        if (IsLeftPressed)
        {
            spawnedObject.transform.Rotate(Vector3.up);
            IsRotating = false;
        }
    }
    public void onPointerDownRRotationButton()
    {
        // If this button gets pressed or held down, it will activate rotation of the model to the right
        IsRightPressed = true;
    }
    public void onPointerUpRRotationButton()
    {
        // If the button gets released the model will no longer rotate
        IsRightPressed = false;
    }
    public void onPointerDownLRotationButton()
    {
        // If this button gets pressed or held down it will activate the model's rotation to the left
        IsLeftPressed = true;
    }
    public void onPointerUpLRotationButton()
    {
        // If this button gets released the rotation of the model will stop
        IsLeftPressed = false;
    }
    //------------------------------------------------------------------ Checking/Placing Rims ------------------------------------------------------------------------------------------//
    void UpdateRimPosition()
    {
        //when the car is spawned in worldspace. the positions the rims need to be placed at will be found.
        if (IsSpawned)
        {
            RFrontR = GameObject.Find("FRRim").GetComponent<Transform>();
            RFrontL = GameObject.Find("FLRim").GetComponent<Transform>();
            RBackR = GameObject.Find("BRRim").GetComponent<Transform>();
            RBackL = GameObject.Find("BLRim").GetComponent<Transform>();
            StartReplacing();
        }
    }
    public void ArcanRim()
    {
        ChangeRim = Arcan;
        currentSelected = EventSystem.current.currentSelectedGameObject;
        StartReplacing();
    }
    public void ArgonRim()
    {
        ChangeRim = Argon;
        currentSelected = EventSystem.current.currentSelectedGameObject;
        StartReplacing();
    }
    public void AstralRim()
    {
        ChangeRim = Astral;
        currentSelected = EventSystem.current.currentSelectedGameObject;
        StartReplacing();
    }
    public void AtomRim()
    {
        ChangeRim = Atom;
        currentSelected = EventSystem.current.currentSelectedGameObject;
        StartReplacing();
    }
    public void StartReplacing()
    {
        // checks if there is already rims placed. if there are rims placed they will be removed and then the new ones will be spawned in
        if (IsPlaced)
        {
            //Debug.Log("Rims Detected! Removing before placing new ones...");
            Destroy(frontright);
            Destroy(frontleft);
            Destroy(backright);
            Destroy(backleft);
            IsPlaced = false;
        }

        // if there are no rims placed or the rims got removed. this will spawn in the type of rim you seleceted
        if (!IsPlaced)
        {
            //Debug.Log("Placing New Rims");
            frontright = Instantiate(ChangeRim, RFrontR);
            frontleft = Instantiate(ChangeRim, RFrontL);
            backright = Instantiate(ChangeRim, RBackR);
            backleft = Instantiate(ChangeRim, RBackL);
            IsPlaced = true;
            ShowSelected.transform.parent = currentSelected.transform;
            ShowSelected.transform.position = currentSelected.transform.position;
        }
    }
    void CheckRimPage(int rimpagenumber)
    {
        // This checks which page needs to be opened and which ones need to be closed
        switch (rimpagenumber)
        {
            case 0:
                RimPages[0].SetActive(true);
                RimPages[1].SetActive(false);
                break;
            case 1:
                RimPages[0].SetActive(false);
                RimPages[1].SetActive(true);
                break;
        }
    }
    public void NextRimPageButton()
    {
        // If the last page hasn't been reached yet this will move on to the next page
        if (CurrentNumber < MaxNumber)
        {
            CurrentNumber++;
            CheckRimPage(CurrentNumber);
        }
    }
    public void PrevRimPageButton()
    {
        // If the first page hasn't been reached yet this will move on to the previous page
        if (CurrentNumber > MinNumber)
        {
            CurrentNumber--;
            CheckRimPage(CurrentNumber);
        }
    }
    //------------------------------------------------------------------ Remove/Change Model --------------------------------------------------------------------------------------------//
    public void RemoveModel()
    {
        // Removes the model if there is a model spawned
        if (IsSpawned)
        {
            Destroy(spawnedObject);
            IsSpawned = false;
            currentSelected = FirstParent;
            ShowSelected.transform.parent = currentSelected.transform;
            ShowSelected.transform.position = currentSelected.transform.position;
            ChangeRim = Arcan;
            CurrentNumber = 0;
            CheckRimPage(CurrentNumber);
            colorUI.Hide();
            ModificationUI.Hide();
            canvas.SetActive(false);

            //For Debugging purposes. This requires that the ARPlaneManager contains a debug prefab
            arplanemanager.enabled = true;
            //foreach(var plane in arplanemanager.trackables)
            //{
                //plane.gameObject.SetActive(true);
            //}
        }
    }
    public void Audi()
    {
        objectToPlace = AudiA4;
        // Closes the menu with all the models/brands
        CloseModelMenu();
        // Removes all previous spawned models
        RemoveModel();
    }
    public void BMW()
    {
        objectToPlace = BMWi8;
        // Closes the menu with all the models/brands
        CloseModelMenu();
        // Removes all previous spawned models
        RemoveModel();
    }
    void CloseModelMenu()
    {
        //Closes the menu with all the car models
        ChangeCarPanel.GetComponent<UI>().Hide();
    }
    public void OpenModelMenu()
    {
        // This will open the menu where you can pick between all the brands
        BrandsSetup();
    }
    public void BrandsSetup()
    {
        // This will close all brands that are opened and lets you select a new/different brand
        CarBrandsPanel.SetActive(true);
        AudiPanel.SetActive(false);
        BMWPanel.SetActive(false);
        Back.SetActive(false);
    }
    public void AudiSetup()
    {
        // This will show every Audi model there is and closes all other brands
        CarBrandsPanel.SetActive(false);
        AudiPanel.SetActive(true);
        BMWPanel.SetActive(false);
        Back.SetActive(true);
    }
    public void BMWSetup()
    {
        // This will show every BMW model there is and closes all other brands
        CarBrandsPanel.SetActive(false);
        AudiPanel.SetActive(false);
        BMWPanel.SetActive(true);
        Back.SetActive(true);
    }
    //------------------------------------------------------------------ Change Carpaint/Rims Color ------------------------------------------------------------------------------------//
    public void CarpaintColor()
    {
        // If the carpaint colors are not showing this will make them active and disables the button so that you cant keep pressing the button without any function
        if (CarpaintButton.interactable == true)
        {
            CurrentColor = 0;
            CheckColorOptions(CurrentColor);
            CarpaintButton.interactable = false;
        }
    }
    public void RimColor()
    {
        // If the rim colors are not showing this will show them and it will disable the rim color button so that you cant keep pressing the button without any function
        if (RimColorButton.interactable == true)
        {
            CurrentColor = 1;
            CheckColorOptions(CurrentColor);
            RimColorButton.interactable = false;
        }
    }
    void CheckColorOptions(int options)
    {
        // Switches between the carpaint colors and the rim colors
        switch (options)
        {
            case 0:
                ColorOptions[0].SetActive(true);
                ColorOptions[1].SetActive(false);
                RimColorButton.interactable = true;
                CarpaintButton.interactable = false;
                break;
            case 1:
                ColorOptions[0].SetActive(false);
                ColorOptions[1].SetActive(true);
                CarpaintButton.interactable = true;
                break;
        }
    }
    public void PickRimColor(int rimcolor)
    {
        // Switches the material color between the 3 choices
        switch (rimcolor)
        {
            case 0:
                RimOutside.color = new Color32(51, 50, 50, 255);
                RimPlating.color = new Color32(241, 241, 241, 255);
                RimOutside.SetFloat("_Metallic", 0.5f);
                RimPlating.SetFloat("_Metallic", 1);
                break;
            case 1:
                RimOutside.color = new Color32(241, 241, 241, 255);
                RimPlating.color = new Color32(241, 241, 241, 255);
                RimOutside.SetFloat("_Metallic", 1);
                RimPlating.SetFloat("_Metallic", 1);
                break;
            case 2:
                RimOutside.color = new Color32(51, 50, 50, 255);
                RimPlating.color = new Color32(51, 50, 50, 255);
                RimOutside.SetFloat("_Metallic", 0.5f);
                RimPlating.SetFloat("_Metallic", 0.5f);
                break;
        }
    }
    void ColorOptionsReset()
    {
        // Resets the whole UI needed for the carpaint & rim colors and resets the color of the rims to standard
        CurrentColor = 0;
        CheckColorOptions(CurrentColor);
        PickRimColor(0);
    }
    //------------------------------------------------------------------ Hints --------------------------------------------------------------------------------------------------------//
    void CheckAnimations()
    {
        if(PlanesFound() && moveDevice)
        {
            moveDeviceObject.SetActive(false);
            moveDevice = false;
            tapToPlaceObject.SetActive(true);
            tapToPlace = true;
        }
        if (IsSpawned && tapToPlace)
        {
            tapToPlaceObject.SetActive(false);
            tapToPlace = false;
        }
    }
    //------------------------------------------------------------------ Scaling ------------------------------------------------------------------------------------------------------//
    void CheckScale()
    {
        if (IsSpawned && !hasScaled)
        {
            float scaleSize = scaling.value;
            spawnedObject.transform.localScale = new Vector3(scaleSize, scaleSize, scaleSize);
        }
    }
    public void ConfirmScale()
    {
        scalingPanel.SetActive(false);
        canvas.SetActive(true);
        hasScaled = true;
    }
    //------------------------------------------------------------------ Loading ------------------------------------------------------------------------------------------------------//
    void Loading()
    {
        StartCoroutine(LoadAssets());
        Screen.orientation = ScreenOrientation.Portrait;
    }
    IEnumerator LoadAssets()
    {
        // Turns everything on to check for errors
        yield return new WaitForSeconds(2);
        spawnedObject = Instantiate(objectToPlace, placementPose.position, placementPose.rotation);
        IsSpawned = true;
        ConfirmScale();
        canvas.SetActive(true);
        ChangeCarPanel.SetActive(true);
        CarBrandsPanel.SetActive(true);
        AudiPanel.SetActive(true);
        BMWPanel.SetActive(true);
        scalingPanel.SetActive(true);
        scaling.value = objectToPlace.transform.localScale.x;
        
        // Turns everything back to default after checking for errors
        yield return new WaitForSeconds(0.5f);
        RemoveModel();
        IsSpawned = false;
        hasScaled = false;
        canvas.SetActive(false);
        ChangeCarPanel.SetActive(false);
        CarBrandsPanel.SetActive(false);
        AudiPanel.SetActive(false);
        BMWPanel.SetActive(false);
        scalingPanel.SetActive(false);
        
        // If no errors show up and everything is turned back to default, Loading is complete and the loading screen should turn off
        yield return new WaitForSeconds(0.1f);
        Screen.orientation = ScreenOrientation.AutoRotation;
        loadingScreen.SetActive(false);
    }
}
