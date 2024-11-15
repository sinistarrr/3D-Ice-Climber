using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

public class SpawnManager : MonoBehaviour
{
    public GameObject unbreakableBlockPrefab;
    public GameObject breakableBlockPrefab;
    public GameObject smallBreakableBlockPrefab;
    public GameObject chickenPrefab;
    public GameObject birdPrefab;
    public GameObject iceFallingPrefab;
    public GameObject cloudPrefab;
    public List<List<GameObject>> listOfGrounds;
    private Vector3 originBlockPosition = new Vector3(-16.5f, -2.0f, -0.5f);
    private Vector3 unbreakableBlockPrefabBounds, breakableBlockPrefabBounds, smallBreakableBlockPrefabBounds;
    private float rowHeight = 3.75f;
    private int maxRowCount = 8;
    private int currentRowCount = 5;
    private int blockCount = 68;
    private List<Vector3> breakableBlocksPrefabsBounds;
    private List<GameObject> breakableBlocksPrefabs;
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
        Debug.Log(currentRowCount);
    }

    private bool SpawnChicken()
    {
        List<GameObject> gameObjects = GameObject.FindGameObjectsWithTag("Chicken").ToList();
        int numberOfChickenObjects = gameObjects.Count();

        if (numberOfChickenObjects < chickensNumberLimit)
        {
            int lineToSpawnChickenOn = GetLineToSpawnChickenOn(gameObjects, numberOfChickenObjects);
            if (lineToSpawnChickenOn >= maxRowCount)
            {
                return false;
            }
            else if (lineToSpawnChickenOn != -1)
            {
                GameObject chickenGameObject;
                Vector3 spawnPos = new Vector3(-xBound, transform.position.y - spawnManagerPositionYOffset + (lineToSpawnChickenOn * rowHeight), transform.position.z - 0.5f);
                if (Random.Range(0, 2) == 0)
                {
                    chickenGameObject = Instantiate(chickenPrefab, spawnPos, chickenPrefab.transform.rotation);
                    chickenGameObject.GetComponent<ChickenBehaviour>().SetDirection(1);
                }
                else
                {
                    chickenGameObject = Instantiate(chickenPrefab, spawnPos, Quaternion.Inverse(chickenPrefab.transform.rotation));
                    chickenGameObject.GetComponent<ChickenBehaviour>().SetDirection(-1);
                }
                chickenGameObject.GetComponent<ChickenBehaviour>().SetLine(lineToSpawnChickenOn);

            }
        }
        return true;
    }

    private int GetLineToSpawnChickenOn(List<GameObject> gameObjects, int numberOfChickenObjects)
    {
        float firstLineYPos = transform.position.y - spawnManagerPositionYOffset;

        // List of possible lines we want to spawn chickens on
        List<int> possibleLinesToSpawnChickenOn = new List<int>() { currentRowCount - (2 + currentRowCount % 2), currentRowCount - (4 + currentRowCount % 2) };
        if (numberOfChickenObjects == 0)
        {
            return possibleLinesToSpawnChickenOn[Random.Range(0, possibleLinesToSpawnChickenOn.Count())];
        }
        else
        {
            for (int i = 0; i < possibleLinesToSpawnChickenOn.Count(); i++)
            {
                if (!gameObjects.Find(chicken => System.Math.Abs(chicken.transform.position.y - (firstLineYPos + rowHeight * possibleLinesToSpawnChickenOn[i])) < 1))
                {
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

            if (!SpawnChicken())
            {
                break;
            }
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
        listOfGrounds = new List<List<GameObject>>(maxRowCount);
        // Create all respective sub-lists of size blockCount
        for (int i = 0; i < maxRowCount; i++)
        {
            listOfGrounds.Add(new List<GameObject>(blockCount));
        }
        // Initialization of sub-lists values
        for (int i = 0; i < currentRowCount; i++)
        {
            CreateRow(i);
        }
    }

    public void CreateRow(int rowNumber)
    {
        for (int j = 0; j < blockCount; j++)
        {
            if (j < (blockCount / 2))
            {

                if (j < (5 + (rowNumber + 1) / 3) || (j > (blockCount / 2) - (6 + (rowNumber + 1) / 3)))
                {
                    Vector3 spawnPos = new Vector3(originBlockPosition.x + j * unbreakableBlockPrefabBounds.x, originBlockPosition.y + rowNumber * rowHeight, originBlockPosition.z);
                    GameObject unbrGameObject = Instantiate(unbreakableBlockPrefab, spawnPos, unbreakableBlockPrefab.transform.rotation);
                    unbrGameObject.GetComponentInChildren<GroundBehaviour>().SetLine(rowNumber);
                    listOfGrounds[rowNumber].Add(unbrGameObject);
                }
                else
                {
                    Vector3 spawnPos = new Vector3(originBlockPosition.x + j * breakableBlockPrefabBounds.x, originBlockPosition.y + rowNumber * rowHeight, originBlockPosition.z);
                    GameObject brGameObject = Instantiate(breakableBlockPrefab, spawnPos, breakableBlockPrefab.transform.rotation);
                    brGameObject.GetComponentInChildren<GroundBehaviour>().SetLine(rowNumber);
                    listOfGrounds[rowNumber].Add(brGameObject);
                }
            }
            else if (rowNumber > 0)
            {
                int randomIndex;
                if (listOfGrounds[rowNumber][j - 1].CompareTag("Ground Breakable"))
                {
                    randomIndex = Random.Range(0, breakableBlocksPrefabs.Count);
                }
                else if (listOfGrounds[rowNumber][j - 1].CompareTag("Ground Breakable Small") && listOfGrounds[rowNumber][j - 2].CompareTag("Ground Breakable"))
                {
                    randomIndex = breakableBlocksPrefabs.Count;
                }
                else if (listOfGrounds[rowNumber][j - 1].CompareTag("Ground Breakable Small"))
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
                    Vector3 spawnPos = new Vector3(originBlockPosition.x + (j - blockCount / 2) * breakableBlocksPrefabsBounds[randomIndex].x, originBlockPosition.y + rowNumber * rowHeight - breakableBlocksPrefabsBounds[randomIndex].y - ((unbreakableBlockPrefabBounds.y - breakableBlocksPrefabsBounds[randomIndex].y) / 2), originBlockPosition.z);
                    GameObject brGameObject = Instantiate(breakableBlocksPrefabs[randomIndex], spawnPos, breakableBlocksPrefabs[randomIndex].transform.rotation);
                    brGameObject.GetComponentInChildren<GroundBehaviour>().SetLine(rowNumber);
                    listOfGrounds[rowNumber].Add(brGameObject);
                }
                else
                {
                    Vector3 spawnPos = new Vector3(originBlockPosition.x + (j - blockCount / 2) * unbreakableBlockPrefabBounds.x, originBlockPosition.y + rowNumber * rowHeight - unbreakableBlockPrefabBounds.y, originBlockPosition.z);
                    GameObject unbrGameObject = Instantiate(unbreakableBlockPrefab, spawnPos, unbreakableBlockPrefab.transform.rotation);
                    unbrGameObject.GetComponentInChildren<GroundBehaviour>().SetLine(rowNumber);
                    listOfGrounds[rowNumber].Add(unbrGameObject);
                    listOfGrounds[rowNumber][j].SetActive(false);
                }
                if (j < (5 + (rowNumber + 1) / 3 + blockCount / 2) || (j > blockCount - (6 + (rowNumber + 1) / 3)))
                {
                    listOfGrounds[rowNumber][j].SetActive(false);
                }
            }
        }
    }

    public void SpawnMountainBorders()
    {
        int maxNumberOfWalls = 8;
        int maxNumberOfLinesPerWall = 8;
        int numberOfLinesPerWall = 8;
        int maxNumberOfBlocksOnALine = 3;
        for (int i = 0; i < maxNumberOfWalls; i++)
        {
            if (i == (maxNumberOfWalls - 1))
            {
                numberOfLinesPerWall = 5;
            }
            for (int j = 0; j < numberOfLinesPerWall; j++)
            {
                for (int k = 0; k < maxNumberOfBlocksOnALine; k++)
                {
                    Vector3 spawnPos = new Vector3(originBlockPosition.x + (i + k + 1) * unbreakableBlockPrefabBounds.x, originBlockPosition.y + maxRowCount * rowHeight + unbreakableBlockPrefabBounds.y * (j + i * maxNumberOfLinesPerWall), originBlockPosition.z);
                    GameObject unbrGameObject = Instantiate(unbreakableBlockPrefab, spawnPos, unbreakableBlockPrefab.transform.rotation);
                    unbrGameObject.GetComponentInChildren<GroundBehaviour>().SetLine(-1);

                    spawnPos = new Vector3(originBlockPosition.x + (blockCount / 2 - 2) * unbreakableBlockPrefabBounds.x - (i + k) * unbreakableBlockPrefabBounds.x, originBlockPosition.y + maxRowCount * rowHeight + unbreakableBlockPrefabBounds.y * (j + i * maxNumberOfLinesPerWall), originBlockPosition.z);
                    unbrGameObject = Instantiate(unbreakableBlockPrefab, spawnPos, unbreakableBlockPrefab.transform.rotation);
                    unbrGameObject.GetComponentInChildren<GroundBehaviour>().SetLine(-1);
                }
            }
        }

    }
    public void SpawnCloudLevelFirstStage()
    {
        HashSet<int> firstLineHoles = new HashSet<int>() { 8, 9, 16, 17, 24, 25 };

        for (int j = 0; j < (blockCount / 2); j++)
        {
            if (!firstLineHoles.Contains(j))
            {
                Vector3 spawnPos = new Vector3(originBlockPosition.x + j * unbreakableBlockPrefabBounds.x, originBlockPosition.y + currentRowCount * rowHeight, originBlockPosition.z);
                GameObject unbrGameObject = Instantiate(unbreakableBlockPrefab, spawnPos, unbreakableBlockPrefab.transform.rotation);
                unbrGameObject.GetComponentInChildren<GroundBehaviour>().SetLine(currentRowCount);
            }
        }
        IncrementCurrentRowCount();

    }
    public void SpawnCloudLevelSecondStage()
    {
        HashSet<int> secondLineHoles = new HashSet<int>() { 0, 1, 2, 3, 4, 5, 6, 11, 12, 13, 20, 21, 22, 27, 28, 29, 30, 31, 32, 33 };

        for (int j = 0; j < (blockCount / 2); j++)
        {

            if (!secondLineHoles.Contains(j))
            {
                Vector3 spawnPos = new Vector3(originBlockPosition.x + j * unbreakableBlockPrefabBounds.x, originBlockPosition.y + currentRowCount * rowHeight - rowHeight / 3f, originBlockPosition.z);
                GameObject unbrGameObject = Instantiate(unbreakableBlockPrefab, spawnPos, unbreakableBlockPrefab.transform.rotation);
                unbrGameObject.GetComponentInChildren<GroundBehaviour>().SetLine(currentRowCount);
            }
        }
        IncrementCurrentRowCount();
    }

    public void SpawnCloudLevelThirdStage()
    {
        // List of our different platforms, they are tuple that contain Height float value of the platform and the HashSet of the block positions of the platform
        List<Tuple<float, HashSet<int>>> platforms = new List<Tuple<float, HashSet<int>>>();

        // Initialization of said platforms, numbers correspond to blocks position
        HashSet<int> firstLineBlocks = new HashSet<int>() { 19, 20, 21, 22 };
        HashSet<int> secondLineBlocks = new HashSet<int>() { 6, 7, 8, 9, 10, 11, 12 };
        HashSet<int> thirdLineBlocks = new HashSet<int>() { 22, 23, 24 };
        HashSet<int> fourthLineBlocks = new HashSet<int>() { 13, 14, 15, 16, 17, 18 };

        // We add the platforms to the list of platforms
        platforms.Add(new Tuple<float, HashSet<int>>((currentRowCount + 1) * rowHeight - 1.25f, firstLineBlocks));
        platforms.Add(new Tuple<float, HashSet<int>>((currentRowCount + 2) * rowHeight - 4.25f, secondLineBlocks));
        platforms.Add(new Tuple<float, HashSet<int>>((currentRowCount + 3) * rowHeight - 5.5f, thirdLineBlocks));
        platforms.Add(new Tuple<float, HashSet<int>>((currentRowCount + 4) * rowHeight - 8.75f, fourthLineBlocks));

        // We start by creating the cloud
        Vector3 spawnPos = new Vector3(cloudPrefab.transform.position.x, originBlockPosition.y + currentRowCount * rowHeight - 1.0f, cloudPrefab.transform.position.z - 0.5f);
        SpawnCloudAtCoordinates(spawnPos, currentRowCount);

        // We create the platforms
        platforms.ForEach(platform => InstantiatePlatform(platform));

    }

    public void SpawnCloudLevelFourthStage()
    {
        // List of our different platforms, they are tuple that contain Height float value of the platform and the HashSet of the block positions of the platform
        List<Tuple<float, HashSet<int>>> platforms = new List<Tuple<float, HashSet<int>>>();

        // Initialization of said platforms, numbers correspond to blocks position
        HashSet<int> firstLineBlocks = new HashSet<int>() { 17, 18, 19, 20 };
        HashSet<int> secondLineBlocks = new HashSet<int>() { 9, 10, 11 };
        HashSet<int> thirdLineBlocks = new HashSet<int>() { 20, 21, 22 };
        HashSet<int> fourthLineBlocks = new HashSet<int>() { 13, 14, 15 };
        HashSet<int> fifthLineBlocks = new HashSet<int>() { 15, 16, 17, 18 };
        HashSet<int> peakLineBlocks = new HashSet<int>() {8, 9, 10, 11, 12, 13, 21, 22, 23, 24, 25};

        // We add the platforms to the list of platforms
        platforms.Add(new Tuple<float, HashSet<int>>((currentRowCount + 1) * rowHeight - 9.25f, firstLineBlocks));
        platforms.Add(new Tuple<float, HashSet<int>>((currentRowCount + 2) * rowHeight - 12.25f, secondLineBlocks));
        platforms.Add(new Tuple<float, HashSet<int>>((currentRowCount + 3) * rowHeight - 14.0f, thirdLineBlocks));
        platforms.Add(new Tuple<float, HashSet<int>>((currentRowCount + 4) * rowHeight - 16.5f, fourthLineBlocks));
        platforms.Add(new Tuple<float, HashSet<int>>((currentRowCount + 5) * rowHeight - 17.25f, fifthLineBlocks));
        platforms.Add(new Tuple<float, HashSet<int>>((currentRowCount + 6) * rowHeight - 18f, peakLineBlocks));

        // We start by creating the cloud
        Vector3 spawnPos = new Vector3(cloudPrefab.transform.position.x, originBlockPosition.y + currentRowCount * rowHeight - 8.5f, cloudPrefab.transform.position.z - 0.5f);
        SpawnCloudAtCoordinates(spawnPos, currentRowCount);

        // We create the platforms
        platforms.ForEach(platform => InstantiatePlatform(platform));
    }

    private void InstantiatePlatform(Tuple<float, HashSet<int>> platform)
    {
        for (int j = 0; j < (blockCount / 2); j++)
        {

            if (platform.Item2.Contains(j))
            {
                Vector3 spawnPos = new Vector3(originBlockPosition.x + j * unbreakableBlockPrefabBounds.x, originBlockPosition.y + platform.Item1, originBlockPosition.z);
                GameObject unbrGameObject = Instantiate(unbreakableBlockPrefab, spawnPos, unbreakableBlockPrefab.transform.rotation);
                unbrGameObject.GetComponentInChildren<GroundBehaviour>().SetLine(currentRowCount);
            }
        }
        IncrementCurrentRowCount();
    }

    public void SpawnCloudAtCoordinates(Vector3 spawnPos, int lineCount)
    {
        GameObject cloudGameObject = Instantiate(cloudPrefab, spawnPos, cloudPrefab.transform.rotation);
        cloudGameObject.GetComponentInChildren<GroundBehaviour>().SetLine(lineCount);
        IncrementCurrentRowCount();
    }
    public void AddRow()
    {
        CreateRow(currentRowCount);
        IncrementCurrentRowCount();
    }
    public void DestroyRow(int rowNumber)
    {
        listOfGrounds[rowNumber].ForEach(blockGameObject => Destroy(blockGameObject));
    }
    public int GetCurrentRowCount()
    {
        return currentRowCount;
    }

    public int GetCurrentMiddleRow()
    {
        return currentRowCount - 3;
    }

    public float GetRowHeight()
    {
        return rowHeight;
    }

    public void IncrementCurrentRowCount()
    {
        currentRowCount++;
    }

    public int GetMaxRow()
    {
        return maxRowCount;
    }
}
