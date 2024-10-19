using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInCameraDetection : MonoBehaviour
{
    private Camera mainCamera;
    private MeshRenderer playerRenderer;
    private Plane[] cameraFrustum;
    private Collider playerCollider;
    private Vector3 targetPosition, cameraPositionUpdate;
    private bool cameraIsMoving = false;
    private const float cameraVerticalTranslationOffset = 2;
    public float cameraTranslationValue = 15; // Default value is 15
    public float cameraSpeed = 0.1f; // Default value is 0.1f
        
    // Start is called before the first frame update
    void Start()
    {
        mainCamera = Camera.main;
        playerRenderer = gameObject.GetComponentInChildren<MeshRenderer>();
        playerCollider = GetComponent<Collider>();
        cameraFrustum = GeometryUtility.CalculateFrustumPlanes(mainCamera);
        targetPosition = mainCamera.transform.position;

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.anyKey)
        {
            var playerBounds = playerCollider.bounds;
            // if the player bounds aren't inside of the camera frustrum
            if(!GeometryUtility.TestPlanesAABB(cameraFrustum, playerBounds)){
                if(!cameraIsMoving){
                    var distancePlayerFromCameraX = Math.Abs(mainCamera.transform.position.x - playerBounds.center.x);
                    var distancePlayerFromCameraY = Math.Abs(mainCamera.transform.position.y - playerBounds.center.y);
                    // we take the distance from the player to the camera and then compare which one is biggest, and move camera accordingly
                    if(distancePlayerFromCameraX > distancePlayerFromCameraY){
                        if(mainCamera.transform.position.x < playerBounds.center.x){
                            //playerRenderer.sharedMaterial.color = Color.blue;
                            targetPosition = mainCamera.transform.position + new Vector3(cameraTranslationValue, 0, 0);
                        }
                        else{
                            //playerRenderer.sharedMaterial.color = Color.red;
                            targetPosition = mainCamera.transform.position - new Vector3(cameraTranslationValue, 0, 0);
                        }
                    }
                    else{
                        if(mainCamera.transform.position.y < playerBounds.center.y){
                            //playerRenderer.sharedMaterial.color = Color.blue;
                            targetPosition = mainCamera.transform.position + new Vector3(0, cameraTranslationValue - cameraVerticalTranslationOffset, 0);
                        }
                        else{
                            //playerRenderer.sharedMaterial.color = Color.red;
                            targetPosition = mainCamera.transform.position - new Vector3(0, cameraTranslationValue - cameraVerticalTranslationOffset, 0);
                        }
                    }
                    cameraIsMoving = true;
                }
            }
            // // Uncomment this if you want to verify if the player is inside the fulstrum
            // else{
            //     playerRenderer.sharedMaterial.color = Color.green;
            // }
        }
        if(cameraIsMoving){ 
            if(Vector3.Distance(mainCamera.transform.position, targetPosition) > cameraSpeed) {
                mainCamera.transform.position = Vector3.MoveTowards(mainCamera.transform.position, targetPosition, cameraSpeed);
                cameraFrustum = GeometryUtility.CalculateFrustumPlanes(mainCamera);
            }
            else{
                cameraIsMoving = false;
            }
        }
    }
}
