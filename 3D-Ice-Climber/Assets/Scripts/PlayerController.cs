using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float horizontalInput, verticalInput, jumpInput;
    public float speed = 5.0f;
    public float jumpForce = 12.0f;
    private float enemyPushForce = 20.0f;
    private float xBound = 11.0f; 
    private bool isJumping = false;
    private bool isOnGround = false;
    private bool jumpingKeyIsReleased = true;
    private Rigidbody playerRb;
    
    // Start is called before the first frame update
    void Start()
    {
        playerRb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        // At each frame we check if the player is jumping, the user jump input, all depending whether 
        // the player is on the ground or not
        playerCheckJumpingState();
        // provides horizontal movement to the player at each frame
        movePlayerHorizontally();
        // locks player controls if in specific state like jumping
        lockPlayerControls();
        constraintPlayerPosition();
        
    }

    void FixedUpdate(){
        // if jumping is detected and player is on the ground, the player goes up on the Y-Axis with a force
        if(isJumping && isOnGround){
            jump();
            isJumping = false;
            isOnGround = false;
            // we slow down the player by 50% when jumping
            horizontalInput /= 2;
        }
    }

    private void OnCollisionEnter(Collision collision){
        if(collision.gameObject.CompareTag("Ground")){
            isOnGround = true;
        }
        if(collision.gameObject.CompareTag("Seal")){
            Debug.Log("Ouch!");

            // If the object we hit is the enemy
            // Calculate Angle Between the collision point and the player
            Vector3 dir = collision.contacts[0].point - transform.position;
            // We then get the opposite (-Vector3) and normalize it
            dir = -dir.normalized;
            // And finally we add force in the direction of dir and multiply it by force. 
            // This will push back the player
            GetComponent<Rigidbody>().AddForce(dir * enemyPushForce, ForceMode.Impulse);
        }
    }

    // Player jumping state control
    private void playerCheckJumpingState(){
        // if the player is not already jumping, is on the ground, released the jump key, and is pressing jump
        if(!isJumping && isOnGround && jumpingKeyIsReleased && jumpInput > 0){
            isJumping = true;
            jumpingKeyIsReleased = false;
        }
        // verification of whether the player has released the jump key or not to avoid jumping in loop
        else if(!jumpingKeyIsReleased && jumpInput == 0 && isOnGround){
            jumpingKeyIsReleased = true;
        }
    }

    private void jump(){
        playerRb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    // reads input of the player on X axis, Y axis and jump input for both keyboard and controller
    private void inputAxisReadings(){
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");
        jumpInput = Input.GetAxisRaw("Jump");
    }
    
    // provides horizontal movement to the player
    private void movePlayerHorizontally(){
        transform.Translate(Vector3.right * horizontalInput * speed * Time.deltaTime);
    }

    // locks some of the controls of the player when the player isn't on the ground (we stop reading the inputs)
    // unless he is in not on the ground, standing still, and descending towards the ground.
    private void lockPlayerControls(){
        if(isOnGround || (!isOnGround && (horizontalInput == 0) && (playerRb.velocity.y <= 0))){
            inputAxisReadings();
        }
    }

    // X axis bound checking to prevent player from going out of bounds
    private void constraintPlayerPosition(){
        if(transform.position.x < -xBound){
            transform.position = new Vector3(xBound, transform.position.y, transform.position.z);
        }
        else if(transform.position.x > xBound){
            transform.position = new Vector3(-xBound, transform.position.y, transform.position.z);
        }
    }
}
