using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float horizontalInput, verticalInput, jumpInput, fireInput;
    public float speed = 5.0f;
    public float jumpForce = 12.0f;
    private float enemyPushForce = 20.0f;
    private float xBound = 11.0f; 
    private bool isJumping = false;
    private bool isOnGround = false;
    private bool jumpingKeyIsReleased = true;
    private bool isMovingRight = true;
    private bool isMovingHorizontally = false;
    private Animator playerAnim;
    private Rigidbody playerRb;
    private bool isCrouching = false;
    public bool isFiring = false;
    private bool gameOver = false;

    // Start is called before the first frame update
    void Start()
    {
        playerRb = GetComponent<Rigidbody>();
        playerAnim = GetComponentInChildren<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if(!gameOver){
            // At each frame we check if the player is jumping, the user jump input, all depending whether 
            // the player is on the ground or not
            PlayerCheckJumpingState();
            // provides horizontal movement to the player at each frame
            MovePlayerHorizontally();
            // locks player controls if in specific state like jumping
            LockPlayerControls();
        }
        ConstraintPlayerPosition();
        
    }

    void FixedUpdate(){
        // if jumping is detected and player is on the ground, the player goes up on the Y-Axis with a force
        if(isJumping && isOnGround){
            Jump();
            isJumping = false;
            isOnGround = false;
            // we slow down the player by 50% when jumping
            horizontalInput /= 2;
        }
    }

    private void OnCollisionEnter(Collision collision){
        if(collision.gameObject.CompareTag("Ground")){
            // Destruction of Block
            if(collision.contacts[0].point.y <= (collision.gameObject.transform.position.y - collision.gameObject.GetComponent<Collider>().bounds.size.y / 2) ){
                // Debug.Log("Position of Player is : " + transform.position );
                // Debug.Log("Position of Object is : " + collision.gameObject.transform.position );
                // Debug.Log("LocalScale of Object is : " + collision.gameObject.transform.localScale );
                // Debug.Log("LocalScale of Player is : " + transform.localScale );
                // Debug.Log("Position of collision is : " + collision.contacts[0].point);
                // Debug.Log("First substraction : " + (collision.gameObject.transform.position.y - collision.gameObject.GetComponent<Collider>().bounds.size.y / 2) );
                // Debug.Log("Second substraction : " + (transform.position.y + GetComponent<Collider>().bounds.size.y / 2) );
                // Debug.Log("Height of player is : " + GetComponent<Collider>().bounds.size.y);
                // Debug.Log("Height of cube is : " + collision.gameObject.GetComponent<Collider>().bounds.size.y);
                Destroy(collision.gameObject);
            }
            else if(collision.contacts[0].point.y >= (collision.gameObject.transform.position.y + collision.gameObject.GetComponent<Collider>().bounds.size.y / 2)){
                isOnGround = true;
            }
        }
        if(collision.gameObject.CompareTag("Chicken")){

            // If the object we hit is the enemy
            // Calculate Angle Between the collision point and the player
            Vector3 dir = collision.contacts[0].point - transform.position;
            // We then get the opposite (-Vector3) and normalize it
            dir = -dir.normalized;
            // And finally we add force in the direction of dir and multiply it by force. 
            // This will push back the player
            GetComponent<Rigidbody>().AddForce(dir * enemyPushForce, ForceMode.Impulse);
            gameOver = true;
            ManageDeathAnimation();
        }
    }

    // Player jumping state control
    private void PlayerCheckJumpingState(){
        // if the player is not already jumping, is on the ground, released the jump key, and is pressing jump
        if(!isJumping && isOnGround && jumpingKeyIsReleased && jumpInput > 0){
            isJumping = true;
            jumpingKeyIsReleased = false;
            ManageJumpingAnimation();
        }
        // verification of whether the player has released the jump key or not to avoid jumping in loop
        else if(!jumpingKeyIsReleased && jumpInput == 0 && isOnGround){
            jumpingKeyIsReleased = true;
        }
    }

    private void Jump(){
        playerRb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    // reads input of the player on X axis, Y axis and jump input for both keyboard and controller
    private void InputAxisReadings(){
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
    private void MovePlayerHorizontally(){
        if(!isCrouching){
            transform.Translate(Vector3.right * horizontalInput * speed * Time.deltaTime, Space.World);
        }
    }

    // locks some of the controls of the player when the player isn't on the ground (we stop reading the inputs)
    // unless he is in not on the ground, standing still, and descending towards the ground.
    private void LockPlayerControls(){
        if(isOnGround || (!isOnGround && (horizontalInput == 0) && (playerRb.velocity.y <= 0))){
            InputAxisReadings();
        }
    }

    // X axis bound checking to prevent player from going out of bounds
    private void ConstraintPlayerPosition(){
        if(transform.position.x < -xBound){
            transform.position = new Vector3(xBound, transform.position.y, transform.position.z);
        }
        else if(transform.position.x > xBound){
            transform.position = new Vector3(-xBound, transform.position.y, transform.position.z);
        }
    }

    private void HorizontalDirectionInversion(){
        if(((horizontalInput < 0 && isMovingRight) || (horizontalInput > 0 && !isMovingRight)) && !isCrouching){
            InversePlayerRotation();
        }
    }

    private void ManageHorizontalAnimation(){
        if((horizontalInput == 0 && isMovingHorizontally) || isCrouching){
            playerAnim.SetFloat("Speed_f", 0);
            isMovingHorizontally = false;
        }
        else if(horizontalInput != 0 && !isMovingHorizontally){
            playerAnim.SetBool("Static_b", true);
            playerAnim.SetFloat("Speed_f", 0.75f);
            isMovingHorizontally = true;
        }
        
    }
    
    private void FireInputAnimation(){
        if(fireInput != 0 && !isFiring){
            playerAnim.SetInteger("WeaponType_int", 10);
            isFiring = true;
            StartCoroutine(FiringCooldown());
        }
    }

    private void ManageJumpingAnimation(){
        playerAnim.SetTrigger("Jump_trig");
    }

    private void ManageCrouchingAnimation(){
        if(verticalInput != 0 && !isCrouching && !isJumping){
            playerAnim.SetBool("Crouch_b", true);
            isCrouching = true;
        }
        else if(verticalInput == 0 && isCrouching){
            playerAnim.SetBool("Crouch_b", false);
            isCrouching = false;
        }
    }

    private void InversePlayerRotation(){
        isMovingRight = !isMovingRight;
        transform.rotation = Quaternion.Inverse(transform.rotation);
    }

    private void ManageDeathAnimation(){
        playerAnim.SetBool("Death_b", true);
        playerAnim.SetInteger("DeathType_int", 2);
    }

    private IEnumerator FiringCooldown()
    {
        yield return new WaitForSeconds(0.6f);
        playerAnim.SetInteger("WeaponType_int", 0);
        isFiring = false;
    }
    
}
