using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float horizontalInput, verticalInput, jumpInput, fireInput;
    public float speed = 5.5f;
    public float jumpForce = 14.0f;
    private float enemyPushForce = 20.0f;
    private float xBound = 17.0f;
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
    private bool jumpCooldownElapsed = false;
    public bool collidingWithGround = false;
    private int collisions = 0;

    // Start is called before the first frame update
    void Start()
    {
        playerRb = GetComponent<Rigidbody>();
        playerAnim = GetComponentInChildren<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!gameOver)
        {
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

    void FixedUpdate()
    {
        // if jumping is detected and player is on the ground, the player goes up on the Y-Axis with a force
        if (isJumping && isOnGround)
        {
            Jump();
            isJumping = false;
            StartCoroutine(JumpCooldown());
            // we slow down the player by 50% when jumping
            horizontalInput /= 2;
        }
        //Debug.Log(collisions);
        if (collisions == 0 || (collisions > 0 && !collidingWithGround))
        {
            isOnGround = false;
        }
        else
        {
            isOnGround = true;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Never returns null because of "root"
        GameObject parentOfGameObjectCollidedWith = collision.transform.root.gameObject;
        collisions++;

        if (!gameOver && (parentOfGameObjectCollidedWith.CompareTag("Ground Breakable") || parentOfGameObjectCollidedWith.CompareTag("Ground Breakable Small")))
        {
            // Destruction of Block
            if ((Math.Abs(collision.contacts[0].point.y - (transform.position.y + GetComponent<Collider>().bounds.size.y)) <= 0.05f) && (Math.Round(collision.contacts[0].point.y, 2) == Math.Round(parentOfGameObjectCollidedWith.transform.position.y - collision.gameObject.GetComponent<Collider>().bounds.size.y / 2, 2)))
            {
                // Debug.Log("collision.contacts[0].point = " + collision.contacts[0].point);
                // Debug.Log("collision.transform.position = " + collision.transform.position);
                // Debug.Log("collision.gameObject.GetComponent<Collider>().bounds.size = " + collision.gameObject.GetComponent<Collider>().bounds.size);
                // Debug.Log("collision.gameObject.GetComponent<Collider>().bounds.size.y = " + collision.gameObject.GetComponent<Collider>().bounds.size.y);
                // Debug.Log("collision.gameObject.GetComponent<Collider>().bounds.size.y / 2 " + collision.gameObject.GetComponent<Collider>().bounds.size.y / 2);
                collision.gameObject.SetActive(false);
                playerAnim.enabled = false;
                collisions--;
            }
        }
        if (!gameOver && (parentOfGameObjectCollidedWith.CompareTag("Ground Breakable") || parentOfGameObjectCollidedWith.CompareTag("Ground Breakable Small") || parentOfGameObjectCollidedWith.CompareTag("Ground")))
        {
            Debug.Log("Math.Round(collision.contacts[0].point.y, 1) = " + Math.Round(collision.contacts[0].point.y, 1));
            Debug.Log("Math.Round(parentOfGameObjectCollidedWith.transform.position.y + collision.gameObject.GetComponent<Collider>().bounds.size.y / 2, 2) = " + Math.Round(parentOfGameObjectCollidedWith.transform.position.y + collision.gameObject.GetComponent<Collider>().bounds.size.y / 2, 1));
            Debug.Log("collision.gameObject.GetComponent<Collider>().bounds.size = " + collision.gameObject.GetComponent<Collider>().bounds.size);
            Debug.Log("collision.gameObject.GetComponent<Collider>().bounds.size.y = " + collision.gameObject.GetComponent<Collider>().bounds.size.y);
            Debug.Log("collision.gameObject.GetComponent<Collider>().bounds.size.y / 2 " + collision.gameObject.GetComponent<Collider>().bounds.size.y / 2);
            if (Math.Abs(Math.Round(collision.contacts[0].point.y, 1) - Math.Round(parentOfGameObjectCollidedWith.transform.position.y + collision.gameObject.GetComponent<Collider>().bounds.size.y / 2, 1)) <= 0.1f)
            {
                collidingWithGround = true;
                if (!jumpCooldownElapsed)
                {
                    jumpCooldownElapsed = true;
                    playerAnim.enabled = true;
                    playerAnim.Rebind();
                    playerAnim.Update(0f);
                    isMovingHorizontally = false;
                    ManageHorizontalAnimation();
                }
            }
            else
            {
                collidingWithGround = false;
            }
        }
        if (collision.gameObject.CompareTag("Chicken"))
        {

            // If the object we hit is the enemy
            // Calculate Angle Between the collision point and the player
            Vector3 dir = collision.contacts[0].point - transform.position;
            // We then get the opposite (-Vector3) and normalize it
            dir = -dir.normalized;
            // And finally we add force in the direction of dir and multiply it by force. 
            // This will push back the player
            playerRb.AddForce(dir * enemyPushForce, ForceMode.Impulse);
            gameOver = true;
            if (!playerAnim.isActiveAndEnabled)
            {
                playerAnim.enabled = true;
            }
            ManageDeathAnimation();
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        collisions--;
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
            playerRb.MovePosition(transform.position + Vector3.right * horizontalInput * speed * Time.deltaTime);
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
        }
        else if (horizontalInput != 0 && !isMovingHorizontally)
        {
            playerAnim.SetBool("Static_b", true);
            playerAnim.SetFloat("Speed_f", 0.75f);
            isMovingHorizontally = true;
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

}
