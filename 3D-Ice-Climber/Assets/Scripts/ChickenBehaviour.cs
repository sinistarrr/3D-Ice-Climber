using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChickenBehaviour : MonoBehaviour
{
    public float speed = 2.0f;
    public float runningSpeedMultiplier = 2.0f;
    public ParticleSystem chickenParticle;
    private SpawnManager spawnManager;
    private float xBound = 17.0f;
    private int collisionsWithGround = 0;
    private bool runningAwayMode = false;
    private bool repairingMode = false;
    private int line;
    public int chickenDirection;
    private bool deathIsActivated = false;
    private bool isFalling = false;

    // Start is called before the first frame update
    void Start()
    {
        spawnManager = GameObject.Find("Spawn Manager").GetComponent<SpawnManager>();
    }

    // Update is called once per frame
    void Update()
    {
        MoveChickenHorizontally();
        MoveChickenVertically();
        ConstraintChickenPosition();
        if(deathIsActivated){
            transform.GetChild(0).Rotate(0,400*Time.deltaTime,0);
        }
    }

    // provides horizontal movement to the chicken
    private void MoveChickenHorizontally()
    {
        if(!isFalling){
            if(runningAwayMode || deathIsActivated){
                transform.Translate(Vector3.forward * speed * runningSpeedMultiplier * Time.deltaTime, Space.Self);
            }
            else{
                transform.Translate(Vector3.forward * speed * Time.deltaTime, Space.Self);
            }
        }
    }
    private void MoveChickenVertically()
    {
        if(isFalling){
            transform.Translate(Vector3.down * speed * runningSpeedMultiplier * Time.deltaTime, Space.Self);
        }
    }

    // X axis bound checking to prevent chicken from going out of bounds
    private void ConstraintChickenPosition()
    {
        if (transform.position.x < -xBound)
        {
            ManageIfChickenRunsAway(xBound);
        }
        else if (transform.position.x > xBound)
        {
            ManageIfChickenRunsAway(-xBound);
        }
        // If the chicken goes too far on the bottom of the screen
        if(transform.position.y < spawnManager.GetVerticalLimitPosition()){
            Destroy(gameObject);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {   
        if(!collision.gameObject.CompareTag("Weapon") && !collision.gameObject.CompareTag("Player")){
            collisionsWithGround++;
        }
        if(collisionsWithGround > 0 && isFalling){
            isFalling = false;
            InverseDirection();
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if(!collision.gameObject.CompareTag("Weapon") && !collision.gameObject.CompareTag("Player")){
            collisionsWithGround--;
        }
        if(!deathIsActivated){
            if((collisionsWithGround == 0) && !runningAwayMode && !repairingMode){
                StartCoroutine(RunningAwayActivation());
                InverseDirection();
            }
            if((collisionsWithGround == 0) && repairingMode){
                SpawnManager smScript = GameObject.Find("Spawn Manager").GetComponent<SpawnManager>();
                int lastBlockCollidedWithIndex = smScript.listOfGrounds[line].FindIndex(elem => Mathf.Approximately(elem.transform.position.x, collision.gameObject.transform.position.x));
                if(!smScript.listOfGrounds[line][lastBlockCollidedWithIndex + chickenDirection].activeInHierarchy){
                    smScript.listOfGrounds[line][lastBlockCollidedWithIndex + chickenDirection].SetActive(true);
                }
                if(!smScript.listOfGrounds[line][lastBlockCollidedWithIndex + chickenDirection * 2].activeInHierarchy){
                    smScript.listOfGrounds[line][lastBlockCollidedWithIndex + chickenDirection * 2].SetActive(true);
                }
                repairingMode = false;
                transform.Find("Chicken/ChickenRig_SHJntGrp/ChickenRig_ROOTSHJnt/Ground_Breakable").gameObject.SetActive(false);

            }
        }
        if((collisionsWithGround == 0) && (deathIsActivated || runningAwayMode)){
            if(!deathIsActivated && runningAwayMode){
                SetDeathActivation(true);
            }
            MakeChickenFall();
        }
    }
    private void ManageIfChickenRunsAway(float xBoundLimit){
        if(deathIsActivated){
            Destroy(gameObject);
        }
        else{
            if(runningAwayMode){
                runningAwayMode = false;
                repairingMode = true;
                InverseDirection();
                transform.Find("Chicken/ChickenRig_SHJntGrp/ChickenRig_ROOTSHJnt/Ground_Breakable").gameObject.SetActive(true);
            }
            else{
                transform.position = new Vector3(xBoundLimit, transform.position.y, transform.position.z);
            }
        }
    }

    public void SetLine(int lineNumber){
        line = lineNumber;
    }
    public int GetLine(){
        return line;
    }
    public void SetDirection(int direction){
        chickenDirection = direction;
    }
    public int GetDirection(){
        return chickenDirection;
    }
    public void InverseDirection(){
        chickenDirection *= -1;
        transform.RotateAround(transform.position, transform.up, 180f);
    }
    public void SetDeathActivation(bool state){
        deathIsActivated = state;
        if(deathIsActivated){
            chickenParticle.Stop();
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            Physics.IgnoreCollision(player.GetComponent<Collider>(), GetComponent<Collider>());
        }
    }
    public bool IsDead(){
        return deathIsActivated;
    }
    public void MakeChickenFall(){
        isFalling = true;
        StartCoroutine(FallingCooldown());
    }
    public void CollisionUpdateOnDestroyedGround(){
        collisionsWithGround--;
    }

    private IEnumerator FallingCooldown()
    {
        GetComponent<Collider>().enabled = false;
        yield return new WaitForSeconds(0.75f);
        GetComponent<Collider>().enabled = true;
    }
    private IEnumerator RunningAwayActivation()
    {
        yield return new WaitForSeconds(0.1f);
        runningAwayMode = true;
    }
}
