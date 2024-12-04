using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEditor.Search;
using UnityEngine;
using Random = UnityEngine.Random;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Rendering.PostProcessing;

public class SpawnManager : MonoBehaviour
{
    public GameObject player;
    public GameObject unbreakableBlockPrefab;
    public GameObject breakableBlockPrefab;
    public GameObject smallBreakableBlockPrefab;
    public GameObject chickenPrefab;
    public GameObject starPrefab;
    public GameObject iceFallingPrefab;
    public GameObject cloudPrefab;
    public GameObject powerupPrefab;
    public GameObject titleScreen;
    public GameObject manualScreen;
    public GameObject titleButtons;
    public GameObject difficultyButtons;
    public AudioClip menuButtonClickSound;
    public ParticleSystem cloudParticle;
    public List<List<GameObject>> listOfGrounds;
    public List<Tuple<float, HashSet<int>>> cloudLevelPlatforms;
    public Vector3 originBlockPosition = new Vector3(-16.5f, -2.0f, -0.5f);
    public Vector3 unbreakableBlockPrefabBounds, breakableBlockPrefabBounds, smallBreakableBlockPrefabBounds, iceFallingPrefabBounds;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI hpText;
    public TextMeshProUGUI floorText;
    public TextMeshProUGUI gameOverText;
    public TextMeshProUGUI victoryText;
    public Button restartButton;
    public Button startButton;
    private Camera gameCamera;
    private List<AudioSource> inGameAudioSources;
    private AudioSource currentInGameAudioSource;
    private AudioSource titleAudio;
    private bool gameIsOver = false;
    private bool victory = false;
    private int score;
    private int playerHP = 0;
    private float rowHeight = 3.75f;
    private int maxRowCount = 5;
    private int currentRowCount = 5;
    private int blockCount = 68;
    private List<Vector3> breakableBlocksPrefabsBounds;
    private List<GameObject> breakableBlocksPrefabs;
    private float xBound = 17.0f;
    private int chickensNumberLimit = 2;
    private float spawnManagerPositionYOffset = 1.75f;
    private int difficultyLevel = -1;
    private bool mediumPhase = false;
    private bool hardPhase = false;
    private bool cloudLevelPhase = false;
    private int midPhaseLineLimit = 15;
    private int hardPhaseLineLimit = 31;
    private int totalCloudLevelPlatforms = 0;
    private float verticalLimitPosition = -5.0f;



    // Start is called before the first frame update
    void Start()
    {
        BoundsVariablesInit();
        BreakableBlockVariablesInit();
        // Creation of the blocks in the game
        ListOfGroundsInit();
        // Spawn manager of the chickens
        StartCoroutine(SpawnChickenPeriodically());

        titleAudio = GetComponent<AudioSource>();
        gameCamera = Camera.main;
        inGameAudioSources = gameCamera.GetComponents<AudioSource>().ToList();
        currentInGameAudioSource = inGameAudioSources.Find(audiosource => audiosource.clip.name.Equals("xDeviruchi - 07 Exploring The Unknown"));


    }

    // Update is called once per frame
    void Update()
    {
        if (!mediumPhase && (currentRowCount > midPhaseLineLimit + 1))
        {
            currentInGameAudioSource.Stop();
            currentInGameAudioSource = inGameAudioSources.Find(audiosource => audiosource.clip.name.Equals("xDeviruchi - 09 Decisive Battle"));
            currentInGameAudioSource.Play();
            mediumPhase = true;
        }
        else if (!hardPhase && (currentRowCount > hardPhaseLineLimit + 1))
        {
            currentInGameAudioSource.Stop();
            currentInGameAudioSource = inGameAudioSources.Find(audiosource => audiosource.clip.name.Equals("xDeviruchi - 08 Mysterious Dungeon"));
            currentInGameAudioSource.Play();
            hardPhase = true;
        }
        else if (!cloudLevelPhase && (currentRowCount > GetMaxRow()))
        {
            currentInGameAudioSource.Stop();
            currentInGameAudioSource = inGameAudioSources.Find(audiosource => audiosource.clip.name.Equals("xDeviruchi - 03 The Icy Cave"));
            currentInGameAudioSource.Play();
            cloudLevelPhase = true;
        }
    }

