using System;
using System.Collections;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

public class GroundBehaviour : MonoBehaviour
{
    private GameObject iceFalling;
    private SpawnManager spawnManager;
    private GameObject lastEncounteredChickenGameObject;
    private GameObject groundParent;
    private bool isCollidingWithChicken = false;
    private int groundLine;
    private float eqSpeed = 10.0f;
    private float eqIntensity = 0.1f;
    private Vector3 initialPosition;

    // Start is called before the first frame update
    void Start()
    {
        groundParent = transform.root.gameObject;
        spawnManager = GameObject.Find("Spawn Manager").GetComponent<SpawnManager>();
        // Spawn manager of the ice falling
        if (!groundParent.CompareTag("Cloud"))
        {
            iceFalling = groundParent.transform.Find("Ice Falling").gameObject;
            if (groundLine != 0)
            {
                if (groundLine > spawnManager.GetMaxRow())
                {
                    StartCoroutine(SpawnFallingIcePeriodically(300));
                }
                else if (groundLine > spawnManager.GetHardPhaseLineLimit())
                {
                    StartCoroutine(SpawnFallingIcePeriodically(100));
                }
                else if (groundLine > spawnManager.GetMidPhaseLineLimit())
                {
                    StartCoroutine(SpawnFallingIcePeriodically(200));
                }
                else
                {
                    StartCoroutine(SpawnFallingIcePeriodically(300));
                }

            }
            if (groundLine % 2 != 0)
            {
                StartCoroutine(RandomEarthQuake());
            }
        }

        initialPosition = groundParent.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        ConstraintBlockPosition();
    }


    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Chicken") && !isCollidingWithChicken)
        {
            isCollidingWithChicken = true;
            if (lastEncounteredChickenGameObject != collision.gameObject)
            {
                lastEncounteredChickenGameObject = collision.gameObject;
            }
        }
    }
    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Chicken") && isCollidingWithChicken)
        {
            isCollidingWithChicken = false;
        }
    }

    public bool IsCollidingWithChicken()
    {
        return isCollidingWithChicken;
    }

    public void SetLine(int line)
    {
        groundLine = line;
    }

    public int GetLine()
    {
        return groundLine;
    }

    public GameObject GetLastChickenCollidedWith()
    {
        return lastEncounteredChickenGameObject;
    }

    private IEnumerator SpawnFallingIcePeriodically(float seconds)
    {
        float waitingTime;

        while (true)
        {
            waitingTime = Random.Range(1.0f, seconds);

            yield return new WaitForSeconds(waitingTime);

            SpawnFallingIce();

        }

    }

    private IEnumerator RandomEarthQuake()
    {
        float waitingTime;

        while (true)
        {
            waitingTime = Random.Range(1.0f, 75.0f);

            yield return new WaitForSeconds(waitingTime);

            if(iceFalling)
            StartCoroutine(MakeEarthQuake(2));


        }

    }

    private IEnumerator MakeEarthQuake(float time)
    {
        float counter = 0;
        while (counter <= time)
        {
            groundParent.transform.position = initialPosition;
            counter += Time.deltaTime;
            groundParent.transform.position += eqIntensity * new Vector3(
                Mathf.PerlinNoise(eqSpeed * Time.time, 1) * ChooseBetweenTwoNumbers(1, -1),
                Mathf.PerlinNoise(eqSpeed * Time.time, 2) * ChooseBetweenTwoNumbers(1, -1),
                Mathf.PerlinNoise(eqSpeed * Time.time, 3) * ChooseBetweenTwoNumbers(1, -1));

            //Wait for a frame so that we don't freeze Unity
            yield return null;
        }
        groundParent.transform.position = initialPosition;
    }

    private void SpawnFallingIce()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, GetComponentInChildren<Collider>().bounds.size.y * 2);
        if (gameObject.activeSelf && !hitColliders.Any(collider => IsColliderBelow(collider)))
        {
            iceFalling.SetActive(true);
        }
    }

    private bool IsColliderBelow(Collider collider)
    {
        GameObject collisionParent = collider.transform.root.gameObject;
        if ((groundParent.transform.position.y - collisionParent.transform.position.y) >= 0.1f && Mathf.Approximately(collisionParent.transform.position.x, groundParent.transform.position.x))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private int ChooseBetweenTwoNumbers(int number1, int number2)
    {
        if (Random.Range(1, 3) == 1)
        {
            return number1;
        }
        else
        {
            return number2;
        }
    }
    // Y axis bound checking to prevent ground from going out of bounds
    private void ConstraintBlockPosition()
    {
        // If the ground quits the screen from bottom
        if(transform.position.y < spawnManager.GetVerticalLimitPosition()){
            Destroy(gameObject);
        }
    }
}
