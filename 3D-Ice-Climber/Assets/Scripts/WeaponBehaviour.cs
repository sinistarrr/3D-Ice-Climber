using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponBehaviour : MonoBehaviour
{
    private PlayerController playerScript;
    private float swordPushForce = 20.0f;
    // Start is called before the first frame update
    void Start()
    {
        playerScript = transform.root.GetComponent<PlayerController>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter(Collider other) {
        if((other.gameObject.CompareTag("Chicken") || other.gameObject.CompareTag("Falling Ice")) && playerScript.isFiring){
            Destroy(other.gameObject);
        }
    }

}
