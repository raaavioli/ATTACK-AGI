using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraHandler : MonoBehaviour
{
    private Camera p1Camera, p2Camera, combatCamera;
    [SerializeField]
    public Camera AnimCamera;
    public static CameraHandler instance;

    void Awake() 
    {
        if (instance == null) {
            instance = this;
        } else {
            Destroy(this);
        }
    }

    void Start()
    {
        Camera[] allCameras = GetComponentsInChildren<Camera>();
        foreach(Camera camera in allCameras) {
            if (camera.name == "p1Camera") p1Camera = camera;
            else if (camera.name == "p2Camera") p2Camera = camera;
            else combatCamera = camera;
        }

        StartSetupCameras();
    }

    public void StartSetupCameras()
    {
        p1Camera.enabled = true;
        p2Camera.enabled = true;
        combatCamera.enabled = false;
    }

    public void StartCombatCamera()
    {
        p1Camera.enabled = false;
        p2Camera.enabled = false;
        AnimCamera.gameObject.SetActive(false);
        combatCamera.enabled = true;
    }

        public void StartAnimCamera()
    {
        p1Camera.enabled = false;
        p2Camera.enabled = false;
        AnimCamera.gameObject.SetActive(true);
        combatCamera.enabled = false;

    }
}
