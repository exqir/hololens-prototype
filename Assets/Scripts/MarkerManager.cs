using HoloToolkit.Unity.SpatialMapping;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoloToolkit.Sharing.Tests;
using HoloToolkit.Sharing;

public class MarkerManager : MonoBehaviour {

    public bool markerPlacementMode;
    public bool guiHit;

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
                        Debug.Log("Hit Pointer GameObject");
                        marker.GetComponent<Pointer>().OnSelection();
                        return;
                    }
                }

                CustomMessages.Instance.SendMarkerRay(Camera.main.ScreenPointToRay(Input.mousePosition));
            }
        }
#endif
    }

    void OnMarkerInput(NetworkInMessage msg)
    {
        long userID = msg.ReadInt64();

        GameObject marker;
        Vector3 position;
#if UNITY_EDITOR
        position = CustomMessages.Instance.ReadVector3(msg);
        CreateMarkerAtPosition(position);
        return;
#endif

        rayCast = CustomMessages.Instance.ReadRay(msg);
        Debug.Log(rayCast.ToString());

        if (Physics.Raycast(rayCast, out hitInfo))
        {
            marker = hitInfo.collider != null ? hitInfo.collider.gameObject : null;
            
            if(marker != null && marker.GetComponent<Pointer>() != null)
            {
                marker.GetComponent<Pointer>().OnSelection();
                CustomMessages.Instance.SendMarkerHit(hitInfo.point);
                return;
            }
        }

        if (Physics.Raycast(rayCast, out hitInfo, spatialMappingManager.PhysicsLayer))
        {
            marker = CreateMarkerAtPosition(hitInfo.point);
            CustomMessages.Instance.SendMarkerPosition(hitInfo.point);
        }
        else
        {
            Vector3 originClippingPosition = new Vector3(rayCast.origin.x, rayCast.origin.y, Camera.main.nearClipPlane);
            marker = CreateMarkerAtPosition(originClippingPosition);
            CustomMessages.Instance.SendMarkerPosition(originClippingPosition);
        }
    }

    void OnMarkerHit(NetworkInMessage msg)
    {
        long userID = msg.ReadInt64();

        Vector3 position = CustomMessages.Instance.ReadVector3(msg);
        GameObject marker = null;

        Collider[] colliders;
        if ((colliders = Physics.OverlapSphere(position, 1f /* Radius */)).Length > 1) //Presuming the object you are testing also has a collider 0 otherwise
        {
            foreach (var collider in colliders)
            {
                marker = collider.gameObject; //This is the game object you collided with
                break; 
            }
        }
        if(marker != null && marker.GetComponent<Pointer>() != null)
        {
            marker.GetComponent<Pointer>().OnSelection();
        }
    }

    private void activateMarker()
    {
        guiHit = false;
    }

    private GameObject CreateMarkerAtPosition(Vector3 position)
    {
        GameObject marker = Instantiate(Resources.Load("SpriteMarker")) as GameObject;
        marker.transform.parent = gameObject.transform;
        marker.transform.localPosition = position;
        SetRotationOfMarker(marker);
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
}
