using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoloToolkit.Unity;
using HoloToolkit.Unity.InputModule;
using UnityEngine.VR.WSA;
using HoloToolkit.Sharing.Tests;

public class Pointer : MonoBehaviour, IInputClickHandler
{
    private int ttl = 300;

    [Tooltip("Set Object to be permanent")]
    public bool permanent = false;

    GazeManager gazeManager;
    WorldAnchorManager worldAnchorManager;
    MarkerManager markerManager;


    // Use this for initialization
    void Start () {
        gazeManager = GazeManager.Instance;
        worldAnchorManager = WorldAnchorManager.Instance;
        if (worldAnchorManager == null)
        {
            Debug.LogError("This script expects that you have a WorldAnchorManager component in your scene.");
        }

        if(permanent)
        {
            //SetColor(Color.red);
        }
    }
	
	// Update is called once per frame
	void Update () {
        if(!permanent)
        {
            ttl--;

            if (ttl <= 0)
            {
                RemovePointer();
            }
            if (ttl < 100)
            {
                float alpha = ttl / 100;
            }
        }
	}

    public void OnInputClicked(InputClickedEventData eventData)
    {
        CustomMessages.Instance.SendMarkerHit(gameObject.name);
        OnSelection();
    }

    public void OnSelection()
    {
        if (permanent)
        {
            RemovePointer();
        }
        else
        {
            permanent = true;
            gameObject.GetComponent<SpriteRenderer>().color = new Color(0f, 255f, 0f);
        }
    }

    private void RemovePointer()
    {
        if(worldAnchorManager != null && gameObject.GetComponent<WorldAnchor>() != null)
        {
            //worldAnchorManager.RemoveAnchor(gameObject);
        }

        markerManager = FindObjectOfType<MarkerManager>();

        if(markerManager != null && markerManager.markerStore.Count > 1)
        {
            //markerManager.markerStore.Remove(gameObject);

            GameObject _thisMarker = markerManager.markerStore.Find(pointer => pointer.gameObject.name.Equals(this.gameObject.name));
            markerManager.markerStore.Remove(_thisMarker);
        }

        Destroy(gameObject);
    }

    public void SetColor(Color color)
    {
        gameObject.GetComponent<SpriteRenderer>().color = color;
    }
}
