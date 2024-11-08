using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundBehaviour : MonoBehaviour
{
    private GameObject lastEncounteredChickenGameObject;
    private bool isCollidingWithChicken = false;
    private int groundLine;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter(Collision collision){
        if(collision.gameObject.CompareTag("Chicken") && !isCollidingWithChicken){
            isCollidingWithChicken = true;
            if(lastEncounteredChickenGameObject != collision.gameObject){
                lastEncounteredChickenGameObject = collision.gameObject;
            }
        }
    }

    private void OnCollisionExit(Collision collision){
        if(collision.gameObject.CompareTag("Chicken") && isCollidingWithChicken){
            isCollidingWithChicken = false;
        }
    }

    public bool IsCollidingWithChicken(){
        return isCollidingWithChicken;
    }

    public void SetLine(int line){
        groundLine = line;
    }

    public int GetLine(){
        return groundLine;
    }

    public GameObject GetLastChickenCollidedWith(){
        return lastEncounteredChickenGameObject;
    }
}
