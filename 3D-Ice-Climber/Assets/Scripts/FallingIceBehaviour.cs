using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

public class FallingIceBehaviour : MonoBehaviour
{
    // private Rigidbody iceRigidbody;
    private Vector3 originalScale;
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private Vector3 originalBounds;
    private SpawnManager spawnManager;
    private bool isCurrentlyActive = true;
    private List<float> iceScales = new List<float>() { 0.05f, 0.125f, 0.25f, 0.5f, 1f };
    private MeshRenderer iceRenderer;
    private float speed = 0;//Don't touch this
    private float maxSpeed = 30;//This is the maximum speed that the object will achieve
    private float acceleration = 12;//How fast will object reach a maximum speed
    private float deceleration = 10;//How fast will object reach a speed of 0
    public bool isCurrentlyMovingDown = false;
    public bool isGrowing = false;
    // Start is called before the first frame update
    void Start()
    {
        spawnManager = GameObject.Find("Spawn Manager").GetComponent<SpawnManager>();
        // iceRigidbody = GetComponent<Rigidbody>();
        // iceRigidbody.constraints = RigidbodyConstraints.FreezeAll;
        originalScale = transform.localScale;
        originalPosition = transform.position;
        originalRotation = transform.rotation;
        originalBounds = GetComponent<Collider>().bounds.size;
        iceRenderer = GetComponent<MeshRenderer>();
        StartCoroutine(IceGrowing());
    }

    // Update is called once per frame
    void Update()
    {
        if (gameObject.activeSelf && !isCurrentlyActive)
        {
            isCurrentlyActive = true;
            StartCoroutine(IceGrowing());
        }
        if (isCurrentlyMovingDown)
        {
            if(speed < maxSpeed){
                speed = speed + acceleration * Time.deltaTime;
            }
            transform.position = new Vector3(transform.position.x, transform.position.y - speed * Time.deltaTime, transform.position.z);
        }



        ConstraintIceFallingPosition();
    }

    private IEnumerator IceGrowing()
    {
        isGrowing = true;
        GetComponent<Collider>().enabled = false;

        float waitingTime = 0.7f;

        for (int i = 0; i < iceScales.Count(); i++)
        {
            transform.localScale = new Vector3(transform.localScale.x, originalScale.y * iceScales[i], transform.localScale.z);
            if (!iceRenderer.enabled)
            {
                iceRenderer.enabled = true;
            }
            float newIceHeight = iceRenderer.bounds.size.y;
            float differenceFromOriginalHeight = (originalBounds.y - newIceHeight) / 2;
            transform.position = new Vector3(transform.position.x, transform.position.y + differenceFromOriginalHeight, transform.position.z);
            yield return new WaitForSeconds(waitingTime);
            transform.position = originalPosition;
        }
        GetComponent<Collider>().enabled = true;
        isCurrentlyMovingDown = true;
        isGrowing = false;


    }

    private void ConstraintIceFallingPosition()
    {
        // If the ground quits the screen from bottom
        if (transform.position.y < spawnManager.GetVerticalLimitPosition())
        {
            transform.position = originalPosition;
            transform.rotation = originalRotation;
            isCurrentlyActive = false;
            iceRenderer.enabled = false;
            isCurrentlyMovingDown = false;
            speed = 0;
            gameObject.SetActive(false);
        }
    }


}
