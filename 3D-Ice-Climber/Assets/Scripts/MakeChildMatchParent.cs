using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MakeChildMatchParent : MonoBehaviour
{
    private GameObject objToSpawn;
    // Start is called before the first frame update
    void Start()
    {
        //spawn object
        objToSpawn = new GameObject("Cool GameObject made from Code");
        objToSpawn.transform.position = GetComponent<Collider>().bounds.center;
        //Add Components
        // objToSpawn.AddComponent<Rigidbody>();
        // objToSpawn.AddComponent<MeshFilter>();
        // objToSpawn.AddComponent<BoxCollider>();
        // objToSpawn.AddComponent<MeshRenderer>();

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
