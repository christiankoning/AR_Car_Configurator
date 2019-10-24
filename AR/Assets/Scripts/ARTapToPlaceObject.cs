using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ARTapToPlaceObject : MonoBehaviour
{
    // AR Camera + Tracking
    private Camera myCamera;
    public GameObject placementIndicator;
    private Pose placementPose;
    private bool placementPoseIsValid = false;
    static List<ARRaycastHit> s_Hits = new List<ARRaycastHit>();
    private bool IsSpawned = false;

    // Objects to spawn
    public GameObject objectToPlace;
    private GameObject spawnedObject;

    // UI
    public GameObject canvas;
    public GameObject ShowSelected;
    private GameObject currentSelected;
    public GameObject FirstParent;

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
    private bool IsPlaced = false;
    public GameObject[] RimPages;
    public Button NextRimPage;
    public Button PrevRimPage;
    private int MinNumber = 0;
    private int MaxNumber = 19;
    private int CurrentNumber;

    // Car Brand + Type
    public Button NextPage;
    public Button PrevPage;
    public GameObject ChangeCarPanel;
    public GameObject CarBrandsPanel;
    public GameObject AudiPanel;
    public GameObject BMWPanel;
    public GameObject Back;
    public GameObject[] BrandPages;
    public GameObject[] AudiPages;
    public GameObject[] BMWPages;
    private string BrandName;
    private string LastPage = "7";
    private int MinPages = 0;
    private int MaxPages = 6;
    private int CurrentPage = 0;
    public Text PageNumber;
    private GameObject OpenCloseTool;

    // Rim color material
    public Material RimOutside;
    public Material RimPlating;

    // UI Switch between Carpaint and Rim Colors
    public Button CarpaintButton;
    public Button RimColorButton;
    public GameObject[] ColorOptions;
    private int CurrentColor = 0;
    private int MaxColorOption = 1;
    private int MinColorOption = 0;

    // References
    private ARSessionOrigin arOrigin;
    [SerializeField] private ARRaycastManager raycastManager;
    public AllModels aModels;
    public AllRims aRims;

    void Start()
    {
        // Searches for the object containing the ARSessionOrigin/ARRaycastManager
        arOrigin = FindObjectOfType<ARSessionOrigin>();
        raycastManager = FindObjectOfType<ARRaycastManager>();

        // UI
        canvas.SetActive(false);

        //Rim
        ChangeRim = aRims.Arcan;

        // Makes the mobile unable to rotate to landscape orientation
        Screen.orientation = ScreenOrientation.Portrait;
        Application.targetFrameRate = 60;
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

        // Update car brands/types
        CheckPageNumber(CurrentPage);

        // Change between Carpaint and Rim Colors
        CheckColorOptions(CurrentColor);
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
                    canvas.SetActive(true);
                }
                else
                {
                    //spawnedObject.transform.Rotate(new Vector3(0, hitPose.rotation.y * 10, 0));
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
        var screenCenter = Camera.current.ViewportToScreenPoint(new Vector3(0.5f, 0.5f));
        var hits = new List<ARRaycastHit>();
        raycastManager.Raycast(screenCenter, hits, TrackableType.Planes);
        //Debug.Log(hits);

        placementPoseIsValid = hits.Count > 0;
        if (placementPoseIsValid)
        {
            placementPose = hits[0].pose;

            var cameraForward = Camera.main.transform.forward;
            var cameraBearing = new Vector3(cameraForward.x, 0, cameraForward.z).normalized;
            placementPose.rotation = Quaternion.LookRotation(cameraBearing);
        }
    }
    //------------------------------------------------------------------ AutoRotate + Change Color --------------------------------------------------------------------------------------//
    public void AutoRotate()
    {
        // if auto rotate gets clicked. if the object is not rotating, this will activate the rotation.
        if(IsRotating == false)
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
        switch(Color)
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
        else if (!IsRightPressed)
        {
            //Rotation stops
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
        else if (!IsLeftPressed)
        {
            // Rotation stops
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
        ChangeRim = aRims.Arcan;
        currentSelected = EventSystem.current.currentSelectedGameObject;
        StartReplacing();
    }
    public void ArgonRim()
    {
        ChangeRim = aRims.Argon;
        currentSelected = EventSystem.current.currentSelectedGameObject;
        StartReplacing();
    }
    public void GunnerRim()
    {
        ChangeRim = aRims.Gunner;
        currentSelected = EventSystem.current.currentSelectedGameObject;
        StartReplacing();
    }
    public void AstralRim()
    {
        ChangeRim = aRims.Astral;
        currentSelected = EventSystem.current.currentSelectedGameObject;
        StartReplacing();
    }
    public void AtomRim()
    {
        ChangeRim = aRims.Atom;
        currentSelected = EventSystem.current.currentSelectedGameObject;
        StartReplacing();
    }
    public void DeaRim()
    {
        ChangeRim = aRims.DEA;
        currentSelected = EventSystem.current.currentSelectedGameObject;
        StartReplacing();
    }
    public void DivaRim()
    {
        ChangeRim = aRims.Diva;
        currentSelected = EventSystem.current.currentSelectedGameObject;
        StartReplacing();
    }
    public void DynamikRim()
    {
        ChangeRim = aRims.Dynamik;
        currentSelected = EventSystem.current.currentSelectedGameObject;
        StartReplacing();
    }
    public void EasyrRim()
    {
        ChangeRim = aRims.EasyR;
        currentSelected = EventSystem.current.currentSelectedGameObject;
        StartReplacing();
    }
    public void EnigmaRim()
    {
        ChangeRim = aRims.Enigma;
        currentSelected = EventSystem.current.currentSelectedGameObject;
        StartReplacing();
    }
    public void EvosRim()
    {
        ChangeRim = aRims.Evos;
        currentSelected = EventSystem.current.currentSelectedGameObject;
        StartReplacing();
    }
    public void IcanRim()
    {
        ChangeRim = aRims.Ican;
        currentSelected = EventSystem.current.currentSelectedGameObject;
        StartReplacing();
    }
    public void KatanaRim()
    {
        ChangeRim = aRims.Katana;
        currentSelected = EventSystem.current.currentSelectedGameObject;
        StartReplacing();
    }
    public void MythosRim()
    {
        ChangeRim = aRims.Mythos;
        currentSelected = EventSystem.current.currentSelectedGameObject;
        StartReplacing();
    }
    public void PakyRim()
    {
        ChangeRim = aRims.Paky;
        currentSelected = EventSystem.current.currentSelectedGameObject;
        StartReplacing();
    }
    public void RevenRim()
    {
        ChangeRim = aRims.Reven;
        currentSelected = EventSystem.current.currentSelectedGameObject;
        StartReplacing();
    }
    public void StellarRim()
    {
        ChangeRim = aRims.Stellar;
        currentSelected = EventSystem.current.currentSelectedGameObject;
        StartReplacing();
    }
    public void TargaRim()
    {
        ChangeRim = aRims.Targa;
        currentSelected = EventSystem.current.currentSelectedGameObject;
        StartReplacing();
    }
    public void WonderRim()
    {
        ChangeRim = aRims.Wonder;
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
            PickRimColor(0);
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
                RimReset();
                break;
            case 1:
                RimPages[0].SetActive(false);
                RimPages[1].SetActive(true);
                RimPages[2].SetActive(false);
                break;
            case 2:
                RimPages[1].SetActive(false);
                RimPages[2].SetActive(true);
                RimPages[3].SetActive(false);
                break;
            case 3:
                RimPages[2].SetActive(false);
                RimPages[3].SetActive(true);
                RimPages[4].SetActive(false);
                break;
            case 4:
                RimPages[3].SetActive(false);
                RimPages[4].SetActive(true);
                RimPages[5].SetActive(false);
                break;
            case 5:
                RimPages[4].SetActive(false);
                RimPages[5].SetActive(true);
                RimPages[6].SetActive(false);
                break;
            case 6:
                RimPages[5].SetActive(false);
                RimPages[6].SetActive(true);
                RimPages[7].SetActive(false);
                break;
            case 7:
                RimPages[6].SetActive(false);
                RimPages[7].SetActive(true);
                RimPages[8].SetActive(false);
                break;
            case 8:
                RimPages[7].SetActive(false);
                RimPages[8].SetActive(true);
                RimPages[9].SetActive(false);
                break;
            case 9:
                RimPages[8].SetActive(false);
                RimPages[9].SetActive(true);
                RimPages[10].SetActive(false);
                break;
            case 10:
                RimPages[9].SetActive(false);
                RimPages[10].SetActive(true);
                RimPages[11].SetActive(false);
                break;
            case 11:
                RimPages[10].SetActive(false);
                RimPages[11].SetActive(true);
                RimPages[12].SetActive(false);
                break;
            case 12:
                RimPages[11].SetActive(false);
                RimPages[12].SetActive(true);
                RimPages[13].SetActive(false);
                break;
            case 13:
                RimPages[12].SetActive(false);
                RimPages[13].SetActive(true);
                RimPages[14].SetActive(false);
                break;
            case 14:
                RimPages[13].SetActive(false);
                RimPages[14].SetActive(true);
                RimPages[15].SetActive(false);
                break;
            case 15:
                RimPages[14].SetActive(false);
                RimPages[15].SetActive(true);
                RimPages[16].SetActive(false);
                break;
            case 16:
                RimPages[15].SetActive(false);
                RimPages[16].SetActive(true);
                RimPages[17].SetActive(false);
                break;
            case 17:
                RimPages[16].SetActive(false);
                RimPages[17].SetActive(true);
                RimPages[18].SetActive(false);
                break;
            case 18:
                RimPages[17].SetActive(false);
                RimPages[18].SetActive(true);
                RimPages[19].SetActive(false);
                break;
            case 19:
                RimPages[18].SetActive(false);
                RimPages[19].SetActive(true);
                break;
        }
    }
    void RimReset()
    {
        // This will reset all the rim pages when a new model gets opened
        RimPages[2].SetActive(false);
        RimPages[3].SetActive(false);
        RimPages[4].SetActive(false);
        RimPages[5].SetActive(false);
        RimPages[6].SetActive(false);
        RimPages[7].SetActive(false);
        RimPages[8].SetActive(false);
        RimPages[9].SetActive(false);
        RimPages[10].SetActive(false);
        RimPages[11].SetActive(false);
        RimPages[12].SetActive(false);
        RimPages[13].SetActive(false);
        RimPages[14].SetActive(false);
        RimPages[15].SetActive(false);
        RimPages[16].SetActive(false);
        RimPages[17].SetActive(false);
        RimPages[18].SetActive(false);
        RimPages[19].SetActive(false);
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
            ChangeRim = aRims.Arcan;
            CurrentNumber = 0;
            CheckRimPage(CurrentNumber);
            canvas.SetActive(false);
        }
    }
    public void Audi(int modelId)
    {
        // makes the object you spawn to an Audi
        switch(modelId)
        {
            case 0:
                objectToPlace = aModels.AudiModels[0];
                break;
            case 1:
                objectToPlace = aModels.AudiModels[1];
                break;
            case 2:
                objectToPlace = aModels.AudiModels[2];
                break;
            case 3:
                objectToPlace = aModels.AudiModels[3];
                break;
            case 4:
                objectToPlace = aModels.AudiModels[4];
                break;
            case 5:
                objectToPlace = aModels.AudiModels[5];
                break;
            case 6:
                objectToPlace = aModels.AudiModels[6];
                break;
            case 7:
                objectToPlace = aModels.AudiModels[7];
                break;
            case 8:
                objectToPlace = aModels.AudiModels[8];
                break;
            case 9:
                objectToPlace = aModels.AudiModels[9];
                break;
        }
        // Closes the menu with all the models/brands
        CloseModelMenu();
        // Removes all previous spawned models
        RemoveModel();
    }
    public void BMW(int modelId)
    {
        // Makes the object to spawn the modelname you clicked on
        switch (modelId)
        {
            case 0:
                objectToPlace = aModels.BMWModels[0];
                break;
            case 1:
                objectToPlace = aModels.BMWModels[1];
                break;
            case 2:
                objectToPlace = aModels.BMWModels[2];
                break;
            case 3:
                objectToPlace = aModels.BMWModels[3];
                break;
            case 4:
                objectToPlace = aModels.BMWModels[4];
                break;
            case 5:
                objectToPlace = aModels.BMWModels[5];
                break;
            case 6:
                objectToPlace = aModels.BMWModels[6];
                break;
            case 7:
                objectToPlace = aModels.BMWModels[7];
                break;
            case 8:
                objectToPlace = aModels.BMWModels[8];
                break;
            case 9:
                objectToPlace = aModels.BMWModels[9];
                break;
            case 10:
                objectToPlace = aModels.BMWModels[10];
                break;
            case 11:
                objectToPlace = aModels.BMWModels[11];
                break;
            case 12:
                objectToPlace = aModels.BMWModels[12];
                break;
            case 13:
                objectToPlace = aModels.BMWModels[13];
                break;
            case 14:
                objectToPlace = aModels.BMWModels[14];
                break;
            case 15:
                objectToPlace = aModels.BMWModels[15];
                break;
            case 16:
                objectToPlace = aModels.BMWModels[16];
                break;
            case 17:
                objectToPlace = aModels.BMWModels[17];
                break;
            case 18:
                objectToPlace = aModels.BMWModels[18];
                break;
            case 19:
                objectToPlace = aModels.BMWModels[19];
                break;
            case 20:
                objectToPlace = aModels.BMWModels[20];
                break;
            case 21:
                objectToPlace = aModels.BMWModels[21];
                break;
            case 22:
                objectToPlace = aModels.BMWModels[22];
                break;
            case 23:
                objectToPlace = aModels.BMWModels[23];
                break;
        }
        // Closes the menu with all the models/brands
        CloseModelMenu();
        // Removes all previous spawned models
        RemoveModel();
    }
    void CloseModelMenu()
    {
        //Closes the menu with all the car models
        OpenCloseTool.SetActive(true);
    }
    public void OpenModelMenu()
    {
        // This will open the menu where you can pick between all the brands
        BrandsSetup();
        OpenCloseTool = EventSystem.current.currentSelectedGameObject;
        OpenCloseTool.SetActive(false);
    }
    public void BrandsSetup()
    {
        // This will close all brands that are opened and lets you select a new/different brand
        CarBrandsPanel.SetActive(true);
        AudiPanel.SetActive(false);
        BMWPanel.SetActive(false);
        Back.SetActive(false);
        BrandName = "None";
        LastPage = "7";
        MinPages = 0;
        MaxPages = 6;
        CurrentPage = 0;
    }
    public void AudiSetup()
    {
        // This will show every Audi model there is and closes all other brands
        BrandName = "Audi";
        LastPage = "2";
        MinPages = 0;
        MaxPages = 1;
        CurrentPage = 0;
        CarBrandsPanel.SetActive(false);
        AudiPanel.SetActive(true);
        BMWPanel.SetActive(false);
        Back.SetActive(true);
    }
    public void BMWSetup()
    {
        // This will show every BMW model there is and closes all other brands
        BrandName = "BMW";
        LastPage = "5";
        MinPages = 0;
        MaxPages = 4;
        CurrentPage = 0;
        CarBrandsPanel.SetActive(false);
        AudiPanel.SetActive(false);
        BMWPanel.SetActive(true);
        Back.SetActive(true);
    }
    public void CheckPageNumber(int pagenumber)
    {
        // If there hasn't been a brand picked yet show all the brands you can pick from
        if (BrandName == "None")
        {
            switch (pagenumber)
            {
                case 0:
                    BrandPages[0].SetActive(true);
                    BrandPages[1].SetActive(false);
                    break;
                case 1:
                    BrandPages[0].SetActive(false);
                    BrandPages[1].SetActive(true);
                    BrandPages[2].SetActive(false);
                    break;
                case 2:
                    BrandPages[1].SetActive(false);
                    BrandPages[2].SetActive(true);
                    BrandPages[3].SetActive(false);
                    break;
                case 3:
                    BrandPages[2].SetActive(false);
                    BrandPages[3].SetActive(true);
                    BrandPages[4].SetActive(false);
                    break;
                case 4:
                    BrandPages[3].SetActive(false);
                    BrandPages[4].SetActive(true);
                    BrandPages[5].SetActive(false);
                    break;
                case 5:
                    BrandPages[4].SetActive(false);
                    BrandPages[5].SetActive(true);
                    BrandPages[6].SetActive(false);
                    break;
                case 6:
                    BrandPages[5].SetActive(false);
                    BrandPages[6].SetActive(true);
                    break;
            }
        }
        else if (BrandName == "Audi")
        {
            switch (pagenumber)
            {
                case 0:
                    AudiPages[0].SetActive(true);
                    AudiPages[1].SetActive(false);
                    break;
                case 1:
                    AudiPages[0].SetActive(false);
                    AudiPages[1].SetActive(true);
                    break;
            }
        }
        else if (BrandName == "BMW")
        {
            switch (pagenumber)
            {
                case 0:
                    BMWPages[0].SetActive(true);
                    BMWPages[1].SetActive(false);
                    break;
                case 1:
                    BMWPages[0].SetActive(false);
                    BMWPages[1].SetActive(true);
                    BMWPages[2].SetActive(false);
                    break;
                case 2:
                    BMWPages[1].SetActive(false);
                    BMWPages[2].SetActive(true);
                    BMWPages[3].SetActive(false);
                    break;
                case 3:
                    BMWPages[2].SetActive(false);
                    BMWPages[3].SetActive(true);
                    BMWPages[4].SetActive(false);
                    break;
                case 4:
                    BMWPages[3].SetActive(false);
                    BMWPages[4].SetActive(true);
                    break;
            }
        }
    }
    public void NextButton()
    {
        // If the last page isn't reached yet this will move to the next page
        if (CurrentPage < MaxPages)
        {
            CurrentPage++;
            CheckPageNumber(CurrentPage);
        }
    }
    public void PrevButton()
    {
        // If the first page isn't reached yet this will move to the previous page
        if (CurrentPage > MinPages)
        {
            CurrentPage--;
            CheckPageNumber(CurrentPage);
        }
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
}