    public void UpdateScore(int valueToAdd)
    {
        if (!gameIsOver)
        {
            score += valueToAdd;
            scoreText.text = "Score: " + score;
        }
    }
    public void UpdateFloor(int floorValue)
    {
        if (!gameIsOver)
        {
            floorText.text = "Floor: " + floorValue;
        }
    }
    public void UpdateHP(int valueToAdd)
    {
        if (!gameIsOver)
        {
            playerHP += valueToAdd;
            hpText.text = "HP: " + playerHP;
        }
    }
    public void ActivateGameOverText()
    {
        gameOverText.gameObject.SetActive(true);
    }
    public void ActivateGameOverState()
    {
        if (!victory)
        {
            gameIsOver = true;
        }
    }
    public void ActivateVictoryText()
    {
        victoryText.gameObject.SetActive(true);
    }
    public void ActivateVictoryState()
    {
        if (!gameIsOver)
        {
            victory = true;
            StartCoroutine(RestartGameAfterFewSeconds(5));
        }

    }
    public void ActivateRestartButton()
    {
        restartButton.gameObject.SetActive(true);
    }

    public void RestartGame()
    {
        titleAudio.PlayOneShot(menuButtonClickSound);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ClickOnStartButton()
    {
        titleAudio.PlayOneShot(menuButtonClickSound);
        titleButtons.SetActive(false);
        difficultyButtons.SetActive(true);
    }
    public void StartGame(int difficulty)
    {
        InitializeInGameStatsInfo(difficulty);
        InitializeInGameText();
        Instantiate(player, new Vector3(-7, originBlockPosition.y + unbreakableBlockPrefabBounds.y / 2, -0.5f), player.transform.rotation);
        InitializeStartingSounds();
        DesactivateMenuEffects();
        CloudLevelPlatformsInit(difficulty);

    }
    private void InitializeInGameStatsInfo(int difficulty)
    {
        difficultyLevel = difficulty;
        maxRowCount = 8 * difficulty * 2;
        score = 0;
        UpdateScore(0);
        UpdateHP(3 - difficulty);
        UpdateFloor(0);
    }
    private void InitializeInGameText()
    {
        titleScreen.SetActive(false);
        scoreText.gameObject.SetActive(true);
        hpText.gameObject.SetActive(true);
        floorText.gameObject.SetActive(true);
    }
    private void InitializeStartingSounds()
    {
        titleAudio.Stop();
        currentInGameAudioSource.PlayOneShot(menuButtonClickSound);
        currentInGameAudioSource.Play();
    }
    private void DesactivateMenuEffects()
    {
        PostProcessVolume ppVolume = gameCamera.GetComponent<PostProcessVolume>();
        DepthOfField dofEffect;
        ColorGrading colorGradingEffect;
        ppVolume.profile.TryGetSettings(out dofEffect); // we get the settings inside DepthOfField variable
        ppVolume.profile.TryGetSettings(out colorGradingEffect); // we get the settings inside ColorGrading variable
        dofEffect.active = false;
        colorGradingEffect.active = false;
    }
    public void StartManualMenu()
    {
        titleAudio.PlayOneShot(menuButtonClickSound);
        titleScreen.SetActive(false);
        manualScreen.SetActive(true);
    }
    public void BackFromManualMenu()
    {
        titleAudio.PlayOneShot(menuButtonClickSound);
        manualScreen.SetActive(false);
        titleScreen.SetActive(true);
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
            waitingTime = Random.Range(1, 5);

            yield return new WaitForSeconds(waitingTime);

            if (!SpawnChicken())
            {
                break;
            }
        }

    }
    private IEnumerator RestartGameAfterFewSeconds(int seconds)
    {
        yield return new WaitForSeconds(seconds);
        RestartGame();
    }


