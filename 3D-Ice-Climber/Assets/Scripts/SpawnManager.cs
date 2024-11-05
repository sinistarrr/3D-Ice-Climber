using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    private int currentRowCount = 5;
    private int blockCount = 68;
    private List<Vector3> breakableBlocksPrefabsBounds;
    private List<GameObject> breakableBlocksPrefabs;
    private List<List<GameObject>> listOfGrounds;
    private float xBound = 17.0f;
    private int chickensNumberLimit = 2;
    private float spawnManagerPositionYOffset = 1.75f;
    // Start is called before the first frame update
    void Start()
    {

        BoundsVariablesInit();
        // be careful that both lists have same prefab order, its important for later
        breakableBlocksPrefabs = new List<GameObject> { breakableBlockPrefab, smallBreakableBlockPrefab };
        breakableBlocksPrefabsBounds = new List<Vector3> { breakableBlockPrefabBounds, smallBreakableBlockPrefabBounds };
        ListOfGroundsInit();
        // Creation of the blocks in the game

        // Spawn manager of the chickens
        StartCoroutine(SpawnChickenPeriodically());
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void SpawnChicken()
    {
        List<GameObject> gameObjects = GameObject.FindGameObjectsWithTag("Chicken").ToList();
        int numberOfChickenObjects = gameObjects.Count();

        if (numberOfChickenObjects < chickensNumberLimit)
        {   
            int lineToSpawnChickenOn = GetLineToSpawnChickenOn(gameObjects, numberOfChickenObjects);
            if(lineToSpawnChickenOn != -1){
                Vector3 spawnPos = new Vector3(-xBound, transform.position.y - spawnManagerPositionYOffset + (lineToSpawnChickenOn * rowHeight), transform.position.z - 0.5f);
                if (Random.Range(0, 2) == 0)
                {
                    Instantiate(chickenPrefab, spawnPos, chickenPrefab.transform.rotation);
                }
                else
                {
                    Instantiate(chickenPrefab, spawnPos, Quaternion.Inverse(chickenPrefab.transform.rotation));
                }
            }
        }
    }

    private int GetLineToSpawnChickenOn(List<GameObject> gameObjects, int numberOfChickenObjects){
        float firstLineYPos = transform.position.y - spawnManagerPositionYOffset;

        // List of possible lines we want to spawn chickens on
        List<int> possibleLinesToSpawnChickenOn = new List<int>(){currentRowCount - (2 + currentRowCount % 2), currentRowCount - (4 + currentRowCount % 2)};
        if(numberOfChickenObjects == 0){
            return possibleLinesToSpawnChickenOn[Random.Range(0, possibleLinesToSpawnChickenOn.Count())];
        }
        else{
            for(int i = 0; i < possibleLinesToSpawnChickenOn.Count(); i++){
                if(!gameObjects.Find(chicken => System.Math.Abs(chicken.transform.position.y - (firstLineYPos + rowHeight * possibleLinesToSpawnChickenOn[i])) < 1)){
                    return possibleLinesToSpawnChickenOn[i];
                }
            }
            return -1;
        }
    }
    private IEnumerator SpawnChickenPeriodically()
    {
        int waitingTime;

        while (true)
        {
            waitingTime = Random.Range(4, 11);

            yield return new WaitForSeconds(waitingTime);

            SpawnChicken();
        }

    }

    private void BoundsVariablesInit()
    {
        GameObject unb = Instantiate(unbreakableBlockPrefab), br = Instantiate(breakableBlockPrefab), smbr = Instantiate(smallBreakableBlockPrefab);
        unbreakableBlockPrefabBounds = unb.GetComponentInChildren<Collider>().bounds.size;
        breakableBlockPrefabBounds = br.GetComponentInChildren<Collider>().bounds.size;
        smallBreakableBlockPrefabBounds = smbr.GetComponentInChildren<Collider>().bounds.size;
        Destroy(unb);
        Destroy(br);
        Destroy(smbr);
    }

    private void ListOfGroundsInit()
    {
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
                if (j < (blockCount / 2))
                {

                    if (j < (5 + (i + 1) / 3) || (j > (blockCount / 2) - (6 + (i + 1) / 3)))
                    {
                        Vector3 spawnPos = new Vector3(originBlockPosition.x + j * unbreakableBlockPrefabBounds.x, originBlockPosition.y + i * rowHeight, originBlockPosition.z);
                        listOfGrounds[i].Add(Instantiate(unbreakableBlockPrefab, spawnPos, unbreakableBlockPrefab.transform.rotation));
                    }
                    else
                    {
                        Vector3 spawnPos = new Vector3(originBlockPosition.x + j * breakableBlockPrefabBounds.x, originBlockPosition.y + i * rowHeight, originBlockPosition.z);
                        listOfGrounds[i].Add(Instantiate(breakableBlockPrefab, spawnPos, breakableBlockPrefab.transform.rotation));
                    }
                }
                else if (i > 0)
                {
                    int randomIndex;
                    if (listOfGrounds[i][j - 1].CompareTag("Ground Breakable"))
                    {
                        randomIndex = Random.Range(0, breakableBlocksPrefabs.Count);
                    }
                    else if (listOfGrounds[i][j - 1].CompareTag("Ground Breakable Small") && listOfGrounds[i][j - 2].CompareTag("Ground Breakable"))
                    {
                        randomIndex = breakableBlocksPrefabs.Count;
                    }
                    else if (listOfGrounds[i][j - 1].CompareTag("Ground Breakable Small"))
                    {
                        randomIndex = breakableBlocksPrefabs.FindIndex(elem => elem.CompareTag("Ground Breakable"));
                    }
                    else
                    {
                        if (Random.Range(0, 2) == 0)
                        {
                            randomIndex = breakableBlocksPrefabs.FindIndex(elem => elem.CompareTag("Ground Breakable Small"));
                        }
                        else
                        {
                            randomIndex = breakableBlocksPrefabs.Count;
                        }

                    }
                    if (randomIndex != breakableBlocksPrefabs.Count)
                    {
                        Vector3 spawnPos = new Vector3(originBlockPosition.x + (j - blockCount / 2) * breakableBlocksPrefabsBounds[randomIndex].x, originBlockPosition.y + i * rowHeight - breakableBlocksPrefabsBounds[randomIndex].y - ((unbreakableBlockPrefabBounds.y - breakableBlocksPrefabsBounds[randomIndex].y) / 2), originBlockPosition.z);
                        listOfGrounds[i].Add(Instantiate(breakableBlocksPrefabs[randomIndex], spawnPos, breakableBlocksPrefabs[randomIndex].transform.rotation));
                    }
                    else
                    {
                        Vector3 spawnPos = new Vector3(originBlockPosition.x + (j - blockCount / 2) * unbreakableBlockPrefabBounds.x, originBlockPosition.y + i * rowHeight - unbreakableBlockPrefabBounds.y, originBlockPosition.z);
                        listOfGrounds[i].Add(Instantiate(unbreakableBlockPrefab, spawnPos, unbreakableBlockPrefab.transform.rotation));
                        listOfGrounds[i][j].SetActive(false);
                    }
                    if (j < (5 + (i + 1) / 3 + blockCount / 2) || (j > blockCount - (6 + (i + 1) / 3)))
                    {
                        listOfGrounds[i][j].SetActive(false);
                    }
                }
            }
        }
    }
}
