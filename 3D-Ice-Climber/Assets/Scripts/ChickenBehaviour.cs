using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChickenBehaviour : MonoBehaviour
{
    public float speed = 2.0f;
    private float xBound = 11.0f; 
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        MoveChickenHorizontally();
        ConstraintChickenPosition();
    }

    // provides horizontal movement to the chicken
    private void MoveChickenHorizontally(){
        transform.Translate(Vector3.forward * speed * Time.deltaTime, Space.Self);
    }

    // X axis bound checking to prevent chicken from going out of bounds
    private void ConstraintChickenPosition(){
        if(transform.position.x < -xBound){
            transform.position = new Vector3(xBound, transform.position.y, transform.position.z);
        }
        else if(transform.position.x > xBound){
            transform.position = new Vector3(-xBound, transform.position.y, transform.position.z);
        }
    }

    private void InitialMovementInit(){
        
    }
}
