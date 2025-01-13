using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaneLimitBehaviour : MonoBehaviour
{

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other){
        if(other.gameObject.CompareTag("Player")){
            other.gameObject.GetComponent<PlayerController>().ManagePlayerDeath(PlayerController.DeathCause.Falling);
        }
        else if(!other.transform.root.gameObject.CompareTag("Player")){
            Destroy(other.gameObject);
        }
    }
}