    private void BoundsVariablesInit()
    {
        GameObject unb = Instantiate(unbreakableBlockPrefab), br = Instantiate(breakableBlockPrefab), smbr = Instantiate(smallBreakableBlockPrefab), icef = Instantiate(iceFallingPrefab);
        unbreakableBlockPrefabBounds = unb.GetComponentInChildren<Collider>().bounds.size;
        breakableBlockPrefabBounds = br.GetComponentInChildren<Collider>().bounds.size;
        smallBreakableBlockPrefabBounds = smbr.GetComponentInChildren<Collider>().bounds.size;
        iceFallingPrefabBounds = icef.GetComponent<Collider>().bounds.size;
        Destroy(unb);
        Destroy(br);
        Destroy(smbr);
        Destroy(icef);
    }

    private void BreakableBlockVariablesInit()
    {
        // be careful that both lists have same prefab order, its important for later
        breakableBlocksPrefabs = new List<GameObject> { breakableBlockPrefab, smallBreakableBlockPrefab };
        breakableBlocksPrefabsBounds = new List<Vector3> { breakableBlockPrefabBounds, smallBreakableBlockPrefabBounds };
    }
    private void CloudLevelPlatformsInit(int difficulty)
    {
        if(difficulty == 1){
            StoreEasyCloudLevel();
        }
        else if(difficulty == 2){
            StoreMediumCloudLevel();
        }
        else{
            StoreHardCloudLevel();
        }
    }

    private void StoreEasyCloudLevel(){
        // List of our different platforms that contain the HashSet of the block positions
        cloudLevelPlatforms = new List<Tuple<float, HashSet<int>>>{
            new Tuple<float, HashSet<int>>(maxRowCount * rowHeight, new HashSet<int> { 8, 9, 16, 17, 24, 25 }),
            new Tuple<float, HashSet<int>>((maxRowCount+1) * rowHeight - rowHeight / 3f, new HashSet<int> { 5,6,11,12,13,14,19,20,21,22,27,28 }),
            new Tuple<float, HashSet<int>>((maxRowCount+2) * rowHeight - 1.0f, new HashSet<int> {}),
            new Tuple<float, HashSet<int>>((maxRowCount + 3) * rowHeight - 1.25f, new HashSet<int> { 19, 20, 21, 22 }),
            new Tuple<float, HashSet<int>>((maxRowCount + 4) * rowHeight - 4.25f, new HashSet<int> { 6, 7, 8, 9, 10, 11, 12 }),
            new Tuple<float, HashSet<int>>((maxRowCount + 5) * rowHeight - 5.5f, new HashSet<int> { 22, 23, 24 }),
            new Tuple<float, HashSet<int>>((maxRowCount + 6) * rowHeight - 8.75f, new HashSet<int> { 13, 14, 15, 16, 17, 18 }),
            new Tuple<float, HashSet<int>>((maxRowCount + 7) * rowHeight - 8.5f, new HashSet<int> {}),
            new Tuple<float, HashSet<int>>((maxRowCount + 8) * rowHeight - 9.25f, new HashSet<int> { 17, 18, 19, 20 }),
            new Tuple<float, HashSet<int>>((maxRowCount + 9) * rowHeight - 12.25f, new HashSet<int> { 9, 10, 11 }),
            new Tuple<float, HashSet<int>>((maxRowCount + 10) * rowHeight - 14.0f, new HashSet<int> { 20, 21, 22 }),
            new Tuple<float, HashSet<int>>((maxRowCount + 11) * rowHeight - 16.5f, new HashSet<int> { 13, 14, 15 }),
            new Tuple<float, HashSet<int>>((maxRowCount + 12) * rowHeight - 17.25f, new HashSet<int> { 15, 16, 17, 18 }),
            new Tuple<float, HashSet<int>>((maxRowCount + 13) * rowHeight - 18f, new HashSet<int> {8, 9, 10, 11, 12, 13, 21, 22, 23, 24, 25})
        };
    }

