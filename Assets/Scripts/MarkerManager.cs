using HoloToolkit.Unity.SpatialMapping;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoloToolkit.Sharing.Tests;
using HoloToolkit.Sharing;
using System;
using HoloToolkit.Unity;

public class MarkerManager : Singleton<MarkerManager> {

    public bool markerPlacementMode;
    public bool guiHit;

    public List<GameObject> markerStore;
    int storeSize = 25;

    bool spatialHit;
    RaycastHit hitInfo;
    Ray rayCast;
    SpatialMappingManager spatialMappingManager;

    //Workaround
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private BoxCollider boxCollider;
    private SphereCollider sphereCollider;

    // Use this for initialization
    void Start () {
        markerPlacementMode = false;
        guiHit = false;
        markerStore = new List<GameObject>();

        spatialMappingManager = SpatialMappingManager.Instance;

        // We care about getting updates for the model transform.
        CustomMessages.Instance.MessageHandlers[CustomMessages.TestMessageID.MarkerPosition] = this.OnMarkerInput;
        CustomMessages.Instance.MessageHandlers[CustomMessages.TestMessageID.MarkerHit] = this.OnMarkerHit;

    }

    // Update is called once per frame
    void Update () {

#if UNITY_EDITOR
        if(guiHit)
        {
            activateMarker();
            return;
        }
        if(guiHit == false && markerPlacementMode)
        {
            if (Input.GetMouseButtonDown(0))
            {

                Debug.Log("Message Send");

                Ray rayCast = Camera.main.ScreenPointToRay(Input.mousePosition);
                GameObject marker;

                if (Physics.Raycast(rayCast, out hitInfo))
                {
                    marker = hitInfo.collider != null ? hitInfo.collider.gameObject : null;

                    if (marker != null && marker.GetComponent<Pointer>() != null)
                    {
                        Debug.Log("Hit Pointer GameObject:" + marker.name);
                        // Should be handled by On InputClicked method of Pointer.cs
                        //marker.GetComponent<Pointer>().OnSelection();
                        //CustomMessages.Instance.SendMarkerHit(marker.name);
                        return;
                    }
                } else
                {
                    CustomMessages.Instance.SendMarkerRay(rayCast);
                }
            }
        }
#endif
    }

    void OnMarkerInput(NetworkInMessage msg)
    {
        long userID = msg.ReadInt64();

        GameObject marker;
        Vector3 position;
        //String owner;
#if UNITY_EDITOR
        position = CustomMessages.Instance.ReadVector3(msg);
        String idString = CustomMessages.Instance.ReadString(msg);
        String owner = CustomMessages.Instance.ReadString(msg);
        //CreateMarkerAtPosition(position, idString, owner);
        CreateMarker(position, idString, owner, false, false);
        return;
#endif

        rayCast = CustomMessages.Instance.ReadRay(msg);
        Debug.Log(rayCast.ToString());

        //if (Physics.Raycast(rayCast, out hitInfo))
        //{
        //    marker = hitInfo.collider != null ? hitInfo.collider.gameObject : null;
            
        //    if(marker != null && marker.GetComponent<Pointer>() != null)
        //    {
        //        marker.GetComponent<Pointer>().OnSelection();
        //        CustomMessages.Instance.SendMarkerHit(hitInfo.point);
        //        return;
        //    }
        //}

        if (Physics.Raycast(rayCast, out hitInfo, spatialMappingManager.PhysicsLayer))
        {
            //marker = CreateMarkerAtPosition(hitInfo.point, null, "remote");
            marker = CreateMarker(hitInfo.point, null, "remote");
            //CustomMessages.Instance.SendMarkerPosition(hitInfo.point, marker.name, "remote");
        }
        else
        {
            Vector3 originClippingPosition = new Vector3(rayCast.origin.x, rayCast.origin.y, Camera.main.nearClipPlane);
            marker = CreateMarker(originClippingPosition, null, "remote");
            //marker = CreateMarkerAtPosition(originClippingPosition, null, "remote");
            //CustomMessages.Instance.SendMarkerPosition(originClippingPosition, marker.name, "remote");
        }
    }

    void OnMarkerHit(NetworkInMessage msg)
    {
        long userID = msg.ReadInt64();
        String idString = CustomMessages.Instance.ReadString(msg);
        Debug.Log(idString);
        //Vector3 position = CustomMessages.Instance.ReadVector3(msg);
        GameObject marker = null;

        //Collider[] colliders;
        //if ((colliders = Physics.OverlapSphere(position, 1f /* Radius */)).Length > 1) //Presuming the object you are testing also has a collider 0 otherwise
        //{
        //    foreach (var collider in colliders)
        //    {
        //        marker = collider.gameObject; //This is the game object you collided with
        //        break;
        //    }
        //}

        marker = GameObject.Find(idString);
        if (marker != null && marker.GetComponent<Pointer>() != null)
        {
            marker.GetComponent<Pointer>().OnSelection();
        }
    }

    private void activateMarker()
    {
        guiHit = false;
    }

    //private GameObject CreateMarkerAtPosition(Vector3 position, String idString, String remoteString)
    //{
    //    GameObject marker = Instantiate(Resources.Load("SpriteMarker")) as GameObject;
    //    marker.transform.parent = gameObject.transform;
    //    marker.transform.localPosition = position;
    //    SetRotationOfMarker(marker);
    //    AddMarkerToStore(marker, idString);
    //    if(remoteString.Equals("remote")) { marker.GetComponent<Pointer>().SetColor(new Color(255f, 0, 0)); };
    //    return marker;
    //}

    public GameObject CreateMarker(Vector3 position, String idString, String owner, Boolean offset = true, Boolean send = true)
    {
        GameObject marker = Instantiate(Resources.Load("SpriteMarker")) as GameObject;
        marker.transform.parent = gameObject.transform;
        marker.transform.localPosition = position;
        if (offset)
        {
            marker.transform.Translate(new Vector3(0f, -0.05f, 0f));
        }
        SetRotationOfMarker(marker);
        AddMarkerToStore(marker, idString);
        if (owner.Equals("remote")) marker.GetComponent<Pointer>().SetColor(new Color(255f, 0, 0));
        if (send) CustomMessages.Instance.SendMarkerPosition(marker.transform.position, marker.name, owner);
        return marker;
    }

    private void SetRotationOfMarker(GameObject pointer)
    {
        Quaternion cameraRotation = Camera.main.transform.localRotation;
        cameraRotation.x = 0;
        cameraRotation.z = 0;
        pointer.transform.rotation = cameraRotation;
        pointer.transform.Rotate(new Vector3(0, 0, 180f));
    }

    private void AddMarkerToStore(GameObject pointer, String idString)
    {
        if (idString == null)
        {
            idString = new DateTime().ToString();
            pointer.name = "Marker_" + idString;
        } else
        {
            pointer.name = idString;
        }
        this.markerStore.Add(pointer);
    }
}
