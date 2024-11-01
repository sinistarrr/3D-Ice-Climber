using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloudBehaviour : MonoBehaviour
{   
    public float speed = 5.0f;
    private bool isMovingOnTheRight;
    private float xBound = 11.0f; 
    // Start is called before the first frame update
    void Start()
    {
        if(Random.Range(0, 2) == 0){
            isMovingOnTheRight = false;
        }
        else{
            isMovingOnTheRight = true;
        }
    }

    // Update is called once per frame
    void Update()
    {
        moveCloudHorizontally();
        constraintCloudPosition();
    }

    // provides horizontal movement to the cloud
    private void moveCloudHorizontally(){
        if(isMovingOnTheRight){
            transform.Translate(Vector3.right * speed * Time.deltaTime, Space.World);
        }
        else{
            transform.Translate(-Vector3.right * speed * Time.deltaTime, Space.World);  
        }
    }

    // X axis bound checking to prevent cloud from going out of bounds
    private void constraintCloudPosition(){
        if(transform.position.x < -xBound){
            transform.position = new Vector3(xBound, transform.position.y, transform.position.z);
        }
        else if(transform.position.x > xBound){
            transform.position = new Vector3(-xBound, transform.position.y, transform.position.z);
        }
    }
}
