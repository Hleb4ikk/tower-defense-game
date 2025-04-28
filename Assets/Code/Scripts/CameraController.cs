using System;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [HideInInspector] Camera mainCamera;
    [HideInInspector] int width;

    void PositionCamera(){
        float centerX = 0;
        float centerY = width * 0.25f;
         
        mainCamera.transform.position = new Vector2(centerX, centerY);
        mainCamera.GetComponent<Camera>().nearClipPlane = 0f;
        mainCamera.GetComponent<Camera>().orthographic = true;
    }

    void UpdateScroll(){
        float scrollYDelta = Input.mouseScrollDelta.y;
        float newCamOrthographicSize = mainCamera.GetComponent<Camera>().orthographicSize;

        if(scrollYDelta != 0){
            newCamOrthographicSize = mainCamera.GetComponent<Camera>().orthographicSize - 0.1f * scrollYDelta;
        }

        mainCamera.GetComponent<Camera>().orthographicSize = Math.Clamp(newCamOrthographicSize, 3.5f, 7f);
    }

    void MoveCamera(){
        float mousePositionXDelta = Input.mousePositionDelta.x;
        float mousePositionYDelta = Input.mousePositionDelta.y;

        float newCamPosX = mainCamera.transform.position.x;
        float newCamPosY = mainCamera.transform.position.y;

        if(mousePositionXDelta != 0 && Input.GetMouseButton(0)){
            newCamPosX = mainCamera.transform.position.x - 0.05f * mousePositionXDelta;
        }

        if(mousePositionYDelta != 0 && Input.GetMouseButton(0)){
            newCamPosY = mainCamera.transform.position.y - 0.05f * mousePositionYDelta;
        }

        mainCamera.transform.position = new Vector2(Math.Clamp(newCamPosX, -10, 10), Math.Clamp(newCamPosY, 0, 10)); 
    }

    void Awake(){
        mainCamera = Camera.main;
        width = GameObject.FindWithTag("Map").GetComponent<MapGenerator>().width;
    }
    void Start()
    {
        PositionCamera();
    }

    void Update(){
        UpdateScroll();
        MoveCamera();
    }
}
