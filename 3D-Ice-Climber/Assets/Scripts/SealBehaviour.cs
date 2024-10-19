using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SealBehaviour : MonoBehaviour
{
    public float speed = 2.0f;
    public bool isMovingOnTheRight;
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
        moveSealHorizontally();
        constraintSealPosition();
    }

    // provides horizontal movement to the seal
    private void moveSealHorizontally(){
        if(isMovingOnTheRight){
            transform.Translate(Vector3.right * speed * Time.deltaTime, Space.World);
        }
        else{
            transform.Translate(-Vector3.right * speed * Time.deltaTime, Space.World);  
        }
    }

    // X axis bound checking to prevent seal from going out of bounds
    private void constraintSealPosition(){
        if(transform.position.x < -xBound){
            transform.position = new Vector3(xBound, transform.position.y, transform.position.z);
        }
        else if(transform.position.x > xBound){
            transform.position = new Vector3(-xBound, transform.position.y, transform.position.z);
        }
    }

    private void initialMovementInit(){
        
    }
}