    private void StoreMediumCloudLevel(){
        // List of our different platforms that contain the HashSet of the block positions
        cloudLevelPlatforms = new List<Tuple<float, HashSet<int>>>{
            new Tuple<float, HashSet<int>>(maxRowCount * rowHeight, new HashSet<int> { 8, 9, 16, 17, 24, 25 }),
            new Tuple<float, HashSet<int>>((maxRowCount+1) * rowHeight - rowHeight / 3f, new HashSet<int> { 5,6,11,12,13,14,19,20,21,22,27,28 }),
            new Tuple<float, HashSet<int>>((maxRowCount+2) * rowHeight - 1.5f, new HashSet<int> {}),
            new Tuple<float, HashSet<int>>((maxRowCount + 3) * rowHeight - 4.0f, new HashSet<int> { 11, 12, 13, 14}),
            new Tuple<float, HashSet<int>>((maxRowCount + 4) * rowHeight - 5.0f, new HashSet<int> { 19, 20, 21, 22 }),
            new Tuple<float, HashSet<int>>((maxRowCount + 5) * rowHeight - 8.0f, new HashSet<int> {}),
            new Tuple<float, HashSet<int>>((maxRowCount + 6) * rowHeight - 8.75f, new HashSet<int> { 13, 14, 15, 16, 17, 18 }),
            new Tuple<float, HashSet<int>>((maxRowCount + 7) * rowHeight - 8.5f, new HashSet<int> {}),
            new Tuple<float, HashSet<int>>((maxRowCount + 8) * rowHeight - 9.25f, new HashSet<int> { 17, 18, 19, 20 }),
            new Tuple<float, HashSet<int>>((maxRowCount + 9) * rowHeight - 13.0f, new HashSet<int> { 11, 12, 13, 14 }),
            new Tuple<float, HashSet<int>>((maxRowCount + 10) * rowHeight - 13f, new HashSet<int> {}),
            new Tuple<float, HashSet<int>>((maxRowCount + 12) * rowHeight - 17.25f, new HashSet<int> { 15, 16, 17, 18, 19, 20 }),
            new Tuple<float, HashSet<int>>((maxRowCount + 13) * rowHeight - 18f, new HashSet<int> {8, 9, 10, 11, 12, 13, 21, 22, 23, 24, 25})
        };
    }

    private void StoreHardCloudLevel(){
        // List of our different platforms that contain the HashSet of the block positions
        cloudLevelPlatforms = new List<Tuple<float, HashSet<int>>>{
            new Tuple<float, HashSet<int>>(maxRowCount * rowHeight, new HashSet<int> { 8, 9, 16, 17, 24, 25 }),
            new Tuple<float, HashSet<int>>((maxRowCount+1) * rowHeight - rowHeight / 3f, new HashSet<int> { 5,6,11,12,13,14,19,20,21,22,27,28 }),
            new Tuple<float, HashSet<int>>((maxRowCount+2) * rowHeight - 1.0f, new HashSet<int> {}),
            new Tuple<float, HashSet<int>>((maxRowCount + 3) * rowHeight - 1.25f, new HashSet<int> { 19, 20, 21, 22 }),
            new Tuple<float, HashSet<int>>((maxRowCount + 4) * rowHeight - 4.25f, new HashSet<int> { 6, 7, 8, 9, 10, 11, 12 }),
            new Tuple<float, HashSet<int>>((maxRowCount + 5) * rowHeight - 5.5f, new HashSet<int> { 22, 23, 24 }),
            new Tuple<float, HashSet<int>>((maxRowCount + 6) * rowHeight - 8.75f, new HashSet<int> { 13, 14, 15, 16, 17, 18 }),
            new Tuple<float, HashSet<int>>((maxRowCount + 7) * rowHeight - 8.5f, new HashSet<int> {}),
            new Tuple<float, HashSet<int>>((maxRowCount + 8) * rowHeight - 9.25f, new HashSet<int> { 17, 18, 19, 20 }),
            new Tuple<float, HashSet<int>>((maxRowCount + 9) * rowHeight - 12.25f, new HashSet<int> { 9, 10, 11 }),
            new Tuple<float, HashSet<int>>((maxRowCount + 10) * rowHeight - 14.0f, new HashSet<int> { 20, 21, 22 }),
            new Tuple<float, HashSet<int>>((maxRowCount + 11) * rowHeight - 16.5f, new HashSet<int> { 13, 14, 15 }),
            new Tuple<float, HashSet<int>>((maxRowCount + 12) * rowHeight - 17.25f, new HashSet<int> { 15, 16, 17, 18 }),
            new Tuple<float, HashSet<int>>((maxRowCount + 13) * rowHeight - 18f, new HashSet<int> {8, 9, 10, 11, 12, 13, 21, 22, 23, 24, 25})
        };
    }



