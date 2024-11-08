using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponBehaviour : MonoBehaviour
{
    private PlayerController playerScript;
    // Start is called before the first frame update
    void Start()
    {
        playerScript = transform.root.GetComponent<PlayerController>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnTriggerEnter(Collider other)
    {
        if(playerScript.isFiring){
            if(other.gameObject.CompareTag("Chicken")){
                ChickenBehaviour chickenScript = other.gameObject.GetComponent<ChickenBehaviour>();
                chickenScript.SetDeathActivation(true);
                other.gameObject.transform.RotateAround(other.gameObject.transform.position, other.gameObject.transform.up, 180f);
            }
            if(other.gameObject.CompareTag("Falling Ice")){
                Destroy(other.gameObject);
            }
        }
    }

}
