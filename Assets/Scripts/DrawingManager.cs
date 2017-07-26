using HoloToolkit.Unity;
using HoloToolkit.Unity.SpatialMapping;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawingManager : Singleton<DrawingManager> {

    public bool drawingMode;

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
        drawingMode = false;
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
