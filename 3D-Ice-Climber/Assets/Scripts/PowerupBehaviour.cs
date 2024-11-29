using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PowerupBehaviour : MonoBehaviour
{
    public ParticleSystem collectibleParticle;
    
    private int pointValue = 100;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter(Collision collision){
        GameObject parentOfGameObjectCollidedWith = collision.transform.root.gameObject;
        if(parentOfGameObjectCollidedWith.CompareTag("Player")){
            SpawnManager spawnManager = GameObject.Find("Spawn Manager").GetComponent<SpawnManager>();
            PlayerController player = GameObject.FindWithTag("Player").GetComponent<PlayerController>();
            Destroy(gameObject);
            spawnManager.UpdateScore(pointValue);
            player.collectibleExplodeParticle.Play();
        }
    }
}
