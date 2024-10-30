using System.Collections;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{   
    public GameObject chicken;
    public GameObject bird;
    public GameObject iceFalling;
    private float xBound = 10.5f; 
    private int chickensNumberLimit = 3;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(SpawnChickenPeriodically());
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void SpawnChicken(){
        int numberOfChickenObjects = GameObject.FindGameObjectsWithTag("Chicken").Length;

        if(numberOfChickenObjects < chickensNumberLimit){
            Vector3 spawnPos = new Vector3(-xBound, transform.position.y, transform.position.z - 0.5f);
            if(Random.Range(0, 2) == 0){
                Instantiate(chicken, spawnPos, chicken.transform.rotation);
            }
            else{
                Instantiate(chicken, spawnPos, Quaternion.Inverse(chicken.transform.rotation));
            }
        }
    }

    private IEnumerator SpawnChickenPeriodically()
    {
        int waitingTime;

        while(true){
            waitingTime = Random.Range(4, 11);

            yield return new WaitForSeconds(waitingTime);

            Debug.Log("Spawning");
            SpawnChicken();
        }
        
    } 
}
