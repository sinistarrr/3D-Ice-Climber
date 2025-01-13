using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloudBehaviour : MonoBehaviour
{   
    public float speed = 5.0f;
    private bool isMovingOnTheRight = false;
    private float xBound = 22.0f;
    // Start is called before the first frame update
    void Start()
    {
        speed = Random.Range(2.0f, 7.0f);
        transform.position = new Vector3(Random.Range(-xBound+1, xBound-1), transform.position.y, transform.position.z);
    }

    // Update is called once per frame
    void Update()
    {
        MoveCloudHorizontally();
        ConstraintCloudPosition();
    }

    // provides horizontal movement to the cloud
    public void MoveCloudHorizontally(){
        if(isMovingOnTheRight){
            transform.Translate(Vector3.right * speed * Time.deltaTime, Space.World);
        }
        else{
            transform.Translate(-Vector3.right * speed * Time.deltaTime, Space.World);  
        }
    }

    // X axis bound checking to prevent cloud from going out of bounds
    private void ConstraintCloudPosition(){
        if(transform.position.x < -xBound){
            transform.position = new Vector3(xBound, transform.position.y, transform.position.z);
        }
        else if(transform.position.x > xBound){
            transform.position = new Vector3(-xBound, transform.position.y, transform.position.z);
        }
    }

    public bool IsMovingRight(){
        return isMovingOnTheRight;
    }
    public void SetIsMovingRight(bool value){
        isMovingOnTheRight = value;
    }

    public float GetSpeed(){
        return speed;
    }
}
