using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

public class FallingIceBehaviour : MonoBehaviour
{
    private Rigidbody iceRigidbody;
    private Vector3 originalScale;
    private Vector3 originalPosition;
    private Vector3 originalBounds;
    // Start is called before the first frame update
    void Start()
    {
        iceRigidbody = GetComponent<Rigidbody>();
        iceRigidbody.constraints = RigidbodyConstraints.FreezeAll;
        originalScale = transform.localScale;
        originalPosition = transform.position;
        originalBounds = GetComponent<Collider>().bounds.size;
        StartCoroutine(IceGrowing());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private IEnumerator IceGrowing()
    {
        List<float> iceScales = new List<float>(){0.05f , 0.125f, 0.25f, 0.5f, 1f};
        float waitingTime = 0.7f;

        for(int i = 0; i < iceScales.Count(); i++){
            transform.localScale = new Vector3(gameObject.transform.localScale.x, originalScale.y * iceScales[i], gameObject.transform.localScale.z);
            float newIceHeight = GetComponent<Renderer>().bounds.size.y;
            float differenceFromOriginalHeight = (originalBounds.y - newIceHeight) / 2;
            transform.position = new Vector3(transform.position.x, transform.position.y + differenceFromOriginalHeight, transform.position.z);
            yield return new WaitForSeconds(waitingTime);
            transform.position = originalPosition;
        }
        iceRigidbody.constraints = RigidbodyConstraints.None;


    }
}
