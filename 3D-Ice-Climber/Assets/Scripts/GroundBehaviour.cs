using System;
using System.Collections;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

public class GroundBehaviour : MonoBehaviour
{
    public GameObject fallingIce;
    private GameObject lastEncounteredChickenGameObject;
    private GameObject groundParent;
    private bool isCollidingWithChicken = false;
    private int groundLine;
    private bool groundHasAnotherGroundBelowIt = false;
    // Start is called before the first frame update
    void Start()
    {
        groundParent = transform.root.gameObject;
        // Spawn manager of the ice falling
        StartCoroutine(SpawnFallingIcePeriodically());
    }

    // Update is called once per frame
    void Update()
    {

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

    private IEnumerator SpawnFallingIcePeriodically()
    {
        float waitingTime;

        while (true)
        {
            waitingTime = Random.Range(1.0f, 500.0f);

            yield return new WaitForSeconds(waitingTime);

            if (!SpawnFallingIce())
            {
                break;
            }
        }

    }
    private bool SpawnFallingIce()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, GetComponentInChildren<Collider>().bounds.size.y * 2);
        if (gameObject.activeSelf && !hitColliders.Any(collider => IsColliderBelow(collider)))
        {

            SpawnManager spawnManagerScript = GameObject.Find("Spawn Manager").GetComponent<SpawnManager>();
            float yPos = groundParent.transform.position.y - GetComponentInChildren<Collider>().bounds.size.y / 2 - spawnManagerScript.GetIceFallingBounds().y / 2;
            Vector3 spawnPos = new Vector3(groundParent.transform.position.x, yPos, groundParent.transform.position.z);
            Instantiate(fallingIce, spawnPos, fallingIce.transform.rotation);
            return true;
        }
        else
        {
            return false;
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
}
