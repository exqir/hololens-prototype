using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VR.WSA.Input;
using HoloToolkit.Unity;
using HoloToolkit.Unity.SpatialMapping;
using HoloToolkit.Unity.InputModule;
using UnityEngine.VR.WSA;
using HoloToolkit.Sharing.Tests;
using HoloToolkit.Sharing;
using System;

public class AirTapManager : Singleton<AirTapManager>, IInputClickHandler
{
    RaycastHit hitInfo;
    bool spatialHit;

    bool pictureMode;
    GameObject pictureDesc;

    public int markerCount;
    GameObject pointer;

    SpatialMappingManager spatialMappingManager;
    WorldAnchorManager worldAnchorManager;
    InputManager inputManager;
    PhotoManager photoManager;
    MarkerManager markerManager;

    //Workaround
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private BoxCollider boxCollider;
    private SphereCollider sphereCollider;

    // Use this for initialization
    void Start () {
        spatialMappingManager = SpatialMappingManager.Instance;
        if (spatialMappingManager == null)
        {
            Debug.LogError("This script expects that you have a SpatialMappingManager component in your scene.");
        }

        //worldAnchorManager = WorldAnchorManager.Instance;
        //if (worldAnchorManager == null)
        //{
        //    Debug.LogError("This script expects that you have a WorldAnchorManager component in your scene.");
        //}

        inputManager = InputManager.Instance;
        photoManager = PhotoManager.Instance;
        markerManager = MarkerManager.Instance;

        inputManager.PushFallbackInputHandler(this.gameObject);
        markerCount = 0;
        pointer = null;

        pictureMode = false;
    }

    // Update is called once per frame
    void Update () {
        if(pictureMode && pictureDesc != null)
        {
            pictureDesc.transform.position = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width / 2, Screen.height / 2, Camera.main.nearClipPlane));
        }
    }

    public void OnInputClicked(InputClickedEventData eventData)
    {
        if(!pictureMode)
        {
            spatialHit = Physics.Raycast(Camera.main.transform.position,
                Camera.main.transform.forward,
                out hitInfo,
                spatialMappingManager.PhysicsLayer);

            if (spatialHit)
            {
                //CreateMarker(hitInfo.point);
                markerManager.CreateMarker(hitInfo.point, null, "holo");
            }
        }
        
        if(pictureMode)
        {
            // Make Picture
            // PhotoManager from https://github.com/JannikLassahn/hololens-photocapture
            photoManager.TakePhoto();
            //photoManager.StopCamera();
            ChangeMode(false);
        }

    }

    public void ActivateHelp()
    {
        ChangeMode(true);
    }

    private void ChangeMode(bool mode)
    {
        pictureMode = mode;

        if (pictureMode && pictureDesc == null)
        {
            pictureDesc = Instantiate(Resources.Load("ProblemCaptureMode")) as GameObject;
            pictureDesc.transform.position = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width / 2, Screen.height / 2, Camera.main.nearClipPlane));
           
        }
        if (!pictureMode && pictureDesc != null)
        {
            Destroy(pictureDesc);
        }
    }

    //private void CreateMarker(Vector3 point)
    //{
    //    pointer = Instantiate(Resources.Load("SpriteMarker")) as GameObject;
    //    markerCount += 1;
    //    pointer.transform.parent = GameObject.Find("Scene Root").transform;
    //    SetPositionOfMarker(point);
    //    CustomMessages.Instance.SendMarkerPosition(point, "Marker_" + new DateTime().ToString(), "holo");

    //}

    //private void SetPositionOfMarker(Vector3 point)
    //{
    //    pointer.transform.position = point;
    //    pointer.transform.Translate(new Vector3(0f, -0.05f, 0f));

    //    Quaternion cameraRotation = Camera.main.transform.localRotation;
    //    cameraRotation.x = 0;
    //    cameraRotation.z = 0;
    //    pointer.transform.rotation = cameraRotation;
    //    pointer.transform.Rotate(new Vector3(0, 0, 180f));
    //}

    //private void FixMarkerInWorld()
    //{
    //    //WorldAnchor worldAnchor = pointer.AddComponent<WorldAnchor>();
    //    if (worldAnchorManager != null && spatialMappingManager != null)
    //    {
    //        worldAnchorManager.AttachAnchor(pointer, "marker_" + markerCount);
    //    }
    //}
}