    private void ListOfGroundsInit()
    {
        // Create list of list of size rowCount
        listOfGrounds = new List<List<GameObject>>();
        // // Create all respective sub-lists of size blockCount
        // for (int i = 0; i < maxRowCount; i++)
        // {
        //     listOfGrounds.Add(new List<GameObject>(blockCount));
        // }
        // Initialization of sub-lists values
        for (int i = 0; i < currentRowCount; i++)
        {
            CreateRow(i);
        }
    }

    public void CreateRow(int rowNumber)
    {
        listOfGrounds.Add(new List<GameObject>(blockCount));
        HashSet<int> holes = new HashSet<int>();
        int index;
        // Creates holes Set that contains the holes in our line
        if ((difficultyLevel > 2) && (rowNumber > hardPhaseLineLimit))
        {
            for (int i = 2; i < blockCount / 2 - 2; i++)
            {
                if (Random.Range(0.0f, 1.0f) > 0.25f)
                {
                    holes.Add(i);
                }
            }
        }
        else if ((difficultyLevel > 1) && (rowNumber > midPhaseLineLimit))
        {
            for (int i = 2; i < blockCount / 2 - 2; i++)
            {
                if (Random.Range(0.0f, 1.0f) > 0.75f)
                {
                    holes.Add(i);
                }
            }
        }
        Debug.Log("Holes = " + String.Join(",", holes));
        // creates an entire line (composed of one line and one subline)
        for (int j = 0; j < blockCount; j++)
        {
            // creates the line on top
            if (j < (blockCount / 2))
            {

                if (((rowNumber < 8) && (j < (5 + (rowNumber + 1) / 3) || (j > (blockCount / 2) - (6 + (rowNumber + 1) / 3)))) || ((rowNumber >= 8) && (j < 7) || (j > (blockCount / 2) - 8)))
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
            // creates the subline, under
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
            // holes handling :
            if (difficultyLevel > 1)
            {
                if (j < (blockCount / 2))
                {
                    index = j;
                }
                else
                {
                    index = j - (blockCount / 2);
                }
                if (holes.Contains(index))
                {
                    if (listOfGrounds[rowNumber][j].activeSelf)
                    {
                        listOfGrounds[rowNumber][j].SetActive(false);
                    }
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

    public void SpawnCloudLevel()
    {
        for (int i = 0; i < cloudLevelPlatforms.Count(); i++)
        {
            InstantiatePlatform(i, cloudLevelPlatforms[i]);
        }
    }

    private void InstantiatePlatform(int index, Tuple<float, HashSet<int>> platform)
    {
        bool hasPowerUp = Random.value < 0.5f;
        int blockToSpawnPowerUpOn = -1;
        int currentBlockNumber = 0;

        if (platform.Item2.Count() > 0)
        {
            if (hasPowerUp)
            {
                blockToSpawnPowerUpOn = Random.Range(0, platform.Item2.Count());
            }
            for (int j = 0; j < (blockCount / 2); j++)
            {
                if (platform.Item2.Contains(j))
                {
                    // Powerup spawning
                    if (hasPowerUp && (currentBlockNumber == blockToSpawnPowerUpOn))
                    {
                        Vector3 powerupSpawnPos = new Vector3(originBlockPosition.x + j * unbreakableBlockPrefabBounds.x, originBlockPosition.y + platform.Item1 + unbreakableBlockPrefabBounds.y * 1.5f, originBlockPosition.z);
                        Instantiate(powerupPrefab, powerupSpawnPos, powerupPrefab.transform.rotation);
                    }
                    // Platform block spawning
                    Vector3 spawnPos = new Vector3(originBlockPosition.x + j * unbreakableBlockPrefabBounds.x, originBlockPosition.y + platform.Item1, originBlockPosition.z);
                    GameObject unbrGameObject = Instantiate(unbreakableBlockPrefab, spawnPos, unbreakableBlockPrefab.transform.rotation);
                    unbrGameObject.GetComponentInChildren<GroundBehaviour>().SetLine(maxRowCount + index);
                    currentBlockNumber++;
                }
            }
        }
        else
        {
            Vector3 cloudPos = new Vector3(cloudPrefab.transform.position.x, originBlockPosition.y + platform.Item1, cloudPrefab.transform.position.z - 0.5f);
            SpawnCloudAtCoordinates(cloudPos, maxRowCount + index);
        }

        IncrementCurrentRowCount();
    }

    public void SpawnCloudAtCoordinates(Vector3 spawnPos, int lineCount)
    {
        GameObject cloudGameObject = Instantiate(cloudPrefab, spawnPos, cloudPrefab.transform.rotation);
        cloudGameObject.GetComponentInChildren<GroundBehaviour>().SetLine(lineCount);
        Instantiate(cloudParticle, new Vector3(transform.position.x, spawnPos.y, spawnPos.z), transform.rotation);
    }

    public void SpawnStar()
    {
        Vector3 spawnPos = new Vector3(starPrefab.transform.position.x, originBlockPosition.y + (maxRowCount + 14) * rowHeight - 7.25f, starPrefab.transform.position.z - 0.5f);
        Instantiate(starPrefab, spawnPos, starPrefab.transform.rotation);
    }
    public void AddRow()
    {
        // Hard and medium mode handling
        if ((currentRowCount % 2 == 1) && (difficultyLevel > 1) && (currentRowCount > midPhaseLineLimit) && (Random.Range(0.0f, 1.0f) > 0.75f))
        {
            Vector3 cloudPos = new Vector3(cloudPrefab.transform.position.x, originBlockPosition.y + currentRowCount * rowHeight, cloudPrefab.transform.position.z - 0.5f);
            SpawnCloudAtCoordinates(cloudPos, currentRowCount);
            listOfGrounds.Add(new List<GameObject>(blockCount));
        }
        else
        {
            CreateRow(currentRowCount);
        }

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

    public Vector3 GetIceFallingBounds()
    {
        return iceFallingPrefabBounds;
    }
    public int GetMaxRow()
    {
        return maxRowCount;
    }

    public int GetBlockCount()
    {
        return blockCount;
    }
    public List<List<GameObject>> GetListOfGrounds()
    {
        return listOfGrounds;
    }
    public List<GameObject> GetListOfGroundsLine(int line)
    {
        return listOfGrounds[line];
    }
    public GameObject GetBlock(int line, int blockNumber)
    {
        return listOfGrounds[line][blockNumber];
    }
    public int GetPlayerHP()
    {
        return playerHP;
    }
    public AudioSource GetCurrentAudioSource()
    {
        return currentInGameAudioSource;
    }
    public int GetMidPhaseLineLimit(){
        return midPhaseLineLimit;
    }
    public int GetHardPhaseLineLimit(){
        return hardPhaseLineLimit;
    }
    public bool GetCloudLevelPhase(){
        return cloudLevelPhase;
    }
    public List<Tuple<float, HashSet<int>>> GetCloudLevelPlatforms(){
        return cloudLevelPlatforms;
    }
    // complexity : O(1) source C# List documentation
    public int GetCloudLevelPlatformsCount(){
        return cloudLevelPlatforms.Count();
    }

    public bool GetVictoryState(){
        return victory;
    }
    public float GetVerticalLimitPosition(){
        return verticalLimitPosition;
    }
    public void SetVerticalLimitPositon(float value){
        verticalLimitPosition = value;
    }
}
