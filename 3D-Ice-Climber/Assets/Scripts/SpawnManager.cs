using System.Collections;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{   
    public GameObject seal;
    public GameObject bird;
    public GameObject iceFalling;
    private float xBound = 10.5f; 
    private int sealsNumberLimit = 3;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(SpawnSealPeriodically());
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void SpawnSeal(){
        int numberOfSealObjects = GameObject.FindGameObjectsWithTag("Seal").Length;

        if(numberOfSealObjects < sealsNumberLimit){
            Vector3 spawnPos = new Vector3(-xBound, transform.position.y, transform.position.z - 0.5f);
            Instantiate(seal, spawnPos, seal.transform.rotation);
        }
    }

    private IEnumerator SpawnSealPeriodically()
    {
        int waitingTime;

        while(true){
            waitingTime = Random.Range(4, 11);

            yield return new WaitForSeconds(waitingTime);

            Debug.Log("Spawning");
            SpawnSeal();
        }
        
    } 
}
