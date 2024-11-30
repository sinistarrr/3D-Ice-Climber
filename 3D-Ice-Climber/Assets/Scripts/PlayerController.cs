
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public ParticleSystem blockExplosionParticle;
    public ParticleSystem chickenExplosionParticle;
    public ParticleSystem groundLandingParticle;
    public ParticleSystem playerDirtParticle;
    public ParticleSystem collectibleExplodeParticle;
    public ParticleSystem starParticle;
    public float horizontalInput, verticalInput, jumpInput, fireInput;
    private float speed = 3.0f;
    public float jumpForce = 14.0f;
    private float enemyPushForce = 20.0f;
    private float xBound = 16.95f;
    private bool isJumping = false;
    private bool isOnGround = false;
    private bool jumpingKeyIsReleased = true;
    private bool isMovingRight = true;
    private bool isMovingHorizontally = false;
    private SpawnManager spawnManager;
    private GameObject planeLimit;
    private GameObject savedCloud;
    public GameObject flyingBlock;
    private Camera gameCamera;
    private Animator playerAnim;
    private Rigidbody playerRb;
    private AudioSource playerAudio;
    public AudioClip jumpSound;
    public AudioClip deathSound;
    public AudioClip collectibleSound;
    public AudioClip victorySound;
    public List<AudioClip> blockOnDestroySound;
    private bool isCrouching = false;
    public bool isFiring = false;
    private bool gameOver = false;
    private bool playerIsDead = false;
    private bool jumpCooldownElapsed = false;
    private bool collidingWithGround = false;
    private int collisions = 0;
    private int playerLine = 0;
    private float cameraMoveDistance = 0;
    private float cameraSpeed = 6f;
    private Vector3 initialCameraPosition;
    private Vector3 initialLimitPosition;
    private float rowHeight;
    private bool cloudLevelStateActivated = false;
    private bool isOnCloud = false;
    private bool playerReachedFourthStage = false;
    private int playerHP;
    private static System.Random rng = new System.Random();


    // Start is called before the first frame update
    void Start()
    {
        playerRb = GetComponent<Rigidbody>();
        playerAnim = GetComponentInChildren<Animator>();
        playerAudio = GetComponent<AudioSource>();
        gameCamera = Camera.main;
        spawnManager = GameObject.Find("Spawn Manager").GetComponent<SpawnManager>();
        playerHP = spawnManager.GetPlayerHP();
        rowHeight = spawnManager.GetRowHeight();
        planeLimit = GameObject.Find("PlaneLimit");
        initialLimitPosition = planeLimit.transform.position;
        initialCameraPosition = gameCamera.transform.position;

    }

    // Update is called once per frame
    void Update()
    {
        if (!playerIsDead)
        {
            // At each frame we check if the player is jumping, the user jump input, all depending whether 
            // the player is on the ground or not
            PlayerCheckJumpingState();
            // provides horizontal movement to the player at each frame
            MovePlayerHorizontally();
            // locks player controls if in specific state like jumping
            LockPlayerControls();
            ManageCameraAndLimitPosition();
        }
        ConstraintPlayerPosition();

    }

    void FixedUpdate()
    {
        ManageJumping();
        CheckIfPlayerIsOnTheGround();
        AddHorizontalMovementIfPlayerIsOnCloud();
    }

    private void OnCollisionEnter(Collision collision)
    {
        IncrementCollisionCounter();

        if (!playerIsDead)
        {
            ManageGroundCollisionsWithPlayer(collision);
            ManageChickenCollisionWithPlayer(collision);
            ManageCollectibleCollisionsWithPlayer(collision);
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        GameObject parentOfGameObjectCollidedWith = collision.transform.root.gameObject;
        DecreaseCollisionCounter();
        if (parentOfGameObjectCollidedWith.CompareTag("Cloud"))
        {
            isOnCloud = false;
        }
    }

    private void ManageCollectibleCollisionsWithPlayer(Collision collision)
    {
        GameObject parentOfGameObjectCollidedWith = collision.transform.root.gameObject;

        if (parentOfGameObjectCollidedWith.CompareTag("Collectible") || parentOfGameObjectCollidedWith.CompareTag("Star"))
        {
            DecreaseCollisionCounter();
            if (parentOfGameObjectCollidedWith.CompareTag("Collectible"))
            {
                playerAudio.PlayOneShot(collectibleSound, 1.0f);
            }
            if (parentOfGameObjectCollidedWith.CompareTag("Star"))
            {
                gameCamera.GetComponent<AudioSource>().Stop();
                playerAudio.PlayOneShot(victorySound, 1.0f);
            }

        }
    }
    private void ManageGroundCollisionsWithPlayer(Collision collision)
    {
        // Never returns null because of "root"
        GameObject parentOfGameObjectCollidedWith = collision.transform.root.gameObject;

        if (parentOfGameObjectCollidedWith.CompareTag("Ground Breakable") || parentOfGameObjectCollidedWith.CompareTag("Ground Breakable Small")
                || parentOfGameObjectCollidedWith.CompareTag("Ground") || parentOfGameObjectCollidedWith.CompareTag("Cloud"))
        {

            if (parentOfGameObjectCollidedWith.CompareTag("Ground Breakable") || parentOfGameObjectCollidedWith.CompareTag("Ground Breakable Small"))
            {
                ManageBlockDestructionByPlayer(parentOfGameObjectCollidedWith, collision);
            }
            ManageGameBehaviourWhenPlayerOnTheGround(parentOfGameObjectCollidedWith, collision);
        }
    }

    private void IncrementCollisionCounter()
    {
        collisions++;
    }

    private void DecreaseCollisionCounter()
    {
        collisions--;
    }

    private void ManageJumping()
    {
        // if jumping is detected and player is on the ground, the player goes up on the Y-Axis with a force
        if (isJumping && isOnGround)
        {
            Jump();
            isJumping = false;
            StartCoroutine(JumpCooldown());
            // we slow down the player by 50% when jumping
            horizontalInput /= 2;
            playerDirtParticle.Stop();
            playerAudio.Stop();
            playerAudio.PlayOneShot(jumpSound, 1.0f);
        }
    }
    private void CheckIfPlayerIsOnTheGround()
    {
        if (collisions == 0 || (collisions > 0 && !collidingWithGround))
        {
            isOnGround = false;
        }
        else
        {
            isOnGround = true;
        }
    }
    private void ManageBlockDestructionByPlayer(GameObject parentOfBlockGameObject, Collision collision)
    {
        // Destruction of Block
        if ((Math.Abs(collision.contacts[0].point.y - (transform.position.y + GetComponent<Collider>().bounds.size.y)) <= 0.05f) && (Math.Round(collision.contacts[0].point.y, 2) == Math.Round(parentOfBlockGameObject.transform.position.y - collision.gameObject.GetComponent<Collider>().bounds.size.y / 2, 2)))
        {
            ManageSendingBlockFlying(parentOfBlockGameObject);
            parentOfBlockGameObject.SetActive(false);
            playerAnim.enabled = false;
            DecreaseCollisionCounter();
            spawnManager.UpdateScore(5);
            blockExplosionParticle.Play();
            playerAudio.PlayOneShot(blockOnDestroySound.ElementAt(rng.Next(blockOnDestroySound.Count())), 1.0f); // plays random sound in the list
            if (parentOfBlockGameObject.CompareTag("Ground Breakable"))
            {
                GroundBehaviour brkGroundScript = parentOfBlockGameObject.GetComponentInChildren<GroundBehaviour>();
                if (brkGroundScript.IsCollidingWithChicken())
                {
                    brkGroundScript.GetLastChickenCollidedWith().GetComponent<ChickenBehaviour>().CollisionUpdateOnDestroyedGround();
                    brkGroundScript.GetLastChickenCollidedWith().GetComponent<ChickenBehaviour>().SetDeathActivation(true);
                    brkGroundScript.GetLastChickenCollidedWith().GetComponent<ChickenBehaviour>().MakeChickenFall();
                }
            }
        }
    }

    private void ManageSendingBlockFlying(GameObject parentOfBlockGameObject)
    {
        GameObject flyingBlockGameObject = Instantiate(flyingBlock, parentOfBlockGameObject.transform.position, parentOfBlockGameObject.transform.rotation);
        GameObject fbChild = flyingBlockGameObject.transform.GetChild(0).gameObject;
        MeshRenderer fbRenderer = fbChild.GetComponent<MeshRenderer>();
        Mesh mesh = parentOfBlockGameObject.GetComponentInChildren<MeshFilter>().sharedMesh;

        fbChild.transform.localScale = parentOfBlockGameObject.transform.localScale;
        fbChild.GetComponent<MeshFilter>().sharedMesh = Instantiate(mesh); // we assign a copy of the mesh to new meshfilter
        fbRenderer.sharedMaterial = parentOfBlockGameObject.GetComponentInChildren<MeshRenderer>().sharedMaterial;
        
        // GameObject go = new GameObject();
        // MeshRenderer newmeshrenderer = go.AddComponent<MeshRenderer>();
        // // Do stuff with newmeshrenderer, eg...
        // newmeshrenderer.sharedMaterial = theAwesomeMaterial;
        // newmeshrenderer.shadowCastingMode = ShadowCastingMode.off;
        // // etc.


    }

    private void ManageGameBehaviourWhenPlayerOnTheGround(GameObject parentOfBlockGameObject, Collision collision)
    {
        if (collision.contacts[0].point.y > parentOfBlockGameObject.transform.position.y)
        {
            collidingWithGround = true;
            Debug.Log("Line of the block is : " + parentOfBlockGameObject.GetComponentInChildren<GroundBehaviour>().GetLine());
            CheckIfPlayerIsOnCloud(parentOfBlockGameObject);
            ManagePlayerAnimationAfterHasHitBlock();
            int groundLine = parentOfBlockGameObject.GetComponentInChildren<GroundBehaviour>().GetLine();
            UpdateGameWhenPlayerOnANewLine(groundLine);
            groundLandingParticle.Play();
        }
        else
        {
            collidingWithGround = false;
        }
    }

    private void UpdateGameWhenPlayerOnANewLine(int groundLine)
    {
        if (groundLine > playerLine)
        {
            playerLine = groundLine;
            spawnManager.UpdateFloor(playerLine);
            spawnManager.UpdateScore(20);
            if (!cloudLevelStateActivated)
            {
                if (playerLine > spawnManager.GetMaxRow() - 3)
                {
                    cloudLevelStateActivated = true;
                    // spawnManager.SpawnCloudLevelFirstStage();
                    spawnManager.SpawnCloudLevel();
                    spawnManager.SpawnMountainBorders();
                    cameraMoveDistance = 1;
                }
                else if (playerLine > spawnManager.GetCurrentMiddleRow())
                {
                    spawnManager.AddRow();
                    cameraMoveDistance = 1;
                }
            }
            else
            {
                if (playerLine == spawnManager.GetMaxRow() - 1)
                {
                    cameraMoveDistance = 1;
                }
                else if (playerLine == spawnManager.GetMaxRow() + 1)
                {
                    cameraMoveDistance = 3;
                }
                else if (!playerReachedFourthStage && (playerLine == (spawnManager.GetMaxRow() + 6) || playerLine == (spawnManager.GetMaxRow() + 5)))
                {
                    playerReachedFourthStage = true;
                    cameraMoveDistance = 3;
                }
                else if (playerLine == spawnManager.GetMaxRow() + 8)
                {
                    cameraMoveDistance = 2.5f;
                }
                else if (playerLine == spawnManager.GetMaxRow() + 13)
                {
                    cameraMoveDistance = 3.0f;
                    spawnManager.SpawnStar();
                    SetJumpForce(22.0f);
                }
            }
        }
    }
    private void CheckIfPlayerIsOnCloud(GameObject parentOfBlockGameObject)
    {
        if (parentOfBlockGameObject.CompareTag("Cloud"))
        {
            isOnCloud = true;
            savedCloud = parentOfBlockGameObject;
        }
    }
    private void ManageChickenCollisionWithPlayer(Collision collision)
    {
        if (collision.gameObject.CompareTag("Chicken") || collision.gameObject.CompareTag("Falling Ice"))
        {

            // If the object we hit is the enemy
            // Calculate Angle Between the collision point and the player
            Vector3 dir = collision.contacts[0].point - transform.position;
            // We then get the opposite (-Vector3) and normalize it
            dir = -dir.normalized;
            // And finally we add force in the direction of dir and multiply it by force. 
            // This will push back the player
            playerRb.AddForce(dir * enemyPushForce, ForceMode.Impulse);
            ManagePlayerDeath(false);
            chickenExplosionParticle.Play();

        }
    }
    public void ManagePlayerDeath(bool diedFalling)
    {
        SetPlayerIsDead(true);
        UpdateIfGameIsOver();
        DecreasePlayerHP();
        ManageRespawningOfPlayer(diedFalling);
        playerDirtParticle.Stop();
        playerAudio.Stop();
        playerAudio.PlayOneShot(deathSound, 1.0f);
        if (!playerAnim.isActiveAndEnabled)
        {
            playerAnim.enabled = true;
        }
        ManageDeathAnimation();
        if (diedFalling)
        {
            StartCoroutine(FreezePlayerOnFall());
        }
    }
    private IEnumerator FreezePlayerOnFall()
    {
        yield return new WaitForSeconds(0.2f);
        playerRb.constraints = RigidbodyConstraints.FreezeAll;

    }
    private void ManageRespawningOfPlayer(bool diedFalling)
    {
        if (!gameOver)
        {
            StartCoroutine(RespawnPlayer(diedFalling));
        }
    }

    private IEnumerator RespawnPlayer(bool diedFalling)
    {
        yield return new WaitForSeconds(3.0f);
        RespawnPlayerOnLine(playerLine);
        SetPlayerIsDead(false);
        ResetPlayerAnimation();
        if (diedFalling)
        {
            playerRb.constraints = RigidbodyConstraints.None;
            playerRb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionZ;
        }

    }

    private void RespawnPlayerOnLine(int line)
    {
        // If Player in Cloud Level
        if (line > spawnManager.GetMaxRow() - 1)
        {
            Vector3 ogBlockPos = spawnManager.originBlockPosition;
            Vector3 unbrBlockPrefabBounds = spawnManager.unbreakableBlockPrefabBounds;
            Tuple<float, HashSet<int>> platform = spawnManager.cloudLevelPlatforms[line - spawnManager.GetMaxRow()];
            List<int> cloudLevelGround;
            float height;

            // If platform is a cloud, we get the one below.
            if (platform.Item2.Count() == 0)
            {
                platform = spawnManager.cloudLevelPlatforms[line - spawnManager.GetMaxRow() - 1];
            }
            cloudLevelGround = platform.Item2.ToList();
            height = platform.Item1;

            cloudLevelGround.OrderBy(_ => rng.Next());
            int blockNbToSpawnPlayerOn = cloudLevelGround.Find(block => IsPositionAvailable(new Vector3(ogBlockPos.x + block * unbrBlockPrefabBounds.x, ogBlockPos.y + height, ogBlockPos.z)));
            RespawnPlayerOnCloudLevelBlock(platform, unbrBlockPrefabBounds, ogBlockPos, blockNbToSpawnPlayerOn);
        }
        else
        {

            // Get a list from script spawnManager that corresponds to specific line that player is on
            // GetRange makes us get only the half of it from 0 to half of max number of blocks on a block line.
            // OrderBy shuffles the result randomly and stores it in a list with ToList()
            // the time complexity of the sort operation stays the typical for QuickSort O(N*logN) average / O(N2) worst case.
            List<Tuple<int, GameObject>> ground = new List<Tuple<int, GameObject>>();
            List<GameObject> spawnManagerGround = spawnManager.GetListOfGroundsLine(line).GetRange(0, spawnManager.GetBlockCount() / 2 - 1);
            for (int i = 0; i < spawnManagerGround.Count(); i++)
            {
                ground.Add(new Tuple<int, GameObject>(i, spawnManagerGround[i]));
            }
            ground = ground.OrderBy(_ => rng.Next()).ToList();
            int blockNbToSpawnPlayerOn = ground.Find(block => IsBlockAvailable(block.Item2)).Item1;
            RespawnPlayerOnBlock(line, blockNbToSpawnPlayerOn);
        }

    }

    private void RespawnPlayerOnBlock(int line, int blockNb)
    {
        GameObject blockParent = spawnManager.GetBlock(line, blockNb).transform.root.gameObject;
        float yPos = blockParent.transform.position.y + blockParent.GetComponentInChildren<Collider>().bounds.size.y / 2;

        transform.position = new Vector3(blockParent.transform.position.x, yPos, transform.position.z);
    }
    private void RespawnPlayerOnCloudLevelBlock(Tuple<float, HashSet<int>> platform, Vector3 unbrBlockPrefabBounds, Vector3 ogBlockPos, int blockNb)
    {
        transform.position = new Vector3(ogBlockPos.x + unbrBlockPrefabBounds.x * blockNb, platform.Item1 + ogBlockPos.y + unbrBlockPrefabBounds.y / 2, transform.position.z);
    }

    private bool IsBlockAvailable(GameObject block)
    {
        return block.activeSelf && IsPositionAvailable(block.transform.position);
    }
    private bool IsPositionAvailable(Vector3 blockPosition)
    {
        // For each block on the line, do the following :
        Collider[] hitColliders = Physics.OverlapSphere(blockPosition, spawnManager.unbreakableBlockPrefabBounds.x * 2);
        // If the block is free to spawn a player on, we spawn it.
        if (!hitColliders.Any(collider => IsColliderAChickenOrOnTop(blockPosition, collider)))
        {
            return true;
        }
        return false;
    }
    private bool IsColliderAChickenOrOnTop(Vector3 blockPosition, Collider collider)
    {
        GameObject colliderParent = collider.transform.root.gameObject;
        //return colliderParent.CompareTag("Chicken");
        if (colliderParent.CompareTag("Chicken") || ((colliderParent.transform.position.y - blockPosition.y) >= 0.1f && Mathf.Approximately(colliderParent.transform.position.x, blockPosition.x)))
        {
            return true;
        }
        return false;
    }


    private void DecreasePlayerHP()
    {
        playerHP--;
        spawnManager.UpdateHP(-1);
    }
    private void UpdateIfGameIsOver()
    {
        if (playerHP <= 0)
        {
            gameOver = true;
            spawnManager.ActivateGameOverText();
            spawnManager.ActivateGameOverState();
            spawnManager.ActivateRestartButton();
        }
    }
    private void SetPlayerIsDead(bool value)
    {
        playerIsDead = value;
    }
    // Player jumping state control
    private void PlayerCheckJumpingState()
    {
        // if the player is not already jumping, is on the ground, released the jump key, and is pressing jump
        if (!isJumping && isOnGround && jumpingKeyIsReleased && jumpInput > 0)
        {
            isJumping = true;
            jumpingKeyIsReleased = false;
            ManageJumpingAnimation();
        }
        // verification of whether the player has released the jump key or not to avoid jumping in loop
        else if (!jumpingKeyIsReleased && jumpInput == 0 && isOnGround)
        {
            jumpingKeyIsReleased = true;
        }
    }

    private void Jump()
    {
        playerRb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    // reads input of the player on X axis, Y axis and jump input for both keyboard and controller
    private void InputAxisReadings()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        fireInput = Input.GetAxisRaw("Fire1");
        FireInputAnimation();
        HorizontalDirectionInversion();
        ManageHorizontalAnimation();
        verticalInput = Input.GetAxisRaw("Vertical");
        ManageCrouchingAnimation();
        jumpInput = Input.GetAxisRaw("Jump");
    }

    // provides horizontal movement to the player
    private void MovePlayerHorizontally()
    {
        if (!isCrouching)
        {
            playerRb.MovePosition(transform.position + horizontalInput * speed * Time.fixedDeltaTime * Vector3.right);
        }
    }

    // locks some of the controls of the player when the player isn't on the ground (we stop reading the inputs)
    // unless he is in not on the ground, standing still, and descending towards the ground.
    private void LockPlayerControls()
    {
        if (isOnGround || (!isOnGround && (horizontalInput == 0) && (playerRb.velocity.y <= 0)))
        {
            InputAxisReadings();
        }
    }

    // X axis bound checking to prevent player from going out of bounds
    private void ConstraintPlayerPosition()
    {
        if (transform.position.x < -xBound)
        {
            transform.position = new Vector3(xBound, transform.position.y, transform.position.z);
        }
        else if (transform.position.x > xBound)
        {
            transform.position = new Vector3(-xBound, transform.position.y, transform.position.z);
        }
    }

    private void HorizontalDirectionInversion()
    {
        if (((horizontalInput < 0 && isMovingRight) || (horizontalInput > 0 && !isMovingRight)) && !isCrouching)
        {
            InversePlayerRotation();
        }
    }

    private void ManageHorizontalAnimation()
    {
        if ((horizontalInput == 0 && isMovingHorizontally) || isCrouching)
        {
            playerAnim.SetFloat("Speed_f", 0);
            isMovingHorizontally = false;
            playerDirtParticle.Stop();
            playerAudio.Stop();
        }
        else if (horizontalInput != 0 && !isMovingHorizontally)
        {
            playerAnim.SetBool("Static_b", true);
            playerAnim.SetFloat("Speed_f", 0.75f);
            isMovingHorizontally = true;
            playerDirtParticle.Play();
            playerAudio.Play();
        }

    }

    private void FireInputAnimation()
    {
        if (fireInput != 0 && !isFiring)
        {
            playerAnim.SetInteger("WeaponType_int", 10);
            isFiring = true;
            StartCoroutine(FiringCooldown());
        }
    }

    private void ManageJumpingAnimation()
    {
        playerAnim.SetTrigger("Jump_trig");
    }

    private void ManageCrouchingAnimation()
    {
        if (verticalInput != 0 && !isCrouching && !isJumping)
        {
            playerAnim.SetBool("Crouch_b", true);
            isCrouching = true;
        }
        else if (verticalInput == 0 && isCrouching)
        {
            playerAnim.SetBool("Crouch_b", false);
            isCrouching = false;
        }
    }

    private void InversePlayerRotation()
    {
        isMovingRight = !isMovingRight;
        transform.rotation = Quaternion.Inverse(transform.rotation);
    }

    private void ManageDeathAnimation()
    {
        playerAnim.SetBool("Death_b", true);
        playerAnim.SetInteger("DeathType_int", 2);
    }

    private void ManageCameraAndLimitPosition()
    {
        if (cameraMoveDistance > 0)
        {
            UpdateCameraAndLimitPosition();
        }
    }

    private void UpdateCameraAndLimitPosition()
    {
        Vector3 destinationPos = initialCameraPosition + Vector3.up * rowHeight * cameraMoveDistance;
        Vector3 destinationLimitPos = initialLimitPosition + Vector3.up * rowHeight * cameraMoveDistance;
        Vector3 updatedPos = Vector3.MoveTowards(gameCamera.transform.position, destinationPos, cameraSpeed * Time.deltaTime * cameraMoveDistance);
        Vector3 updatedLimitPos = Vector3.MoveTowards(planeLimit.transform.position, destinationLimitPos, cameraSpeed * Time.deltaTime * cameraMoveDistance);
        if (destinationPos == updatedPos)
        {
            initialCameraPosition = destinationPos;
            initialLimitPosition = destinationLimitPos;
            cameraMoveDistance = 0;
        }

        planeLimit.transform.position = updatedLimitPos;
        gameCamera.transform.position = updatedPos;
    }

    private IEnumerator FiringCooldown()
    {
        yield return new WaitForSeconds(0.6f);
        playerAnim.SetInteger("WeaponType_int", 0);
        isFiring = false;
    }

    private IEnumerator JumpCooldown()
    {
        yield return new WaitForSeconds(0.2f);
        jumpCooldownElapsed = false;

    }

    private void MovePlayerAlongSideCloud(GameObject cloudPrefab)
    {
        CloudBehaviour cloudScript = cloudPrefab.GetComponent<CloudBehaviour>();

        if (cloudScript.IsMovingRight())
        {
            transform.Translate(Vector3.right * cloudScript.GetSpeed() * Time.deltaTime, Space.World);
        }
        else
        {
            transform.Translate(-Vector3.right * cloudScript.GetSpeed() * Time.deltaTime, Space.World);
        }

    }

    private void ManagePlayerAnimationAfterHasHitBlock()
    {
        if (!jumpCooldownElapsed)
        {
            jumpCooldownElapsed = true;
            ResetPlayerAnimation();
        }
    }

    private void ResetPlayerAnimation()
    {
        playerAnim.enabled = true;
        playerAnim.Rebind();
        playerAnim.Update(0f);
        isMovingHorizontally = false;
        ManageHorizontalAnimation();
    }
    private void AddHorizontalMovementIfPlayerIsOnCloud()
    {
        if (isOnCloud)
        {
            MovePlayerAlongSideCloud(savedCloud);
        }
    }

    private void SetJumpForce(float newForce)
    {
        jumpForce = newForce;
    }

}
