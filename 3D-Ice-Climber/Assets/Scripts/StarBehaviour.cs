using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StarBehaviour : MonoBehaviour
{
    public float speed = 5.0f;
    private bool isMovingOnTheRight;
    private float xBound = 18.5f;
    private int pointValue = 5000;
    
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
        moveStarHorizontally();
        constraintStarPosition();
    }

    private void OnCollisionEnter(Collision collision){
        GameObject parentOfGameObjectCollidedWith = collision.transform.root.gameObject;
        if(parentOfGameObjectCollidedWith.CompareTag("Player")){
            SpawnManager spawnManager = GameObject.Find("Spawn Manager").GetComponent<SpawnManager>();
            PlayerController player = GameObject.FindWithTag("Player").GetComponent<PlayerController>();
            Destroy(gameObject);
            spawnManager.UpdateScore(pointValue);
            spawnManager.ActivateVictoryText();
            spawnManager.ActivateVictoryState();
            player.starParticle.Play();
        }
    }

    // provides horizontal movement to the cloud
    public void moveStarHorizontally(){
        if(isMovingOnTheRight){
            transform.Translate(Vector3.right * speed * Time.deltaTime, Space.World);
        }
        else{
            transform.Translate(-Vector3.right * speed * Time.deltaTime, Space.World);  
        }
    }

    // X axis bound checking to prevent cloud from going out of bounds
    private void constraintStarPosition(){
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

    public float GetSpeed(){
        return speed;
    }
}
