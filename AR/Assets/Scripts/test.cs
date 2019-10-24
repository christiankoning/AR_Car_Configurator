using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
[RequireComponent(typeof(ARRaycastManager))]
public class test : MonoBehaviour
{
    ///
    ///Reflection Probes: Shows real time reflection of the physical world
    ///Problems: 
    ///How to use it?
    ///What are the limitations?
    ///How to call the reflecting material to become a texture on the carpaint?
    ///
    ///
    ///
    ///Reflection probes would make the carpaint material and the model look more realistic.
    ///To make the scaling and placing the object better check through the arfoundation-samples project
    ///arfoundation-samples project has some interesting scenes with some helpful tools. Might be worth looking through to get better quality in ar

    public GameObject PlacedPrefab;
    private GameObject spawnedObject;
    private ARRaycastManager m_RaycastManager;
    static List<ARRaycastHit> s_Hits = new List<ARRaycastHit>();
    private bool moveDevice = true;
    private bool tapToPlace = false;
    public GameObject moveDeviceObject;
    public GameObject tapToPlaceObject;
    private bool isSpawned = false;
    public GameObject canvas;
    public ARPlaneManager m_PlaneManager;

    ARPlaneManager planemanager
    {
        // Checks the amount of planes that have been placed (keeps it on 0 if there are no planes found (yet))
        get { return m_PlaneManager; }
        set { m_PlaneManager = value; }
    }
    bool TryGetTouchPosition(out Vector3 touchPosition)
    {
#if UNITY_EDITOR
        if (Input.GetMouseButton(0))
        {
            var mousePosition = Input.mousePosition;
            touchPosition = new Vector3(mousePosition.x, mousePosition.y, mousePosition.z);
            return true;
        }
#else
        if(Input.touchCount > 0)
        {
            touchPosition = Input.GetTouch(0).position;
            return true;
        }
#endif
        touchPosition = default;
        return false;
    }
    void Awake()
    {
        //Searches for the object that contains the ARRaycastManager component
        m_RaycastManager = GetComponent<ARRaycastManager>();
    }
    void Update()
    {
        AnimationChecker();
        CheckUI();
        if (!TryGetTouchPosition(out Vector3 touchPosition))
        {
            return;
        }
        if (m_RaycastManager.Raycast(touchPosition, s_Hits, TrackableType.PlaneWithinPolygon))
        {
            var hitPose = s_Hits[0].pose;
            if (spawnedObject == null)
            {
                spawnedObject = Instantiate(PlacedPrefab, hitPose.position, hitPose.rotation);
                isSpawned = true;
                foreach (var plane in planemanager.trackables)
                {
                    plane.gameObject.SetActive(false);
                }
                m_PlaneManager.enabled = false;
            }
        }
    }
    bool PlanesFound()
    {
        if (planemanager == null)
        {
            return false;
        }
        return planemanager.trackables.count > 0;
    }
    void AnimationChecker()
    {
        if (PlanesFound() && moveDevice)
        {
            moveDeviceObject.SetActive(false);
            moveDevice = false;
            tapToPlaceObject.SetActive(true);
            tapToPlace = true;
        }
        if (isSpawned && tapToPlace)
        {
            tapToPlaceObject.SetActive(false);
            tapToPlace = false;
        }
    }
    void CheckUI()
    {
        if (isSpawned)
        {
            canvas.SetActive(true);
        }
        else
        {
            canvas.SetActive(false);
        }
    }

}