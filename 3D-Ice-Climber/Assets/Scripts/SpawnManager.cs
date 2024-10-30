using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{   
    public GameObject unbreakableBlockPrefab;
    public GameObject breakableBlockPrefab;
    public GameObject smallBreakableBlockPrefab;
    public GameObject chickenPrefab;
    public GameObject birdPrefab;
    public GameObject iceFallingPrefab;
    private Vector3 originBlockPosition = new Vector3(-16.5f, -2.0f, -0.5f);
    private Vector3 unbreakableBlockPrefabBounds, breakableBlockPrefabBounds, smallBreakableBlockPrefabBounds;
    private float rowHeight = 3.75f;
    private int rowCount = 8;
    private int currentRowCount = 4;
    private int blockCount = 34;
    private List<List<GameObject>> listOfGrounds;
    private float xBound = 10.5f; 
    private int chickensNumberLimit = 3;
    // Start is called before the first frame update
    void Start()
    {
        BoundsVariablesInit();
        ListOfGroundsInit();
        // Creation of the blocks in the game
        
        // Spawn manager of the chickens
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
                Instantiate(chickenPrefab, spawnPos, chickenPrefab.transform.rotation);
            }
            else{
                Instantiate(chickenPrefab, spawnPos, Quaternion.Inverse(chickenPrefab.transform.rotation));
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

    private void BoundsVariablesInit(){
        GameObject unb = Instantiate(unbreakableBlockPrefab), br = Instantiate(breakableBlockPrefab), smbr = Instantiate(smallBreakableBlockPrefab);
        unbreakableBlockPrefabBounds = unb.GetComponent<Collider>().bounds.size;
        breakableBlockPrefabBounds = br.GetComponent<Collider>().bounds.size;
        smallBreakableBlockPrefabBounds = smbr.GetComponent<Collider>().bounds.size;
        Destroy(unb);
        Destroy(br);
        Destroy(smbr);
    }

    private void ListOfGroundsInit(){
        // Create list of list of size rowCount
        listOfGrounds = new List<List<GameObject>>(rowCount);
        // Create all respective sub-lists of size blockCount
        for (int i = 0; i < rowCount; i++)
        {
            listOfGrounds.Add(new List<GameObject>(blockCount));
        }
        // Initialization of sub-lists values
        for (int i = 0; i < currentRowCount; i++)
        {
            for (int j = 0; j < blockCount; j++)
            {
                //collision.gameObject.GetComponent<Collider>().bounds.size
                
                Vector3 spawnPos = new Vector3(originBlockPosition.x + j * unbreakableBlockPrefabBounds.x, originBlockPosition.y + i * rowHeight, originBlockPosition.z);
                listOfGrounds[i].Add(Instantiate(unbreakableBlockPrefab, spawnPos, unbreakableBlockPrefab.transform.rotation));
            }
        }
    }
}
