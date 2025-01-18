using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class UFOBehaviour : MonoBehaviour
{

    public int ufoColor = 0;
    public GameObject flyingBlock;
    private int movementPhase = 0;
    private int shootingStep = 0;
    private float ufoHeightMargin = 6.0f;
    private float ufoEnteringPhaseSpeed = 3.0f;
    private float xRange = 13.0f;
    private float laserScaleChangeSpeed = 20.0f;
    private int numberOfMovementsToDo;
    private int numberOfMovementsExecuted = 0;
    private float ufoLeftRightDestination;
    private float outLaserLimit = 7.0f;
    private float inLaserLimit = 5.0f;
    private bool ufoLeftOrRightMovementIsFinished = true;
    private bool blinkingIsFinished = false;
    private bool reachedHorizontalPosition = false;
    private bool hasStartedMoveToPos = false;
    private bool ufoIsActivated = false;
    private bool entersAudioHasBeenPlayed = false;
    private bool shootingAudioHasBeenPlayed = false;
    private bool leavingAudioHasBeenPlayed = false;
    private GameObject transparentLaser;
    private GameObject laser;
    private SpawnManager spawnManager;
    private Camera mainCamera;
    private AudioSource ufoAudioSource;
    public AudioClip entersSound;
    public AudioClip horizontalMoveSound;
    public AudioClip shootingSound;
    public AudioClip leavingSound;
    private float soundsVolume = 0.01f;



    private enum UFOColor
    {
        Red,
        Green,
        Blue
    }

    private enum Laser
    {
        Activating,
        Blinking,
        Growing,
        Executing,
        Shrinking,
        Departing
    }

    private enum UFO
    {
        Enters,
        MovingHorizontally,
        Shooting,
        Leaving
    }

    // Start is called before the first frame update
    void Start()
    {
        ufoAudioSource = GetComponent<AudioSource>();
        // numberOfMovementsToDo = Random.Range(3, 10);
        // ufoLeftRightDestination = Random.Range(-xRange, xRange);
        transparentLaser = transform.Find("Transparent_Laser").gameObject;
        laser = transform.Find("Laser").gameObject;
        spawnManager = GameObject.Find("Spawn Manager").GetComponent<SpawnManager>();
        // originalUFOTransformPosition = transform.position;
        mainCamera = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        if(ufoIsActivated){
            switch ((UFO)movementPhase)
            {
                // Phase where the UFO enters the screen
                case UFO.Enters:
                    UFOManageEnteringPhase();
                    break;
                // Phase where the UFO moves left and right above the player
                case UFO.MovingHorizontally:
                    UFOManageLeftRightPhase();
                    break;
                // Phase where the UFO shoots the place
                case UFO.Shooting:
                    UFOManageShootingPhase(ufoColor);
                    break;
                // Phase where the UFO exits the screen
                case UFO.Leaving:
                    UFOManageLeavingPhase();
                    break;
                default:
                    break;
            }
        }
        // Constraint UFO Y axis Movement
        UFOVerticalAxisConstraint();
        
    }
    public void DesactivateUFO(){
        gameObject.SetActive(false);
        ufoIsActivated = false;
    }
    public void StartUFO(){
        numberOfMovementsToDo = Random.Range(3, 10);
        ufoLeftRightDestination = Random.Range(-xRange, xRange);
        movementPhase = 0;
        shootingStep = 0;
        ufoLeftOrRightMovementIsFinished = true;
        blinkingIsFinished = false;
        reachedHorizontalPosition = false;
        hasStartedMoveToPos = false;
        entersAudioHasBeenPlayed = false;
        shootingAudioHasBeenPlayed = false;
        leavingAudioHasBeenPlayed = false;
        gameObject.SetActive(true);
        ufoIsActivated = true;
    }

    private void UFOVerticalAxisConstraint(){
        if(!Mathf.Approximately(transform.position.y - mainCamera.transform.position.y, ufoHeightMargin)){
            transform.position = new Vector3(transform.position.x, mainCamera.transform.position.y + ufoHeightMargin, transform.position.z);
        }
    }

    private void UFOManageEnteringPhase()
    {
        if(!entersAudioHasBeenPlayed){
            ufoAudioSource.PlayOneShot(entersSound, soundsVolume);
            entersAudioHasBeenPlayed = true;
        }
        
        Vector3 destinationPos = new Vector3(transform.position.x, transform.position.y, -1);
        Vector3 updatedPos = Vector3.MoveTowards(transform.position, destinationPos, ufoEnteringPhaseSpeed * Time.deltaTime * Vector3.Distance(destinationPos, transform.position));
        if (Vector3.Distance(destinationPos, updatedPos) >= 0.1f)
        {
            transform.position = updatedPos;
        }
        else
        {
            movementPhase++;
            StartCoroutine(PostEnteringPhaseCooldown());
        }
    }

    private void UFOManageLeavingPhase()
    {
        if(!leavingAudioHasBeenPlayed){
            leavingAudioHasBeenPlayed = true;
            ufoAudioSource.PlayOneShot(leavingSound, soundsVolume);
        }
        Vector3 destinationPos = new Vector3(mainCamera.transform.position.x, mainCamera.transform.position.y + ufoHeightMargin, mainCamera.transform.position.z);
        Vector3 updatedPos = Vector3.MoveTowards(transform.position, destinationPos, ufoEnteringPhaseSpeed * Time.deltaTime * Vector3.Distance(destinationPos, transform.position));
        if (Vector3.Distance(destinationPos, updatedPos) >= 0.1f)
        {
            transform.position = updatedPos;
        }
        else
        {
            movementPhase++;
            DesactivateUFO();
            spawnManager.SetUFOIsInUse(false);
        }
    }

    private void UFOManageLeftRightPhase()
    {
        if (!ufoLeftOrRightMovementIsFinished)
        {
            if (reachedHorizontalPosition && hasStartedMoveToPos)
            {
                reachedHorizontalPosition = false;
                hasStartedMoveToPos = false;
                ufoLeftRightDestination = Random.Range(-xRange, xRange);
                // Handling of minimum distance to travel (to avoid UFO almost not moving because of horizontal movement randomization)
                UFOManageMinHorizontalMovement();
                numberOfMovementsExecuted++;
                ufoLeftOrRightMovementIsFinished = true;
                if (numberOfMovementsExecuted == numberOfMovementsToDo)
                {
                    numberOfMovementsExecuted = 0;
                    movementPhase++;
                }
                else
                {
                    StartCoroutine(PostLeftOrRightMovementCooldown());
                }
            }
            else if (!hasStartedMoveToPos)
            {
                ufoAudioSource.PlayOneShot(horizontalMoveSound, soundsVolume);
                hasStartedMoveToPos = true;
                StartCoroutine(MoveToPosition(new Vector3(ufoLeftRightDestination, transform.position.y, -1), 0.5f));
            }
        }
    }

    private void UFOManageMinHorizontalMovement()
    {
        if (Mathf.Abs(ufoLeftRightDestination - transform.position.x) < 3.5f)
        {
            // if destination on the left side
            if (ufoLeftRightDestination < transform.position.x)
            {
                if (transform.position.x - 3.5f < -xRange)
                {
                    ufoLeftRightDestination = transform.position.x + 3.5f;
                }
                else
                {
                    ufoLeftRightDestination = transform.position.x - 3.5f;
                }
            }
            // if destination on the right side
            else
            {
                if (transform.position.x + 3.5f > xRange)
                {
                    ufoLeftRightDestination = transform.position.x - 3.5f;
                }
                else
                {
                    ufoLeftRightDestination = transform.position.x + 3.5f;
                }
            }
        }
    }
    private void UFOManageShootingPhase(int ufoColor)
    {
        // Do as such : Step 1 : activate the first laser (make it blink), make first laser grow, then activate second laser, make second laser grow with a certain speed
        //              Step 2 : Detect everything on path of the laser, make a list of objects collided with, destroy them and so, kill player if in path, make camera shake effect
        //              Step 3 : shrink the normal laser, then shrink the transparent laser. and desactivate them
        //              Step 4 : finished, UFO can go, we go to phase number 3

        switch ((Laser)shootingStep)
        {
            // Activate blinking of the laser
            case Laser.Activating:
                StartCoroutine(MakeGameObjectBlink(laser));
                shootingStep++;
                break;
            // Blinking currently active
            case Laser.Blinking:
                if (blinkingIsFinished)
                {
                    shootingStep++;
                }
                break;
            // Make laser growing animation here
            case Laser.Growing:
                ManageLaserGrowing();
                break;
            case Laser.Executing:
                // Make laser shooting consequences here
                switch ((UFOColor)ufoColor)
                {
                    case UFOColor.Red:
                        // Destroy stuff
                        ManageUFODestroying();
                        break;
                    case UFOColor.Green:
                        // Repair stuff
                        ManageUFORebuilding();
                        break;
                    case UFOColor.Blue:
                        // TP stuff
                        // ManageUFOTeleporting();
                        if(Random.Range(0, 2) % 2 == 0){
                            ManageUFODestroying();
                        }
                        else{
                            ManageUFORebuilding();
                        }
                        
                        break;
                    default:
                        break;
                }
                shootingStep++;
                break;
            // Make laser shrinking animation here
            case Laser.Shrinking:
                // When finished :
                ManageLaserShrinking();
                
                break;

        }


    }

    private void ManageLaserGrowing()
    {
        if(!shootingAudioHasBeenPlayed){
            ufoAudioSource.PlayOneShot(shootingSound, soundsVolume);
            shootingAudioHasBeenPlayed = true;
        }
        if(!transparentLaser.activeSelf){
            transparentLaser.SetActive(true);
        }
        if (transparentLaser.transform.localScale.x >= outLaserLimit && transparentLaser.transform.localScale.z >= outLaserLimit)
        {
            if(!laser.activeSelf){
                laser.SetActive(true);
            }
            if (laser.transform.localScale.x >= inLaserLimit && laser.transform.localScale.z >= inLaserLimit)
            {
                shootingStep++;
            }
            else
            {
                laser.transform.localScale += new Vector3(laserScaleChangeSpeed, 0, laserScaleChangeSpeed) * Time.deltaTime;
            }
        }
        else
        {
            transparentLaser.transform.localScale += new Vector3(laserScaleChangeSpeed, 0, laserScaleChangeSpeed) * Time.deltaTime;
        }
    }

    private void ManageLaserShrinking()
    {
        if (laser.transform.localScale.x <= 0.1f && laser.transform.localScale.z <= 0.1f)
        {
            if(laser.activeSelf){
                laser.SetActive(false);
            }
            if (transparentLaser.transform.localScale.x <= 0.1f && transparentLaser.transform.localScale.z <= 0.1f)
            {
                if(transparentLaser.activeSelf){
                    transparentLaser.SetActive(false);
                }
                movementPhase++;
            }
            else
            {
                transparentLaser.transform.localScale -= new Vector3(laserScaleChangeSpeed, 0, laserScaleChangeSpeed) * Time.deltaTime;
            }
        }
        else
        {
            laser.transform.localScale -= new Vector3(laserScaleChangeSpeed, 0, laserScaleChangeSpeed) * Time.deltaTime;
        }
    }

    private void ManageUFODestroying()
    {
        // Get the position of laser, 8.5 being its diameter we can get the zone being between laser.transform.position.x - 8.5 / 2, laser.transform.position.x + 8.5 / 2
        // we search in list of grounds between spawnManager.GetCurrentRowCount() and spawnManager.GetCurrentRowCount() - 4 if position of block match, and we destroy

        if(spawnManager.GetCurrentRowCount() - 1 < spawnManager.GetMaxRow() - 1){
            PlayerController player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
            GameObject[] chickens = GameObject.FindGameObjectsWithTag("Chicken");

            // Destruction of Blocks from the UFO
            for (int i = spawnManager.GetCurrentRowCount() - 5; i < spawnManager.GetCurrentRowCount() - 1; i++)
            {
                spawnManager.GetListOfGroundsLine(i).ForEach(block =>
                {
                    if (IsInsideLaserZone(block) && block.activeSelf)
                    {
                        DestroyBlock(block);
                    }
                });
            }
            // Destruction of Player from the UFO
            if (IsInsideLaserZone(player.gameObject))
            {
                player.ManagePlayerDeath(PlayerController.DeathCause.UFOLaser);
                if(player.GetCollisionCounter() > 0){
                    player.DecreaseCollisionCounter();
                }
            }


            // Destruction of the chicken by the UFO
            foreach (GameObject chicken in chickens)
            {
                if (IsInsideLaserZone(chicken))
                {
                    Destroy(chicken);
                }
            }
}


    }
    private void ManageUFORebuilding()
    {
        // Get the position of laser, 8.5 being its diameter we can get the zone being between laser.transform.position.x - 8.5 / 2, laser.transform.position.x + 8.5 / 2
        // we search in list of grounds between spawnManager.GetCurrentRowCount() and spawnManager.GetCurrentRowCount() - 4 if position of block match, and we rebuild
        if(spawnManager.GetCurrentRowCount() - 1 < spawnManager.GetMaxRow() - 1){

            for (int i = spawnManager.GetCurrentRowCount() - 5; i <= spawnManager.GetCurrentRowCount() - 1; i++)
            {
                spawnManager.GetListOfGroundsLine(i).ForEach(block =>
                {
                    if (IsInsideLaserZone(block) && !block.activeSelf)
                    {
                        RebuildBlock(block);
                    }
                });
            }
        }
    }

    private bool IsInsideLaserZone(GameObject element)
    {
        if ((element.transform.position.x >= laser.transform.position.x - inLaserLimit / 2) && (element.transform.position.x <= laser.transform.position.x + inLaserLimit / 2))
        {
            return true;
        }
        return false;
    }

    private void DestroyBlock(GameObject block)
    {
        ManageSendingBlockFlying(block);
        block.SetActive(false);
    }

    private void RebuildBlock(GameObject block)
    {
        if(!block.CompareTag("Ground")){
            block.SetActive(true);
        }
    }
    private IEnumerator PostEnteringPhaseCooldown()
    {
        yield return new WaitForSeconds(1.5f);
        ufoLeftOrRightMovementIsFinished = false;
    }

    private IEnumerator PostLeftOrRightMovementCooldown()
    {
        yield return new WaitForSeconds(Random.Range(1, 3));
        ufoLeftOrRightMovementIsFinished = false;
    }

    private IEnumerator MakeGameObjectBlink(GameObject element)
    {
        int i = 0;
        while (i < 5)
        {
            element.SetActive(true);
            yield return new WaitForSeconds(0.1f);
            element.SetActive(false);
            yield return new WaitForSeconds(0.1f);
            i++;
        }
        blinkingIsFinished = true;
    }

    private void ManageSendingBlockFlying(GameObject block)
    {
        GameObject flyingBlockGameObject = Instantiate(flyingBlock, block.transform.position, block.transform.rotation);
        GameObject fbChild = flyingBlockGameObject.transform.GetChild(0).gameObject;
        MeshRenderer fbRenderer = fbChild.GetComponent<MeshRenderer>();
        Mesh mesh = block.GetComponentInChildren<MeshFilter>().sharedMesh;

        fbChild.transform.localScale = block.transform.localScale;
        fbChild.GetComponent<MeshFilter>().sharedMesh = Instantiate(mesh); // we assign a copy of the mesh to new meshfilter
        fbRenderer.sharedMaterial = block.GetComponentInChildren<MeshRenderer>().sharedMaterial;
    }

    public IEnumerator MoveToPosition(Vector3 destination, float timeToMove)
    {
        var currentPos = transform.position;
        var t = 0f;
        while (t < 1)
        {
            t += Time.deltaTime / timeToMove;
            transform.position = Vector3.Lerp(currentPos, destination, t);
            UFOVerticalAxisConstraint();
            yield return null;
        }
        reachedHorizontalPosition = true;
    }

    public float GetUFOHeightMargin(){
        return ufoHeightMargin;
    }
}
